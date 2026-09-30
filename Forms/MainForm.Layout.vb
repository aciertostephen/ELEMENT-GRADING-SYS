Partial Public Class MainForm

    '========================================
    ' LAYOUT
    '========================================

    Private Sub UpdateLayout()

        If SplitContainer1.Width <= 0 OrElse SplitContainer1.Height <= 0 Then
            Return
        End If

        '60% LEFT / 40% RIGHT
        SplitContainer1.SplitterDistance = CInt(SplitContainer1.ClientSize.Width * 0.6)

        UpdateLeftPanel()
        UpdateRightPanel()

        ' App header window buttons
        If pnlHeader.Width > 0 Then
            btnExitApp.Location = New Point(pnlHeader.Width - btnExitApp.Width, 0)
            btnMinApp.Location = New Point(btnExitApp.Left - btnMinApp.Width, 0)
        End If

    End Sub

    Private Sub UpdateLeftPanel()

        Dim panelWidth As Integer = SplitContainer1.Panel1.ClientSize.Width
        Dim panelHeight As Integer = SplitContainer1.Panel1.ClientSize.Height

        Dim centerX As Integer = panelWidth \ 2

        '---------------- SCHOOL NAME ----------------
        lblSchool.AutoSize = False
        lblSchool.Size = New Size(CInt(panelWidth * 0.4), CInt(panelHeight * 0.035))
        lblSchool.Location = New Point(centerX - (lblSchool.Width \ 2), CInt(panelHeight * 0.105))
        lblSchool.TextAlign = ContentAlignment.MiddleCenter

        '---------------- LOGOS ----------------
        Dim logoSize As Integer = CInt(panelWidth * 0.075)

        picLogo.Size = New Size(logoSize, logoSize)   ' PLP
        picCCS.Size = New Size(logoSize, logoSize)    ' CCS

        Dim logoGap As Integer = CInt(panelWidth * 0.02)
        Dim logoY As Integer = lblSchool.Top + (lblSchool.Height - logoSize) \ 2 - 5

        picLogo.Location = New Point(lblSchool.Left - logoGap - logoSize, logoY)
        picCCS.Location = New Point(lblSchool.Right + logoGap, logoY)

        '---------------- GRADE ----------------
        lblGrade.AutoSize = False
        lblGrade.Size = New Size(CInt(panelWidth * 0.45), CInt(panelHeight * 0.095))
        lblGrade.Location = New Point(centerX - (lblGrade.Width \ 2) - 17, CInt(panelHeight * 0.4))
        lblGrade.TextAlign = ContentAlignment.MiddleCenter

        '---------------- ENCODER ----------------
        lblEncoder.AutoSize = False
        lblEncoder.Size = New Size(CInt(panelWidth * 0.55), CInt(panelHeight * 0.095))
        lblEncoder.Location = New Point(centerX - (lblEncoder.Width \ 2) - 17,
                                        lblGrade.Bottom - CInt(panelHeight * 0.005))
        lblEncoder.TextAlign = ContentAlignment.MiddleCenter

        '---------------- TITLE FONT SIZE ----------------
        Dim titleFontSize As Single = Math.Max(30, panelWidth * 0.052)

        lblGrade.Font = New Font(lblGrade.Font.FontFamily, titleFontSize, FontStyle.Bold)
        lblEncoder.Font = New Font(lblEncoder.Font.FontFamily, titleFontSize, FontStyle.Bold)

        '---------------- ELEMENT ----------------
        lblElement.AutoSize = False
        lblElement.Size = New Size(CInt(panelWidth * 0.12), CInt(panelHeight * 0.035))
        lblElement.Location = New Point(centerX - (lblElement.Width \ 2) - 15, CInt(panelHeight * 0.915))
        lblElement.TextAlign = ContentAlignment.MiddleCenter

    End Sub

    Private Sub UpdateRightPanel()

        ' Everything now lives inside pnlLoginCard, so lay out relative to it
        Dim panelWidth As Integer = pnlLoginCard.ClientSize.Width
        Dim panelHeight As Integer = pnlLoginCard.ClientSize.Height

        If panelWidth <= 0 OrElse panelHeight <= 0 Then
            Return
        End If

        Dim centerX As Integer = panelWidth \ 2
        Dim contentX As Integer = centerX - (492 \ 2)

        '---------------- VERTICAL SPACING ----------------
        Dim headingTop As Integer = CInt(panelHeight * 0.2)
        Dim headingGap As Integer = CInt(panelHeight * 0.01)
        Dim labelGap As Integer = CInt(panelHeight * 0.045)
        Dim fieldGap As Integer = CInt(panelHeight * 0.01)
        Dim buttonGap As Integer = CInt(panelHeight * 0.055)
        Dim forgotGap As Integer = CInt(panelHeight * 0.015)

        '---------------- WINDOW BUTTONS ----------------
        btnExitLog.Location = New Point(panelWidth - btnExitLog.Width, 0)
        btnMinLog.Location = New Point(btnExitLog.Left - btnMinLog.Width, 0)

        '---------------- "LOG IN TO YOUR" 327 x 45 ----------------
        lblLogintext.AutoSize = False
        lblLogintext.Size = New Size(327, 45)
        lblLogintext.Location = New Point(centerX - (lblLogintext.Width \ 2), headingTop)
        lblLogintext.TextAlign = ContentAlignment.MiddleCenter

        '---------------- "ACCOUNT" 188 x 43 ----------------
        lblAccounttext.AutoSize = False
        lblAccounttext.Size = New Size(188, 43)
        lblAccounttext.Location = New Point(centerX - (lblAccounttext.Width \ 2),
                                            lblLogintext.Bottom + headingGap)
        lblAccounttext.TextAlign = ContentAlignment.MiddleCenter

        '---------------- USERNAME LABEL 172 x 28 ----------------
        lblUsername.AutoSize = False
        lblUsername.Size = New Size(172, 28)
        lblUsername.Location = New Point(contentX, lblAccounttext.Bottom + labelGap)
        lblUsername.TextAlign = ContentAlignment.MiddleLeft

        '---------------- USERNAME BOX 492 x 51 ----------------
        pnlUser.Size = New Size(492, 51)
        pnlUser.Location = New Point(contentX, lblUsername.Bottom + fieldGap)
        FitTextBox(pnlUser, txtUsername)

        '---------------- PASSWORD LABEL 108 x 28 ----------------
        lblPassword.AutoSize = False
        lblPassword.Size = New Size(108, 28)
        lblPassword.Location = New Point(contentX, pnlUser.Bottom + fieldGap)
        lblPassword.TextAlign = ContentAlignment.MiddleLeft

        '---------------- PASSWORD BOX 492 x 51 ----------------
        pnlPass.Size = New Size(492, 51)
        pnlPass.Location = New Point(contentX, lblPassword.Bottom + fieldGap)
        FitTextBox(pnlPass, txtPassword)

        '---------------- LOGIN BUTTON 492 x 65 ----------------
        btnLogin.Size = New Size(492, 65)
        btnLogin.Location = New Point(contentX, pnlPass.Bottom + buttonGap)

        '---------------- FORGOT PASSWORD 139 x 20 ----------------
        linkForgot.AutoSize = False
        linkForgot.Size = New Size(139, 20)
        linkForgot.Location = New Point(btnLogin.Right - linkForgot.Width, btnLogin.Bottom + forgotGap)
        linkForgot.TextAlign = ContentAlignment.MiddleRight

    End Sub

    ' Keeps the borderless TextBox centered inside its rounded panel
    Private Sub FitTextBox(host As Control, tb As TextBox)
        tb.Left = 13
        tb.Width = host.Width - 26
        tb.Top = (host.Height - tb.Height) \ 2
    End Sub

    Private Sub CenterExitPanel()
        pnlExitConfirmation.Location = New Point(
            (Me.ClientSize.Width - pnlExitConfirmation.Width) \ 2,
            (Me.ClientSize.Height - pnlExitConfirmation.Height) \ 2)
    End Sub

End Class