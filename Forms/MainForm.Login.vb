Partial Public Class MainForm

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click

        Dim role As String = "Admin"   ' default

        If txtUsername.Text.Trim().ToLower() = "teacher" Then
            role = "Teacher"
        ElseIf txtUsername.Text.Trim().ToLower() = "student" Then
            role = "Student"
        End If

        pnlLogin.Visible = False
        pnlApp.Visible = True

        SetupNavForRole(role)

        Select Case role
            Case "Teacher"
                ShowPage(New ucTeacherDashboard())
            Case "Student"
                ShowPage(New ucStudentDashboard())   ' once it exists
            Case Else
                ShowPage(New ucUserManagement())
        End Select

    End Sub
End Class