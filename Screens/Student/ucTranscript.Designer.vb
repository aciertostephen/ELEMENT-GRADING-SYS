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
        pnlStudentTranscriptBG = New RoundedPanel()
        lblOfficialCard = New Label()
        Label1 = New Label()
        pnlStudentTranscriptBG.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlStudentTranscriptBG
        ' 
        pnlStudentTranscriptBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentTranscriptBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentTranscriptBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentTranscriptBG.BorderThickness = 1
        pnlStudentTranscriptBG.Controls.Add(Label1)
        pnlStudentTranscriptBG.Controls.Add(lblOfficialCard)
        pnlStudentTranscriptBG.Location = New Point(30, 19)
        pnlStudentTranscriptBG.Name = "pnlStudentTranscriptBG"
        pnlStudentTranscriptBG.Radius = 20
        pnlStudentTranscriptBG.Size = New Size(1864, 879)
        pnlStudentTranscriptBG.TabIndex = 3
        ' 
        ' lblOfficialCard
        ' 
        lblOfficialCard.AutoSize = True
        lblOfficialCard.BackColor = Color.Transparent
        lblOfficialCard.Font = New Font("Times New Roman", 22.2F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblOfficialCard.Location = New Point(602, 32)
        lblOfficialCard.Name = "lblOfficialCard"
        lblOfficialCard.Size = New Size(673, 42)
        lblOfficialCard.TabIndex = 0
        lblOfficialCard.Text = "OFFICIAL ACADEMIC GRADE CARD"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.BackColor = Color.Transparent
        Label1.Font = New Font("Times New Roman", 16.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(782, 87)
        Label1.Name = "Label1"
        Label1.Size = New Size(302, 33)
        Label1.TabIndex = 1
        Label1.Text = "First Semester 2026-2027"
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
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlStudentTranscriptBG As RoundedPanel
    Friend WithEvents lblOfficialCard As Label
    Friend WithEvents Label1 As Label

End Class
