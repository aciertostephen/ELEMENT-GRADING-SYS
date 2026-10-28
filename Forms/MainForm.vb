Imports System.Drawing
Imports System.Windows.Forms

Public Class MainForm
    Inherits Form
    Implements IMessageFilter

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler Application.ThreadException, Sub(s, ex) Msg.Fail("complete the action", ex.Exception)
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

    '========================================
    ' ADMIN LOGOUT HANDLER
    '========================================
    Private Sub btnLogoutAdmin_Click(sender As Object, e As EventArgs) Handles btnLogoutAdmin.Click
        ' 1. Show confirmation dialog
        Dim result As DialogResult = MessageBox.Show(
        "Are you sure you want to log out?",
        "Confirm Logout",
        MessageBoxButtons.YesNo)

        ' 2. If the user clicks 'No', cancel the logout process
        If result <> DialogResult.Yes Then Return

        ' 3. Proceed with existing logout logic if confirmed
        SessionManager.EndSession()

        If pnlApp IsNot Nothing Then
            pnlApp.Visible = False
        End If

        If pnlLoginCard IsNot Nothing Then
            pnlLoginCard.Visible = True
            pnlLoginCard.BringToFront()
        End If

        If txtUsername IsNot Nothing Then txtUsername.Clear()
        If txtPassword IsNot Nothing Then txtPassword.Clear()
        If txtUsername IsNot Nothing Then txtUsername.Focus()
    End Sub

    Private Sub StyleWindowButton(b As Button, hoverColor As Color, downColor As Color)
        If b Is Nothing Then Return

        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.FlatAppearance.MouseOverBackColor = hoverColor
        b.FlatAppearance.MouseDownBackColor = downColor
    End Sub

    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const WM_NCLBUTTONDOWN As Integer = &H2A1

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        ' Register the message filter when MainForm loads
        Application.AddMessageFilter(Me)
    End Sub

    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        ' Intercept left mouse button clicks
        If m.Msg = WM_LBUTTONDOWN OrElse m.Msg = WM_NCLBUTTONDOWN Then
            ' Only process if the notification panel is currently visible
            If pnlNotifications IsNot Nothing AndAlso pnlNotifications.Visible Then
                Dim clickPt As Point = Cursor.Position

                ' Get screen bounds of the notification panel
                Dim panelRect As Rectangle = pnlNotifications.RectangleToScreen(pnlNotifications.ClientRectangle)

                ' Get screen bounds of the notification bell button (so clicking it doesn't instantly re-open/close glitch)
                Dim bellRect As Rectangle = Rectangle.Empty
                If btnNotif IsNot Nothing Then
                    bellRect = btnNotif.RectangleToScreen(btnNotif.ClientRectangle)
                End If

                ' If the click occurred outside both the panel and the bell button, hide the panel
                If Not panelRect.Contains(clickPt) AndAlso Not bellRect.Contains(clickPt) Then
                    pnlNotifications.Visible = False
                End If
            End If
        End If

        Return False ' Allow the click event to continue normally to target controls
    End Function

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        ' Clean up the message filter when closing
        Application.RemoveMessageFilter(Me)
        MyBase.OnFormClosed(e)
    End Sub
End Class