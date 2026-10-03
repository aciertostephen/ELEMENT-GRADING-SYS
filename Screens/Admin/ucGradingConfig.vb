Public Class ucGradingConfig
    Private Sub ucGradingConfig_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadGradingScale()
    End Sub
    Private Sub LoadGradingScale()
        dgvGradingScale.Rows.Clear()
        dgvGradingScale.Rows.Add("96.00% - 100%", "1")
        dgvGradingScale.Rows.Add("91.00% - 95.99%", "1.25")
        dgvGradingScale.Rows.Add("86.00% - 90.99%", "1.5")
        dgvGradingScale.Rows.Add("75.00% - 85.99%", "2.00 - 2.50")
        dgvGradingScale.Rows.Add("Below 75.00%", "5")
    End Sub

End Class
