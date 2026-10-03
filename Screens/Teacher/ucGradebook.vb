Public Class ucGradebook
    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Dim mainForm As MainForm = TryCast(Me.FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            mainForm.ShowPage(New ucTeacherDashboard())
        End If
    End Sub
    Private Sub StyleRubricButton(b As Button, text As String)
        b.Text = text
        b.AutoSize = True
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 1
        b.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200)
        b.BackColor = Color.White
        b.ForeColor = Color.Gray
        b.Font = New Font("Century Gothic", 9)
        b.Margin = New Padding(0, 0, 10, 0)
    End Sub
    Private Sub ucGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadDummyStudents()
    End Sub

    Private Sub LoadDummyStudents()
        dgvGradebook.Rows.Clear()
        For i As Integer = 1 To 14
            dgvGradebook.Rows.Add(i, "Last Name, First Name M.I.", "", "5%", "5.00", "", "", "", "50.00", "", "50.00", "10.00", "", "0.00", "", "50.00", "", "")
        Next
    End Sub

End Class
