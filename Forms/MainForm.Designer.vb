<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        pnlLogin = New Panel()
        SplitContainer1 = New SplitContainer()
        LoginDesign = New Panel()
        picLogo = New PictureBox()
        picCCS = New PictureBox()
        lblGrade = New Label()
        lblSchool = New Label()
        lblElement = New Label()
        lblEncoder = New Label()
        pnlLoginCard = New Panel()
        btnMinLog = New Button()
        btnExitLog = New Button()
        pnlUser = New RoundedPanel()
        txtUsername = New TextBox()
        pnlPass = New RoundedPanel()
        txtPassword = New TextBox()
        btnLogin = New ReaLTaiizor.Controls.Button()
        lblAccounttext = New Label()
        lblLogintext = New Label()
        lblUsername = New Label()
        lblPassword = New Label()
        linkForgot = New LinkLabel()
        btnEXITF = New ReaLTaiizor.Controls.Button()
        btnCANCEL = New ReaLTaiizor.Controls.Button()
        exitQuestions = New Label()
        pnlExitConfirmation = New RoundedPanel()
        pnlApp = New Panel()
        pnlHeader = New Panel()
        btnMinApp = New Button()
        btnExitApp = New Button()
        flowNav = New FlowLayoutPanel()
        lblAppTitle = New Label()
        picLogoApp = New PictureBox()
        picCCSApp = New PictureBox()
        pnlContent = New Panel()
        pnlLogin.SuspendLayout()
        CType(SplitContainer1, ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        LoginDesign.SuspendLayout()
        CType(picLogo, ComponentModel.ISupportInitialize).BeginInit()
        CType(picCCS, ComponentModel.ISupportInitialize).BeginInit()
        pnlLoginCard.SuspendLayout()
        pnlUser.SuspendLayout()
        pnlPass.SuspendLayout()
        pnlExitConfirmation.SuspendLayout()
        pnlApp.SuspendLayout()
        pnlHeader.SuspendLayout()
        CType(picLogoApp, ComponentModel.ISupportInitialize).BeginInit()
        CType(picCCSApp, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' pnlLogin
        ' 
        pnlLogin.Controls.Add(SplitContainer1)
        pnlLogin.Dock = DockStyle.Fill
        pnlLogin.Location = New Point(0, 0)
        pnlLogin.Name = "pnlLogin"
        pnlLogin.Size = New Size(1924, 1055)
        pnlLogin.TabIndex = 0
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.IsSplitterFixed = True
        SplitContainer1.Location = New Point(0, 0)
        SplitContainer1.Name = "SplitContainer1"
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(LoginDesign)
        SplitContainer1.Panel1MinSize = 0
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(pnlLoginCard)
        SplitContainer1.Panel2MinSize = 0
        SplitContainer1.Size = New Size(1924, 1055)
        SplitContainer1.SplitterDistance = 1160
        SplitContainer1.SplitterWidth = 1
        SplitContainer1.TabIndex = 0
        ' 
        ' LoginDesign
        ' 
        LoginDesign.BackColor = Color.Transparent
        LoginDesign.BackgroundImage = My.Resources.Resources.LOGIN_BG
        LoginDesign.BackgroundImageLayout = ImageLayout.Stretch
        LoginDesign.Controls.Add(picLogo)
        LoginDesign.Controls.Add(picCCS)
        LoginDesign.Controls.Add(lblGrade)
        LoginDesign.Controls.Add(lblSchool)
        LoginDesign.Controls.Add(lblElement)
        LoginDesign.Controls.Add(lblEncoder)
        LoginDesign.Dock = DockStyle.Fill
        LoginDesign.Location = New Point(0, 0)
        LoginDesign.Name = "LoginDesign"
        LoginDesign.Size = New Size(1160, 1055)
        LoginDesign.TabIndex = 0
        ' 
        ' picLogo
        ' 
        picLogo.BackgroundImage = My.Resources.Resources.PLP_LOGO
        picLogo.BackgroundImageLayout = ImageLayout.Zoom
        picLogo.Location = New Point(173, 110)
        picLogo.Name = "picLogo"
        picLogo.Size = New Size(100, 100)
        picLogo.TabIndex = 1
        picLogo.TabStop = False
        ' 
        ' picCCS
        ' 
        picCCS.BackgroundImage = My.Resources.Resources.compscilogo
        picCCS.BackgroundImageLayout = ImageLayout.Zoom
        picCCS.Location = New Point(946, 110)
        picCCS.Name = "picCCS"
        picCCS.Size = New Size(100, 100)
        picCCS.TabIndex = 0
        picCCS.TabStop = False
        ' 
        ' lblGrade
        ' 
        lblGrade.AutoSize = True
        lblGrade.Font = New Font("Century Gothic", 60.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblGrade.ForeColor = Color.White
        lblGrade.Location = New Point(394, 526)
        lblGrade.Name = "lblGrade"
        lblGrade.Size = New Size(388, 117)
        lblGrade.TabIndex = 5
        lblGrade.Text = "GRADE"
        lblGrade.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblSchool
        ' 
        lblSchool.AutoSize = True
        lblSchool.Font = New Font("Century Gothic", 12.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblSchool.ForeColor = Color.FromArgb(CByte(247), CByte(242), CByte(101))
        lblSchool.Location = New Point(406, 150)
        lblSchool.Name = "lblSchool"
        lblSchool.Size = New Size(389, 23)
        lblSchool.TabIndex = 4
        lblSchool.Text = "PAMANTASAN NG LUNGSOD NG PASIG"
        ' 
        ' lblElement
        ' 
        lblElement.AutoSize = True
        lblElement.Font = New Font("Century Gothic", 10.8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblElement.ForeColor = Color.White
        lblElement.Location = New Point(573, 1010)
        lblElement.Name = "lblElement"
        lblElement.Size = New Size(82, 22)
        lblElement.TabIndex = 3
        lblElement.Text = "ELEMENT"
        lblElement.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblEncoder
        ' 
        lblEncoder.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lblEncoder.Font = New Font("Century Gothic", 60.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblEncoder.ForeColor = Color.White
        lblEncoder.Location = New Point(329, 643)
        lblEncoder.Name = "lblEncoder"
        lblEncoder.Size = New Size(522, 102)
        lblEncoder.TabIndex = 2
        lblEncoder.Text = "ENCODER"
        ' 
        ' pnlLoginCard
        ' 
        pnlLoginCard.BackColor = Color.White
        pnlLoginCard.Controls.Add(btnMinLog)
        pnlLoginCard.Controls.Add(btnExitLog)
        pnlLoginCard.Controls.Add(pnlUser)
        pnlLoginCard.Controls.Add(pnlPass)
        pnlLoginCard.Controls.Add(btnLogin)
        pnlLoginCard.Controls.Add(lblAccounttext)
        pnlLoginCard.Controls.Add(lblLogintext)
        pnlLoginCard.Controls.Add(lblUsername)
        pnlLoginCard.Controls.Add(lblPassword)
        pnlLoginCard.Controls.Add(linkForgot)
        pnlLoginCard.Dock = DockStyle.Fill
        pnlLoginCard.Location = New Point(0, 0)
        pnlLoginCard.Name = "pnlLoginCard"
        pnlLoginCard.Size = New Size(763, 1055)
        pnlLoginCard.TabIndex = 0
        ' 
        ' btnMinLog
        ' 
        btnMinLog.BackColor = Color.Transparent
        btnMinLog.BackgroundImageLayout = ImageLayout.None
        btnMinLog.FlatAppearance.BorderSize = 0
        btnMinLog.FlatStyle = FlatStyle.Flat
        btnMinLog.Font = New Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnMinLog.Location = New Point(673, 0)
        btnMinLog.Name = "btnMinLog"
        btnMinLog.Size = New Size(46, 32)
        btnMinLog.TabIndex = 5
        btnMinLog.Text = "─"
        btnMinLog.UseVisualStyleBackColor = False
        ' 
        ' btnExitLog
        ' 
        btnExitLog.BackColor = Color.Transparent
        btnExitLog.BackgroundImageLayout = ImageLayout.None
        btnExitLog.FlatAppearance.BorderSize = 0
        btnExitLog.FlatStyle = FlatStyle.Flat
        btnExitLog.Font = New Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnExitLog.Location = New Point(719, 0)
        btnExitLog.Name = "btnExitLog"
        btnExitLog.Size = New Size(46, 32)
        btnExitLog.TabIndex = 6
        btnExitLog.Text = "✕"
        btnExitLog.UseVisualStyleBackColor = False
        ' 
        ' pnlUser
        ' 
        pnlUser.ActiveBorderColor = Color.CornflowerBlue
        pnlUser.BackColor = Color.FromArgb(CByte(224), CByte(224), CByte(224))
        pnlUser.BorderColor = Color.Black
        pnlUser.BorderThickness = 1
        pnlUser.Controls.Add(txtUsername)
        pnlUser.Location = New Point(146, 393)
        pnlUser.Name = "pnlUser"
        pnlUser.Radius = 8
        pnlUser.Size = New Size(492, 51)
        pnlUser.TabIndex = 0
        ' 
        ' txtUsername
        ' 
        txtUsername.BackColor = Color.FromArgb(CByte(224), CByte(224), CByte(224))
        txtUsername.BorderStyle = BorderStyle.None
        txtUsername.Font = New Font("Century Gothic", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtUsername.Location = New Point(13, 14)
        txtUsername.Name = "txtUsername"
        txtUsername.Size = New Size(465, 25)
        txtUsername.TabIndex = 1
        ' 
        ' pnlPass
        ' 
        pnlPass.ActiveBorderColor = Color.CornflowerBlue
        pnlPass.BackColor = Color.FromArgb(CByte(224), CByte(224), CByte(224))
        pnlPass.BorderColor = Color.Black
        pnlPass.BorderThickness = 1
        pnlPass.Controls.Add(txtPassword)
        pnlPass.Location = New Point(146, 490)
        pnlPass.Name = "pnlPass"
        pnlPass.Radius = 8
        pnlPass.Size = New Size(492, 51)
        pnlPass.TabIndex = 1
        ' 
        ' txtPassword
        ' 
        txtPassword.BackColor = Color.FromArgb(CByte(224), CByte(224), CByte(224))
        txtPassword.BorderStyle = BorderStyle.None
        txtPassword.Font = New Font("Century Gothic", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtPassword.Location = New Point(14, 15)
        txtPassword.Name = "txtPassword"
        txtPassword.Size = New Size(465, 25)
        txtPassword.TabIndex = 1
        ' 
        ' btnLogin
        ' 
        btnLogin.BackColor = Color.Transparent
        btnLogin.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnLogin.EnteredBorderColor = Color.Empty
        btnLogin.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnLogin.Font = New Font("Century Gothic", 13.8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnLogin.Image = Nothing
        btnLogin.ImageAlign = ContentAlignment.MiddleLeft
        btnLogin.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnLogin.Location = New Point(146, 634)
        btnLogin.Name = "btnLogin"
        btnLogin.PressedBorderColor = Color.Transparent
        btnLogin.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnLogin.Size = New Size(492, 65)
        btnLogin.TabIndex = 2
        btnLogin.Text = "Log in"
        btnLogin.TextAlignment = StringAlignment.Center
        btnLogin.UseWaitCursor = True
        ' 
        ' lblAccounttext
        ' 
        lblAccounttext.BackColor = Color.Transparent
        lblAccounttext.Font = New Font("Century Gothic", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblAccounttext.Location = New Point(319, 210)
        lblAccounttext.Name = "lblAccounttext"
        lblAccounttext.Size = New Size(188, 43)
        lblAccounttext.TabIndex = 0
        lblAccounttext.Text = "ACCOUNT"
        lblAccounttext.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblLogintext
        ' 
        lblLogintext.BackColor = Color.Transparent
        lblLogintext.Font = New Font("Century Gothic", 19.8000011F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblLogintext.Location = New Point(244, 165)
        lblLogintext.Name = "lblLogintext"
        lblLogintext.Size = New Size(290, 45)
        lblLogintext.TabIndex = 0
        lblLogintext.Text = "LOG IN TO YOUR"
        ' 
        ' lblUsername
        ' 
        lblUsername.BackColor = Color.Transparent
        lblUsername.Font = New Font("Century Gothic", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblUsername.Location = New Point(146, 360)
        lblUsername.Name = "lblUsername"
        lblUsername.Size = New Size(172, 28)
        lblUsername.TabIndex = 0
        lblUsername.Text = "Username/Email"
        ' 
        ' lblPassword
        ' 
        lblPassword.BackColor = Color.Transparent
        lblPassword.Font = New Font("Century Gothic", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblPassword.Location = New Point(146, 459)
        lblPassword.Name = "lblPassword"
        lblPassword.Size = New Size(108, 28)
        lblPassword.TabIndex = 0
        lblPassword.Text = "Password"
        lblPassword.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' linkForgot
        ' 
        linkForgot.BackColor = Color.Transparent
        linkForgot.Font = New Font("Century Gothic", 9.0F, FontStyle.Bold)
        linkForgot.LinkBehavior = LinkBehavior.NeverUnderline
        linkForgot.LinkColor = Color.FromArgb(CByte(126), CByte(126), CByte(126))
        linkForgot.Location = New Point(499, 702)
        linkForgot.Name = "linkForgot"
        linkForgot.Size = New Size(139, 20)
        linkForgot.TabIndex = 3
        linkForgot.TabStop = True
        linkForgot.Text = "Forgot Password?"
        ' 
        ' btnEXITF
        ' 
        btnEXITF.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        btnEXITF.BackColor = Color.Transparent
        btnEXITF.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnEXITF.EnteredBorderColor = Color.Transparent
        btnEXITF.EnteredColor = Color.FromArgb(CByte(220), CByte(70), CByte(70))
        btnEXITF.Font = New Font("Century Gothic", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnEXITF.Image = Nothing
        btnEXITF.ImageAlign = ContentAlignment.MiddleLeft
        btnEXITF.InactiveColor = Color.FromArgb(CByte(200), CByte(51), CByte(51))
        btnEXITF.Location = New Point(526, 128)
        btnEXITF.Name = "btnEXITF"
        btnEXITF.PressedBorderColor = Color.Transparent
        btnEXITF.PressedColor = Color.FromArgb(CByte(165), CByte(38), CByte(38))
        btnEXITF.Size = New Size(100, 42)
        btnEXITF.TabIndex = 3
        btnEXITF.Text = "EXIT"
        btnEXITF.TextAlignment = StringAlignment.Center
        btnEXITF.UseWaitCursor = True
        ' 
        ' btnCANCEL
        ' 
        btnCANCEL.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        btnCANCEL.BackColor = Color.Transparent
        btnCANCEL.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnCANCEL.EnteredBorderColor = Color.Transparent
        btnCANCEL.EnteredColor = Color.FromArgb(CByte(125), CByte(132), CByte(145))
        btnCANCEL.Font = New Font("Century Gothic", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnCANCEL.Image = Nothing
        btnCANCEL.ImageAlign = ContentAlignment.MiddleLeft
        btnCANCEL.InactiveColor = Color.FromArgb(CByte(107), CByte(114), CByte(128))
        btnCANCEL.Location = New Point(418, 128)
        btnCANCEL.Name = "btnCANCEL"
        btnCANCEL.PressedBorderColor = Color.Transparent
        btnCANCEL.PressedColor = Color.FromArgb(CByte(86), CByte(92), CByte(103))
        btnCANCEL.Size = New Size(100, 42)
        btnCANCEL.TabIndex = 4
        btnCANCEL.Text = "CANCEL"
        btnCANCEL.TextAlignment = StringAlignment.Center
        btnCANCEL.UseWaitCursor = True
        ' 
        ' exitQuestions
        ' 
        exitQuestions.BackColor = Color.Transparent
        exitQuestions.Font = New Font("Century Gothic", 24.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        exitQuestions.Location = New Point(34, 35)
        exitQuestions.Name = "exitQuestions"
        exitQuestions.Size = New Size(424, 49)
        exitQuestions.TabIndex = 0
        exitQuestions.Text = "Do you want to exit?"
        ' 
        ' pnlExitConfirmation
        ' 
        pnlExitConfirmation.ActiveBorderColor = Color.Black
        pnlExitConfirmation.BackColor = Color.White
        pnlExitConfirmation.BorderColor = Color.Black
        pnlExitConfirmation.BorderThickness = 1
        pnlExitConfirmation.Controls.Add(exitQuestions)
        pnlExitConfirmation.Controls.Add(btnCANCEL)
        pnlExitConfirmation.Controls.Add(btnEXITF)
        pnlExitConfirmation.Location = New Point(637, 427)
        pnlExitConfirmation.Name = "pnlExitConfirmation"
        pnlExitConfirmation.Radius = 8
        pnlExitConfirmation.Size = New Size(650, 200)
        pnlExitConfirmation.TabIndex = 5
        pnlExitConfirmation.Visible = False
        ' 
        ' pnlApp
        ' 
        pnlApp.Controls.Add(pnlHeader)
        pnlApp.Controls.Add(pnlContent)
        pnlApp.Dock = DockStyle.Fill
        pnlApp.Location = New Point(0, 0)
        pnlApp.Name = "pnlApp"
        pnlApp.Size = New Size(1924, 1055)
        pnlApp.TabIndex = 6
        ' 
        ' pnlHeader
        ' 
        pnlHeader.BackColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        pnlHeader.Controls.Add(btnMinApp)
        pnlHeader.Controls.Add(btnExitApp)
        pnlHeader.Controls.Add(flowNav)
        pnlHeader.Controls.Add(lblAppTitle)
        pnlHeader.Controls.Add(picLogoApp)
        pnlHeader.Controls.Add(picCCSApp)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Size = New Size(1924, 138)
        pnlHeader.TabIndex = 0
        ' 
        ' btnMinApp
        ' 
        btnMinApp.BackColor = Color.Transparent
        btnMinApp.BackgroundImageLayout = ImageLayout.None
        btnMinApp.FlatAppearance.BorderSize = 0
        btnMinApp.FlatStyle = FlatStyle.Flat
        btnMinApp.Font = New Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnMinApp.Location = New Point(1827, 0)
        btnMinApp.Name = "btnMinApp"
        btnMinApp.Size = New Size(46, 32)
        btnMinApp.TabIndex = 6
        btnMinApp.Text = "─"
        btnMinApp.UseVisualStyleBackColor = False
        ' 
        ' btnExitApp
        ' 
        btnExitApp.BackColor = Color.Transparent
        btnExitApp.BackgroundImageLayout = ImageLayout.None
        btnExitApp.FlatAppearance.BorderSize = 0
        btnExitApp.FlatStyle = FlatStyle.Flat
        btnExitApp.Font = New Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnExitApp.Location = New Point(1878, 0)
        btnExitApp.Name = "btnExitApp"
        btnExitApp.Size = New Size(46, 32)
        btnExitApp.TabIndex = 7
        btnExitApp.Text = "✕"
        btnExitApp.UseVisualStyleBackColor = False
        ' 
        ' flowNav
        ' 
        flowNav.AutoSize = True
        flowNav.AutoSizeMode = AutoSizeMode.GrowAndShrink
        flowNav.BackColor = Color.Transparent
        flowNav.Location = New Point(671, 110)
        flowNav.Name = "flowNav"
        flowNav.Size = New Size(0, 0)
        flowNav.TabIndex = 3
        flowNav.WrapContents = False
        ' 
        ' lblAppTitle
        ' 
        lblAppTitle.AutoSize = True
        lblAppTitle.BackColor = Color.Transparent
        lblAppTitle.Font = New Font("Century Gothic", 28.2F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblAppTitle.ForeColor = Color.FromArgb(CByte(255), CByte(242), CByte(127))
        lblAppTitle.Location = New Point(115, 47)
        lblAppTitle.Name = "lblAppTitle"
        lblAppTitle.Size = New Size(450, 56)
        lblAppTitle.TabIndex = 2
        lblAppTitle.Text = "GRADE ENCODING"
        ' 
        ' picLogoApp
        ' 
        picLogoApp.BackColor = Color.Transparent
        picLogoApp.BackgroundImage = My.Resources.Resources.PLP_LOGO
        picLogoApp.BackgroundImageLayout = ImageLayout.Zoom
        picLogoApp.Location = New Point(34, 44)
        picLogoApp.Name = "picLogoApp"
        picLogoApp.Size = New Size(65, 65)
        picLogoApp.TabIndex = 0
        picLogoApp.TabStop = False
        ' 
        ' picCCSApp
        ' 
        picCCSApp.BackColor = Color.Transparent
        picCCSApp.BackgroundImage = My.Resources.Resources.compscilogo
        picCCSApp.BackgroundImageLayout = ImageLayout.Zoom
        picCCSApp.Location = New Point(575, 44)
        picCCSApp.Name = "picCCSApp"
        picCCSApp.Size = New Size(65, 65)
        picCCSApp.TabIndex = 1
        picCCSApp.TabStop = False
        ' 
        ' pnlContent
        ' 
        pnlContent.Dock = DockStyle.Fill
        pnlContent.Location = New Point(0, 0)
        pnlContent.Name = "pnlContent"
        pnlContent.Size = New Size(1924, 1055)
        pnlContent.TabIndex = 1
        ' 
        ' MainForm
        ' 
        AutoScaleDimensions = New SizeF(8.0F, 20.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = SystemColors.Control
        ClientSize = New Size(1924, 1055)
        Controls.Add(pnlLogin)
        Controls.Add(pnlApp)
        Controls.Add(pnlExitConfirmation)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "MainForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "ELEMENT - GRADING SYSTEM"
        WindowState = FormWindowState.Maximized
        pnlLogin.ResumeLayout(False)
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        LoginDesign.ResumeLayout(False)
        LoginDesign.PerformLayout()
        CType(picLogo, ComponentModel.ISupportInitialize).EndInit()
        CType(picCCS, ComponentModel.ISupportInitialize).EndInit()
        pnlLoginCard.ResumeLayout(False)
        pnlUser.ResumeLayout(False)
        pnlUser.PerformLayout()
        pnlPass.ResumeLayout(False)
        pnlPass.PerformLayout()
        pnlExitConfirmation.ResumeLayout(False)
        pnlApp.ResumeLayout(False)
        pnlHeader.ResumeLayout(False)
        pnlHeader.PerformLayout()
        CType(picLogoApp, ComponentModel.ISupportInitialize).EndInit()
        CType(picCCSApp, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlLogin As Panel
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents LoginDesign As Panel
    Friend WithEvents picLogo As PictureBox
    Friend WithEvents picCCS As PictureBox
    Friend WithEvents lblGrade As Label
    Friend WithEvents lblSchool As Label
    Friend WithEvents lblElement As Label
    Friend WithEvents lblEncoder As Label
    Friend WithEvents pnlLoginCard As Panel
    Friend WithEvents lblPassword As Label
    Friend WithEvents lblAccounttext As Label
    Friend WithEvents lblLogintext As Label
    Friend WithEvents lblUsername As Label
    Friend WithEvents linkForgot As LinkLabel
    Friend WithEvents btnLogin As ReaLTaiizor.Controls.Button
    Friend WithEvents TextBoxEdit1 As ReaLTaiizor.Controls.TextBoxEdit
    Friend WithEvents txtUsername As TextBox
    Friend WithEvents pnlUser As RoundedPanel
    Friend WithEvents pnlPass As RoundedPanel
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents btnMinLog As Button
    Friend WithEvents btnExitLog As Button
    Friend WithEvents btnEXITF As ReaLTaiizor.Controls.Button
    Friend WithEvents exitQuestions As Label
    Friend WithEvents btnCANCEL As ReaLTaiizor.Controls.Button
    Friend WithEvents pnlExitConfirmation As RoundedPanel
    Friend WithEvents pnlApp As Panel
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents pnlContent As Panel
    Friend WithEvents picLogoApp As PictureBox
    Friend WithEvents picCCSApp As PictureBox
    Friend WithEvents lblAppTitle As Label
    Friend WithEvents flowNav As FlowLayoutPanel
    Friend WithEvents btnMinApp As Button
    Friend WithEvents btnExitApp As Button

End Class
