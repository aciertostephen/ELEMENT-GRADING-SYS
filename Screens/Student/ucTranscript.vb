Public Class ucTranscript

    Private Sub ucTranscript_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblStudentInfo.Text = "Student: Last Name, First name M.I." & vbCrLf & "Section: BSCS 2A"
        lblGWA.Text = "GWA : 1.35"

        LoadDummyGrades()
    End Sub

    Private Sub LoadDummyGrades()
        dgvTranscript.Rows.Clear()
        For i As Integer = 1 To 8
            dgvTranscript.Rows.Add("SUBJECT CODE", "SUBJECT NAME", "Last Name, First Name M.I.", "1.35")
        Next
    End Sub

End Class