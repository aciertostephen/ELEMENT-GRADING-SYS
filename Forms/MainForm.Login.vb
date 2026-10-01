Partial Public Class MainForm

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        pnlLogin.Visible = False
        pnlApp.Visible = True

        SetupNavForRole("Admin")
        ShowPage(New ucUserManagement())
    End Sub

End Class