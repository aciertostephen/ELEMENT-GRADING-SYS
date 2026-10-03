<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucGradingConfig
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
        tblGrading = New TableLayoutPanel()
        tblConfigCards = New TableLayoutPanel()
        RoundedPanel1 = New RoundedPanel()
        lblTermValue = New Label()
        lblSchoolYearValue = New Label()
        lblEndDateCaption = New Label()
        lblStartDateCaption = New Label()
        lblStartDateValue = New Label()
        lblEndDateValue = New Label()
        lblTermCaption = New Label()
        lblSchoolYearCaption = New Label()
        lblSemesterTitle = New Label()
        btnEditSemester = New ReaLTaiizor.Controls.Button()
        RoundedPanel2 = New RoundedPanel()
        btnEditEncoding = New ReaLTaiizor.Controls.Button()
        lblDeadlineValue = New Label()
        lblLockStatusValue = New Label()
        lblLockStatusCaption = New Label()
        lblDeadlineCaption = New Label()
        lblEncodingTitle = New Label()
        pnlGradingScale = New RoundedPanel()
        btnEditScale = New ReaLTaiizor.Controls.Button()
        dgvGradingScale = New DataGridView()
        percentage = New DataGridViewTextBoxColumn()
        gwa = New DataGridViewTextBoxColumn()
        lblScaleTitle = New Label()
        tblGrading.SuspendLayout()
        tblConfigCards.SuspendLayout()
        RoundedPanel1.SuspendLayout()
        RoundedPanel2.SuspendLayout()
        pnlGradingScale.SuspendLayout()
        CType(dgvGradingScale, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' tblGrading
        ' 
        tblGrading.ColumnCount = 1
        tblGrading.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tblGrading.Controls.Add(tblConfigCards, 0, 0)
        tblGrading.Controls.Add(pnlGradingScale, 0, 1)
        tblGrading.Location = New Point(0, 0)
        tblGrading.Name = "tblGrading"
        tblGrading.Padding = New Padding(20)
        tblGrading.RowCount = 2
        tblGrading.RowStyles.Add(New RowStyle(SizeType.Absolute, 300F))
        tblGrading.RowStyles.Add(New RowStyle(SizeType.Absolute, 400F))
        tblGrading.Size = New Size(1924, 740)
        tblGrading.TabIndex = 0
        ' 
        ' tblConfigCards
        ' 
        tblConfigCards.ColumnCount = 2
        tblConfigCards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tblConfigCards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tblConfigCards.Controls.Add(RoundedPanel1, 0, 0)
        tblConfigCards.Controls.Add(RoundedPanel2, 1, 0)
        tblConfigCards.Dock = DockStyle.Fill
        tblConfigCards.Location = New Point(23, 23)
        tblConfigCards.Name = "tblConfigCards"
        tblConfigCards.RowCount = 1
        tblConfigCards.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tblConfigCards.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tblConfigCards.Size = New Size(1878, 294)
        tblConfigCards.TabIndex = 0
        ' 
        ' RoundedPanel1
        ' 
        RoundedPanel1.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel1.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel1.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel1.BorderThickness = 1
        RoundedPanel1.Controls.Add(lblTermValue)
        RoundedPanel1.Controls.Add(lblSchoolYearValue)
        RoundedPanel1.Controls.Add(lblEndDateCaption)
        RoundedPanel1.Controls.Add(lblStartDateCaption)
        RoundedPanel1.Controls.Add(lblStartDateValue)
        RoundedPanel1.Controls.Add(lblEndDateValue)
        RoundedPanel1.Controls.Add(lblTermCaption)
        RoundedPanel1.Controls.Add(lblSchoolYearCaption)
        RoundedPanel1.Controls.Add(lblSemesterTitle)
        RoundedPanel1.Controls.Add(btnEditSemester)
        RoundedPanel1.Dock = DockStyle.Fill
        RoundedPanel1.Location = New Point(0, 0)
        RoundedPanel1.Margin = New Padding(0, 0, 10, 0)
        RoundedPanel1.Name = "RoundedPanel1"
        RoundedPanel1.Radius = 12
        RoundedPanel1.Size = New Size(929, 294)
        RoundedPanel1.TabIndex = 0
        ' 
        ' lblTermValue
        ' 
        lblTermValue.AutoSize = True
        lblTermValue.BackColor = Color.Transparent
        lblTermValue.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblTermValue.ForeColor = Color.Black
        lblTermValue.Location = New Point(379, 92)
        lblTermValue.Name = "lblTermValue"
        lblTermValue.Size = New Size(19, 21)
        lblTermValue.TabIndex = 18
        lblTermValue.Text = "1"
        ' 
        ' lblSchoolYearValue
        ' 
        lblSchoolYearValue.AutoSize = True
        lblSchoolYearValue.BackColor = Color.Transparent
        lblSchoolYearValue.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblSchoolYearValue.ForeColor = Color.Black
        lblSchoolYearValue.Location = New Point(379, 64)
        lblSchoolYearValue.Name = "lblSchoolYearValue"
        lblSchoolYearValue.Size = New Size(88, 21)
        lblSchoolYearValue.TabIndex = 17
        lblSchoolYearValue.Text = "2026-2027"
        ' 
        ' lblEndDateCaption
        ' 
        lblEndDateCaption.AutoSize = True
        lblEndDateCaption.BackColor = Color.Transparent
        lblEndDateCaption.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblEndDateCaption.ForeColor = Color.Black
        lblEndDateCaption.Location = New Point(64, 154)
        lblEndDateCaption.Name = "lblEndDateCaption"
        lblEndDateCaption.Size = New Size(143, 21)
        lblEndDateCaption.TabIndex = 16
        lblEndDateCaption.Text = "Term End Date :"
        ' 
        ' lblStartDateCaption
        ' 
        lblStartDateCaption.AutoSize = True
        lblStartDateCaption.BackColor = Color.Transparent
        lblStartDateCaption.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblStartDateCaption.ForeColor = Color.Black
        lblStartDateCaption.Location = New Point(64, 122)
        lblStartDateCaption.Name = "lblStartDateCaption"
        lblStartDateCaption.Size = New Size(151, 21)
        lblStartDateCaption.TabIndex = 15
        lblStartDateCaption.Text = "Term Start Date :"
        ' 
        ' lblStartDateValue
        ' 
        lblStartDateValue.AutoSize = True
        lblStartDateValue.BackColor = Color.Transparent
        lblStartDateValue.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblStartDateValue.ForeColor = Color.Black
        lblStartDateValue.Location = New Point(379, 122)
        lblStartDateValue.Name = "lblStartDateValue"
        lblStartDateValue.Size = New Size(128, 21)
        lblStartDateValue.TabIndex = 14
        lblStartDateValue.Text = "DD/MM/YYYY"
        ' 
        ' lblEndDateValue
        ' 
        lblEndDateValue.AutoSize = True
        lblEndDateValue.BackColor = Color.Transparent
        lblEndDateValue.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblEndDateValue.ForeColor = Color.Black
        lblEndDateValue.Location = New Point(379, 154)
        lblEndDateValue.Name = "lblEndDateValue"
        lblEndDateValue.Size = New Size(128, 21)
        lblEndDateValue.TabIndex = 12
        lblEndDateValue.Text = "DD/MM/YYYY"
        ' 
        ' lblTermCaption
        ' 
        lblTermCaption.AutoSize = True
        lblTermCaption.BackColor = Color.Transparent
        lblTermCaption.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblTermCaption.ForeColor = Color.Black
        lblTermCaption.Location = New Point(64, 92)
        lblTermCaption.Name = "lblTermCaption"
        lblTermCaption.Size = New Size(59, 21)
        lblTermCaption.TabIndex = 8
        lblTermCaption.Text = "Term :"
        ' 
        ' lblSchoolYearCaption
        ' 
        lblSchoolYearCaption.AutoSize = True
        lblSchoolYearCaption.BackColor = Color.Transparent
        lblSchoolYearCaption.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblSchoolYearCaption.ForeColor = Color.Black
        lblSchoolYearCaption.Location = New Point(64, 64)
        lblSchoolYearCaption.Name = "lblSchoolYearCaption"
        lblSchoolYearCaption.Size = New Size(113, 21)
        lblSchoolYearCaption.TabIndex = 7
        lblSchoolYearCaption.Text = "School Year:"
        ' 
        ' lblSemesterTitle
        ' 
        lblSemesterTitle.AutoSize = True
        lblSemesterTitle.BackColor = Color.Transparent
        lblSemesterTitle.Font = New Font("Century Gothic", 10.8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblSemesterTitle.ForeColor = Color.FromArgb(CByte(126), CByte(126), CByte(126))
        lblSemesterTitle.Location = New Point(36, 18)
        lblSemesterTitle.Name = "lblSemesterTitle"
        lblSemesterTitle.Size = New Size(307, 22)
        lblSemesterTitle.TabIndex = 6
        lblSemesterTitle.Text = "ACTIVE SEMESTER CONFIGURATION"
        ' 
        ' btnEditSemester
        ' 
        btnEditSemester.BackColor = Color.Transparent
        btnEditSemester.BackgroundImageLayout = ImageLayout.Zoom
        btnEditSemester.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnEditSemester.EnteredBorderColor = Color.Empty
        btnEditSemester.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnEditSemester.Font = New Font("Century Gothic", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnEditSemester.Image = Nothing
        btnEditSemester.ImageAlign = ContentAlignment.MiddleLeft
        btnEditSemester.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnEditSemester.Location = New Point(36, 232)
        btnEditSemester.Name = "btnEditSemester"
        btnEditSemester.PressedBorderColor = Color.Transparent
        btnEditSemester.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnEditSemester.Size = New Size(195, 39)
        btnEditSemester.TabIndex = 5
        btnEditSemester.Text = "Edit"
        btnEditSemester.TextAlignment = StringAlignment.Center
        btnEditSemester.UseWaitCursor = True
        ' 
        ' RoundedPanel2
        ' 
        RoundedPanel2.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel2.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel2.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel2.BorderThickness = 1
        RoundedPanel2.Controls.Add(btnEditEncoding)
        RoundedPanel2.Controls.Add(lblDeadlineValue)
        RoundedPanel2.Controls.Add(lblLockStatusValue)
        RoundedPanel2.Controls.Add(lblLockStatusCaption)
        RoundedPanel2.Controls.Add(lblDeadlineCaption)
        RoundedPanel2.Controls.Add(lblEncodingTitle)
        RoundedPanel2.Dock = DockStyle.Fill
        RoundedPanel2.Location = New Point(949, 0)
        RoundedPanel2.Margin = New Padding(10, 0, 0, 0)
        RoundedPanel2.Name = "RoundedPanel2"
        RoundedPanel2.Radius = 12
        RoundedPanel2.Size = New Size(929, 294)
        RoundedPanel2.TabIndex = 1
        ' 
        ' btnEditEncoding
        ' 
        btnEditEncoding.BackColor = Color.Transparent
        btnEditEncoding.BackgroundImageLayout = ImageLayout.Zoom
        btnEditEncoding.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnEditEncoding.EnteredBorderColor = Color.Empty
        btnEditEncoding.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnEditEncoding.Font = New Font("Century Gothic", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnEditEncoding.Image = Nothing
        btnEditEncoding.ImageAlign = ContentAlignment.MiddleLeft
        btnEditEncoding.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnEditEncoding.Location = New Point(49, 232)
        btnEditEncoding.Name = "btnEditEncoding"
        btnEditEncoding.PressedBorderColor = Color.Transparent
        btnEditEncoding.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnEditEncoding.Size = New Size(195, 39)
        btnEditEncoding.TabIndex = 19
        btnEditEncoding.Text = "Edit"
        btnEditEncoding.TextAlignment = StringAlignment.Center
        btnEditEncoding.UseWaitCursor = True
        ' 
        ' lblDeadlineValue
        ' 
        lblDeadlineValue.AutoSize = True
        lblDeadlineValue.BackColor = Color.Transparent
        lblDeadlineValue.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblDeadlineValue.ForeColor = Color.Black
        lblDeadlineValue.Location = New Point(374, 64)
        lblDeadlineValue.Name = "lblDeadlineValue"
        lblDeadlineValue.Size = New Size(222, 21)
        lblDeadlineValue.TabIndex = 11
        lblDeadlineValue.Text = "HH:MM:  -  MM/DD/YYYY"
        ' 
        ' lblLockStatusValue
        ' 
        lblLockStatusValue.AutoSize = True
        lblLockStatusValue.BackColor = Color.Transparent
        lblLockStatusValue.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblLockStatusValue.ForeColor = Color.FromArgb(CByte(32), CByte(124), CByte(64))
        lblLockStatusValue.Location = New Point(374, 92)
        lblLockStatusValue.Name = "lblLockStatusValue"
        lblLockStatusValue.Size = New Size(204, 21)
        lblLockStatusValue.TabIndex = 10
        lblLockStatusValue.Text = "Faculty Encoding Open"
        ' 
        ' lblLockStatusCaption
        ' 
        lblLockStatusCaption.AutoSize = True
        lblLockStatusCaption.BackColor = Color.Transparent
        lblLockStatusCaption.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblLockStatusCaption.ForeColor = Color.Black
        lblLockStatusCaption.Location = New Point(49, 92)
        lblLockStatusCaption.Name = "lblLockStatusCaption"
        lblLockStatusCaption.Size = New Size(237, 21)
        lblLockStatusCaption.TabIndex = 9
        lblLockStatusCaption.Text = "Global System Lock Status :"
        ' 
        ' lblDeadlineCaption
        ' 
        lblDeadlineCaption.AutoSize = True
        lblDeadlineCaption.BackColor = Color.Transparent
        lblDeadlineCaption.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblDeadlineCaption.ForeColor = Color.Black
        lblDeadlineCaption.Location = New Point(49, 64)
        lblDeadlineCaption.Name = "lblDeadlineCaption"
        lblDeadlineCaption.Size = New Size(177, 21)
        lblDeadlineCaption.TabIndex = 8
        lblDeadlineCaption.Text = "Encoding Deadline :"
        ' 
        ' lblEncodingTitle
        ' 
        lblEncodingTitle.AutoSize = True
        lblEncodingTitle.BackColor = Color.Transparent
        lblEncodingTitle.Font = New Font("Century Gothic", 10.8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblEncodingTitle.ForeColor = Color.FromArgb(CByte(126), CByte(126), CByte(126))
        lblEncodingTitle.Location = New Point(49, 18)
        lblEncodingTitle.Name = "lblEncodingTitle"
        lblEncodingTitle.Size = New Size(324, 22)
        lblEncodingTitle.TabIndex = 7
        lblEncodingTitle.Text = "GRADE ENCODING CONFIGURATION"
        ' 
        ' pnlGradingScale
        ' 
        pnlGradingScale.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlGradingScale.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlGradingScale.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlGradingScale.BorderThickness = 1
        pnlGradingScale.Controls.Add(btnEditScale)
        pnlGradingScale.Controls.Add(dgvGradingScale)
        pnlGradingScale.Controls.Add(lblScaleTitle)
        pnlGradingScale.Dock = DockStyle.Fill
        pnlGradingScale.Location = New Point(20, 340)
        pnlGradingScale.Margin = New Padding(0, 20, 0, 0)
        pnlGradingScale.Name = "pnlGradingScale"
        pnlGradingScale.Radius = 12
        pnlGradingScale.Size = New Size(1884, 380)
        pnlGradingScale.TabIndex = 1
        ' 
        ' btnEditScale
        ' 
        btnEditScale.BackColor = Color.Transparent
        btnEditScale.BackgroundImageLayout = ImageLayout.Zoom
        btnEditScale.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnEditScale.EnteredBorderColor = Color.Empty
        btnEditScale.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnEditScale.Font = New Font("Century Gothic", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnEditScale.Image = Nothing
        btnEditScale.ImageAlign = ContentAlignment.MiddleLeft
        btnEditScale.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnEditScale.Location = New Point(39, 322)
        btnEditScale.Name = "btnEditScale"
        btnEditScale.PressedBorderColor = Color.Transparent
        btnEditScale.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnEditScale.Size = New Size(195, 29)
        btnEditScale.TabIndex = 9
        btnEditScale.Text = "Edit"
        btnEditScale.TextAlignment = StringAlignment.Center
        btnEditScale.UseWaitCursor = True
        ' 
        ' dgvGradingScale
        ' 
        dgvGradingScale.AllowUserToAddRows = False
        dgvGradingScale.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvGradingScale.BackgroundColor = Color.White
        dgvGradingScale.BorderStyle = BorderStyle.None
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        DataGridViewCellStyle1.Font = New Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle1.ForeColor = SystemColors.WindowText
        DataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = DataGridViewTriState.True
        dgvGradingScale.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        dgvGradingScale.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvGradingScale.Columns.AddRange(New DataGridViewColumn() {percentage, gwa})
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = SystemColors.Window
        DataGridViewCellStyle2.Font = New Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle2.ForeColor = SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.False
        dgvGradingScale.DefaultCellStyle = DataGridViewCellStyle2
        dgvGradingScale.Location = New Point(65, 65)
        dgvGradingScale.Name = "dgvGradingScale"
        dgvGradingScale.RowHeadersVisible = False
        dgvGradingScale.RowHeadersWidth = 51
        dgvGradingScale.Size = New Size(1730, 180)
        dgvGradingScale.TabIndex = 8
        ' 
        ' percentage
        ' 
        percentage.HeaderText = "Percentage Range"
        percentage.MinimumWidth = 6
        percentage.Name = "percentage"
        ' 
        ' gwa
        ' 
        gwa.HeaderText = "GWA"
        gwa.MinimumWidth = 6
        gwa.Name = "gwa"
        ' 
        ' lblScaleTitle
        ' 
        lblScaleTitle.AutoSize = True
        lblScaleTitle.BackColor = Color.Transparent
        lblScaleTitle.Font = New Font("Century Gothic", 10.8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblScaleTitle.ForeColor = Color.FromArgb(CByte(126), CByte(126), CByte(126))
        lblScaleTitle.Location = New Point(39, 29)
        lblScaleTitle.Name = "lblScaleTitle"
        lblScaleTitle.Size = New Size(245, 22)
        lblScaleTitle.TabIndex = 7
        lblScaleTitle.Text = "GRADING SCALE AND GWA"
        ' 
        ' ucGradingConfig
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(tblGrading)
        Name = "ucGradingConfig"
        Size = New Size(1924, 917)
        tblGrading.ResumeLayout(False)
        tblConfigCards.ResumeLayout(False)
        RoundedPanel1.ResumeLayout(False)
        RoundedPanel1.PerformLayout()
        RoundedPanel2.ResumeLayout(False)
        RoundedPanel2.PerformLayout()
        pnlGradingScale.ResumeLayout(False)
        pnlGradingScale.PerformLayout()
        CType(dgvGradingScale, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tblGrading As TableLayoutPanel
    Friend WithEvents tblConfigCards As TableLayoutPanel
    Friend WithEvents RoundedPanel1 As RoundedPanel
    Friend WithEvents lblSemesterTitle As Label
    Friend WithEvents btnEditSemester As ReaLTaiizor.Controls.Button
    Friend WithEvents Label4 As Label
    Friend WithEvents sa As Label
    Friend WithEvents lblTermCaption As Label
    Friend WithEvents RoundedPanel2 As RoundedPanel
    Friend WithEvents lblSchoolYearCaption As Label
    Friend WithEvents lblStartDateValue As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents lblEndDateValue As Label
    Friend WithEvents lblStartDateCaption As Label
    Friend WithEvents lblEndDateCaption As Label
    Friend WithEvents lblTermValue As Label
    Friend WithEvents lblSchoolYearValue As Label
    Friend WithEvents lblDeadlineValue As Label
    Friend WithEvents lblLockStatusValue As Label
    Friend WithEvents lblLockStatusCaption As Label
    Friend WithEvents lblDeadlineCaption As Label
    Friend WithEvents lblEncodingTitle As Label
    Friend WithEvents btnEditEncoding As ReaLTaiizor.Controls.Button
    Friend WithEvents pnlGradingScale As RoundedPanel
    Friend WithEvents lblScaleTitle As Label
    Friend WithEvents dgvGradingScale As DataGridView
    Friend WithEvents btnEditScale As ReaLTaiizor.Controls.Button
    Friend WithEvents percentage As DataGridViewTextBoxColumn
    Friend WithEvents gwa As DataGridViewTextBoxColumn


End Class
