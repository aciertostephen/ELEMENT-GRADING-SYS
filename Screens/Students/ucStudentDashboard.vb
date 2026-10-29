Imports MySqlConnector

Public Class ucStudentDashboard

    Private _loggedInUserId As Integer ' Set this based on the active user session

    Public Sub New(userId As Integer)
        InitializeComponent()
        _loggedInUserId = userId
    End Sub

    ' Parameterless constructor: uses whoever is logged in (no hardcoded student)
    Public Sub New()
        InitializeComponent()
        _loggedInUserId = SessionManager.CurrentUserId
    End Sub

    Private Sub ucStudentDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadStudentCourses()
    End Sub

    Private Sub LoadStudentCourses()
        flowRubric.Controls.Clear()
        Try
            ' 1. Fetch student_id from student_profile using user_id
            Dim studentQuery As String = "SELECT student_id FROM student_profile WHERE user_id = @uid"
            Dim studentDt As DataTable = DatabaseHelper.ExecuteQuery(studentQuery, {New MySqlParameter("@uid", _loggedInUserId)})

            If studentDt IsNot Nothing AndAlso studentDt.Rows.Count > 0 Then
                Dim studentId As String = studentDt.Rows(0)("student_id").ToString()

                ' 2. Query actual enrolled courses, class details, teachers, and current grades for the active term
                Dim query As String = "SELECT e.enrollment_id, c.class_id, co.course_code, co.course_name, " &
                                 "CONCAT(s.section_code) AS section_name, " &
                                 "CONCAT(tp.last_name, ', ', tp.first_name) AS teacher_name, " &
                                 "COALESCE(CASE WHEN c.grades_released_at IS NULL THEN NULL ELSE e.final_grade END, 0.00) AS final_grade " &
                                 "FROM enrollment e " &
                                 "JOIN classes c ON e.class_id = c.class_id " &
                                 "JOIN course co ON c.course_id = co.course_id " &
                                 "JOIN section s ON c.section_id = s.section_id " &
                                 "JOIN teacher_profile tp ON c.teacher_id = tp.teacher_id " &
                                 "JOIN academic_term t ON c.term_id = t.term_id " &
                                 "WHERE e.student_id = @sid AND e.status <> 'Dropped' AND t.is_active = 1"

                Dim dt As DataTable = DatabaseHelper.ExecuteQuery(query, {New MySqlParameter("@sid", studentId)})

                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    For Each row As DataRow In dt.Rows
                        Dim classId As Integer = Convert.ToInt32(row("class_id"))
                        Dim enrollmentId As String = row("enrollment_id").ToString()
                        Dim courseText As String = $"{row("course_code")} - {row("course_name")}"
                        Dim sectionText As String = row("section_name").ToString()
                        Dim teacherText As String = row("teacher_name").ToString()
                        Dim finalGrade As Decimal = Convert.ToDecimal(row("final_grade"))

                        Dim card As New ucCourseCard()
                        card.SetDataStudent(courseText, sectionText, teacherText, finalGrade)
                        card.Margin = New Padding(15)

                        ' Store classId and enrollmentId inside the card's Tag property for retrieval on click
                        card.Tag = New Tuple(Of Integer, String)(classId, enrollmentId)

                        AddHandler card.CardClicked, AddressOf OnCourseCardClicked
                        flowRubric.Controls.Add(card)
                    Next
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading dashboard courses: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
    End Sub

    Private Sub OnCourseCardClicked(sender As Object, e As EventArgs)
        ' sender may be the card itself or one of its inner controls, so walk up to the card
        Dim ctrl As Control = TryCast(sender, Control)
        While ctrl IsNot Nothing AndAlso Not TypeOf ctrl Is ucCourseCard
            ctrl = ctrl.Parent
        End While
        If ctrl Is Nothing OrElse ctrl.Tag Is Nothing Then Return

        Dim ids = DirectCast(ctrl.Tag, Tuple(Of Integer, String))
        Dim mainForm As MainForm = TryCast(Me.FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            ' Pass the selected class and enrollment context into the student gradebook view
            mainForm.ShowPage(New ucStudentGradebook(ids.Item1, ids.Item2))
        End If
    End Sub

End Class
