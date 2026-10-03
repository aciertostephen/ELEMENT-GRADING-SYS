Public Class ucTeacherDashboard

    Private Sub ucTeacherDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadDummyCourses()
    End Sub

    Private Sub LoadDummyCourses()
        flowCourses.Controls.Clear()
        For i As Integer = 1 To 6
            Dim card As New ucCourseCard()
            card.SetData("COURSECODE - SUBJECT CODE", "BSCS 2A", "2", "42", 78)
            card.Margin = New Padding(15)
            AddHandler card.CardClicked, AddressOf OnCourseCardClicked
            flowCourses.Controls.Add(card)
        Next
    End Sub

    Private Sub OnCourseCardClicked(sender As Object, e As EventArgs)
        ' Teacher clicking a card opens the editable gradebook
        Dim mainForm As MainForm = TryCast(Me.FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            mainForm.ShowPage(New ucGradebook())
        End If
    End Sub

End Class