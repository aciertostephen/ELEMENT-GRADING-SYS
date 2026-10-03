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
    Public Sub SetDataStudent(courseCode As String, section As String, professor As String, progress As Integer)
        lblCourseCode.Text = courseCode
        lblSection.Text = "SECTION : " & section
        lblYear.Text = "PROFESSOR : " & professor   ' reusing the "Year" label's position for Professor
        lblEnrolled.Visible = False                  ' hide the Enrolled line entirely
        lblProgress.Text = "ENCODING PROGRESS : " & progress & "%"
        ProgressBar1.Value = progress
    End Sub

    Private Sub ucCourseCard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Cursor = Cursors.Hand
        HookClickRecursive(Me)
    End Sub

    ' Makes every control inside the card (and the card itself) clickable,
    ' and shows a hand cursor so it actually feels like a button.
    Private Sub HookClickRecursive(parent As Control)
        For Each child As Control In parent.Controls
            child.Cursor = Cursors.Hand
            AddHandler child.Click, AddressOf ChildClicked
            If child.HasChildren Then
                HookClickRecursive(child)
            End If
        Next
    End Sub

    Private Sub ChildClicked(sender As Object, e As EventArgs)
        RaiseEvent CardClicked(Me, e)
    End Sub

    Private Sub ucCourseCard_Click(sender As Object, e As EventArgs) Handles Me.Click
        RaiseEvent CardClicked(Me, e)
    End Sub

End Class