Imports MySqlConnector
Imports ScottPlot

Public Class ucAnalytics

    Private _studentId As String

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(studentId As String)
        Me.New()
        _studentId = studentId
    End Sub

    Private Sub ucAnalytics_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadGradeStandingChart()
        LoadComponentAnalyticsChart()
        LoadAverageTrendChart()
    End Sub

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
                Dim query As String = "SELECT co.course_code, co.course_name, COALESCE(CASE WHEN c.grades_released_at IS NULL THEN NULL ELSE e.final_grade END, 0) AS final_grade " &
                                     "FROM enrollment e " &
                                     "JOIN classes c ON e.class_id = c.class_id " &
                                     "JOIN course co ON c.course_id = co.course_id " &
                                     "JOIN academic_term t ON c.term_id = t.term_id " &
                                     "WHERE e.student_id = @sid AND t.is_active = 1"

                Dim dt As DataTable = DatabaseHelper.ExecuteQuery(query, {New MySqlParameter("@sid", targetStudentId)})

                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    For Each row As DataRow In dt.Rows
                        labelsList.Add(row("course_code").ToString())
                        valuesList.Add(Convert.ToDouble(row("final_grade")))
                    Next
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading analytics courses: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
        Dim data = AnalyticsService.ComponentAverages(GetStudentId())
        If data.Count > 0 Then
            FormsPlot2.Plot.Add.Bars(data.Select(Function(d) d.Value).ToArray())
            FormsPlot2.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, data.Count).Select(Function(i) CDbl(i)).ToArray(),
                                         data.Select(Function(d) d.Key).ToArray())
        End If
        FormsPlot2.Refresh()
    End Sub

    Private Sub LoadAverageTrendChart()
        FormsPlot3.Plot.Clear()
        Dim data = AnalyticsService.TermTrend(GetStudentId())
        If data.Count > 0 Then
            FormsPlot3.Plot.Add.Scatter(Enumerable.Range(0, data.Count).Select(Function(i) CDbl(i)).ToArray(),
                                data.Select(Function(d) d.Value).ToArray())
            FormsPlot3.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, data.Count).Select(Function(i) CDbl(i)).ToArray(),
                                         data.Select(Function(d) d.Key).ToArray())
        End If
        FormsPlot3.Refresh()
    End Sub

    Private Sub FormsPlot2_Load(sender As Object, e As EventArgs) Handles FormsPlot2.Load

    End Sub

End Class