<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucTranscript
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
        pnlStudentTranscriptBG = New RoundedPanel()
        btnExportStudent = New ReaLTaiizor.Controls.Button()
        pnlStudentInfo = New Panel()
        dgvTranscript = New DataGridView()
        coursecode = New DataGridViewTextBoxColumn()
        subjectname = New DataGridViewTextBoxColumn()
        professor = New DataGridViewTextBoxColumn()
        grade = New DataGridViewTextBoxColumn()
        pnlTopHead = New Panel()
        pnlLine = New Panel()
        lblGWA = New Label()
        lblStudentInfo = New Label()
        lblCardSubtitle = New Label()
        lblCardTitle = New Label()
        pnlStudentTranscriptBG.SuspendLayout()
        pnlStudentInfo.SuspendLayout()
        CType(dgvTranscript, ComponentModel.ISupportInitialize).BeginInit()
        pnlTopHead.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlStudentTranscriptBG
        ' 
        pnlStudentTranscriptBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentTranscriptBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentTranscriptBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentTranscriptBG.BorderThickness = 1
        pnlStudentTranscriptBG.Controls.Add(btnExportStudent)
        pnlStudentTranscriptBG.Controls.Add(pnlStudentInfo)
        pnlStudentTranscriptBG.Controls.Add(lblCardSubtitle)
        pnlStudentTranscriptBG.Controls.Add(lblCardTitle)
        pnlStudentTranscriptBG.Location = New Point(30, 19)
        pnlStudentTranscriptBG.Name = "pnlStudentTranscriptBG"
        pnlStudentTranscriptBG.Radius = 20
        pnlStudentTranscriptBG.Size = New Size(1864, 879)
        pnlStudentTranscriptBG.TabIndex = 3
        ' 
        ' btnExportStudent
        ' 
        btnExportStudent.BackColor = Color.Transparent
        btnExportStudent.BackgroundImageLayout = ImageLayout.Zoom
        btnExportStudent.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnExportStudent.EnteredBorderColor = Color.Empty
        btnExportStudent.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnExportStudent.Font = New Font("Century Gothic", 10.2F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnExportStudent.Image = Nothing
        btnExportStudent.ImageAlign = ContentAlignment.MiddleLeft
        btnExportStudent.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnExportStudent.Location = New Point(291, 700)
        btnExportStudent.Name = "btnExportStudent"
        btnExportStudent.PressedBorderColor = Color.Transparent
        btnExportStudent.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnExportStudent.Size = New Size(204, 41)
        btnExportStudent.TabIndex = 10
        btnExportStudent.Text = "Export"
        btnExportStudent.TextAlignment = StringAlignment.Center
        btnExportStudent.UseWaitCursor = True
        ' 
        ' pnlStudentInfo
        ' 
        pnlStudentInfo.BackColor = Color.Transparent
        pnlStudentInfo.Controls.Add(dgvTranscript)
        pnlStudentInfo.Controls.Add(pnlTopHead)
        pnlStudentInfo.Location = New Point(273, 144)
        pnlStudentInfo.Name = "pnlStudentInfo"
        pnlStudentInfo.Size = New Size(1305, 531)
        pnlStudentInfo.TabIndex = 2
        ' 
        ' dgvTranscript
        ' 
        dgvTranscript.AllowUserToAddRows = False
        dgvTranscript.AllowUserToDeleteRows = False
        dgvTranscript.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvTranscript.BackgroundColor = Color.White
        dgvTranscript.BorderStyle = BorderStyle.None
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle1.BackColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        DataGridViewCellStyle1.Font = New Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        DataGridViewCellStyle1.ForeColor = Color.White
        DataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = Color.White
        DataGridViewCellStyle1.WrapMode = DataGridViewTriState.True
        dgvTranscript.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        dgvTranscript.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvTranscript.Columns.AddRange(New DataGridViewColumn() {coursecode, subjectname, professor, grade})
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle2.BackColor = Color.White
        DataGridViewCellStyle2.Font = New Font("Segoe UI", 9F)
        DataGridViewCellStyle2.ForeColor = SystemColors.ControlText
        DataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.False
        dgvTranscript.DefaultCellStyle = DataGridViewCellStyle2
        dgvTranscript.Dock = DockStyle.Fill
        dgvTranscript.GridColor = Color.White
        dgvTranscript.Location = New Point(0, 99)
        dgvTranscript.Margin = New Padding(20, 0, 20, 0)
        dgvTranscript.Name = "dgvTranscript"
        dgvTranscript.ReadOnly = True
        dgvTranscript.RowHeadersVisible = False
        dgvTranscript.RowHeadersWidth = 51
        dgvTranscript.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvTranscript.Size = New Size(1305, 432)
        dgvTranscript.TabIndex = 9
        ' 
        ' coursecode
        ' 
        coursecode.HeaderText = "CODE"
        coursecode.MinimumWidth = 6
        coursecode.Name = "coursecode"
        coursecode.ReadOnly = True
        ' 
        ' subjectname
        ' 
        subjectname.HeaderText = "SUBJECTS"
        subjectname.MinimumWidth = 6
        subjectname.Name = "subjectname"
        subjectname.ReadOnly = True
        ' 
        ' professor
        ' 
        professor.HeaderText = "PROFESSOR"
        professor.MinimumWidth = 6
        professor.Name = "professor"
        professor.ReadOnly = True
        ' 
        ' grade
        ' 
        grade.HeaderText = "GRADE"
        grade.MinimumWidth = 6
        grade.Name = "grade"
        grade.ReadOnly = True
        ' 
        ' pnlTopHead
        ' 
        pnlTopHead.Controls.Add(pnlLine)
        pnlTopHead.Controls.Add(lblGWA)
        pnlTopHead.Controls.Add(lblStudentInfo)
        pnlTopHead.Dock = DockStyle.Top
        pnlTopHead.Location = New Point(0, 0)
        pnlTopHead.Name = "pnlTopHead"
        pnlTopHead.Size = New Size(1305, 99)
        pnlTopHead.TabIndex = 8
        ' 
        ' pnlLine
        ' 
        pnlLine.BackColor = SystemColors.ButtonShadow
        pnlLine.Dock = DockStyle.Top
        pnlLine.Location = New Point(0, 0)
        pnlLine.Name = "pnlLine"
        pnlLine.Size = New Size(1305, 1)
        pnlLine.TabIndex = 0
        ' 
        ' lblGWA
        ' 
        lblGWA.AutoSize = True
        lblGWA.BackColor = Color.Transparent
        lblGWA.Font = New Font("Times New Roman", 13.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblGWA.Location = New Point(1170, 18)
        lblGWA.Name = "lblGWA"
        lblGWA.Size = New Size(0, 26)
        lblGWA.TabIndex = 6
        ' 
        ' lblStudentInfo
        ' 
        lblStudentInfo.AutoSize = True
        lblStudentInfo.BackColor = Color.Transparent
        lblStudentInfo.Font = New Font("Times New Roman", 13.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblStudentInfo.Location = New Point(18, 18)
        lblStudentInfo.Name = "lblStudentInfo"
        lblStudentInfo.Size = New Size(0, 26)
        lblStudentInfo.TabIndex = 2
        ' 
        ' lblCardSubtitle
        ' 
        lblCardSubtitle.AutoSize = True
        lblCardSubtitle.BackColor = Color.Transparent
        lblCardSubtitle.Font = New Font("Times New Roman", 16.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblCardSubtitle.Location = New Point(782, 87)
        lblCardSubtitle.Name = "lblCardSubtitle"
        lblCardSubtitle.Size = New Size(302, 33)
        lblCardSubtitle.TabIndex = 1
        lblCardSubtitle.Text = "First Semester 2026-2027"
        ' 
        ' lblCardTitle
        ' 
        lblCardTitle.AutoSize = True
        lblCardTitle.BackColor = Color.Transparent
        lblCardTitle.Font = New Font("Times New Roman", 22.2F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblCardTitle.Location = New Point(602, 32)
        lblCardTitle.Name = "lblCardTitle"
        lblCardTitle.Size = New Size(673, 42)
        lblCardTitle.TabIndex = 0
        lblCardTitle.Text = "OFFICIAL ACADEMIC GRADE CARD"
        ' 
        ' ucTranscript
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(pnlStudentTranscriptBG)
        Name = "ucTranscript"
        Size = New Size(1924, 917)
        pnlStudentTranscriptBG.ResumeLayout(False)
        pnlStudentTranscriptBG.PerformLayout()
        pnlStudentInfo.ResumeLayout(False)
        CType(dgvTranscript, ComponentModel.ISupportInitialize).EndInit()
        pnlTopHead.ResumeLayout(False)
        pnlTopHead.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlStudentTranscriptBG As RoundedPanel
    Friend WithEvents lblCardTitle As Label
    Friend WithEvents lblCardSubtitle As Label
    Friend WithEvents pnlStudentInfo As Panel
    Friend WithEvents pnlLine As Panel
    Friend WithEvents lblStudentInfo As Label
    Friend WithEvents pnlTopHead As Panel
    Friend WithEvents lblGWA As Label
    Friend WithEvents dgvTranscript As DataGridView
    Friend WithEvents coursecode As DataGridViewTextBoxColumn
    Friend WithEvents subjectname As DataGridViewTextBoxColumn
    Friend WithEvents professor As DataGridViewTextBoxColumn
    Friend WithEvents grade As DataGridViewTextBoxColumn
    Friend WithEvents btnExportStudent As ReaLTaiizor.Controls.Button

End Class
