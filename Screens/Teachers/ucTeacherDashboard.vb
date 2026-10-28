Imports System.ComponentModel
Imports System.Data
Imports System.Linq
Imports MySqlConnector

Public Class ucTeacherDashboard

    Private _classes As DataTable
    Private _yearFilter As String = ""
    Private _courseFilter As String = ""

    Private Sub ucTeacherDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If LicenseManager.UsageMode = LicenseUsageMode.Designtime Then Return
        LoadCourses()
    End Sub

    '========================================
    ' LOAD THE TEACHER'S CLASSES (ACTIVE TERM)
    '========================================
    Private Sub LoadCourses()
        Try
            Dim sql As String =
                "SELECT c.class_id, co.course_code, co.course_name, s.section_code, " &
                "(SELECT COUNT(*) FROM enrollment e WHERE e.class_id = c.class_id AND e.status = 'Enrolled') AS student_count, " &
                "(SELECT COUNT(*) FROM enrollment e WHERE e.class_id = c.class_id AND e.status = 'Enrolled' " &
                "    AND e.final_grade IS NOT NULL) AS graded_count, " &
                "(SELECT MIN(sp.year_level) FROM enrollment e2 " &
                "    JOIN student_profile sp ON sp.student_id = e2.student_id " &
                "    WHERE e2.class_id = c.class_id) AS year_level " &
                "FROM classes c " &
                "JOIN course co ON co.course_id = c.course_id " &
                "JOIN section s ON s.section_id = c.section_id " &
                "JOIN teacher_profile tp ON tp.teacher_id = c.teacher_id " &
                "JOIN academic_term t ON t.term_id = c.term_id " &
                "WHERE tp.user_id = @uid AND t.is_active = 1 " &
                "ORDER BY co.course_code, co.course_name, s.section_code"

            _classes = DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@uid", SessionManager.CurrentUserId)})
        Catch ex As Exception
            _classes = Nothing
            MessageBox.Show("Error loading courses: " & ex.Message, "Database Error",
                            MessageBoxButtons.OK, MessageBoxIcon.None)
        End Try

        RenderCards()
    End Sub

    '========================================
    ' CARDS (uses your existing ucCourseCard)
    '========================================
    Private Sub RenderCards()
        flowCourses.SuspendLayout()

        Dim old As List(Of Control) = flowCourses.Controls.Cast(Of Control)().ToList()
        flowCourses.Controls.Clear()
        For Each c As Control In old
            c.Dispose()
        Next

        Dim shown As Integer = 0

        If _classes IsNot Nothing Then
            For Each r As DataRow In _classes.Rows
                Dim yr As String = Convert.ToString(r("year_level"))
                Dim code As String = Convert.ToString(r("course_code"))

                If _yearFilter <> "" AndAlso yr <> _yearFilter Then Continue For
                If _courseFilter <> "" AndAlso code <> _courseFilter Then Continue For

                Dim students As Integer = Convert.ToInt32(r("student_count"))
                Dim graded As Integer = Convert.ToInt32(r("graded_count"))
                Dim progress As Integer = If(students > 0, CInt(Math.Round(graded * 100.0 / students)), 0)

                Dim card As New ucCourseCard()
                card.SetData($"{code} - {r("course_name")}",
                             Convert.ToString(r("section_code")),
                             yr,
                             students.ToString(),
                             progress)
                card.Tag = Convert.ToInt32(r("class_id"))   ' remember which class this card is
                card.Margin = New Padding(15)
                AddHandler card.CardClicked, AddressOf OnCourseCardClicked
                flowCourses.Controls.Add(card)
                shown += 1
            Next
        End If

        If shown = 0 Then
            Dim lbl As New Label()
            lbl.AutoSize = True
            lbl.Font = New Font("Century Gothic", 11)
            lbl.ForeColor = Color.FromArgb(126, 126, 126)
            lbl.Margin = New Padding(20)
            lbl.Text = If(_classes Is Nothing OrElse _classes.Rows.Count = 0,
                          "No classes are assigned to you for the active term.",
                          "No classes match the selected filters.")
            flowCourses.Controls.Add(lbl)
        End If

        flowCourses.ResumeLayout(True)
    End Sub

    Private Sub OnCourseCardClicked(sender As Object, e As EventArgs)
        ' Find the card (sender may be the card itself or one of its inner controls)
        Dim ctrl As Control = TryCast(sender, Control)
        While ctrl IsNot Nothing AndAlso Not TypeOf ctrl Is ucCourseCard
            ctrl = ctrl.Parent
        End While
        If ctrl Is Nothing OrElse ctrl.Tag Is Nothing Then Return

        Dim classId As Integer = Convert.ToInt32(ctrl.Tag)
        Dim main As MainForm = TryCast(Me.FindForm(), MainForm)
        If main Is Nothing Then Return

        ' Opens the gradebook with this class's students (swap after the click handler finishes)
        BeginInvoke(New Action(Sub() main.ShowPage(New ucGradebook(classId))))
    End Sub

    '========================================
    ' FILTERS
    '========================================
    Private Sub btnFilterYear_Click(sender As Object, e As EventArgs) Handles btnFilterYear.Click
        If _classes Is Nothing Then Return

        Dim menu As New ContextMenuStrip()
        Dim years = _classes.AsEnumerable().
            Select(Function(r) Convert.ToString(r("year_level"))).
            Where(Function(y) y <> "").
            Distinct().
            OrderBy(Function(y) y)

        For Each y As String In years
            Dim yr As String = y
            menu.Items.Add($"Year {yr}", Nothing,
                           Sub(s, ev)
                               _yearFilter = yr
                               btnFilterYear.Text = $"Year {yr}  ▾"
                               RenderCards()
                           End Sub)
        Next

        menu.Show(btnFilterYear, New Point(0, btnFilterYear.Height))
    End Sub

    Private Sub btnFilterCourses_Click(sender As Object, e As EventArgs) Handles btnFilterCourses.Click
        If _classes Is Nothing Then Return

        Dim menu As New ContextMenuStrip()
        Dim codes = _classes.AsEnumerable().
            Select(Function(r) Convert.ToString(r("course_code"))).
            Distinct().
            OrderBy(Function(c) c)

        For Each c As String In codes
            Dim code As String = c
            menu.Items.Add(code, Nothing,
                           Sub(s, ev)
                               _courseFilter = code
                               btnFilterCourses.Text = $"{code} ▾"
                               RenderCards()
                           End Sub)
        Next

        menu.Show(btnFilterCourses, New Point(0, btnFilterCourses.Height))
    End Sub

    Private Sub btnFilterAllYear_Click(sender As Object, e As EventArgs) Handles btnFilterAllYear.Click
        _yearFilter = ""
        btnFilterYear.Text = "Year Level  ▾"
        RenderCards()
    End Sub

    Private Sub btnFilterAllCourses_Click(sender As Object, e As EventArgs) Handles btnFilterAllCourses.Click
        _courseFilter = ""
        btnFilterCourses.Text = "Select Courses ▾"
        RenderCards()
    End Sub

End Class
