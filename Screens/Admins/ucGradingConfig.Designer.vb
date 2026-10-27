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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        RoundedPanel1 = New RoundedPanel()
        lblSYEndValue = New Label()
        lblSYStartValue = New Label()
        btnEditSchoolYear = New ReaLTaiizor.Controls.Button()
        btnNewSchoolYear = New ReaLTaiizor.Controls.Button()
        Label6 = New Label()
        Label5 = New Label()
        cboSchoolYear = New ComboBox()
        Label4 = New Label()
        Label1 = New Label()
        lblSem1Value = New RoundedPanel()
        btnEditSemesters = New ReaLTaiizor.Controls.Button()
        Label12 = New Label()
        lblBreakValue = New Label()
        lblSem2Value = New Label()
        Label7 = New Label()
        Label2 = New Label()
        Label8 = New Label()
        Label9 = New Label()
        RoundedPanel3 = New RoundedPanel()
        btnSetActive = New ReaLTaiizor.Controls.Button()
        btnEditEncoding = New ReaLTaiizor.Controls.Button()
        cboEncodingTerm = New ComboBox()
        lblActiveTermValue = New Label()
        Label18 = New Label()
        lblLockStatusValue = New Label()
        Label20 = New Label()
        Label3 = New Label()
        Label15 = New Label()
        blEncodingStartValue = New Label()
        Label16 = New Label()
        Label14 = New Label()
        lblDeadlineValue = New Label()
        RoundedPanel2 = New RoundedPanel()
        EditGradingScale = New ReaLTaiizor.Controls.Button()
        RoundedPanel1.SuspendLayout()
        lblSem1Value.SuspendLayout()
        RoundedPanel3.SuspendLayout()
        RoundedPanel2.SuspendLayout()
        SuspendLayout()
        ' 
        ' RoundedPanel1
        ' 
        RoundedPanel1.ActiveBorderColor = Color.Transparent
        RoundedPanel1.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel1.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        RoundedPanel1.BorderThickness = 1
        RoundedPanel1.Controls.Add(lblSYEndValue)
        RoundedPanel1.Controls.Add(lblSYStartValue)
        RoundedPanel1.Controls.Add(btnEditSchoolYear)
        RoundedPanel1.Controls.Add(btnNewSchoolYear)
        RoundedPanel1.Controls.Add(Label6)
        RoundedPanel1.Controls.Add(Label5)
        RoundedPanel1.Controls.Add(cboSchoolYear)
        RoundedPanel1.Controls.Add(Label4)
        RoundedPanel1.Controls.Add(Label1)
        RoundedPanel1.Location = New Point(13, 17)
        RoundedPanel1.Name = "RoundedPanel1"
        RoundedPanel1.Radius = 20
        RoundedPanel1.Size = New Size(621, 343)
        RoundedPanel1.TabIndex = 0
        ' 
        ' lblSYEndValue
        ' 
        lblSYEndValue.AutoSize = True
        lblSYEndValue.Location = New Point(325, 197)
        lblSYEndValue.Name = "lblSYEndValue"
        lblSYEndValue.Size = New Size(53, 20)
        lblSYEndValue.TabIndex = 14
        lblSYEndValue.Text = "Label8"
        ' 
        ' lblSYStartValue
        ' 
        lblSYStartValue.AutoSize = True
        lblSYStartValue.Location = New Point(325, 147)
        lblSYStartValue.Name = "lblSYStartValue"
        lblSYStartValue.Size = New Size(53, 20)
        lblSYStartValue.TabIndex = 13
        lblSYStartValue.Text = "Label7"
        ' 
        ' btnEditSchoolYear
        ' 
        btnEditSchoolYear.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnEditSchoolYear.BackColor = Color.Transparent
        btnEditSchoolYear.BackgroundImageLayout = ImageLayout.Zoom
        btnEditSchoolYear.BorderColor = Color.Transparent
        btnEditSchoolYear.EnteredBorderColor = Color.Transparent
        btnEditSchoolYear.EnteredColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnEditSchoolYear.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnEditSchoolYear.Image = Nothing
        btnEditSchoolYear.ImageAlign = ContentAlignment.MiddleLeft
        btnEditSchoolYear.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnEditSchoolYear.Location = New Point(349, 278)
        btnEditSchoolYear.Name = "btnEditSchoolYear"
        btnEditSchoolYear.PressedBorderColor = Color.Transparent
        btnEditSchoolYear.PressedColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnEditSchoolYear.Size = New Size(127, 35)
        btnEditSchoolYear.TabIndex = 12
        btnEditSchoolYear.Text = "Edit"
        btnEditSchoolYear.TextAlignment = StringAlignment.Center
        btnEditSchoolYear.UseWaitCursor = True
        ' 
        ' btnNewSchoolYear
        ' 
        btnNewSchoolYear.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnNewSchoolYear.BackColor = Color.Transparent
        btnNewSchoolYear.BackgroundImageLayout = ImageLayout.Zoom
        btnNewSchoolYear.BorderColor = Color.Transparent
        btnNewSchoolYear.EnteredBorderColor = Color.Transparent
        btnNewSchoolYear.EnteredColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnNewSchoolYear.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnNewSchoolYear.Image = Nothing
        btnNewSchoolYear.ImageAlign = ContentAlignment.MiddleLeft
        btnNewSchoolYear.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnNewSchoolYear.Location = New Point(132, 278)
        btnNewSchoolYear.Name = "btnNewSchoolYear"
        btnNewSchoolYear.PressedBorderColor = Color.Transparent
        btnNewSchoolYear.PressedColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnNewSchoolYear.Size = New Size(200, 35)
        btnNewSchoolYear.TabIndex = 11
        btnNewSchoolYear.Text = "New School Year"
        btnNewSchoolYear.TextAlignment = StringAlignment.Center
        btnNewSchoolYear.UseWaitCursor = True
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.Font = New Font("Century Gothic", 10.2F)
        Label6.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label6.Location = New Point(140, 196)
        Label6.Name = "Label6"
        Label6.Size = New Size(48, 21)
        Label6.TabIndex = 4
        Label6.Text = "Ends"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Font = New Font("Century Gothic", 10.2F)
        Label5.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label5.Location = New Point(140, 146)
        Label5.Name = "Label5"
        Label5.Size = New Size(56, 21)
        Label5.TabIndex = 3
        Label5.Text = "Starts"
        ' 
        ' cboSchoolYear
        ' 
        cboSchoolYear.Font = New Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cboSchoolYear.FormattingEnabled = True
        cboSchoolYear.Location = New Point(329, 94)
        cboSchoolYear.Name = "cboSchoolYear"
        cboSchoolYear.Size = New Size(179, 28)
        cboSchoolYear.TabIndex = 2
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Font = New Font("Century Gothic", 10.2F)
        Label4.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label4.Location = New Point(140, 94)
        Label4.Name = "Label4"
        Label4.Size = New Size(108, 21)
        Label4.TabIndex = 1
        Label4.Text = "School Year"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Century Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        Label1.Location = New Point(225, 29)
        Label1.Name = "Label1"
        Label1.Size = New Size(148, 23)
        Label1.TabIndex = 0
        Label1.Text = "SCHOOL YEAR"
        ' 
        ' lblSem1Value
        ' 
        lblSem1Value.ActiveBorderColor = Color.Transparent
        lblSem1Value.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        lblSem1Value.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        lblSem1Value.BorderThickness = 1
        lblSem1Value.Controls.Add(btnEditSemesters)
        lblSem1Value.Controls.Add(Label12)
        lblSem1Value.Controls.Add(lblBreakValue)
        lblSem1Value.Controls.Add(lblSem2Value)
        lblSem1Value.Controls.Add(Label7)
        lblSem1Value.Controls.Add(Label2)
        lblSem1Value.Controls.Add(Label8)
        lblSem1Value.Controls.Add(Label9)
        lblSem1Value.Location = New Point(650, 17)
        lblSem1Value.Name = "lblSem1Value"
        lblSem1Value.Radius = 20
        lblSem1Value.Size = New Size(621, 343)
        lblSem1Value.TabIndex = 1
        ' 
        ' btnEditSemesters
        ' 
        btnEditSemesters.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnEditSemesters.BackColor = Color.Transparent
        btnEditSemesters.BackgroundImageLayout = ImageLayout.Zoom
        btnEditSemesters.BorderColor = Color.Transparent
        btnEditSemesters.EnteredBorderColor = Color.Transparent
        btnEditSemesters.EnteredColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnEditSemesters.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnEditSemesters.Image = Nothing
        btnEditSemesters.ImageAlign = ContentAlignment.MiddleLeft
        btnEditSemesters.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnEditSemesters.Location = New Point(214, 278)
        btnEditSemesters.Name = "btnEditSemesters"
        btnEditSemesters.PressedBorderColor = Color.Transparent
        btnEditSemesters.PressedColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnEditSemesters.Size = New Size(200, 35)
        btnEditSemesters.TabIndex = 15
        btnEditSemesters.Text = "Edit Semesters"
        btnEditSemesters.TextAlignment = StringAlignment.Center
        btnEditSemesters.UseWaitCursor = True
        ' 
        ' Label12
        ' 
        Label12.AutoSize = True
        Label12.Location = New Point(323, 94)
        Label12.Name = "Label12"
        Label12.Size = New Size(53, 20)
        Label12.TabIndex = 18
        Label12.Text = "Label7"
        ' 
        ' lblBreakValue
        ' 
        lblBreakValue.AutoSize = True
        lblBreakValue.Location = New Point(323, 197)
        lblBreakValue.Name = "lblBreakValue"
        lblBreakValue.Size = New Size(53, 20)
        lblBreakValue.TabIndex = 16
        lblBreakValue.Text = "Label8"
        ' 
        ' lblSem2Value
        ' 
        lblSem2Value.AutoSize = True
        lblSem2Value.Location = New Point(323, 147)
        lblSem2Value.Name = "lblSem2Value"
        lblSem2Value.Size = New Size(53, 20)
        lblSem2Value.TabIndex = 15
        lblSem2Value.Text = "Label7"
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.Font = New Font("Century Gothic", 10.2F)
        Label7.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label7.Location = New Point(120, 196)
        Label7.Name = "Label7"
        Label7.Size = New Size(137, 21)
        Label7.TabIndex = 17
        Label7.Text = "Semester Break"
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Century Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        Label2.Location = New Point(256, 29)
        Label2.Name = "Label2"
        Label2.Size = New Size(98, 23)
        Label2.TabIndex = 1
        Label2.Text = "SEMESTER"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Font = New Font("Century Gothic", 10.2F)
        Label8.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label8.Location = New Point(120, 147)
        Label8.Name = "Label8"
        Label8.Size = New Size(121, 21)
        Label8.TabIndex = 16
        Label8.Text = "2nd Semester"
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.Font = New Font("Century Gothic", 10.2F)
        Label9.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label9.Location = New Point(120, 94)
        Label9.Name = "Label9"
        Label9.Size = New Size(113, 21)
        Label9.TabIndex = 15
        Label9.Text = "1st Semester"
        ' 
        ' RoundedPanel3
        ' 
        RoundedPanel3.ActiveBorderColor = Color.Transparent
        RoundedPanel3.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel3.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        RoundedPanel3.BorderThickness = 1
        RoundedPanel3.Controls.Add(btnSetActive)
        RoundedPanel3.Controls.Add(btnEditEncoding)
        RoundedPanel3.Controls.Add(cboEncodingTerm)
        RoundedPanel3.Controls.Add(lblActiveTermValue)
        RoundedPanel3.Controls.Add(Label18)
        RoundedPanel3.Controls.Add(lblLockStatusValue)
        RoundedPanel3.Controls.Add(Label20)
        RoundedPanel3.Controls.Add(Label3)
        RoundedPanel3.Controls.Add(Label15)
        RoundedPanel3.Controls.Add(blEncodingStartValue)
        RoundedPanel3.Controls.Add(Label16)
        RoundedPanel3.Controls.Add(Label14)
        RoundedPanel3.Controls.Add(lblDeadlineValue)
        RoundedPanel3.Location = New Point(1287, 17)
        RoundedPanel3.Name = "RoundedPanel3"
        RoundedPanel3.Radius = 20
        RoundedPanel3.Size = New Size(621, 343)
        RoundedPanel3.TabIndex = 2
        ' 
        ' btnSetActive
        ' 
        btnSetActive.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnSetActive.BackColor = Color.Transparent
        btnSetActive.BackgroundImageLayout = ImageLayout.Zoom
        btnSetActive.BorderColor = Color.Transparent
        btnSetActive.EnteredBorderColor = Color.Transparent
        btnSetActive.EnteredColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnSetActive.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnSetActive.Image = Nothing
        btnSetActive.ImageAlign = ContentAlignment.MiddleLeft
        btnSetActive.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnSetActive.Location = New Point(323, 278)
        btnSetActive.Name = "btnSetActive"
        btnSetActive.PressedBorderColor = Color.Transparent
        btnSetActive.PressedColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnSetActive.Size = New Size(179, 35)
        btnSetActive.TabIndex = 30
        btnSetActive.Text = "Set as Active"
        btnSetActive.TextAlignment = StringAlignment.Center
        btnSetActive.UseWaitCursor = True
        ' 
        ' btnEditEncoding
        ' 
        btnEditEncoding.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnEditEncoding.BackColor = Color.Transparent
        btnEditEncoding.BackgroundImageLayout = ImageLayout.Zoom
        btnEditEncoding.BorderColor = Color.Transparent
        btnEditEncoding.EnteredBorderColor = Color.Transparent
        btnEditEncoding.EnteredColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnEditEncoding.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnEditEncoding.Image = Nothing
        btnEditEncoding.ImageAlign = ContentAlignment.MiddleLeft
        btnEditEncoding.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnEditEncoding.Location = New Point(117, 278)
        btnEditEncoding.Name = "btnEditEncoding"
        btnEditEncoding.PressedBorderColor = Color.Transparent
        btnEditEncoding.PressedColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        btnEditEncoding.Size = New Size(200, 35)
        btnEditEncoding.TabIndex = 29
        btnEditEncoding.Text = "Edit Deadline"
        btnEditEncoding.TextAlignment = StringAlignment.Center
        btnEditEncoding.UseWaitCursor = True
        ' 
        ' cboEncodingTerm
        ' 
        cboEncodingTerm.Font = New Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cboEncodingTerm.FormattingEnabled = True
        cboEncodingTerm.Location = New Point(342, 89)
        cboEncodingTerm.Name = "cboEncodingTerm"
        cboEncodingTerm.Size = New Size(179, 28)
        cboEncodingTerm.TabIndex = 15
        ' 
        ' lblActiveTermValue
        ' 
        lblActiveTermValue.AutoSize = True
        lblActiveTermValue.Location = New Point(339, 234)
        lblActiveTermValue.Name = "lblActiveTermValue"
        lblActiveTermValue.Size = New Size(53, 20)
        lblActiveTermValue.TabIndex = 26
        lblActiveTermValue.Text = "Label8"
        ' 
        ' Label18
        ' 
        Label18.AutoSize = True
        Label18.Font = New Font("Century Gothic", 10.2F)
        Label18.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label18.Location = New Point(106, 198)
        Label18.Name = "Label18"
        Label18.Size = New Size(61, 21)
        Label18.TabIndex = 27
        Label18.Text = "Status"
        ' 
        ' lblLockStatusValue
        ' 
        lblLockStatusValue.AutoSize = True
        lblLockStatusValue.Location = New Point(339, 198)
        lblLockStatusValue.Name = "lblLockStatusValue"
        lblLockStatusValue.Size = New Size(53, 20)
        lblLockStatusValue.TabIndex = 25
        lblLockStatusValue.Text = "Label7"
        ' 
        ' Label20
        ' 
        Label20.AutoSize = True
        Label20.Font = New Font("Century Gothic", 10.2F)
        Label20.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label20.Location = New Point(106, 233)
        Label20.Name = "Label20"
        Label20.Size = New Size(110, 21)
        Label20.TabIndex = 28
        Label20.Text = "Active Term"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Century Gothic", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label3.ForeColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        Label3.Location = New Point(225, 29)
        Label3.Name = "Label3"
        Label3.Size = New Size(194, 23)
        Label3.TabIndex = 2
        Label3.Text = "GRADE ENCODING"
        ' 
        ' Label15
        ' 
        Label15.AutoSize = True
        Label15.Font = New Font("Century Gothic", 10.2F)
        Label15.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label15.Location = New Point(106, 128)
        Label15.Name = "Label15"
        Label15.Size = New Size(131, 21)
        Label15.TabIndex = 22
        Label15.Text = "Encoding Start"
        ' 
        ' blEncodingStartValue
        ' 
        blEncodingStartValue.AutoSize = True
        blEncodingStartValue.Location = New Point(339, 128)
        blEncodingStartValue.Name = "blEncodingStartValue"
        blEncodingStartValue.Size = New Size(53, 20)
        blEncodingStartValue.TabIndex = 19
        blEncodingStartValue.Text = "Label7"
        ' 
        ' Label16
        ' 
        Label16.AutoSize = True
        Label16.Font = New Font("Century Gothic", 10.2F)
        Label16.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label16.Location = New Point(106, 94)
        Label16.Name = "Label16"
        Label16.Size = New Size(85, 21)
        Label16.TabIndex = 20
        Label16.Text = "Semester"
        ' 
        ' Label14
        ' 
        Label14.AutoSize = True
        Label14.Font = New Font("Century Gothic", 10.2F)
        Label14.ForeColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        Label14.Location = New Point(106, 163)
        Label14.Name = "Label14"
        Label14.Size = New Size(167, 21)
        Label14.TabIndex = 23
        Label14.Text = "Encoding Deadline"
        ' 
        ' lblDeadlineValue
        ' 
        lblDeadlineValue.AutoSize = True
        lblDeadlineValue.Location = New Point(339, 164)
        lblDeadlineValue.Name = "lblDeadlineValue"
        lblDeadlineValue.Size = New Size(53, 20)
        lblDeadlineValue.TabIndex = 21
        lblDeadlineValue.Text = "Label8"
        ' 
        ' RoundedPanel2
        ' 
        RoundedPanel2.ActiveBorderColor = Color.Transparent
        RoundedPanel2.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel2.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        RoundedPanel2.BorderThickness = 1
        RoundedPanel2.Controls.Add(EditGradingScale)
        RoundedPanel2.Location = New Point(13, 376)
        RoundedPanel2.Name = "RoundedPanel2"
        RoundedPanel2.Radius = 20
        RoundedPanel2.Size = New Size(1895, 538)
        RoundedPanel2.TabIndex = 19
        ' 
        ' EditGradingScale
        ' 
        EditGradingScale.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        EditGradingScale.BackColor = Color.Transparent
        EditGradingScale.BackgroundImageLayout = ImageLayout.Zoom
        EditGradingScale.BorderColor = Color.Transparent
        EditGradingScale.EnteredBorderColor = Color.Transparent
        EditGradingScale.EnteredColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        EditGradingScale.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        EditGradingScale.Image = Nothing
        EditGradingScale.ImageAlign = ContentAlignment.MiddleLeft
        EditGradingScale.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        EditGradingScale.Location = New Point(18, 483)
        EditGradingScale.Name = "EditGradingScale"
        EditGradingScale.PressedBorderColor = Color.Transparent
        EditGradingScale.PressedColor = Color.FromArgb(CByte(23), CByte(61), CByte(50))
        EditGradingScale.Size = New Size(200, 35)
        EditGradingScale.TabIndex = 12
        EditGradingScale.Text = "Edit Grading Scale"
        EditGradingScale.TextAlignment = StringAlignment.Center
        EditGradingScale.UseWaitCursor = True
        ' 
        ' ucGradingConfig
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(RoundedPanel2)
        Controls.Add(RoundedPanel3)
        Controls.Add(lblSem1Value)
        Controls.Add(RoundedPanel1)
        Name = "ucGradingConfig"
        Size = New Size(1924, 917)
        RoundedPanel1.ResumeLayout(False)
        RoundedPanel1.PerformLayout()
        lblSem1Value.ResumeLayout(False)
        lblSem1Value.PerformLayout()
        RoundedPanel3.ResumeLayout(False)
        RoundedPanel3.PerformLayout()
        RoundedPanel2.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
    Friend WithEvents Label4 As Label
    Friend WithEvents sa As Label
    Friend WithEvents lblSYStartValue As Label
    Friend WithEvents RoundedPanel1 As RoundedPanel
    Friend WithEvents lblSem1Value As RoundedPanel
    Friend WithEvents RoundedPanel3 As RoundedPanel
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents cboSchoolYear As ComboBox
    Friend WithEvents Label6 As Label
    Friend WithEvents btnEditSchoolYear As ReaLTaiizor.Controls.Button
    Friend WithEvents btnNewSchoolYear As ReaLTaiizor.Controls.Button
    Friend WithEvents lblSYEndValue As Label
    Friend WithEvents Label12 As Label
    Friend WithEvents lblBreakValue As Label
    Friend WithEvents lblSem2Value As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents btnEditSemesters As ReaLTaiizor.Controls.Button
    Friend WithEvents Label10 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents Label15 As Label
    Friend WithEvents blEncodingStartValue As Label
    Friend WithEvents Label16 As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents lblDeadlineValue As Label
    Friend WithEvents cboEncodingTerm As ComboBox
    Friend WithEvents lblActiveTermValue As Label
    Friend WithEvents Label18 As Label
    Friend WithEvents lblLockStatusValue As Label
    Friend WithEvents Label20 As Label
    Friend WithEvents btnSetActive As ReaLTaiizor.Controls.Button
    Friend WithEvents btnEditEncoding As ReaLTaiizor.Controls.Button
    Friend WithEvents RoundedPanel2 As RoundedPanel
    Friend WithEvents EditGradingScale As ReaLTaiizor.Controls.Button


End Class
