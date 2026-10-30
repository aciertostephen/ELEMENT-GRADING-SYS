Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class MainForm

    Private _currentRole As String

    Private Sub BuildHeaderControls()
        _flowNavHeader = New FlowLayoutPanel With {.Name = "codedHeaderNavigation", .AutoSize = False, .Size = New Size(0, 40), .WrapContents = False, .BackColor = Color.Transparent, .FlowDirection = FlowDirection.LeftToRight, .Margin = Padding.Empty, .Padding = Padding.Empty}
        _btnProfileHeader = CreateHeaderAction("PROFILE")
        _btnLogoutHeader = CreateHeaderAction("LOG OUT")
        _btnRequestsHeader = CreateHeaderAction("REQUESTS")
        AddHandler _btnProfileHeader.Click, AddressOf ProfileHeader_Click
        AddHandler _btnLogoutHeader.Click, AddressOf btnLogoutAdmin_Click
        AddHandler _btnRequestsHeader.Click, AddressOf btnBell_Click
        pnlHeader.Controls.AddRange({_flowNavHeader, _btnProfileHeader, _btnLogoutHeader, _btnRequestsHeader})
    End Sub

    Private Function CreateHeaderAction(caption As String) As Button
        Dim button As New Button With {.Text = caption, .AutoSize = False, .FlatStyle = FlatStyle.Flat, .UseVisualStyleBackColor = False, .BackColor = Color.Transparent, .ForeColor = Color.White, .Font = New Font("Century Gothic", 11.0F, FontStyle.Regular), .TextAlign = ContentAlignment.MiddleCenter, .Cursor = Cursors.Hand, .TabStop = False}
        button.FlatAppearance.BorderSize = 0
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 108, 89)
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 70, 56)
        Return button
    End Function

    Private Sub ProfileHeader_Click(sender As Object, e As EventArgs)
        SetActiveNavControl(_btnProfileHeader)
        Dim profile = ProfileService.Load(SessionManager.CurrentUserId)
        Dim page As New ucProfileSettings()
        page.SetData(SessionManager.CurrentFullName, _currentRole, profile.RoleLabel, profile.Department, profile.Email, profile.Phone)
        ShowPage(page)
    End Sub

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

        If _flowNavHeader IsNot Nothing Then DisposeAndClear(_flowNavHeader)
        If pnlContent IsNot Nothing Then DisposeAndClear(pnlContent)
        If flowNotifList IsNot Nothing Then DisposeAndClear(flowNotifList)

        StopNotificationPolling()
        If pnlNotifications IsNot Nothing Then pnlNotifications.Visible = False
        If _btnRequestsHeader IsNot Nothing Then _btnRequestsHeader.Visible = False
        If _btnLogoutHeader IsNot Nothing Then _btnLogoutHeader.Visible = False
        If _btnProfileHeader IsNot Nothing Then _btnProfileHeader.Visible = False

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

        If _flowNavHeader IsNot Nothing Then DisposeAndClear(_flowNavHeader)

        Dim isAdmin As Boolean = String.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
        Dim isTeacher As Boolean = String.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase) _
                                   OrElse String.Equals(role, "Faculty", StringComparison.OrdinalIgnoreCase)
        Dim isStudent As Boolean = String.Equals(role, "Student", StringComparison.OrdinalIgnoreCase)

        If _btnRequestsHeader IsNot Nothing Then _btnRequestsHeader.Visible = True
        If _btnLogoutHeader IsNot Nothing Then _btnLogoutHeader.Visible = (isAdmin OrElse isTeacher OrElse isStudent)
        If pnlNotifications IsNot Nothing Then pnlNotifications.Visible = False
        If _btnProfileHeader IsNot Nothing Then _btnProfileHeader.Visible = (isTeacher OrElse isStudent)

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

        UpdateHeaderActions()
        StartNotificationPolling()   ' red badge on the bell, refreshed every 30 seconds

    End Sub

    Private Sub AddNavButton(text As String, onClick As Action, Optional isDefault As Boolean = False)

        If _flowNavHeader Is Nothing Then Return

        Dim b As New Button()
        b.Text = text
        b.Margin = New Padding(18, 0, 0, 0)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 110, 80)
        b.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 90, 65)
        b.BackColor = Color.Transparent
        b.UseVisualStyleBackColor = False
        b.ForeColor = Color.White
        b.Cursor = Cursors.Hand
        b.TabStop = False

        Using boldFont As New Font("Century Gothic", 11, FontStyle.Bold)
            Dim boldSize As Size = TextRenderer.MeasureText(text, boldFont)
            b.AutoSize = False
            b.Size = New Size(boldSize.Width + 20, boldSize.Height + 10)
        End Using
        b.Height = 40
        b.TextAlign = ContentAlignment.MiddleCenter

        b.Font = New Font("Century Gothic", 11, If(isDefault, FontStyle.Bold, FontStyle.Regular))

        AddHandler b.Click, Sub(s, e)
                                SetActiveNavControl(b)
                                onClick()
                            End Sub

        _flowNavHeader.Controls.Add(b)

    End Sub

    '========================================
    ' NOTIFICATIONS MANAGEMENT
    '========================================

    '========================================
    ' PROFILE LINK & ACTIVE TAB STYLING
    '========================================
    Private Sub SetActiveNavControl(active As Control)

        If _flowNavHeader IsNot Nothing Then
            For Each ctrl As Control In _flowNavHeader.Controls
                SetFontStyle(ctrl, FontStyle.Regular)
            Next
        End If

        If _btnProfileHeader IsNot Nothing Then SetFontStyle(_btnProfileHeader, FontStyle.Regular)

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
