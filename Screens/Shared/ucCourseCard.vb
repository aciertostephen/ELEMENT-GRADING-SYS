Public Class ucCourseCard

    Public Event CardClicked(sender As Object, e As EventArgs)

    Public Sub SetData(courseCode As String, section As String, year As String, enrolled As String, progress As Integer)
        lblCourseCode.Text = courseCode
        lblSection.Text = "SECTION : " & section
        lblYear.Text = "YEAR: " & year
        lblEnrolled.Text = "ENROLLED: " & enrolled
        lblProgress.Text = "ENCODING PROGRESS : " & progress & "%"
        ProgressBar1.Value = progress
    End Sub

    Private Sub ucCourseCard_Click(sender As Object, e As EventArgs) Handles Me.Click
        RaiseEvent CardClicked(Me, e)
    End Sub

End Class