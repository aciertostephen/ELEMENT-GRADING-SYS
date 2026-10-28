Imports MySqlConnector
Imports ScottPlot
Imports Color = System.Drawing.Color

Public Class ucAnalytics

    Private _studentId As String
    Private ReadOnly cboTrendPeriod As New ComboBox() With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 150, .Font = New Font("Century Gothic", 10.8F)}
    Private _componentData As List(Of KeyValuePair(Of String, Double)) = New List(Of KeyValuePair(Of String, Double))

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(studentId As String)
        Me.New()
        _studentId = studentId
    End Sub

    Private Sub ucAnalytics_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        BuildAnalyticsLayout()
        LoadCourseFilter()
        cboTrendPeriod.Items.AddRange(New Object() {"Weekly", "Monthly", "Yearly"})
        cboTrendPeriod.SelectedItem = "Monthly"
        AddHandler cboCourses.SelectedIndexChanged, AddressOf FilterChanged
        AddHandler cboTrendPeriod.SelectedIndexChanged, AddressOf FilterChanged
        LoadGradeStandingChart()
        LoadComponentAnalyticsChart()
        LoadAverageTrendChart()
    End Sub

    Private Sub BuildAnalyticsLayout()
        Controls.Remove(tblMainDock)
        BackColor = Color.FromArgb(246, 248, 247)
        Dim surface As New RoundedPanel With {
            .Dock = DockStyle.Fill, .Margin = New Padding(20), .Padding = New Padding(24),
            .BackColor = Color.White, .BorderColor = Color.FromArgb(209, 213, 219), .ActiveBorderColor = Color.FromArgb(209, 213, 219),
            .BorderThickness = 1, .Radius = 16
        }
        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 3,
            .BackColor = Color.Transparent, .Padding = New Padding(0), .Margin = New Padding(0)
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 58.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 53.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 47.0F))

        Dim heading As New System.Windows.Forms.Label With {
            .Text = "Your analytics", .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New System.Drawing.Font("Century Gothic", 17.0F, System.Drawing.FontStyle.Bold), .ForeColor = Color.FromArgb(34, 90, 74)
        }
        cboCourses.Font = New Font("Segoe UI", 10.0F)
        cboCourses.Size = New Size(225, 32)
        cboCourses.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cboCourses.Margin = New Padding(8, 10, 8, 6)
        cboTrendPeriod.Font = New Font("Segoe UI", 10.0F)
        cboTrendPeriod.Size = New Size(150, 32)
        cboTrendPeriod.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cboTrendPeriod.Margin = New Padding(8, 10, 0, 6)
        Dim filters As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill, .FlowDirection = FlowDirection.RightToLeft, .WrapContents = False,
            .BackColor = Color.Transparent, .Margin = New Padding(0)
        }
        filters.Controls.Add(cboTrendPeriod)
        filters.Controls.Add(cboCourses)
        layout.Controls.Add(heading, 0, 0)
        layout.Controls.Add(filters, 1, 0)

        Dim topLeft As Control = MakeChart("Grade standing", FormsPlot1)
        Dim topRight As Control = MakeChart("Category breakdown", FormsPlot2)
        Dim bottom As Control = MakeChart("Released grade trend", FormsPlot3)
        layout.Controls.Add(topLeft, 0, 1)
        layout.Controls.Add(topRight, 1, 1)
        layout.Controls.Add(bottom, 0, 2)
        layout.SetColumnSpan(bottom, 2)
        surface.Controls.Add(layout)
        Controls.Add(surface)
        surface.BringToFront()
    End Sub

    Private Function MakeChart(title As String, plot As Control) As Control
        Dim region As New TableLayoutPanel With {
            .Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 2,
            .BackColor = Color.Transparent, .Padding = New Padding(10, 4, 10, 8), .Margin = New Padding(4)
        }
        region.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        region.RowStyles.Add(New RowStyle(SizeType.Absolute, 32.0F))
        region.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        Dim caption As New System.Windows.Forms.Label With {
            .Text = title, .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New System.Drawing.Font("Century Gothic", 11.0F, System.Drawing.FontStyle.Bold), .ForeColor = Color.FromArgb(35, 41, 38)
        }
        plot.Dock = DockStyle.Fill
        plot.Margin = New Padding(0)
        region.Controls.Add(caption, 0, 0)
        region.Controls.Add(plot, 0, 1)
        Return region
    End Function

    Private Sub LoadCourseFilter()
        Dim dt As DataTable = AnalyticsService.ActiveCourseClasses(GetStudentId())
        Dim all As DataRow = dt.NewRow()
        all("class_id") = 0
        all("label") = "All Subjects"
        dt.Rows.InsertAt(all, 0)
        cboCourses.DataSource = Nothing
        cboCourses.Items.Clear()
        cboCourses.DisplayMember = "label"
        cboCourses.ValueMember = "class_id"
        cboCourses.DataSource = dt
        cboCourses.SelectedValue = 0
        pnlOverallAverage.Text = "Released Grade Trend"
        lblGradeComponent.Text = "Category Breakdown"

    End Sub

    Private Sub FilterChanged(sender As Object, e As EventArgs)
        LoadComponentAnalyticsChart()
        LoadAverageTrendChart()
    End Sub

    Private Function SelectedClassId() As Integer
        If cboCourses.SelectedValue Is Nothing Then Return 0
        Dim id As Integer
        Return If(Integer.TryParse(cboCourses.SelectedValue.ToString(), id), id, 0)
    End Function

    ' Uses the student id passed in; otherwise looks up the logged-in student (no hardcoded student)
    Private Function GetStudentId() As String
        If Not String.IsNullOrEmpty(_studentId) Then Return _studentId

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT student_id FROM student_profile WHERE user_id = @uid",
            {New MySqlParameter("@uid", SessionManager.CurrentUserId)})

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            _studentId = dt.Rows(0)("student_id").ToString()
        End If
        Return _studentId
    End Function

    Private Sub LoadGradeStandingChart()
        Dim labelsList As New List(Of String)()
        Dim valuesList As New List(Of Double)()

        Try
            Dim targetStudentId As String = GetStudentId()

            If Not String.IsNullOrEmpty(targetStudentId) Then
                ' Query the student's actual courses, course codes, and final grades for the active term
                Dim query As String = "SELECT co.course_code, co.course_name, e.final_grade " &
                                     "FROM enrollment e " &
                                     "JOIN classes c ON e.class_id = c.class_id " &
                                     "JOIN course co ON c.course_id = co.course_id " &
                                     "JOIN academic_term t ON c.term_id = t.term_id " &
                                     "WHERE e.student_id = @sid AND e.status <> 'Dropped' AND t.is_active = 1 " &
                                     "AND c.grades_released_at IS NOT NULL AND e.final_grade IS NOT NULL"

                Dim dt As DataTable = DatabaseHelper.ExecuteQuery(query, {New MySqlParameter("@sid", targetStudentId)})

                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    For Each row As DataRow In dt.Rows
                        labelsList.Add(row("course_code").ToString())
                        valuesList.Add(Convert.ToDouble(row("final_grade")))
                    Next
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading analytics courses: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.None)
        End Try

        Dim labels() As String = labelsList.ToArray()
        Dim values() As Double = valuesList.ToArray()

        FormsPlot1.Plot.Clear()
        If values.Length > 0 Then
            Dim bar = FormsPlot1.Plot.Add.Bars(values)
            FormsPlot1.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, labels.Length).Select(Function(i) CDbl(i)).ToArray(), labels)
            FormsPlot1.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45 ' Rotate labels for better readability
        End If
        FormsPlot1.Refresh()
    End Sub

    Private Sub LoadComponentAnalyticsChart()
        FormsPlot2.Plot.Clear()
        Dim classId As Integer = SelectedClassId()
        _componentData = AnalyticsService.ComponentAverages(GetStudentId(), classId)
        lblGradeComponent.Text = If(classId = 0, "Category Breakdown - All Subjects", "Category Breakdown - Selected Subject")
        If _componentData.Count > 0 Then
            FormsPlot2.Plot.Add.Bars(_componentData.Select(Function(d) d.Value).ToArray())
            FormsPlot2.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, _componentData.Count).Select(Function(i) CDbl(i)).ToArray(),
                                         _componentData.Select(Function(d) d.Key).ToArray())
        End If
        FormsPlot2.Refresh()
    End Sub

    Private Sub LoadAverageTrendChart()
        FormsPlot3.Plot.Clear()
        Dim period As String = If(cboTrendPeriod.SelectedItem Is Nothing, "Monthly", cboTrendPeriod.SelectedItem.ToString())
        Dim data = AnalyticsService.GradeTrend(GetStudentId(), period)
        If data.Count > 0 Then
            FormsPlot3.Plot.Add.Scatter(Enumerable.Range(0, data.Count).Select(Function(i) CDbl(i)).ToArray(),
                                data.Select(Function(d) d.Value).ToArray())
            FormsPlot3.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, data.Count).Select(Function(i) CDbl(i)).ToArray(),
                                         data.Select(Function(d) d.Key).ToArray())
        End If
        FormsPlot3.Refresh()
    End Sub

End Class
