Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class MainForm

    Private _currentRole As String

    '========================================
    ' PAGE NAVIGATION HELPERS
    '========================================

    Public Sub ShowPage(page As Control)
        If page Is Nothing Then Return

        If pnlLoginCard IsNot Nothing Then
            pnlLoginCard.Visible = False
        End If

        If pnlApp IsNot Nothing Then
            pnlApp.Visible = True
            pnlApp.Dock = DockStyle.Fill
            pnlApp.BringToFront()
        End If

        If pnlContent IsNot Nothing Then
            Dim oldPages As New List(Of Control)
            For Each c As Control In pnlContent.Controls
                oldPages.Add(c)
            Next

            pnlContent.SuspendLayout()
            pnlContent.Controls.Clear()
            page.Dock = DockStyle.Fill
            UiTheme.HookButtonReset(page)
            pnlContent.Controls.Add(page)
            page.BringToFront()
            pnlContent.ResumeLayout(True)

            For Each c As Control In oldPages
                c.Dispose()
            Next
        End If
    End Sub

    '========================================
    ' PUBLIC DASHBOARD ROUTING
    '========================================
    Public Sub ShowAdminDashboard()
        SetupNavForRole("Admin")
        ShowPage(New ucUserManagement())
    End Sub

    Public Sub ShowTeacherDashboard()
        SetupNavForRole("Teacher")
        ShowPage(New ucTeacherDashboard())   ' dashboard first
    End Sub

    Public Sub ShowStudentDashboard()
        SetupNavForRole("Student")
        ShowPage(New ucStudentDashboard())
    End Sub

    '========================================
    ' LOGOUT / BACK TO LOGIN SCREEN
    '========================================

    Public Sub ShowLoginScreen()
        _currentRole = Nothing

        If flowNav IsNot Nothing Then DisposeAndClear(flowNav)
        If pnlContent IsNot Nothing Then DisposeAndClear(pnlContent)
        If flowNotifList IsNot Nothing Then DisposeAndClear(flowNotifList)

        StopNotificationPolling()
        If pnlNotifications IsNot Nothing Then pnlNotifications.Visible = False
        If btnBell IsNot Nothing Then btnBell.Visible = False
        If btnLogoutAdmin IsNot Nothing Then btnLogoutAdmin.Visible = False
        If lblProfileSettingsLink IsNot Nothing Then lblProfileSettingsLink.Visible = False

        If pnlApp IsNot Nothing Then pnlApp.Visible = False

        If pnlLoginCard IsNot Nothing Then
            pnlLoginCard.Visible = True
            pnlLoginCard.BringToFront()
        End If

        If txtPassword IsNot Nothing Then txtPassword.Clear()
        If txtUsername IsNot Nothing Then
            txtUsername.Clear()
            txtUsername.Focus()
        End If
    End Sub

    Private Sub DisposeAndClear(container As Control)
        Dim items As New List(Of Control)
        For Each c As Control In container.Controls
            items.Add(c)
        Next
        container.Controls.Clear()
        For Each c As Control In items
            c.Dispose()
        Next
    End Sub

    '========================================
    ' ROLE NAVIGATION SETUP
    '========================================
    Private Sub SetupNavForRole(role As String)

        _currentRole = role

        If flowNav IsNot Nothing Then DisposeAndClear(flowNav)

        Dim isAdmin As Boolean = String.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
        Dim isTeacher As Boolean = String.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase) _
                                   OrElse String.Equals(role, "Faculty", StringComparison.OrdinalIgnoreCase)
        Dim isStudent As Boolean = String.Equals(role, "Student", StringComparison.OrdinalIgnoreCase)

        If btnBell IsNot Nothing Then btnBell.Visible = True
        If btnLogoutAdmin IsNot Nothing Then btnLogoutAdmin.Visible = (isAdmin OrElse isTeacher OrElse isStudent)
        If pnlNotifications IsNot Nothing Then pnlNotifications.Visible = False
        If lblProfileSettingsLink IsNot Nothing Then
            lblProfileSettingsLink.Visible = (isTeacher OrElse isStudent)
            lblProfileSettingsLink.Text = "PROFILE"
            SetFontStyle(lblProfileSettingsLink, FontStyle.Regular)
        End If

        Select Case True
            Case isAdmin
                AddNavButton("USER MANAGEMENT", Sub() ShowPage(New ucUserManagement()), isDefault:=True)
                AddNavButton("ACADEMIC MANAGEMENT", Sub() ShowPage(New ucAcademicManagement()))
                AddNavButton("GRADING CONFIGURATION", Sub() ShowPage(New ucGradingConfig()))
            Case isTeacher
                AddNavButton("MY COURSES", Sub() ShowPage(New ucTeacherDashboard()), isDefault:=True)

            Case isStudent
                AddNavButton("MAIN DASHBOARD", Sub() ShowPage(New ucStudentDashboard()), isDefault:=True)
                AddNavButton("ALL TRANSCRIPTS", Sub() ShowPage(New ucTranscript()))
                AddNavButton("ANALYTICS", Sub() ShowPage(New ucAnalytics()))
        End Select

        StartNotificationPolling()   ' red badge on the bell, refreshed every 30 seconds

    End Sub

    Private Sub AddNavButton(text As String, onClick As Action, Optional isDefault As Boolean = False)

        If flowNav Is Nothing Then Return

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
        b.Cursor = Cursors.Hand
        b.TabStop = False

        Using boldFont As New Font("Century Gothic", 10, FontStyle.Bold)
            Dim boldSize As Size = TextRenderer.MeasureText(text, boldFont)
            b.AutoSize = False
            b.Size = New Size(boldSize.Width + 20, boldSize.Height + 10)
        End Using
        b.TextAlign = ContentAlignment.MiddleCenter

        b.Font = New Font("Century Gothic", 10, If(isDefault, FontStyle.Bold, FontStyle.Regular))

        AddHandler b.Click, Sub(s, e)
                                SetActiveNavControl(b)
                                onClick()
                            End Sub

        flowNav.Controls.Add(b)

    End Sub

    '========================================
    ' NOTIFICATIONS MANAGEMENT
    '========================================

    '========================================
    ' PROFILE LINK & ACTIVE TAB STYLING
    '========================================
    Private Sub lblProfileSettingsLink_Click(sender As Object, e As EventArgs) Handles lblProfileSettingsLink.Click
        SetActiveNavControl(lblProfileSettingsLink)

        Dim p = ProfileService.Load(SessionManager.CurrentUserId)
        Dim page As New ucProfileSettings()
        page.SetData(SessionManager.CurrentFullName, _currentRole, p.RoleLabel, p.Department, p.Email, p.Phone)
        ShowPage(page)
    End Sub

    Private Sub SetActiveNavControl(active As Control)

        If flowNav IsNot Nothing Then
            For Each ctrl As Control In flowNav.Controls
                SetFontStyle(ctrl, FontStyle.Regular)
            Next
        End If

        If lblProfileSettingsLink IsNot Nothing Then
            SetFontStyle(lblProfileSettingsLink, FontStyle.Regular)
        End If

        If active IsNot Nothing Then
            SetFontStyle(active, FontStyle.Bold)
        End If

    End Sub

    Private Sub SetFontStyle(ctrl As Control, style As FontStyle)
        If ctrl Is Nothing OrElse ctrl.Font.Style = style Then Return
        Dim old As Font = ctrl.Font
        ctrl.Font = New Font(old, style)
        If old IsNot Control.DefaultFont AndAlso ctrl.Font IsNot old Then
            Try
                old.Dispose()
            Catch
            End Try
        End If
    End Sub

End Class
