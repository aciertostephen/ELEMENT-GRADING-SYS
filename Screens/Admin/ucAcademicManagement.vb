Public Class ucAcademicManagement

    Private Sub ucAcademicManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddStatBox("SEMESTER", "1")
        AddStatBox("TOTAL SECTIONS", "24")
        AddStatBox("TOTAL COURSES", "46")
        cboYearLevel.SelectedIndex = 0
        cboSection.SelectedIndex = 0
        cboAY.SelectedIndex = 0
        cboSemester.SelectedIndex = 0
        LoadDummyCourses()
    End Sub

    Private Sub AddStatBox(caption As String, value As String)
        Dim box As New RoundedPanel()
        box.Size = New Size(350, 80)
        box.BackColor = Color.White
        box.Margin = New Padding(15, 15, 0, 0)

        Dim lblCaption As New Label()
        lblCaption.Text = caption
        lblCaption.ForeColor = Color.Gray
        lblCaption.Font = New Font("Century Gothic", 8, FontStyle.Regular)
        lblCaption.Location = New Point(15, 12)
        lblCaption.AutoSize = True

        Dim lblValue As New Label()
        lblValue.Text = value
        lblValue.Font = New Font("Century Gothic", 17, FontStyle.Bold)
        lblValue.Location = New Point(13, 32)
        lblValue.AutoSize = True

        box.Controls.Add(lblCaption)
        box.Controls.Add(lblValue)
        flowStats.Controls.Add(box)
    End Sub
    Private Sub LoadDummyCourses()
        flowCourses.Controls.Clear()
        For i As Integer = 1 To 6
            Dim card As New ucCourseCard()
            card.SetData("COURSECODE - SUBJECT CODE", "BSCS 2A", "2", "42", 78)
            card.Margin = New Padding(15)
            AddHandler card.CardClicked, Sub() MessageBox.Show("Card clicked — will open course detail later")
            flowCourses.Controls.Add(card)
        Next
    End Sub

End Class
