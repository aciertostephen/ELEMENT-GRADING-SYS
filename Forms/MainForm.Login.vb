Partial Public Class MainForm

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        pnlLogin.Visible = False
        pnlApp.Visible = True

        SetupNavForRole("Admin")           ' hardcoded for now — real role comes from DB later
        ShowPage(New ucUserManagement())   ' default landing page
    End Sub

End Class