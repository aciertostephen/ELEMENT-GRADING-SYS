<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucAnalytics
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
        tblMainDock = New TableLayoutPanel()
        RoundedPanel1 = New RoundedPanel()
        tblLeftDock = New TableLayoutPanel()
        pnlBottomBG = New RoundedPanel()
        pnlOverallAverage = New Label()
        pnlAverageGrade = New Panel()
        FormsPlot3 = New ScottPlot.WinForms.FormsPlot()
        pnlTopBG = New RoundedPanel()
        lblOverallGradeStanding = New Label()
        pnlOverallStanding = New Panel()
        FormsPlot1 = New ScottPlot.WinForms.FormsPlot()
        lblGradeComponent = New Label()
        cboCourses = New ComboBox()
        Panel1 = New Panel()
        FormsPlot2 = New ScottPlot.WinForms.FormsPlot()
        tblMainDock.SuspendLayout()
        RoundedPanel1.SuspendLayout()
        tblLeftDock.SuspendLayout()
        pnlBottomBG.SuspendLayout()
        pnlAverageGrade.SuspendLayout()
        pnlTopBG.SuspendLayout()
        pnlOverallStanding.SuspendLayout()
        Panel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' tblMainDock
        ' 
        tblMainDock.BackColor = Color.Transparent
        tblMainDock.ColumnCount = 2
        tblMainDock.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60F))
        tblMainDock.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 40F))
        tblMainDock.Controls.Add(RoundedPanel1, 1, 0)
        tblMainDock.Controls.Add(tblLeftDock, 0, 0)
        tblMainDock.Dock = DockStyle.Fill
        tblMainDock.Location = New Point(0, 0)
        tblMainDock.Name = "tblMainDock"
        tblMainDock.RowCount = 1
        tblMainDock.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tblMainDock.Size = New Size(1924, 917)
        tblMainDock.TabIndex = 0
        ' 
        ' RoundedPanel1
        ' 
        RoundedPanel1.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel1.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel1.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        RoundedPanel1.BorderThickness = 1
        RoundedPanel1.Controls.Add(Panel1)
        RoundedPanel1.Controls.Add(cboCourses)
        RoundedPanel1.Controls.Add(lblGradeComponent)
        RoundedPanel1.Dock = DockStyle.Fill
        RoundedPanel1.Location = New Point(1164, 10)
        RoundedPanel1.Margin = New Padding(10, 10, 20, 20)
        RoundedPanel1.Name = "RoundedPanel1"
        RoundedPanel1.Padding = New Padding(3)
        RoundedPanel1.Radius = 12
        RoundedPanel1.Size = New Size(740, 887)
        RoundedPanel1.TabIndex = 2
        ' 
        ' tblLeftDock
        ' 
        tblLeftDock.ColumnCount = 1
        tblLeftDock.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tblLeftDock.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 20F))
        tblLeftDock.Controls.Add(pnlBottomBG, 0, 1)
        tblLeftDock.Controls.Add(pnlTopBG, 0, 0)
        tblLeftDock.Dock = DockStyle.Fill
        tblLeftDock.Location = New Point(3, 3)
        tblLeftDock.Name = "tblLeftDock"
        tblLeftDock.RowCount = 2
        tblLeftDock.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tblLeftDock.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tblLeftDock.Size = New Size(1148, 911)
        tblLeftDock.TabIndex = 0
        ' 
        ' pnlBottomBG
        ' 
        pnlBottomBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlBottomBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlBottomBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlBottomBG.BorderThickness = 1
        pnlBottomBG.Controls.Add(pnlOverallAverage)
        pnlBottomBG.Controls.Add(pnlAverageGrade)
        pnlBottomBG.Dock = DockStyle.Fill
        pnlBottomBG.Location = New Point(20, 465)
        pnlBottomBG.Margin = New Padding(20, 10, 10, 20)
        pnlBottomBG.Name = "pnlBottomBG"
        pnlBottomBG.Padding = New Padding(3)
        pnlBottomBG.Radius = 12
        pnlBottomBG.Size = New Size(1118, 426)
        pnlBottomBG.TabIndex = 1
        ' 
        ' pnlOverallAverage
        ' 
        pnlOverallAverage.AutoSize = True
        pnlOverallAverage.BackColor = Color.Transparent
        pnlOverallAverage.Font = New Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        pnlOverallAverage.Location = New Point(41, 28)
        pnlOverallAverage.Name = "pnlOverallAverage"
        pnlOverallAverage.Size = New Size(416, 21)
        pnlOverallAverage.TabIndex = 3
        pnlOverallAverage.Text = "Overall Average Grade Component Analyrics"
        ' 
        ' pnlAverageGrade
        ' 
        pnlAverageGrade.BackColor = Color.Transparent
        pnlAverageGrade.Controls.Add(FormsPlot3)
        pnlAverageGrade.Location = New Point(168, 65)
        pnlAverageGrade.Name = "pnlAverageGrade"
        pnlAverageGrade.Size = New Size(768, 324)
        pnlAverageGrade.TabIndex = 2
        ' 
        ' FormsPlot3
        ' 
        FormsPlot3.Dock = DockStyle.Fill
        FormsPlot3.Location = New Point(0, 0)
        FormsPlot3.Name = "FormsPlot3"
        FormsPlot3.Size = New Size(768, 324)
        FormsPlot3.TabIndex = 0
        ' 
        ' pnlTopBG
        ' 
        pnlTopBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTopBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTopBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTopBG.BorderThickness = 1
        pnlTopBG.Controls.Add(lblOverallGradeStanding)
        pnlTopBG.Controls.Add(pnlOverallStanding)
        pnlTopBG.Dock = DockStyle.Fill
        pnlTopBG.Location = New Point(20, 10)
        pnlTopBG.Margin = New Padding(20, 10, 10, 10)
        pnlTopBG.Name = "pnlTopBG"
        pnlTopBG.Padding = New Padding(3)
        pnlTopBG.Radius = 12
        pnlTopBG.Size = New Size(1118, 435)
        pnlTopBG.TabIndex = 0
        ' 
        ' lblOverallGradeStanding
        ' 
        lblOverallGradeStanding.AutoSize = True
        lblOverallGradeStanding.BackColor = Color.Transparent
        lblOverallGradeStanding.Font = New Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblOverallGradeStanding.Location = New Point(41, 22)
        lblOverallGradeStanding.Name = "lblOverallGradeStanding"
        lblOverallGradeStanding.Size = New Size(216, 21)
        lblOverallGradeStanding.TabIndex = 2
        lblOverallGradeStanding.Text = "Overall Grade Standing"
        ' 
        ' pnlOverallStanding
        ' 
        pnlOverallStanding.BackColor = Color.Transparent
        pnlOverallStanding.Controls.Add(FormsPlot1)
        pnlOverallStanding.Location = New Point(168, 59)
        pnlOverallStanding.Name = "pnlOverallStanding"
        pnlOverallStanding.Size = New Size(768, 324)
        pnlOverallStanding.TabIndex = 1
        ' 
        ' FormsPlot1
        ' 
        FormsPlot1.Dock = DockStyle.Fill
        FormsPlot1.Location = New Point(0, 0)
        FormsPlot1.Name = "FormsPlot1"
        FormsPlot1.Size = New Size(768, 324)
        FormsPlot1.TabIndex = 0
        ' 
        ' lblGradeComponent
        ' 
        lblGradeComponent.AutoSize = True
        lblGradeComponent.BackColor = Color.Transparent
        lblGradeComponent.Font = New Font("Century Gothic", 10.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblGradeComponent.Location = New Point(34, 25)
        lblGradeComponent.Name = "lblGradeComponent"
        lblGradeComponent.Size = New Size(371, 21)
        lblGradeComponent.TabIndex = 4
        lblGradeComponent.Text = "Grade Component Analytics Per Subject"
        ' 
        ' cboCourses
        ' 
        cboCourses.Font = New Font("Century Gothic", 16.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cboCourses.FormattingEnabled = True
        cboCourses.Items.AddRange(New Object() {""})
        cboCourses.Location = New Point(545, 11)
        cboCourses.Name = "cboCourses"
        cboCourses.Size = New Size(178, 41)
        cboCourses.TabIndex = 8
        ' 
        ' Panel1
        ' 
        Panel1.BackColor = Color.Transparent
        Panel1.Controls.Add(FormsPlot2)
        Panel1.Location = New Point(34, 174)
        Panel1.Name = "Panel1"
        Panel1.Size = New Size(664, 602)
        Panel1.TabIndex = 9
        ' 
        ' FormsPlot2
        ' 
        FormsPlot2.Dock = DockStyle.Fill
        FormsPlot2.Location = New Point(0, 0)
        FormsPlot2.Name = "FormsPlot2"
        FormsPlot2.Size = New Size(664, 602)
        FormsPlot2.TabIndex = 0
        ' 
        ' ucAnalytics
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(tblMainDock)
        Name = "ucAnalytics"
        Size = New Size(1924, 917)
        tblMainDock.ResumeLayout(False)
        RoundedPanel1.ResumeLayout(False)
        RoundedPanel1.PerformLayout()
        tblLeftDock.ResumeLayout(False)
        pnlBottomBG.ResumeLayout(False)
        pnlBottomBG.PerformLayout()
        pnlAverageGrade.ResumeLayout(False)
        pnlTopBG.ResumeLayout(False)
        pnlTopBG.PerformLayout()
        pnlOverallStanding.ResumeLayout(False)
        Panel1.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tblMainDock As TableLayoutPanel
    Friend WithEvents tblLeftDock As TableLayoutPanel
    Friend WithEvents pnlTopBG As RoundedPanel
    Friend WithEvents pnlBottomBG As RoundedPanel
    Friend WithEvents pnlOverallStanding As Panel
    Friend WithEvents FormsPlot1 As ScottPlot.WinForms.FormsPlot
    Friend WithEvents RoundedPanel1 As RoundedPanel
    Friend WithEvents pnlOverallAverage As Label
    Friend WithEvents pnlAverageGrade As Panel
    Friend WithEvents FormsPlot3 As ScottPlot.WinForms.FormsPlot
    Friend WithEvents lblOverallGradeStanding As Label
    Friend WithEvents lblGradeComponent As Label
    Friend WithEvents cboCourses As ComboBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents FormsPlot2 As ScottPlot.WinForms.FormsPlot

End Class
