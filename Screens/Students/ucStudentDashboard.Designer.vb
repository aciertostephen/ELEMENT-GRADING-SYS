<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucStudentDashboard
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
        pnlStudentDashBG = New RoundedPanel()
        flowRubric = New FlowLayoutPanel()
        pnlStudentDashBG.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlStudentDashBG
        ' 
        pnlStudentDashBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentDashBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentDashBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlStudentDashBG.BorderThickness = 1
        pnlStudentDashBG.Controls.Add(flowRubric)
        pnlStudentDashBG.Location = New Point(30, 19)
        pnlStudentDashBG.Name = "pnlStudentDashBG"
        pnlStudentDashBG.Radius = 20
        pnlStudentDashBG.Size = New Size(1864, 879)
        pnlStudentDashBG.TabIndex = 2
        ' 
        ' flowRubric
        ' 
        flowRubric.AutoScroll = True
        flowRubric.Dock = DockStyle.Fill
        flowRubric.Location = New Point(0, 0)
        flowRubric.Name = "flowRubric"
        flowRubric.Padding = New Padding(20)
        flowRubric.Size = New Size(1864, 879)
        flowRubric.TabIndex = 0
        ' 
        ' ucStudentDashboard
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(pnlStudentDashBG)
        Name = "ucStudentDashboard"
        Size = New Size(1924, 917)
        pnlStudentDashBG.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlStudentDashBG As RoundedPanel
    Friend WithEvents flowRubric As FlowLayoutPanel

End Class
