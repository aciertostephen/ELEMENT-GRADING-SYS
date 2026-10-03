<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucUserManagement
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
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
        tblMain = New TableLayoutPanel()
        pnlUserBG = New RoundedPanel()
        dgvUsers = New DataGridView()
        user = New DataGridViewTextBoxColumn()
        username = New DataGridViewTextBoxColumn()
        role = New DataGridViewTextBoxColumn()
        email = New DataGridViewTextBoxColumn()
        phone = New DataGridViewTextBoxColumn()
        dateadd = New DataGridViewTextBoxColumn()
        status = New DataGridViewTextBoxColumn()
        pnlToolbar = New Panel()
        btnFilter = New ReaLTaiizor.Controls.Button()
        pnlSearch = New RoundedPanel()
        txtSearch = New TextBox()
        pnlStats = New Panel()
        flowStats = New FlowLayoutPanel()
        pnlMainBG = New RoundedPanel()
        btnDeleteBTN = New ReaLTaiizor.Controls.Button()
        linkResetPass = New LinkLabel()
        linkDeactivate = New LinkLabel()
        linkEditInfo = New LinkLabel()
        btnCreateUser = New ReaLTaiizor.Controls.Button()
        pnlDeleteConfirmation = New RoundedPanel()
        lblDeleteConfirm = New Label()
        btnCancelDeletion = New ReaLTaiizor.Controls.Button()
        btnDeleteAccount = New ReaLTaiizor.Controls.Button()
        tblMain.SuspendLayout()
        pnlUserBG.SuspendLayout()
        CType(dgvUsers, ComponentModel.ISupportInitialize).BeginInit()
        pnlToolbar.SuspendLayout()
        pnlSearch.SuspendLayout()
        pnlStats.SuspendLayout()
        pnlMainBG.SuspendLayout()
        pnlDeleteConfirmation.SuspendLayout()
        SuspendLayout()
        ' 
        ' tblMain
        ' 
        tblMain.ColumnCount = 2
        tblMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tblMain.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 445F))
        tblMain.Controls.Add(pnlUserBG, 0, 0)
        tblMain.Controls.Add(pnlMainBG, 1, 0)
        tblMain.Dock = DockStyle.Fill
        tblMain.Location = New Point(0, 0)
        tblMain.Name = "tblMain"
        tblMain.RowCount = 2
        tblMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tblMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tblMain.Size = New Size(1924, 917)
        tblMain.TabIndex = 2
        ' 
        ' pnlUserBG
        ' 
        pnlUserBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlUserBG.Anchor = AnchorStyles.None
        pnlUserBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlUserBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlUserBG.BorderThickness = 1
        pnlUserBG.Controls.Add(dgvUsers)
        pnlUserBG.Controls.Add(pnlToolbar)
        pnlUserBG.Controls.Add(pnlStats)
        pnlUserBG.Location = New Point(13, 3)
        pnlUserBG.Name = "pnlUserBG"
        pnlUserBG.Radius = 20
        pnlUserBG.Size = New Size(1453, 891)
        pnlUserBG.TabIndex = 1
        ' 
        ' dgvUsers
        ' 
        dgvUsers.AllowUserToAddRows = False
        dgvUsers.AllowUserToDeleteRows = False
        dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvUsers.BackgroundColor = Color.White
        dgvUsers.BorderStyle = BorderStyle.None
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        DataGridViewCellStyle1.Font = New Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle1.ForeColor = Color.White
        DataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = Color.White
        DataGridViewCellStyle1.WrapMode = DataGridViewTriState.True
        dgvUsers.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvUsers.Columns.AddRange(New DataGridViewColumn() {user, username, role, email, phone, dateadd, status})
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = Color.White
        DataGridViewCellStyle2.Font = New Font("Segoe UI", 9F)
        DataGridViewCellStyle2.ForeColor = SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.False
        dgvUsers.DefaultCellStyle = DataGridViewCellStyle2
        dgvUsers.GridColor = Color.White
        dgvUsers.Location = New Point(17, 163)
        dgvUsers.Name = "dgvUsers"
        dgvUsers.RowHeadersVisible = False
        dgvUsers.RowHeadersWidth = 51
        dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvUsers.Size = New Size(1415, 725)
        dgvUsers.TabIndex = 2
        ' 
        ' user
        ' 
        user.HeaderText = "User ID"
        user.MinimumWidth = 6
        user.Name = "user"
        ' 
        ' username
        ' 
        username.HeaderText = "Full Name"
        username.MinimumWidth = 6
        username.Name = "username"
        ' 
        ' role
        ' 
        role.HeaderText = "Role"
        role.MinimumWidth = 6
        role.Name = "role"
        ' 
        ' email
        ' 
        email.HeaderText = "Email Address"
        email.MinimumWidth = 6
        email.Name = "email"
        ' 
        ' phone
        ' 
        phone.HeaderText = "Phone Number"
        phone.MinimumWidth = 6
        phone.Name = "phone"
        ' 
        ' dateadd
        ' 
        dateadd.HeaderText = "Date Added"
        dateadd.MinimumWidth = 6
        dateadd.Name = "dateadd"
        ' 
        ' status
        ' 
        status.HeaderText = "Status"
        status.MinimumWidth = 6
        status.Name = "status"
        ' 
        ' pnlToolbar
        ' 
        pnlToolbar.Controls.Add(btnFilter)
        pnlToolbar.Controls.Add(pnlSearch)
        pnlToolbar.Dock = DockStyle.Top
        pnlToolbar.Location = New Point(0, 100)
        pnlToolbar.Name = "pnlToolbar"
        pnlToolbar.Size = New Size(1453, 57)
        pnlToolbar.TabIndex = 1
        ' 
        ' btnFilter
        ' 
        btnFilter.BackColor = Color.Transparent
        btnFilter.BackgroundImageLayout = ImageLayout.Zoom
        btnFilter.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnFilter.EnteredBorderColor = Color.Empty
        btnFilter.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnFilter.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnFilter.Image = Nothing
        btnFilter.ImageAlign = ContentAlignment.MiddleLeft
        btnFilter.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnFilter.Location = New Point(323, 8)
        btnFilter.Name = "btnFilter"
        btnFilter.PressedBorderColor = Color.Transparent
        btnFilter.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnFilter.Size = New Size(145, 40)
        btnFilter.TabIndex = 3
        btnFilter.Text = "▾ Filter"
        btnFilter.TextAlignment = StringAlignment.Center
        btnFilter.UseWaitCursor = True
        ' 
        ' pnlSearch
        ' 
        pnlSearch.ActiveBorderColor = Color.CornflowerBlue
        pnlSearch.BackColor = Color.White
        pnlSearch.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        pnlSearch.BorderThickness = 1
        pnlSearch.Controls.Add(txtSearch)
        pnlSearch.Location = New Point(17, 8)
        pnlSearch.Name = "pnlSearch"
        pnlSearch.Radius = 8
        pnlSearch.Size = New Size(300, 40)
        pnlSearch.TabIndex = 2
        ' 
        ' txtSearch
        ' 
        txtSearch.BackColor = Color.White
        txtSearch.BorderStyle = BorderStyle.None
        txtSearch.Font = New Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtSearch.Location = New Point(9, 10)
        txtSearch.Name = "txtSearch"
        txtSearch.PlaceholderText = "Search"
        txtSearch.Size = New Size(236, 19)
        txtSearch.TabIndex = 1
        ' 
        ' pnlStats
        ' 
        pnlStats.Controls.Add(flowStats)
        pnlStats.Dock = DockStyle.Top
        pnlStats.Location = New Point(0, 0)
        pnlStats.Name = "pnlStats"
        pnlStats.Size = New Size(1453, 100)
        pnlStats.TabIndex = 0
        ' 
        ' flowStats
        ' 
        flowStats.Dock = DockStyle.Fill
        flowStats.Location = New Point(0, 0)
        flowStats.Name = "flowStats"
        flowStats.Size = New Size(1453, 100)
        flowStats.TabIndex = 0
        flowStats.WrapContents = False
        ' 
        ' pnlMainBG
        ' 
        pnlMainBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlMainBG.Anchor = AnchorStyles.None
        pnlMainBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlMainBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlMainBG.BorderThickness = 1
        pnlMainBG.Controls.Add(btnDeleteBTN)
        pnlMainBG.Controls.Add(linkResetPass)
        pnlMainBG.Controls.Add(linkDeactivate)
        pnlMainBG.Controls.Add(linkEditInfo)
        pnlMainBG.Controls.Add(btnCreateUser)
        pnlMainBG.Location = New Point(1496, 3)
        pnlMainBG.Name = "pnlMainBG"
        pnlMainBG.Radius = 20
        pnlMainBG.Size = New Size(410, 891)
        pnlMainBG.TabIndex = 0
        ' 
        ' btnDeleteBTN
        ' 
        btnDeleteBTN.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        btnDeleteBTN.BackColor = Color.Transparent
        btnDeleteBTN.BorderColor = Color.Transparent
        btnDeleteBTN.EnteredBorderColor = Color.Transparent
        btnDeleteBTN.EnteredColor = Color.FromArgb(CByte(220), CByte(70), CByte(70))
        btnDeleteBTN.Font = New Font("Century Gothic", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnDeleteBTN.Image = Nothing
        btnDeleteBTN.ImageAlign = ContentAlignment.MiddleLeft
        btnDeleteBTN.InactiveColor = Color.FromArgb(CByte(200), CByte(51), CByte(51))
        btnDeleteBTN.Location = New Point(90, 807)
        btnDeleteBTN.Name = "btnDeleteBTN"
        btnDeleteBTN.PressedBorderColor = Color.Transparent
        btnDeleteBTN.PressedColor = Color.FromArgb(CByte(165), CByte(38), CByte(38))
        btnDeleteBTN.Size = New Size(235, 36)
        btnDeleteBTN.TabIndex = 9
        btnDeleteBTN.Text = "Delete Account"
        btnDeleteBTN.TextAlignment = StringAlignment.Center
        btnDeleteBTN.UseWaitCursor = True
        ' 
        ' linkResetPass
        ' 
        linkResetPass.BackColor = Color.Transparent
        linkResetPass.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        linkResetPass.ForeColor = Color.Black
        linkResetPass.LinkBehavior = LinkBehavior.NeverUnderline
        linkResetPass.LinkColor = Color.Black
        linkResetPass.Location = New Point(25, 265)
        linkResetPass.Name = "linkResetPass"
        linkResetPass.Size = New Size(146, 23)
        linkResetPass.TabIndex = 8
        linkResetPass.TabStop = True
        linkResetPass.Text = "Reset Password"
        ' 
        ' linkDeactivate
        ' 
        linkDeactivate.BackColor = Color.Transparent
        linkDeactivate.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        linkDeactivate.ForeColor = Color.Black
        linkDeactivate.LinkBehavior = LinkBehavior.NeverUnderline
        linkDeactivate.LinkColor = Color.Black
        linkDeactivate.Location = New Point(25, 223)
        linkDeactivate.Name = "linkDeactivate"
        linkDeactivate.Size = New Size(146, 23)
        linkDeactivate.TabIndex = 7
        linkDeactivate.TabStop = True
        linkDeactivate.Text = "Deactivate"
        ' 
        ' linkEditInfo
        ' 
        linkEditInfo.BackColor = Color.Transparent
        linkEditInfo.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        linkEditInfo.ForeColor = Color.Black
        linkEditInfo.LinkBehavior = LinkBehavior.NeverUnderline
        linkEditInfo.LinkColor = Color.Black
        linkEditInfo.Location = New Point(25, 176)
        linkEditInfo.Name = "linkEditInfo"
        linkEditInfo.Size = New Size(146, 23)
        linkEditInfo.TabIndex = 6
        linkEditInfo.TabStop = True
        linkEditInfo.Text = "Edit Information"
        ' 
        ' btnCreateUser
        ' 
        btnCreateUser.BackColor = Color.Transparent
        btnCreateUser.BackgroundImageLayout = ImageLayout.Zoom
        btnCreateUser.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnCreateUser.EnteredBorderColor = Color.Empty
        btnCreateUser.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnCreateUser.Font = New Font("Century Gothic", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnCreateUser.Image = Nothing
        btnCreateUser.ImageAlign = ContentAlignment.MiddleLeft
        btnCreateUser.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnCreateUser.Location = New Point(90, 64)
        btnCreateUser.Name = "btnCreateUser"
        btnCreateUser.PressedBorderColor = Color.Transparent
        btnCreateUser.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnCreateUser.Size = New Size(235, 36)
        btnCreateUser.TabIndex = 4
        btnCreateUser.Text = "Create New User"
        btnCreateUser.TextAlignment = StringAlignment.Center
        btnCreateUser.UseWaitCursor = True
        ' 
        ' pnlDeleteConfirmation
        ' 
        pnlDeleteConfirmation.ActiveBorderColor = Color.Black
        pnlDeleteConfirmation.BackColor = Color.White
        pnlDeleteConfirmation.BorderColor = Color.Black
        pnlDeleteConfirmation.BorderThickness = 1
        pnlDeleteConfirmation.Controls.Add(lblDeleteConfirm)
        pnlDeleteConfirmation.Controls.Add(btnCancelDeletion)
        pnlDeleteConfirmation.Controls.Add(btnDeleteAccount)
        pnlDeleteConfirmation.Location = New Point(637, 427)
        pnlDeleteConfirmation.Name = "pnlDeleteConfirmation"
        pnlDeleteConfirmation.Radius = 8
        pnlDeleteConfirmation.Size = New Size(650, 200)
        pnlDeleteConfirmation.TabIndex = 6
        pnlDeleteConfirmation.Visible = False
        ' 
        ' lblDeleteConfirm
        ' 
        lblDeleteConfirm.BackColor = Color.Transparent
        lblDeleteConfirm.Font = New Font("Century Gothic", 19.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblDeleteConfirm.Location = New Point(21, 24)
        lblDeleteConfirm.Name = "lblDeleteConfirm"
        lblDeleteConfirm.Size = New Size(593, 49)
        lblDeleteConfirm.TabIndex = 0
        lblDeleteConfirm.Text = "Do you want to delete this account?"
        ' 
        ' btnCancelDeletion
        ' 
        btnCancelDeletion.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        btnCancelDeletion.BackColor = Color.Transparent
        btnCancelDeletion.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnCancelDeletion.EnteredBorderColor = Color.Transparent
        btnCancelDeletion.EnteredColor = Color.FromArgb(CByte(125), CByte(132), CByte(145))
        btnCancelDeletion.Font = New Font("Century Gothic", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnCancelDeletion.Image = Nothing
        btnCancelDeletion.ImageAlign = ContentAlignment.MiddleLeft
        btnCancelDeletion.InactiveColor = Color.FromArgb(CByte(107), CByte(114), CByte(128))
        btnCancelDeletion.Location = New Point(418, 128)
        btnCancelDeletion.Name = "btnCancelDeletion"
        btnCancelDeletion.PressedBorderColor = Color.Transparent
        btnCancelDeletion.PressedColor = Color.FromArgb(CByte(86), CByte(92), CByte(103))
        btnCancelDeletion.Size = New Size(100, 42)
        btnCancelDeletion.TabIndex = 4
        btnCancelDeletion.Text = "CANCEL"
        btnCancelDeletion.TextAlignment = StringAlignment.Center
        btnCancelDeletion.UseWaitCursor = True
        ' 
        ' btnDeleteAccount
        ' 
        btnDeleteAccount.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        btnDeleteAccount.BackColor = Color.Transparent
        btnDeleteAccount.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnDeleteAccount.EnteredBorderColor = Color.Transparent
        btnDeleteAccount.EnteredColor = Color.FromArgb(CByte(220), CByte(70), CByte(70))
        btnDeleteAccount.Font = New Font("Century Gothic", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnDeleteAccount.Image = Nothing
        btnDeleteAccount.ImageAlign = ContentAlignment.MiddleLeft
        btnDeleteAccount.InactiveColor = Color.FromArgb(CByte(200), CByte(51), CByte(51))
        btnDeleteAccount.Location = New Point(526, 128)
        btnDeleteAccount.Name = "btnDeleteAccount"
        btnDeleteAccount.PressedBorderColor = Color.Transparent
        btnDeleteAccount.PressedColor = Color.FromArgb(CByte(165), CByte(38), CByte(38))
        btnDeleteAccount.Size = New Size(100, 42)
        btnDeleteAccount.TabIndex = 3
        btnDeleteAccount.Text = "DELETE"
        btnDeleteAccount.TextAlignment = StringAlignment.Center
        btnDeleteAccount.UseWaitCursor = True
        ' 
        ' ucUserManagement
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(tblMain)
        Controls.Add(pnlDeleteConfirmation)
        Name = "ucUserManagement"
        Size = New Size(1924, 917)
        tblMain.ResumeLayout(False)
        pnlUserBG.ResumeLayout(False)
        CType(dgvUsers, ComponentModel.ISupportInitialize).EndInit()
        pnlToolbar.ResumeLayout(False)
        pnlSearch.ResumeLayout(False)
        pnlSearch.PerformLayout()
        pnlStats.ResumeLayout(False)
        pnlMainBG.ResumeLayout(False)
        pnlDeleteConfirmation.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
    Friend WithEvents tblMain As TableLayoutPanel
    Friend WithEvents pnlMainBG As RoundedPanel
    Friend WithEvents pnlUserBG As RoundedPanel
    Friend WithEvents pnlStats As Panel
    Friend WithEvents pnlToolbar As Panel
    Friend WithEvents flowStats As FlowLayoutPanel
    Friend WithEvents pnlSearch As RoundedPanel
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents btnFilter As ReaLTaiizor.Controls.Button
    Friend WithEvents dgvUsers As DataGridView
    Friend WithEvents user As DataGridViewTextBoxColumn
    Friend WithEvents username As DataGridViewTextBoxColumn
    Friend WithEvents role As DataGridViewTextBoxColumn
    Friend WithEvents email As DataGridViewTextBoxColumn
    Friend WithEvents phone As DataGridViewTextBoxColumn
    Friend WithEvents dateadd As DataGridViewTextBoxColumn
    Friend WithEvents status As DataGridViewTextBoxColumn
    Friend WithEvents btnCreateUser As ReaLTaiizor.Controls.Button
    Friend WithEvents linkEditInfo As LinkLabel
    Friend WithEvents linkResetPass As LinkLabel
    Friend WithEvents linkDeactivate As LinkLabel
    Friend WithEvents btnDeleteBTN As ReaLTaiizor.Controls.Button
    Friend WithEvents pnlDeleteConfirmation As RoundedPanel
    Friend WithEvents lblDeleteConfirm As Label
    Friend WithEvents btnCancelDeletion As ReaLTaiizor.Controls.Button
    Friend WithEvents btnDeleteAccount As ReaLTaiizor.Controls.Button

End Class
