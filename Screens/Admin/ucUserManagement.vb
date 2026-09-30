Public Class ucUserManagement

    Private Sub ucUserManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddStatBox("TOTAL USERS", "946")
        AddStatBox("FACULTY", "46")
        AddStatBox("STUDENTS", "890")
        AddStatBox("INACTIVE", "10")
        LoadDummyUsers()
    End Sub

    Private Sub LoadDummyUsers()
        dgvUsers.Rows.Clear()
        For i As Integer = 1 To 12
            dgvUsers.Rows.Add("25-00571", "Last Name, First Name", "Student", "name@plpasig.edu.ph", "9673347151", "MM/DD/YYYY", "Active")
        Next
        For i As Integer = 1 To 12
            dgvUsers.Rows.Add("EMP001", "Last Name, First Name", "Professor", "name@plpasig.edu.ph", "9673347151", "MM/DD/YYYY", "Active")
        Next
    End Sub

    Private Sub AddStatBox(caption As String, value As String)

        Dim box As New RoundedPanel()
        box.Size = New Size(343, 70)
        box.BackColor = Color.White
        box.Margin = New Padding(15, 25, 0, 0)

        Dim lblCaption As New Label()
        lblCaption.Text = caption
        lblCaption.ForeColor = Color.Gray
        lblCaption.Font = New Font("Century Gothic", 8, FontStyle.Regular)
        lblCaption.Location = New Point(15, 12)
        lblCaption.AutoSize = True

        Dim lblValue As New Label()
        lblValue.Text = value
        lblValue.Font = New Font("Century Gothic", 15, FontStyle.Bold)
        lblValue.ForeColor = Color.Black
        lblValue.Location = New Point(13, 32)
        lblValue.AutoSize = True

        box.Controls.Add(lblCaption)
        box.Controls.Add(lblValue)
        flowStats.Controls.Add(box)

    End Sub

    Private Sub btnDeleteBTN_Click(sender As Object, e As EventArgs) Handles btnDeleteBTN.Click
        CenterDeletePanel()
        pnlDeleteConfirmation.Visible = True
        pnlDeleteConfirmation.BringToFront()
    End Sub
    Private Sub btnCancelDeletion_Click(sender As Object, e As EventArgs) Handles btnCancelDeletion.Click
        pnlDeleteConfirmation.Visible = False
    End Sub

    Private Sub CenterDeletePanel()
        pnlDeleteConfirmation.Location = New Point(
            (Me.ClientSize.Width - pnlDeleteConfirmation.Width) \ 2,
            (Me.ClientSize.Height - pnlDeleteConfirmation.Height) \ 2)
    End Sub

End Class