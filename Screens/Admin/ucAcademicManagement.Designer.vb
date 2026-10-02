<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucAcademicManagement
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
        pnlAcadBG = New RoundedPanel()
        pnlCards = New Panel()
        flowCourses = New FlowLayoutPanel()
        pnlFilters = New Panel()
        btnAddSection = New ReaLTaiizor.Controls.Button()
        btnAddCourse = New ReaLTaiizor.Controls.Button()
        btnAdjustSchedule = New ReaLTaiizor.Controls.Button()
        cboSemester = New ComboBox()
        cboAY = New ComboBox()
        cboSection = New ComboBox()
        cboYearLevel = New ComboBox()
        lblYear = New Label()
        lblSection = New Label()
        lblAY = New Label()
        lblSemester = New Label()
        pnlStats = New Panel()
        flowStats = New FlowLayoutPanel()
        pnlAcadBG.SuspendLayout()
        pnlCards.SuspendLayout()
        pnlFilters.SuspendLayout()
        pnlStats.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlAcadBG
        ' 
        pnlAcadBG.ActiveBorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlAcadBG.BackColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlAcadBG.BorderColor = Color.FromArgb(CByte(248), CByte(248), CByte(248))
        pnlAcadBG.BorderThickness = 1
        pnlAcadBG.Controls.Add(pnlCards)
        pnlAcadBG.Controls.Add(pnlFilters)
        pnlAcadBG.Controls.Add(pnlStats)
        pnlAcadBG.Location = New Point(30, 25)
        pnlAcadBG.Name = "pnlAcadBG"
        pnlAcadBG.Radius = 20
        pnlAcadBG.Size = New Size(1864, 879)
        pnlAcadBG.TabIndex = 0
        ' 
        ' pnlCards
        ' 
        pnlCards.AutoScroll = True
        pnlCards.Controls.Add(flowCourses)
        pnlCards.Dock = DockStyle.Fill
        pnlCards.Location = New Point(0, 170)
        pnlCards.Name = "pnlCards"
        pnlCards.Size = New Size(1864, 709)
        pnlCards.TabIndex = 2
        ' 
        ' flowCourses
        ' 
        flowCourses.AutoScroll = True
        flowCourses.Dock = DockStyle.Fill
        flowCourses.Location = New Point(0, 0)
        flowCourses.Name = "flowCourses"
        flowCourses.Size = New Size(1864, 709)
        flowCourses.TabIndex = 0
        ' 
        ' pnlFilters
        ' 
        pnlFilters.Controls.Add(btnAddSection)
        pnlFilters.Controls.Add(btnAddCourse)
        pnlFilters.Controls.Add(btnAdjustSchedule)
        pnlFilters.Controls.Add(cboSemester)
        pnlFilters.Controls.Add(cboAY)
        pnlFilters.Controls.Add(cboSection)
        pnlFilters.Controls.Add(cboYearLevel)
        pnlFilters.Controls.Add(lblYear)
        pnlFilters.Controls.Add(lblSection)
        pnlFilters.Controls.Add(lblAY)
        pnlFilters.Controls.Add(lblSemester)
        pnlFilters.Dock = DockStyle.Top
        pnlFilters.Location = New Point(0, 100)
        pnlFilters.Name = "pnlFilters"
        pnlFilters.Size = New Size(1864, 70)
        pnlFilters.TabIndex = 1
        ' 
        ' btnAddSection
        ' 
        btnAddSection.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnAddSection.BackColor = Color.Transparent
        btnAddSection.BackgroundImageLayout = ImageLayout.Zoom
        btnAddSection.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnAddSection.EnteredBorderColor = Color.Empty
        btnAddSection.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnAddSection.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnAddSection.Image = Nothing
        btnAddSection.ImageAlign = ContentAlignment.MiddleLeft
        btnAddSection.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnAddSection.Location = New Point(1686, 17)
        btnAddSection.Name = "btnAddSection"
        btnAddSection.PressedBorderColor = Color.Transparent
        btnAddSection.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnAddSection.Size = New Size(145, 35)
        btnAddSection.TabIndex = 11
        btnAddSection.Text = "Add Section"
        btnAddSection.TextAlignment = StringAlignment.Center
        btnAddSection.UseWaitCursor = True
        ' 
        ' btnAddCourse
        ' 
        btnAddCourse.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnAddCourse.BackColor = Color.Transparent
        btnAddCourse.BackgroundImageLayout = ImageLayout.Zoom
        btnAddCourse.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnAddCourse.EnteredBorderColor = Color.Empty
        btnAddCourse.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnAddCourse.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnAddCourse.Image = Nothing
        btnAddCourse.ImageAlign = ContentAlignment.MiddleLeft
        btnAddCourse.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnAddCourse.Location = New Point(1521, 17)
        btnAddCourse.Name = "btnAddCourse"
        btnAddCourse.PressedBorderColor = Color.Transparent
        btnAddCourse.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnAddCourse.Size = New Size(145, 35)
        btnAddCourse.TabIndex = 10
        btnAddCourse.Text = "Add Course"
        btnAddCourse.TextAlignment = StringAlignment.Center
        btnAddCourse.UseWaitCursor = True
        ' 
        ' btnAdjustSchedule
        ' 
        btnAdjustSchedule.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnAdjustSchedule.BackColor = Color.Transparent
        btnAdjustSchedule.BackgroundImageLayout = ImageLayout.Zoom
        btnAdjustSchedule.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnAdjustSchedule.EnteredBorderColor = Color.Empty
        btnAdjustSchedule.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnAdjustSchedule.Font = New Font("Century Gothic", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnAdjustSchedule.Image = Nothing
        btnAdjustSchedule.ImageAlign = ContentAlignment.MiddleLeft
        btnAdjustSchedule.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnAdjustSchedule.Location = New Point(1357, 17)
        btnAdjustSchedule.Name = "btnAdjustSchedule"
        btnAdjustSchedule.PressedBorderColor = Color.Transparent
        btnAdjustSchedule.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnAdjustSchedule.Size = New Size(145, 35)
        btnAdjustSchedule.TabIndex = 9
        btnAdjustSchedule.Text = "Adjust Schedule"
        btnAdjustSchedule.TextAlignment = StringAlignment.Center
        btnAdjustSchedule.UseWaitCursor = True
        ' 
        ' cboSemester
        ' 
        cboSemester.Font = New Font("Century Gothic", 16.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cboSemester.FormattingEnabled = True
        cboSemester.Items.AddRange(New Object() {"1st", "2nd", "Mid Year"})
        cboSemester.Location = New Point(1119, 14)
        cboSemester.Name = "cboSemester"
        cboSemester.Size = New Size(225, 41)
        cboSemester.TabIndex = 8
        ' 
        ' cboAY
        ' 
        cboAY.Font = New Font("Century Gothic", 16.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cboAY.FormattingEnabled = True
        cboAY.Items.AddRange(New Object() {"2026-2027"})
        cboAY.Location = New Point(753, 14)
        cboAY.Name = "cboAY"
        cboAY.Size = New Size(225, 41)
        cboAY.TabIndex = 7
        ' 
        ' cboSection
        ' 
        cboSection.Font = New Font("Century Gothic", 16.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cboSection.FormattingEnabled = True
        cboSection.Items.AddRange(New Object() {"BSCS1A", "BSCS2A", "BSCS3A", "BSCS4A"})
        cboSection.Location = New Point(434, 14)
        cboSection.Name = "cboSection"
        cboSection.Size = New Size(225, 41)
        cboSection.TabIndex = 6
        ' 
        ' cboYearLevel
        ' 
        cboYearLevel.Font = New Font("Century Gothic", 16.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cboYearLevel.FormattingEnabled = True
        cboYearLevel.Items.AddRange(New Object() {"1st Year", "2nd Year", "3rd Year", "4th Year"})
        cboYearLevel.Location = New Point(91, 14)
        cboYearLevel.Name = "cboYearLevel"
        cboYearLevel.Size = New Size(225, 41)
        cboYearLevel.TabIndex = 5
        ' 
        ' lblYear
        ' 
        lblYear.AutoSize = True
        lblYear.BackColor = Color.Transparent
        lblYear.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblYear.Location = New Point(18, 13)
        lblYear.Name = "lblYear"
        lblYear.Size = New Size(54, 42)
        lblYear.TabIndex = 0
        lblYear.Text = "Year" & vbCrLf & "Level"
        ' 
        ' lblSection
        ' 
        lblSection.AutoSize = True
        lblSection.BackColor = Color.Transparent
        lblSection.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblSection.Location = New Point(357, 24)
        lblSection.Name = "lblSection"
        lblSection.Size = New Size(71, 21)
        lblSection.TabIndex = 1
        lblSection.Text = "Section"
        ' 
        ' lblAY
        ' 
        lblAY.AutoSize = True
        lblAY.BackColor = Color.Transparent
        lblAY.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblAY.Location = New Point(715, 24)
        lblAY.Name = "lblAY"
        lblAY.Size = New Size(34, 21)
        lblAY.TabIndex = 2
        lblAY.Text = "AY"
        ' 
        ' lblSemester
        ' 
        lblSemester.AutoSize = True
        lblSemester.BackColor = Color.Transparent
        lblSemester.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblSemester.Location = New Point(1030, 24)
        lblSemester.Name = "lblSemester"
        lblSemester.Size = New Size(85, 21)
        lblSemester.TabIndex = 3
        lblSemester.Text = "Semester"
        ' 
        ' pnlStats
        ' 
        pnlStats.Controls.Add(flowStats)
        pnlStats.Dock = DockStyle.Top
        pnlStats.Location = New Point(0, 0)
        pnlStats.Name = "pnlStats"
        pnlStats.Size = New Size(1864, 100)
        pnlStats.TabIndex = 0
        ' 
        ' flowStats
        ' 
        flowStats.Dock = DockStyle.Fill
        flowStats.Location = New Point(0, 0)
        flowStats.Name = "flowStats"
        flowStats.Size = New Size(1864, 100)
        flowStats.TabIndex = 0
        ' 
        ' ucAcademicManagement
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        Controls.Add(pnlAcadBG)
        Name = "ucAcademicManagement"
        Size = New Size(1924, 917)
        pnlAcadBG.ResumeLayout(False)
        pnlCards.ResumeLayout(False)
        pnlFilters.ResumeLayout(False)
        pnlFilters.PerformLayout()
        pnlStats.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlAcadBG As RoundedPanel
    Friend WithEvents pnlCards As Panel
    Friend WithEvents pnlFilters As Panel
    Friend WithEvents pnlStats As Panel
    Friend WithEvents flowStats As FlowLayoutPanel
    Friend WithEvents lblYear As Label
    Friend WithEvents lblSection As Label
    Friend WithEvents lblAY As Label
    Friend WithEvents lblSemester As Label
    Friend WithEvents cboYearLevel As ComboBox
    Friend WithEvents cboSection As ComboBox
    Friend WithEvents cboSemester As ComboBox
    Friend WithEvents cboAY As ComboBox
    Friend WithEvents btnAddSection As ReaLTaiizor.Controls.Button
    Friend WithEvents btnAddCourse As ReaLTaiizor.Controls.Button
    Friend WithEvents btnAdjustSchedule As ReaLTaiizor.Controls.Button
    Friend WithEvents flowCourses As FlowLayoutPanel

End Class
