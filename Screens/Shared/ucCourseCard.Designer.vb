<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucCourseCard
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
        lblCourseCode = New Label()
        pnlCourseCard = New RoundedPanel()
        ProgressBar1 = New ProgressBar()
        lblProgress = New Label()
        lblEnrolled = New Label()
        lblYear = New Label()
        lblSection = New Label()
        pnlCourseBG = New RoundedPanel()
        pnlCourseCard.SuspendLayout()
        pnlCourseBG.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblCourseCode
        ' 
        lblCourseCode.AutoSize = True
        lblCourseCode.BackColor = Color.Transparent
        lblCourseCode.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblCourseCode.Location = New Point(16, 23)
        lblCourseCode.Name = "lblCourseCode"
        lblCourseCode.Size = New Size(270, 21)
        lblCourseCode.TabIndex = 1
        lblCourseCode.Text = "COURSECODE - SUBJECT CODE"
        ' 
        ' pnlCourseCard
        ' 
        pnlCourseCard.ActiveBorderColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        pnlCourseCard.BackColor = Color.White
        pnlCourseCard.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        pnlCourseCard.BorderThickness = 1
        pnlCourseCard.Controls.Add(ProgressBar1)
        pnlCourseCard.Controls.Add(lblProgress)
        pnlCourseCard.Controls.Add(lblEnrolled)
        pnlCourseCard.Controls.Add(lblYear)
        pnlCourseCard.Controls.Add(lblSection)
        pnlCourseCard.Controls.Add(lblCourseCode)
        pnlCourseCard.Location = New Point(14, 0)
        pnlCourseCard.Name = "pnlCourseCard"
        pnlCourseCard.Radius = 20
        pnlCourseCard.Size = New Size(486, 243)
        pnlCourseCard.TabIndex = 2
        ' 
        ' ProgressBar1
        ' 
        ProgressBar1.Location = New Point(16, 195)
        ProgressBar1.Name = "ProgressBar1"
        ProgressBar1.Size = New Size(351, 18)
        ProgressBar1.TabIndex = 6
        ' 
        ' lblProgress
        ' 
        lblProgress.AutoSize = True
        lblProgress.BackColor = Color.Transparent
        lblProgress.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblProgress.Location = New Point(16, 153)
        lblProgress.Name = "lblProgress"
        lblProgress.Size = New Size(243, 21)
        lblProgress.TabIndex = 5
        lblProgress.Text = "ENCODING PROGRESS : 78%"
        ' 
        ' lblEnrolled
        ' 
        lblEnrolled.AutoSize = True
        lblEnrolled.BackColor = Color.Transparent
        lblEnrolled.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblEnrolled.Location = New Point(16, 119)
        lblEnrolled.Name = "lblEnrolled"
        lblEnrolled.Size = New Size(123, 21)
        lblEnrolled.TabIndex = 4
        lblEnrolled.Text = "ENROLLED: 42"
        ' 
        ' lblYear
        ' 
        lblYear.AutoSize = True
        lblYear.BackColor = Color.Transparent
        lblYear.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblYear.Location = New Point(16, 89)
        lblYear.Name = "lblYear"
        lblYear.Size = New Size(72, 21)
        lblYear.TabIndex = 3
        lblYear.Text = "YEAR: 2"
        ' 
        ' lblSection
        ' 
        lblSection.AutoSize = True
        lblSection.BackColor = Color.Transparent
        lblSection.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblSection.Location = New Point(16, 58)
        lblSection.Name = "lblSection"
        lblSection.Size = New Size(164, 21)
        lblSection.TabIndex = 2
        lblSection.Text = "SECTION : BSCS 2A"
        ' 
        ' pnlCourseBG
        ' 
        pnlCourseBG.ActiveBorderColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        pnlCourseBG.BackColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        pnlCourseBG.BorderColor = Color.FromArgb(CByte(209), CByte(213), CByte(219))
        pnlCourseBG.BorderThickness = 1
        pnlCourseBG.Controls.Add(pnlCourseCard)
        pnlCourseBG.Dock = DockStyle.Fill
        pnlCourseBG.Location = New Point(0, 0)
        pnlCourseBG.Name = "pnlCourseBG"
        pnlCourseBG.Radius = 20
        pnlCourseBG.Size = New Size(500, 243)
        pnlCourseBG.TabIndex = 3
        ' 
        ' ucCourseCard
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Transparent
        Controls.Add(pnlCourseBG)
        Name = "ucCourseCard"
        Size = New Size(500, 243)
        pnlCourseCard.ResumeLayout(False)
        pnlCourseCard.PerformLayout()
        pnlCourseBG.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents lblCourseCode As Label
    Friend WithEvents pnlCourseCard As RoundedPanel
    Friend WithEvents ProgressBar1 As ProgressBar
    Friend WithEvents lblProgress As Label
    Friend WithEvents lblEnrolled As Label
    Friend WithEvents lblYear As Label
    Friend WithEvents lblSection As Label
    Friend WithEvents pnlCourseBG As RoundedPanel

End Class
