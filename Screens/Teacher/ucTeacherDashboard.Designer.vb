<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucTeacherDashboard
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
        pnlTeacherBG = New RoundedPanel()
        pnlSectionCards = New Panel()
        flowCourses = New FlowLayoutPanel()
        pnlToolTDash = New Panel()
        btnFilterAllYear = New ReaLTaiizor.Controls.AloneButton()
        btnFilterAllCourses = New ReaLTaiizor.Controls.AloneButton()
        btnFilterCourses = New ReaLTaiizor.Controls.Button()
        btnFilterYear = New ReaLTaiizor.Controls.Button()
        pnlTeacherBG.SuspendLayout()
        pnlSectionCards.SuspendLayout()
        pnlToolTDash.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlTeacherBG
        ' 
        pnlTeacherBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTeacherBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTeacherBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlTeacherBG.BorderThickness = 1
        pnlTeacherBG.Controls.Add(pnlSectionCards)
        pnlTeacherBG.Controls.Add(pnlToolTDash)
        pnlTeacherBG.Location = New Point(30, 19)
        pnlTeacherBG.Name = "pnlTeacherBG"
        pnlTeacherBG.Radius = 20
        pnlTeacherBG.Size = New Size(1864, 879)
        pnlTeacherBG.TabIndex = 1
        ' 
        ' pnlSectionCards
        ' 
        pnlSectionCards.BackColor = Color.Transparent
        pnlSectionCards.Controls.Add(flowCourses)
        pnlSectionCards.Dock = DockStyle.Fill
        pnlSectionCards.Location = New Point(0, 94)
        pnlSectionCards.Name = "pnlSectionCards"
        pnlSectionCards.Size = New Size(1864, 785)
        pnlSectionCards.TabIndex = 6
        ' 
        ' flowCourses
        ' 
        flowCourses.AutoScroll = True
        flowCourses.Dock = DockStyle.Fill
        flowCourses.Location = New Point(0, 0)
        flowCourses.Name = "flowCourses"
        flowCourses.Padding = New Padding(20, 0, 20, 20)
        flowCourses.Size = New Size(1864, 785)
        flowCourses.TabIndex = 0
        ' 
        ' pnlToolTDash
        ' 
        pnlToolTDash.BackColor = Color.Transparent
        pnlToolTDash.Controls.Add(btnFilterAllYear)
        pnlToolTDash.Controls.Add(btnFilterAllCourses)
        pnlToolTDash.Controls.Add(btnFilterCourses)
        pnlToolTDash.Controls.Add(btnFilterYear)
        pnlToolTDash.Dock = DockStyle.Top
        pnlToolTDash.Location = New Point(0, 0)
        pnlToolTDash.Name = "pnlToolTDash"
        pnlToolTDash.Size = New Size(1864, 94)
        pnlToolTDash.TabIndex = 5
        ' 
        ' btnFilterAllYear
        ' 
        btnFilterAllYear.BackColor = Color.Transparent
        btnFilterAllYear.EnabledCalc = True
        btnFilterAllYear.Font = New Font("Century Gothic", 10.2F)
        btnFilterAllYear.ForeColor = Color.Black
        btnFilterAllYear.Location = New Point(194, 30)
        btnFilterAllYear.Name = "btnFilterAllYear"
        btnFilterAllYear.Size = New Size(98, 40)
        btnFilterAllYear.TabIndex = 10
        btnFilterAllYear.Text = "All"
        ' 
        ' btnFilterAllCourses
        ' 
        btnFilterAllCourses.BackColor = Color.Transparent
        btnFilterAllCourses.EnabledCalc = True
        btnFilterAllCourses.Font = New Font("Century Gothic", 10.2F)
        btnFilterAllCourses.ForeColor = Color.Black
        btnFilterAllCourses.Location = New Point(518, 30)
        btnFilterAllCourses.Name = "btnFilterAllCourses"
        btnFilterAllCourses.Size = New Size(98, 40)
        btnFilterAllCourses.TabIndex = 8
        btnFilterAllCourses.Text = "All"
        ' 
        ' btnFilterCourses
        ' 
        btnFilterCourses.BackColor = Color.Transparent
        btnFilterCourses.BackgroundImageLayout = ImageLayout.Zoom
        btnFilterCourses.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnFilterCourses.EnteredBorderColor = Color.Empty
        btnFilterCourses.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnFilterCourses.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnFilterCourses.Image = Nothing
        btnFilterCourses.ImageAlign = ContentAlignment.MiddleLeft
        btnFilterCourses.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnFilterCourses.Location = New Point(318, 30)
        btnFilterCourses.Name = "btnFilterCourses"
        btnFilterCourses.PressedBorderColor = Color.Transparent
        btnFilterCourses.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnFilterCourses.Size = New Size(184, 40)
        btnFilterCourses.TabIndex = 5
        btnFilterCourses.Text = "Select Courses ▾"
        btnFilterCourses.TextAlignment = StringAlignment.Center
        btnFilterCourses.UseWaitCursor = True
        ' 
        ' btnFilterYear
        ' 
        btnFilterYear.BackColor = Color.Transparent
        btnFilterYear.BackgroundImageLayout = ImageLayout.Zoom
        btnFilterYear.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnFilterYear.EnteredBorderColor = Color.Empty
        btnFilterYear.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnFilterYear.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnFilterYear.Image = Nothing
        btnFilterYear.ImageAlign = ContentAlignment.MiddleLeft
        btnFilterYear.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnFilterYear.Location = New Point(33, 30)
        btnFilterYear.Name = "btnFilterYear"
        btnFilterYear.PressedBorderColor = Color.Transparent
        btnFilterYear.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnFilterYear.Size = New Size(145, 40)
        btnFilterYear.TabIndex = 4
        btnFilterYear.Text = "Year Level  ▾"
        btnFilterYear.TextAlignment = StringAlignment.Center
        btnFilterYear.UseWaitCursor = True
        ' 
        ' ucTeacherDashboard
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(pnlTeacherBG)
        Name = "ucTeacherDashboard"
        Size = New Size(1924, 917)
        pnlTeacherBG.ResumeLayout(False)
        pnlSectionCards.ResumeLayout(False)
        pnlToolTDash.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlTeacherBG As RoundedPanel
    Friend WithEvents btnFilterYear As ReaLTaiizor.Controls.Button
    Friend WithEvents pnlSectionCards As Panel
    Friend WithEvents pnlToolTDash As Panel
    Friend WithEvents flowCourses As FlowLayoutPanel
    Friend WithEvents btnFilterCourses As ReaLTaiizor.Controls.Button
    Friend WithEvents btnAllYear As ReaLTaiizor.Controls.Button
    Friend WithEvents btnAllCourse As ReaLTaiizor.Controls.Button
    Friend WithEvents btnFilterAllCourses As ReaLTaiizor.Controls.AloneButton
    Friend WithEvents btnFilterAllYear As ReaLTaiizor.Controls.AloneButton
    Friend WithEvents Button1 As ReaLTaiizor.Controls.Button

End Class
