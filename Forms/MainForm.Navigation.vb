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
                AddNavButton("USER MANAGEMENT", Sub() ShowPage(New ucUserManagement()), isDefault:=True)
                AddNavButton("ACADEMIC MANAGEMENT", Sub() ShowPage(New ucAcademicManagement()))
                AddNavButton("GRADING CONFIGURATION", Sub() ShowPage(New ucGradingConfig()))

            Case "Teacher"
                ' no nav links per the mockup — Profile Settings only

            Case "Student"
                AddNavButton("MAIN DASHBOARD", Sub() ShowPage(New ucStudentDashboard()), isDefault:=True)
                AddNavButton("ALL TRANSCRIPTS", Sub() ShowPage(New ucTranscript()))
                AddNavButton("ANALYTICS", Sub() ShowPage(New ucAnalytics()))
        End Select

    End Sub

    Private Sub AddNavButton(text As String, onClick As Action, Optional isDefault As Boolean = False)

        Dim b As New Button()
        b.Text = text
        b.Margin = New Padding(40, 0, 0, 0)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 110, 80)
        b.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 90, 65)
        b.BackColor = Color.Transparent
        b.UseVisualStyleBackColor = False
        b.ForeColor = Color.White
        b.Font = New Font("Century Gothic", 12, FontStyle.Bold)   ' measure as bold
        b.Cursor = Cursors.Hand
        b.TabStop = False

        ' Lock in the bold-sized width so toggling bold never changes button size
        Dim boldSize As Size = TextRenderer.MeasureText(text, b.Font)
        b.AutoSize = False
        b.Size = New Size(boldSize.Width + 20, boldSize.Height + 10)
        b.TextAlign = ContentAlignment.MiddleCenter

        AddHandler b.Click, Sub(s, e)
                                For Each ctrl As Control In flowNav.Controls
                                    ctrl.Font = New Font(ctrl.Font, FontStyle.Regular)
                                Next
                                b.Font = New Font(b.Font, FontStyle.Bold)
                                onClick()
                            End Sub

        flowNav.Controls.Add(b)

        ' If this is the page that loads by default, mark it active right now
        If isDefault Then
            b.Font = New Font(b.Font, FontStyle.Bold)
        Else
            b.Font = New Font(b.Font, FontStyle.Regular)
        End If

    End Sub

End Class