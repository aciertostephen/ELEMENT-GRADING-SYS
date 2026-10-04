Public Class MainForm

    '========================================
    ' LOAD / RESIZE
    '========================================

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.WindowState = FormWindowState.Maximized
        Me.FormBorderStyle = FormBorderStyle.None
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.IsSplitterFixed = True

        ' Exit confirmation must start hidden
        pnlExitConfirmation.Visible = False

        ' Password box (was handled by Guna before)
        txtPassword.UseSystemPasswordChar = True

        ' Window buttons: flat, no border, real hover colors
        StyleWindowButton(btnMinLog, Color.FromArgb(230, 230, 230), Color.FromArgb(210, 210, 210))
        StyleWindowButton(btnExitLog, Color.FromArgb(232, 17, 35), Color.FromArgb(241, 112, 122))

        StyleWindowButton(btnMinApp, Color.FromArgb(30, 110, 80), Color.FromArgb(20, 90, 65))
        StyleWindowButton(btnExitApp, Color.FromArgb(232, 17, 35), Color.FromArgb(241, 112, 122))

        btnNotif.Text = ChrW(&HEA8F)
        btnNotif.Font = New Font("Segoe MDL2 Assets", 18)
        btnNotif.TextAlign = ContentAlignment.MiddleCenter
        UpdateLayout()

    End Sub

    Private Sub MainForm_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        UpdateLayout()

    End Sub

    Private Sub StyleWindowButton(b As Button, hoverColor As Color, downColor As Color)
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.FlatAppearance.MouseOverBackColor = hoverColor
        b.FlatAppearance.MouseDownBackColor = downColor
        b.UseVisualStyleBackColor = False
        b.TabStop = False
    End Sub


    '========================================
    ' WINDOW BUTTONS (top-right of login card)
    '========================================

    Private Sub btnMinLog_Click(sender As Object, e As EventArgs) Handles btnMinLog.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub btnExitLog_Click(sender As Object, e As EventArgs) Handles btnExitLog.Click
        CenterExitPanel()
        pnlExitConfirmation.Visible = True
        pnlExitConfirmation.BringToFront()
    End Sub

    ' White glyph on the red hover
    Private Sub btnExitLog_MouseEnter(sender As Object, e As EventArgs) Handles btnExitLog.MouseEnter
        btnExitLog.ForeColor = Color.White
    End Sub

    Private Sub btnExitLog_MouseLeave(sender As Object, e As EventArgs) Handles btnExitLog.MouseLeave
        btnExitLog.ForeColor = Color.Black
    End Sub


    '========================================
    ' EXIT CONFIRMATION
    '========================================

    Private Sub btnEXITF_Click(sender As Object, e As EventArgs) Handles btnEXITF.Click
        Application.Exit()
    End Sub

    Private Sub btnCANCEL_Click(sender As Object, e As EventArgs) Handles btnCANCEL.Click
        pnlExitConfirmation.Visible = False
    End Sub

    Private Sub btnMinApp_Click(sender As Object, e As EventArgs) Handles btnMinApp.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub btnExitApp_Click(sender As Object, e As EventArgs) Handles btnExitApp.Click
        CenterExitPanel()
        pnlExitConfirmation.Visible = True
        pnlExitConfirmation.BringToFront()
    End Sub

    Private Sub btnExitApp_MouseEnter(sender As Object, e As EventArgs) Handles btnExitApp.MouseEnter
        btnExitApp.ForeColor = Color.White
    End Sub

    Private Sub btnExitApp_MouseLeave(sender As Object, e As EventArgs) Handles btnExitApp.MouseLeave
        btnExitApp.ForeColor = Color.Black
    End Sub

    Private Sub btnNotif_Click(sender As Object, e As EventArgs) Handles btnNotif.Click

        If pnlNotifications.Visible Then
            pnlNotifications.Visible = False
            Return
        End If

        LoadNotifications()

        pnlNotifications.Location = New Point(
        btnNotif.PointToScreen(Point.Empty).X - Me.PointToScreen(Point.Empty).X - 25 - pnlNotifications.Width + btnNotif.Width,
        pnlHeader.Bottom + 25
    )

        pnlNotifications.Visible = True
        pnlNotifications.BringToFront()

    End Sub
    Private Sub MainForm_Click(sender As Object, e As EventArgs) Handles Me.Click
        If pnlNotifications.Visible Then
            pnlNotifications.Visible = False
        End If
    End Sub

    Private Sub btnLogoutAdmin_Click(sender As Object, e As EventArgs) Handles btnLogoutAdmin.Click
        pnlApp.Visible = False
        pnlLogin.Visible = True
    End Sub
End Class