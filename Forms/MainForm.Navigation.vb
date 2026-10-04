Partial Public Class MainForm

    Friend Sub ShowPage(page As UserControl)
        pnlContent.Controls.Clear()
        page.Dock = DockStyle.Fill
        pnlContent.Controls.Add(page)
    End Sub

    Private _currentRole As String
    Private Sub SetupNavForRole(role As String)

        _currentRole = role
        flowNav.Controls.Clear()

        ' Bell icon is Admin-only per the mockup
        btnNotif.Visible = (role = "Admin")
        pnlNotifications.Visible = False

        ' Profile Settings is Teacher/Student-only per the mockup (Admin doesn't have it)
        lblProfileSettingsLink.Visible = (role = "Teacher" OrElse role = "Student")

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
        b.Font = New Font("Century Gothic", 10, FontStyle.Bold)
        b.Cursor = Cursors.Hand
        b.TabStop = False

        Dim boldSize As Size = TextRenderer.MeasureText(text, b.Font)
        b.AutoSize = False
        b.Size = New Size(boldSize.Width + 20, boldSize.Height + 10)
        b.TextAlign = ContentAlignment.MiddleCenter

        AddHandler b.Click, Sub(s, e)
                                SetActiveNavControl(b)
                                onClick()
                            End Sub

        flowNav.Controls.Add(b)

        If isDefault Then
            b.Font = New Font(b.Font, FontStyle.Bold)
        Else
            b.Font = New Font(b.Font, FontStyle.Regular)
        End If

    End Sub

    Private Sub LoadNotifications()

        flowNotifList.Controls.Clear()

        AddNotification("HH:MM AM  -  DD-MM-YYYY", "Official Midterm Grades for BSCS 1A - GE 001 are now available.", False)
        AddNotification("HH:MM AM  -  DD-MM-YYYY", "Subject Add / Drop / Transfer Application", False)
        AddNotification("HH:MM AM  -  DD-MM-YYYY", "Class Update Notice", False)
        AddNotification("HH:MM AM  -  DD-MM-YYYY", "Official Midterm Grades for BSCS 1A - GE 001 are now available.", False)
        AddNotification("HH:MM AM  -  DD-MM-YYYY", "Subject Add / Drop / Transfer Application", False)
        AddNotification("HH:MM AM  -  DD-MM-YYYY", "Class Update Notice", True)   ' Completed

    End Sub

    Private Sub AddNotification(timestamp As String, message As String, isCompleted As Boolean)
        Dim item As New ucNotificationItem()
        item.Width = flowNotifList.Width - 25
        item.SetData(timestamp, message, isCompleted)
        AddHandler item.ActionClicked, Sub() MessageBox.Show("Review clicked: " & message)
        flowNotifList.Controls.Add(item)
    End Sub

    Private Sub lblProfileSettingsLink_Click(sender As Object, e As EventArgs) Handles lblProfileSettingsLink.Click

        SetActiveNavControl(lblProfileSettingsLink)

        Dim page As New ucProfileSettings()
        page.SetData(
        "Last Name, Full Name M.I.",
        _currentRole,
        If(_currentRole = "Teacher", "Professor", "Student"),
        "Department", "Email", "Phone Number"
    )
        ShowPage(page)

    End Sub
    Private Sub SetActiveNavControl(active As Control)

        For Each ctrl As Control In flowNav.Controls
            ctrl.Font = New Font(ctrl.Font, FontStyle.Regular)
        Next

        lblProfileSettingsLink.Font = New Font(lblProfileSettingsLink.Font, FontStyle.Regular)

        active.Font = New Font(active.Font, FontStyle.Bold)

    End Sub

End Class