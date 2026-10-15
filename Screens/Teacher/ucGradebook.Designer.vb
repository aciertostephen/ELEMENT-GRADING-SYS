<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucGradebook
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
        Dim DataGridViewCellStyle3 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As DataGridViewCellStyle = New DataGridViewCellStyle()
        pnlTop = New RoundedPanel()
        btnBack = New ReaLTaiizor.Controls.Button()
        lblGradebook = New Label()
        tblMainGBook = New TableLayoutPanel()
        pnlRubric = New RoundedPanel()
        tblMainRubric = New TableLayoutPanel()
        pnlBottomActions = New Panel()
        btnSaveChanges = New ReaLTaiizor.Controls.Button()
        btnSubmit = New ReaLTaiizor.Controls.Button()
        btnExportExcel = New ReaLTaiizor.Controls.Button()
        dgvGradebook = New DataGridView()
        number = New DataGridViewTextBoxColumn()
        studname = New DataGridViewTextBoxColumn()
        status = New DataGridViewTextBoxColumn()
        part = New DataGridViewTextBoxColumn()
        gra = New DataGridViewTextBoxColumn()
        EXTRA = New DataGridViewTextBoxColumn()
        Seatwork = New DataGridViewTextBoxColumn()
        quiz1 = New DataGridViewTextBoxColumn()
        GRAQ1 = New DataGridViewTextBoxColumn()
        quiz2 = New DataGridViewTextBoxColumn()
        GRAQ2 = New DataGridViewTextBoxColumn()
        quizp = New DataGridViewTextBoxColumn()
        project = New DataGridViewTextBoxColumn()
        graproject = New DataGridViewTextBoxColumn()
        exam = New DataGridViewTextBoxColumn()
        GRAexam = New DataGridViewTextBoxColumn()
        majorexam = New DataGridViewTextBoxColumn()
        grademajorexam = New DataGridViewTextBoxColumn()
        pnlTopGradeBook = New Label()
        btnSaveConfig = New ReaLTaiizor.Controls.Button()
        btnAddColumn = New ReaLTaiizor.Controls.Button()
        pnlTopRubricBG = New RoundedPanel()
        btnConfigureWeights = New Button()
        pnlTop.SuspendLayout()
        tblMainGBook.SuspendLayout()
        pnlRubric.SuspendLayout()
        tblMainRubric.SuspendLayout()
        pnlBottomActions.SuspendLayout()
        CType(dgvGradebook, ComponentModel.ISupportInitialize).BeginInit()
        pnlTopRubricBG.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlTop
        ' 
        pnlTop.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTop.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTop.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTop.BorderThickness = 1
        pnlTop.Controls.Add(btnBack)
        pnlTop.Controls.Add(lblGradebook)
        pnlTop.Dock = DockStyle.Fill
        pnlTop.Location = New Point(20, 20)
        pnlTop.Margin = New Padding(0)
        pnlTop.Name = "pnlTop"
        pnlTop.Radius = 20
        pnlTop.Size = New Size(1884, 75)
        pnlTop.TabIndex = 0
        ' 
        ' btnBack
        ' 
        btnBack.BackColor = Color.Transparent
        btnBack.BackgroundImageLayout = ImageLayout.Zoom
        btnBack.BorderColor = Color.FromArgb(CByte(141), CByte(161), CByte(180))
        btnBack.EnteredBorderColor = Color.Empty
        btnBack.EnteredColor = Color.FromArgb(CByte(141), CByte(161), CByte(180))
        btnBack.Font = New Font("Century Gothic", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnBack.Image = Nothing
        btnBack.ImageAlign = ContentAlignment.MiddleLeft
        btnBack.InactiveColor = Color.FromArgb(CByte(141), CByte(161), CByte(180))
        btnBack.Location = New Point(1710, 18)
        btnBack.Name = "btnBack"
        btnBack.PressedBorderColor = Color.Transparent
        btnBack.PressedColor = Color.FromArgb(CByte(141), CByte(161), CByte(180))
        btnBack.Size = New Size(149, 39)
        btnBack.TabIndex = 8
        btnBack.Text = "←      Back"
        btnBack.TextAlignment = StringAlignment.Center
        btnBack.UseWaitCursor = True
        ' 
        ' lblGradebook
        ' 
        lblGradebook.AutoSize = True
        lblGradebook.BackColor = Color.Transparent
        lblGradebook.Font = New Font("Century Gothic", 13.2000008F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblGradebook.ForeColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        lblGradebook.Location = New Point(29, 22)
        lblGradebook.Name = "lblGradebook"
        lblGradebook.Size = New Size(154, 27)
        lblGradebook.TabIndex = 7
        lblGradebook.Text = "GRADEBOOK"
        ' 
        ' tblMainGBook
        ' 
        tblMainGBook.ColumnCount = 1
        tblMainGBook.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tblMainGBook.Controls.Add(pnlTop, 0, 0)
        tblMainGBook.Controls.Add(pnlRubric, 0, 1)
        tblMainGBook.Dock = DockStyle.Fill
        tblMainGBook.Location = New Point(0, 0)
        tblMainGBook.Name = "tblMainGBook"
        tblMainGBook.Padding = New Padding(20)
        tblMainGBook.RowCount = 2
        tblMainGBook.RowStyles.Add(New RowStyle(SizeType.Absolute, 75F))
        tblMainGBook.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tblMainGBook.Size = New Size(1924, 917)
        tblMainGBook.TabIndex = 1
        ' 
        ' pnlRubric
        ' 
        pnlRubric.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlRubric.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlRubric.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlRubric.BorderThickness = 1
        pnlRubric.Controls.Add(tblMainRubric)
        pnlRubric.Dock = DockStyle.Fill
        pnlRubric.Location = New Point(20, 115)
        pnlRubric.Margin = New Padding(0, 20, 10, 0)
        pnlRubric.Name = "pnlRubric"
        pnlRubric.Radius = 20
        pnlRubric.Size = New Size(1874, 782)
        pnlRubric.TabIndex = 1
        ' 
        ' tblMainRubric
        ' 
        tblMainRubric.ColumnCount = 1
        tblMainRubric.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tblMainRubric.Controls.Add(pnlBottomActions, 0, 2)
        tblMainRubric.Controls.Add(dgvGradebook, 0, 1)
        tblMainRubric.Controls.Add(pnlTopRubricBG, 0, 0)
        tblMainRubric.Dock = DockStyle.Fill
        tblMainRubric.Location = New Point(0, 0)
        tblMainRubric.Name = "tblMainRubric"
        tblMainRubric.RowCount = 3
        tblMainRubric.RowStyles.Add(New RowStyle(SizeType.Absolute, 125F))
        tblMainRubric.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tblMainRubric.RowStyles.Add(New RowStyle(SizeType.Absolute, 75F))
        tblMainRubric.Size = New Size(1874, 782)
        tblMainRubric.TabIndex = 0
        ' 
        ' pnlBottomActions
        ' 
        pnlBottomActions.BackColor = Color.Transparent
        pnlBottomActions.Controls.Add(btnSaveChanges)
        pnlBottomActions.Controls.Add(btnSubmit)
        pnlBottomActions.Controls.Add(btnExportExcel)
        pnlBottomActions.Dock = DockStyle.Fill
        pnlBottomActions.Location = New Point(3, 710)
        pnlBottomActions.Name = "pnlBottomActions"
        pnlBottomActions.Size = New Size(1868, 69)
        pnlBottomActions.TabIndex = 18
        ' 
        ' btnSaveChanges
        ' 
        btnSaveChanges.BackColor = Color.Transparent
        btnSaveChanges.BackgroundImageLayout = ImageLayout.Zoom
        btnSaveChanges.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnSaveChanges.EnteredBorderColor = Color.Empty
        btnSaveChanges.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnSaveChanges.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnSaveChanges.Image = Nothing
        btnSaveChanges.ImageAlign = ContentAlignment.MiddleLeft
        btnSaveChanges.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnSaveChanges.Location = New Point(1673, 15)
        btnSaveChanges.Name = "btnSaveChanges"
        btnSaveChanges.PressedBorderColor = Color.Transparent
        btnSaveChanges.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnSaveChanges.Size = New Size(178, 40)
        btnSaveChanges.TabIndex = 20
        btnSaveChanges.Text = "Save Changes"
        btnSaveChanges.TextAlignment = StringAlignment.Center
        btnSaveChanges.UseWaitCursor = True
        ' 
        ' btnSubmit
        ' 
        btnSubmit.BackColor = Color.Transparent
        btnSubmit.BackgroundImageLayout = ImageLayout.Zoom
        btnSubmit.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnSubmit.EnteredBorderColor = Color.Empty
        btnSubmit.EnteredColor = Color.FromArgb(CByte(220), CByte(70), CByte(70))
        btnSubmit.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnSubmit.Image = Nothing
        btnSubmit.ImageAlign = ContentAlignment.MiddleLeft
        btnSubmit.InactiveColor = Color.FromArgb(CByte(200), CByte(51), CByte(51))
        btnSubmit.Location = New Point(201, 15)
        btnSubmit.Name = "btnSubmit"
        btnSubmit.PressedBorderColor = Color.Transparent
        btnSubmit.PressedColor = Color.FromArgb(CByte(165), CByte(38), CByte(38))
        btnSubmit.Size = New Size(178, 40)
        btnSubmit.TabIndex = 19
        btnSubmit.Text = "Export"
        btnSubmit.TextAlignment = StringAlignment.Center
        btnSubmit.UseWaitCursor = True
        ' 
        ' btnExportExcel
        ' 
        btnExportExcel.BackColor = Color.Transparent
        btnExportExcel.BackgroundImageLayout = ImageLayout.Zoom
        btnExportExcel.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnExportExcel.EnteredBorderColor = Color.Empty
        btnExportExcel.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnExportExcel.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnExportExcel.Image = Nothing
        btnExportExcel.ImageAlign = ContentAlignment.MiddleLeft
        btnExportExcel.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnExportExcel.Location = New Point(17, 15)
        btnExportExcel.Name = "btnExportExcel"
        btnExportExcel.PressedBorderColor = Color.Transparent
        btnExportExcel.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnExportExcel.Size = New Size(178, 40)
        btnExportExcel.TabIndex = 18
        btnExportExcel.Text = "Export Excel"
        btnExportExcel.TextAlignment = StringAlignment.Center
        btnExportExcel.UseWaitCursor = True
        ' 
        ' dgvGradebook
        ' 
        dgvGradebook.AllowUserToAddRows = False
        dgvGradebook.AllowUserToDeleteRows = False
        dgvGradebook.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvGradebook.BackgroundColor = Color.White
        dgvGradebook.BorderStyle = BorderStyle.None
        DataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle3.BackColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        DataGridViewCellStyle3.Font = New Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle3.ForeColor = Color.White
        DataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle3.SelectionForeColor = Color.White
        DataGridViewCellStyle3.WrapMode = DataGridViewTriState.True
        dgvGradebook.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle3
        dgvGradebook.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvGradebook.Columns.AddRange(New DataGridViewColumn() {number, studname, status, part, gra, EXTRA, Seatwork, quiz1, GRAQ1, quiz2, GRAQ2, quizp, project, graproject, exam, GRAexam, majorexam, grademajorexam})
        DataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle4.BackColor = Color.White
        DataGridViewCellStyle4.Font = New Font("Segoe UI", 9F)
        DataGridViewCellStyle4.ForeColor = SystemColors.ControlText
        DataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle4.WrapMode = DataGridViewTriState.False
        dgvGradebook.DefaultCellStyle = DataGridViewCellStyle4
        dgvGradebook.Dock = DockStyle.Fill
        dgvGradebook.GridColor = Color.White
        dgvGradebook.Location = New Point(20, 125)
        dgvGradebook.Margin = New Padding(20, 0, 20, 0)
        dgvGradebook.Name = "dgvGradebook"
        dgvGradebook.RowHeadersVisible = False
        dgvGradebook.RowHeadersWidth = 51
        dgvGradebook.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvGradebook.Size = New Size(1834, 582)
        dgvGradebook.TabIndex = 3
        ' 
        ' number
        ' 
        number.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        number.HeaderText = "NO."
        number.MinimumWidth = 6
        number.Name = "number"
        number.Width = 66
        ' 
        ' studname
        ' 
        studname.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        studname.HeaderText = "STUDENT NAME"
        studname.MinimumWidth = 6
        studname.Name = "studname"
        studname.Width = 145
        ' 
        ' status
        ' 
        status.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        status.HeaderText = "DRP"
        status.MinimumWidth = 6
        status.Name = "status"
        status.Width = 67
        ' 
        ' part
        ' 
        part.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        part.HeaderText = "PART"
        part.MinimumWidth = 6
        part.Name = "part"
        part.Width = 72
        ' 
        ' gra
        ' 
        gra.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        gra.HeaderText = "GRA"
        gra.MinimumWidth = 6
        gra.Name = "gra"
        gra.Width = 71
        ' 
        ' EXTRA
        ' 
        EXTRA.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        EXTRA.HeaderText = "EXTRA %"
        EXTRA.MinimumWidth = 6
        EXTRA.Name = "EXTRA"
        EXTRA.Width = 96
        ' 
        ' Seatwork
        ' 
        Seatwork.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        Seatwork.HeaderText = "SW %"
        Seatwork.MinimumWidth = 6
        Seatwork.Name = "Seatwork"
        Seatwork.Width = 76
        ' 
        ' quiz1
        ' 
        quiz1.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        quiz1.HeaderText = "Q1"
        quiz1.MinimumWidth = 6
        quiz1.Name = "quiz1"
        quiz1.Width = 59
        ' 
        ' GRAQ1
        ' 
        GRAQ1.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        GRAQ1.HeaderText = "GRA"
        GRAQ1.MinimumWidth = 6
        GRAQ1.Name = "GRAQ1"
        GRAQ1.Width = 71
        ' 
        ' quiz2
        ' 
        quiz2.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        quiz2.HeaderText = "Q2"
        quiz2.MinimumWidth = 6
        quiz2.Name = "quiz2"
        quiz2.Width = 59
        ' 
        ' GRAQ2
        ' 
        GRAQ2.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        GRAQ2.HeaderText = "GRA"
        GRAQ2.MinimumWidth = 6
        GRAQ2.Name = "GRAQ2"
        GRAQ2.Width = 71
        ' 
        ' quizp
        ' 
        quizp.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        quizp.HeaderText = "QUIZ"
        quizp.MinimumWidth = 6
        quizp.Name = "quizp"
        quizp.Width = 74
        ' 
        ' project
        ' 
        project.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        project.HeaderText = "PROJECT %"
        project.MinimumWidth = 6
        project.Name = "project"
        project.Width = 117
        ' 
        ' graproject
        ' 
        graproject.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        graproject.HeaderText = "GRA"
        graproject.MinimumWidth = 6
        graproject.Name = "graproject"
        graproject.Width = 71
        ' 
        ' exam
        ' 
        exam.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        exam.HeaderText = "EXAM"
        exam.MinimumWidth = 6
        exam.Name = "exam"
        exam.Width = 81
        ' 
        ' GRAexam
        ' 
        GRAexam.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        GRAexam.HeaderText = "GRA"
        GRAexam.MinimumWidth = 6
        GRAexam.Name = "GRAexam"
        GRAexam.Width = 71
        ' 
        ' majorexam
        ' 
        majorexam.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        majorexam.HeaderText = "MAJOR EXAM"
        majorexam.MinimumWidth = 6
        majorexam.Name = "majorexam"
        majorexam.Width = 140
        ' 
        ' grademajorexam
        ' 
        grademajorexam.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
        grademajorexam.HeaderText = "GRADE"
        grademajorexam.MinimumWidth = 6
        grademajorexam.Name = "grademajorexam"
        grademajorexam.Width = 90
        ' 
        ' pnlTopGradeBook
        ' 
        pnlTopGradeBook.AutoSize = True
        pnlTopGradeBook.BackColor = Color.Transparent
        pnlTopGradeBook.Font = New Font("Century Gothic", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        pnlTopGradeBook.ForeColor = Color.FromArgb(CByte(126), CByte(126), CByte(126))
        pnlTopGradeBook.Location = New Point(22, 31)
        pnlTopGradeBook.Name = "pnlTopGradeBook"
        pnlTopGradeBook.Size = New Size(205, 23)
        pnlTopGradeBook.TabIndex = 9
        pnlTopGradeBook.Text = "Active Gradebook :"
        ' 
        ' btnSaveConfig
        ' 
        btnSaveConfig.BackColor = Color.Transparent
        btnSaveConfig.BackgroundImageLayout = ImageLayout.Zoom
        btnSaveConfig.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnSaveConfig.EnteredBorderColor = Color.Empty
        btnSaveConfig.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnSaveConfig.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnSaveConfig.Image = Nothing
        btnSaveConfig.ImageAlign = ContentAlignment.MiddleLeft
        btnSaveConfig.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnSaveConfig.Location = New Point(1626, 24)
        btnSaveConfig.Name = "btnSaveConfig"
        btnSaveConfig.PressedBorderColor = Color.Transparent
        btnSaveConfig.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnSaveConfig.Size = New Size(184, 40)
        btnSaveConfig.TabIndex = 16
        btnSaveConfig.Text = "Save Gradebook"
        btnSaveConfig.TextAlignment = StringAlignment.Center
        btnSaveConfig.UseWaitCursor = True
        ' 
        ' btnAddColumn
        ' 
        btnAddColumn.BackColor = Color.Transparent
        btnAddColumn.BackgroundImageLayout = ImageLayout.Zoom
        btnAddColumn.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnAddColumn.EnteredBorderColor = Color.Empty
        btnAddColumn.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnAddColumn.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnAddColumn.Image = Nothing
        btnAddColumn.ImageAlign = ContentAlignment.MiddleLeft
        btnAddColumn.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnAddColumn.Location = New Point(1435, 24)
        btnAddColumn.Name = "btnAddColumn"
        btnAddColumn.PressedBorderColor = Color.Transparent
        btnAddColumn.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnAddColumn.Size = New Size(184, 40)
        btnAddColumn.TabIndex = 17
        btnAddColumn.Text = "+   Add Column"
        btnAddColumn.TextAlignment = StringAlignment.Center
        btnAddColumn.UseWaitCursor = True
        ' 
        ' pnlTopRubricBG
        ' 
        pnlTopRubricBG.ActiveBorderColor = Color.White
        pnlTopRubricBG.BackColor = Color.White
        pnlTopRubricBG.BorderColor = Color.White
        pnlTopRubricBG.BorderThickness = 1
        pnlTopRubricBG.Controls.Add(btnConfigureWeights)
        pnlTopRubricBG.Controls.Add(btnAddColumn)
        pnlTopRubricBG.Controls.Add(btnSaveConfig)
        pnlTopRubricBG.Controls.Add(pnlTopGradeBook)
        pnlTopRubricBG.Dock = DockStyle.Fill
        pnlTopRubricBG.Location = New Point(20, 20)
        pnlTopRubricBG.Margin = New Padding(20)
        pnlTopRubricBG.Name = "pnlTopRubricBG"
        pnlTopRubricBG.Radius = 20
        pnlTopRubricBG.Size = New Size(1834, 85)
        pnlTopRubricBG.TabIndex = 0
        ' 
        ' btnConfigureWeights
        ' 
        btnConfigureWeights.Location = New Point(233, 30)
        btnConfigureWeights.Name = "btnConfigureWeights"
        btnConfigureWeights.Size = New Size(162, 29)
        btnConfigureWeights.TabIndex = 18
        btnConfigureWeights.Text = "Set Weights"
        btnConfigureWeights.UseVisualStyleBackColor = True
        ' 
        ' ucGradebook
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(tblMainGBook)
        Name = "ucGradebook"
        Size = New Size(1924, 917)
        pnlTop.ResumeLayout(False)
        pnlTop.PerformLayout()
        tblMainGBook.ResumeLayout(False)
        pnlRubric.ResumeLayout(False)
        tblMainRubric.ResumeLayout(False)
        pnlBottomActions.ResumeLayout(False)
        CType(dgvGradebook, ComponentModel.ISupportInitialize).EndInit()
        pnlTopRubricBG.ResumeLayout(False)
        pnlTopRubricBG.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlTop As RoundedPanel
    Friend WithEvents tblMainGBook As TableLayoutPanel
    Friend WithEvents lblGradebook As Label
    Friend WithEvents btnBack As ReaLTaiizor.Controls.Button
    Friend WithEvents pnlRubric As RoundedPanel
    Friend WithEvents tblMainRubric As TableLayoutPanel
    Friend WithEvents dgvGradebook As DataGridView
    Friend WithEvents pnlBottomActions As Panel
    Friend WithEvents btnSaveChanges As ReaLTaiizor.Controls.Button
    Friend WithEvents btnSubmit As ReaLTaiizor.Controls.Button
    Friend WithEvents btnExportExcel As ReaLTaiizor.Controls.Button
    Friend WithEvents number As DataGridViewTextBoxColumn
    Friend WithEvents studname As DataGridViewTextBoxColumn
    Friend WithEvents status As DataGridViewTextBoxColumn
    Friend WithEvents part As DataGridViewTextBoxColumn
    Friend WithEvents gra As DataGridViewTextBoxColumn
    Friend WithEvents EXTRA As DataGridViewTextBoxColumn
    Friend WithEvents Seatwork As DataGridViewTextBoxColumn
    Friend WithEvents quiz1 As DataGridViewTextBoxColumn
    Friend WithEvents GRAQ1 As DataGridViewTextBoxColumn
    Friend WithEvents quiz2 As DataGridViewTextBoxColumn
    Friend WithEvents GRAQ2 As DataGridViewTextBoxColumn
    Friend WithEvents quizp As DataGridViewTextBoxColumn
    Friend WithEvents project As DataGridViewTextBoxColumn
    Friend WithEvents graproject As DataGridViewTextBoxColumn
    Friend WithEvents exam As DataGridViewTextBoxColumn
    Friend WithEvents GRAexam As DataGridViewTextBoxColumn
    Friend WithEvents majorexam As DataGridViewTextBoxColumn
    Friend WithEvents grademajorexam As DataGridViewTextBoxColumn
    Friend WithEvents pnlTopRubricBG As RoundedPanel
    Friend WithEvents btnConfigureWeights As Button
    Friend WithEvents btnAddColumn As ReaLTaiizor.Controls.Button
    Friend WithEvents btnSaveConfig As ReaLTaiizor.Controls.Button
    Friend WithEvents pnlTopGradeBook As Label

End Class
