Public Class ucStudentDashboard

    Private Sub ucStudentDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadDummyCourses()
    End Sub

    Private Sub LoadDummyCourses()
        flowRubric.Controls.Clear()
        For i As Integer = 1 To 6
            Dim card As New ucCourseCard()
            card.SetDataStudent("COURSECODE - SUBJECT CODE", "BSCS 2A", "LAST NAME, FIRST NAME", 78)
            card.Margin = New Padding(15)
            AddHandler card.CardClicked, AddressOf OnCourseCardClicked
            flowRubric.Controls.Add(card)
        Next
    End Sub

    Private Sub OnCourseCardClicked(sender As Object, e As EventArgs)
        Dim mainForm As MainForm = TryCast(Me.FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            mainForm.ShowPage(New ucStudentGradebook())
        End If
    End Sub

End Class