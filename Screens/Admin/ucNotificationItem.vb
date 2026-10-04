Public Class ucNotificationItem

    Public Event ActionClicked(sender As Object, e As EventArgs)

    Public Sub SetData(timestamp As String, message As String, isCompleted As Boolean)
        lblTimestamp.Text = timestamp
        lblMessage.Text = message

        If isCompleted Then
            btnAction.Text = "Completed"
            btnAction.BackColor = Color.FromArgb(220, 220, 220)
            btnAction.ForeColor = Color.Gray
            btnAction.Enabled = False
        Else
            btnAction.Text = "Review"
            btnAction.BackColor = Color.FromArgb(34, 90, 74)
            btnAction.ForeColor = Color.White
            btnAction.Enabled = True
        End If
    End Sub

    Private Sub btnAction_Click(sender As Object, e As EventArgs) Handles btnAction.Click
        RaiseEvent ActionClicked(Me, e)
    End Sub

End Class