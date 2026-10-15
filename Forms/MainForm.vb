Public Class MainForm

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.WindowState = FormWindowState.Maximized
        Me.FormBorderStyle = FormBorderStyle.None
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        If SplitContainer1 IsNot Nothing Then
            SplitContainer1.Dock = DockStyle.Fill
            SplitContainer1.IsSplitterFixed = True
        End If

        If pnlExitConfirmation IsNot Nothing Then
            pnlExitConfirmation.Visible = False
        End If

        If txtPassword IsNot Nothing Then
            txtPassword.UseSystemPasswordChar = True
        End If

        If btnMinLog IsNot Nothing Then StyleWindowButton(btnMinLog, Color.FromArgb(230, 230, 230), Color.FromArgb(210, 210, 210))
        If btnExitLog IsNot Nothing Then StyleWindowButton(btnExitLog, Color.FromArgb(232, 17, 35), Color.FromArgb(241, 112, 122))
        If btnMinApp IsNot Nothing Then StyleWindowButton(btnMinApp, Color.FromArgb(230, 230, 230), Color.FromArgb(210, 210, 210))
        If btnExitApp IsNot Nothing Then StyleWindowButton(btnExitApp, Color.FromArgb(232, 17, 35), Color.FromArgb(241, 112, 122))

        UpdateLayout()

    End Sub

    '========================================
    ' APPLICATION EXIT & MINIMIZE HANDLERS
    '========================================
    Private Sub btnExitApp_Click(sender As Object, e As EventArgs) Handles btnExitApp.Click, btnExitLog.Click
        Application.Exit()
    End Sub

    Private Sub btnMinApp_Click(sender As Object, e As EventArgs) Handles btnMinApp.Click, btnMinLog.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub MainForm_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        UpdateLayout()
    End Sub

    Private Sub StyleWindowButton(b As Button, hoverColor As Color, downColor As Color)
        If b Is Nothing Then Return

        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.FlatAppearance.MouseOverBackColor = hoverColor
        b.FlatAppearance.MouseDownBackColor = downColor
    End Sub

End Class