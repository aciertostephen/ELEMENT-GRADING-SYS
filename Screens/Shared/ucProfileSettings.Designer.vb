<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucProfileSettings
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ucProfileSettings))
        picAvatar = New PictureBox()
        pnlInfoCard = New RoundedPanel()
        lblPhone = New Label()
        lblEmail = New Label()
        linkEditInfo = New LinkLabel()
        lblDepartment = New Label()
        lblRole = New Label()
        lblFullName = New Label()
        btnLogOut = New ReaLTaiizor.Controls.Button()
        btnBack = New ReaLTaiizor.Controls.Button()
        linkProfilePicture = New ReaLTaiizor.Controls.LinkLabelEdit()
        CType(picAvatar, ComponentModel.ISupportInitialize).BeginInit()
        pnlInfoCard.SuspendLayout()
        SuspendLayout()
        ' 
        ' picAvatar
        ' 
        picAvatar.BackgroundImage = CType(resources.GetObject("picAvatar.BackgroundImage"), Image)
        picAvatar.BackgroundImageLayout = ImageLayout.Zoom
        picAvatar.InitialImage = Nothing
        picAvatar.Location = New Point(862, 71)
        picAvatar.Name = "picAvatar"
        picAvatar.Size = New Size(200, 200)
        picAvatar.TabIndex = 0
        picAvatar.TabStop = False
        ' 
        ' pnlInfoCard
        ' 
        pnlInfoCard.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlInfoCard.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlInfoCard.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        pnlInfoCard.BorderThickness = 1
        pnlInfoCard.Controls.Add(lblPhone)
        pnlInfoCard.Controls.Add(lblEmail)
        pnlInfoCard.Controls.Add(linkEditInfo)
        pnlInfoCard.Controls.Add(lblDepartment)
        pnlInfoCard.Controls.Add(lblRole)
        pnlInfoCard.Controls.Add(lblFullName)
        pnlInfoCard.Location = New Point(487, 320)
        pnlInfoCard.Name = "pnlInfoCard"
        pnlInfoCard.Radius = 20
        pnlInfoCard.Size = New Size(933, 237)
        pnlInfoCard.TabIndex = 3
        ' 
        ' lblPhone
        ' 
        lblPhone.AutoSize = True
        lblPhone.BackColor = Color.Transparent
        lblPhone.Font = New Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblPhone.ForeColor = Color.Black
        lblPhone.Location = New Point(41, 180)
        lblPhone.Name = "lblPhone"
        lblPhone.Size = New Size(157, 23)
        lblPhone.TabIndex = 11
        lblPhone.Text = "Phone Number"
        ' 
        ' lblEmail
        ' 
        lblEmail.AutoSize = True
        lblEmail.BackColor = Color.Transparent
        lblEmail.Font = New Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblEmail.ForeColor = Color.Black
        lblEmail.Location = New Point(41, 157)
        lblEmail.Name = "lblEmail"
        lblEmail.Size = New Size(62, 23)
        lblEmail.TabIndex = 10
        lblEmail.Text = "Email"
        ' 
        ' linkEditInfo
        ' 
        linkEditInfo.ActiveLinkColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        linkEditInfo.AutoSize = True
        linkEditInfo.BackColor = Color.Transparent
        linkEditInfo.DisabledLinkColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        linkEditInfo.Font = New Font("Century Gothic", 10.8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        linkEditInfo.LinkBehavior = LinkBehavior.NeverUnderline
        linkEditInfo.LinkColor = Color.FromArgb(CByte(32), CByte(124), CByte(64))
        linkEditInfo.Location = New Point(41, 135)
        linkEditInfo.Name = "linkEditInfo"
        linkEditInfo.Size = New Size(226, 22)
        linkEditInfo.TabIndex = 9
        linkEditInfo.TabStop = True
        linkEditInfo.Text = "Edit Account Information"
        ' 
        ' lblDepartment
        ' 
        lblDepartment.AutoSize = True
        lblDepartment.BackColor = Color.Transparent
        lblDepartment.Font = New Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblDepartment.ForeColor = Color.Black
        lblDepartment.Location = New Point(41, 100)
        lblDepartment.Name = "lblDepartment"
        lblDepartment.Size = New Size(130, 23)
        lblDepartment.TabIndex = 7
        lblDepartment.Text = "Department"
        ' 
        ' lblRole
        ' 
        lblRole.AutoSize = True
        lblRole.BackColor = Color.Transparent
        lblRole.Font = New Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblRole.ForeColor = Color.Black
        lblRole.Location = New Point(41, 77)
        lblRole.Name = "lblRole"
        lblRole.Size = New Size(85, 23)
        lblRole.TabIndex = 4
        lblRole.Text = "Student"
        ' 
        ' lblFullName
        ' 
        lblFullName.AutoSize = True
        lblFullName.BackColor = Color.Transparent
        lblFullName.Font = New Font("Century Gothic", 16.2F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblFullName.ForeColor = Color.Black
        lblFullName.Location = New Point(37, 34)
        lblFullName.Name = "lblFullName"
        lblFullName.Size = New Size(364, 34)
        lblFullName.TabIndex = 3
        lblFullName.Text = "Last Name, Full Name M.I."
        ' 
        ' btnLogOut
        ' 
        btnLogOut.BackColor = Color.Transparent
        btnLogOut.BorderColor = Color.Transparent
        btnLogOut.EnteredBorderColor = Color.Transparent
        btnLogOut.EnteredColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnLogOut.Font = New Font("Century Gothic", 10.2F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnLogOut.Image = Nothing
        btnLogOut.ImageAlign = ContentAlignment.MiddleLeft
        btnLogOut.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnLogOut.Location = New Point(882, 581)
        btnLogOut.Name = "btnLogOut"
        btnLogOut.PressedBorderColor = Color.Transparent
        btnLogOut.PressedColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnLogOut.Size = New Size(159, 48)
        btnLogOut.TabIndex = 4
        btnLogOut.Text = "Log out"
        btnLogOut.TextAlignment = StringAlignment.Center
        btnLogOut.UseWaitCursor = True
        ' 
        ' btnBack
        ' 
        btnBack.BackColor = Color.Transparent
        btnBack.BackgroundImageLayout = ImageLayout.Zoom
        btnBack.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        btnBack.EnteredBorderColor = Color.Transparent
        btnBack.EnteredColor = Color.DarkGray
        btnBack.Font = New Font("Century Gothic", 10.2F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnBack.Image = Nothing
        btnBack.ImageAlign = ContentAlignment.MiddleLeft
        btnBack.InactiveColor = Color.LightGray
        btnBack.Location = New Point(15, 14)
        btnBack.Name = "btnBack"
        btnBack.PressedBorderColor = Color.Transparent
        btnBack.PressedColor = Color.DarkGray
        btnBack.Size = New Size(149, 39)
        btnBack.TabIndex = 9
        btnBack.Text = "←   Back"
        btnBack.TextAlignment = StringAlignment.Center
        btnBack.UseWaitCursor = True
        ' 
        ' linkProfilePicture
        ' 
        linkProfilePicture.ActiveLinkColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        linkProfilePicture.AutoSize = True
        linkProfilePicture.BackColor = Color.Transparent
        linkProfilePicture.DisabledLinkColor = Color.FromArgb(CByte(32), CByte(124), CByte(64))
        linkProfilePicture.Font = New Font("Century Gothic", 10.2F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        linkProfilePicture.ForeColor = Color.FromArgb(CByte(32), CByte(124), CByte(64))
        linkProfilePicture.LinkBehavior = LinkBehavior.NeverUnderline
        linkProfilePicture.LinkColor = Color.FromArgb(CByte(32), CByte(124), CByte(64))
        linkProfilePicture.Location = New Point(892, 277)
        linkProfilePicture.Name = "linkProfilePicture"
        linkProfilePicture.Size = New Size(148, 19)
        linkProfilePicture.TabIndex = 10
        linkProfilePicture.TabStop = True
        linkProfilePicture.Text = "Set Profile Picture"
        linkProfilePicture.VisitedLinkColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        ' 
        ' ucProfileSettings
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(linkProfilePicture)
        Controls.Add(btnBack)
        Controls.Add(btnLogOut)
        Controls.Add(pnlInfoCard)
        Controls.Add(picAvatar)
        Name = "ucProfileSettings"
        Size = New Size(1924, 917)
        CType(picAvatar, ComponentModel.ISupportInitialize).EndInit()
        pnlInfoCard.ResumeLayout(False)
        pnlInfoCard.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents picAvatar As PictureBox
    Friend WithEvents lblCardSubtitle As Label
    Friend WithEvents pnlInfoCard As RoundedPanel
    Friend WithEvents lblFullName As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents lblDepartment As Label
    Friend WithEvents lblRole As Label
    Friend WithEvents btnLogOut As ReaLTaiizor.Controls.Button
    Friend WithEvents linkEditInfo As LinkLabel
    Friend WithEvents lblPhone As Label
    Friend WithEvents lblEmail As Label
    Friend WithEvents btnBack As ReaLTaiizor.Controls.Button
    Friend WithEvents DungeonLinkLabel1 As ReaLTaiizor.Controls.DungeonLinkLabel
    Friend WithEvents linkSetProfile As ReaLTaiizor.Controls.LinkLabelEdit
    Friend WithEvents linkSetProfilePicture As ReaLTaiizor.Controls.LinkLabelEdit
    Friend WithEvents linkProfilePicture As ReaLTaiizor.Controls.LinkLabelEdit

End Class
