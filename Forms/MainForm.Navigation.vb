Partial Public Class MainForm

    Private Sub ShowPage(page As UserControl)
        pnlContent.Controls.Clear()
        page.Dock = DockStyle.Fill
        pnlContent.Controls.Add(page)
    End Sub

    Private Sub SetupNavForRole(role As String)

        flowNav.Controls.Clear()

        Select Case role
            Case "Admin"
                AddNavButton("USER MANAGEMENT", Sub() ShowPage(New ucAcademicManagement()))
                AddNavButton("ACADEMIC MANAGEMENT", Sub() ShowPage(New ucAcademicManagement()))
                AddNavButton("GRADING CONFIGURATION", Sub() ShowPage(New ucGradingConfig()))

            Case "Teacher"
                ' no nav links per the mockup — Profile Settings only

            Case "Student"
                AddNavButton("MAIN DASHBOARD", Sub() ShowPage(New ucStudentDashboard()))
                AddNavButton("ALL TRANSCRIPTS", Sub() ShowPage(New ucTranscript()))
                AddNavButton("ANALYTICS", Sub() ShowPage(New ucAnalytics()))
        End Select

    End Sub

    Private Sub AddNavButton(text As String, onClick As Action)

        Dim b As New Button()
        b.Text = text
        b.AutoSize = True
        b.Margin = New Padding(20, 0, 0, 0)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 110, 80)   ' subtle green highlight, not white
        b.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 90, 65)    ' slightly darker on click
        b.BackColor = Color.Transparent
        b.UseVisualStyleBackColor = False
        b.ForeColor = Color.White
        b.Font = New Font("Century Gothic", 10, FontStyle.Bold)
        b.Cursor = Cursors.Hand
        b.TabStop = False

        AddHandler b.Click, Sub(s, e) onClick()

        flowNav.Controls.Add(b)

    End Sub

End Class