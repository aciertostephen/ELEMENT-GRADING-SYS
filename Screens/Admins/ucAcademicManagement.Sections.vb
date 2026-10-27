Imports System.Windows.Forms

' =====================================================================
' ucAcademicManagement - Sections part: right-click menu, Student Placement button.
' =====================================================================
Partial Public Class ucAcademicManagement

    Private _sectionMenu As ContextMenuStrip
    Private _menuSectionId As Integer
    Private btnPlacement As Button

    Friend Sub InitSectionMenu()
        If dgvSections Is Nothing OrElse _sectionMenu IsNot Nothing Then Return

        _sectionMenu = New ContextMenuStrip()
        Dim miStudents As ToolStripItem = _sectionMenu.Items.Add("View students (history)...")
        Dim miClasses As ToolStripItem = _sectionMenu.Items.Add("View this section's classes")
        _sectionMenu.Items.Add(New ToolStripSeparator())
        Dim miEdit As ToolStripItem = _sectionMenu.Items.Add("Edit section...")
        Dim miMove As ToolStripItem = _sectionMenu.Items.Add("Move students into this section...")
        Dim miDelete As ToolStripItem = _sectionMenu.Items.Add("Delete section")
        AddHandler miStudents.Click, Sub() ViewSectionStudents(_menuSectionId)
        AddHandler miClasses.Click, Sub() ViewSectionClasses(_menuSectionId)
        AddHandler miEdit.Click, Sub() EditSection(_menuSectionId)
        AddHandler miMove.Click, Sub() OpenPlacement(0, _menuSectionId)
        AddHandler miDelete.Click, Sub() DeleteSection(_menuSectionId)
        AddHandler dgvSections.CellMouseDown, AddressOf SectionRowMouseDown

        ' "Student Placement" button next to the view dropdown (Sections view only)
        btnPlacement = New Button() With {
            .Text = "Student Placement...",
            .Width = 160,
            .Height = cboViewMode.Height + 2,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left,
            .Visible = False
        }
        btnPlacement.Location = New Point(cboViewMode.Right + 10, cboViewMode.Top - 1)
        cboViewMode.Parent.Controls.Add(btnPlacement)
        btnPlacement.BringToFront()
        AddHandler btnPlacement.Click, Sub() OpenPlacement(0, 0)
    End Sub

    Private Sub SectionRowMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs)
        If e.Button <> MouseButtons.Right OrElse e.RowIndex < 0 Then Return
        dgvSections.ClearSelection()
        dgvSections.Rows(e.RowIndex).Selected = True
        _menuSectionId = Convert.ToInt32(dgvSections.Rows(e.RowIndex).Cells("section_id").Value)
        _sectionMenu.Show(Cursor.Position)
    End Sub

    Private Sub AfterSectionChange()
        PopulateDropdowns()
        SelectActiveYearInFilter()
        LoadAcademicStats()
        RefreshCurrentView()
    End Sub

    ' ---------- history: who was in this section, and its classes ----------
    Private Sub ViewSectionStudents(sectionId As Integer)
        Try
            Using dlg As New SectionRosterDialog(sectionId)
                dlg.ShowDialog(FindForm())
            End Using
        Catch ex As Exception
            Msg.Fail("show the students", ex)
        End Try
    End Sub

    ''' <summary>Jumps to the Classes view filtered to this section's school year, year level and code.</summary>
    Private Sub ViewSectionClasses(sectionId As Integer)
        Try
            Dim info = SectionService.GetInfo(sectionId)
            If info Is Nothing Then Return
            Dim i As Integer = cboAY.Items.IndexOf(info.SchoolYear)
            If i > 0 Then cboAY.SelectedIndex = i
            Dim y As Integer = cboYearLevel.Items.IndexOf(AcademicContext.OrdinalYear(info.YearLevel))
            If y > 0 Then cboYearLevel.SelectedIndex = y
            Dim c As Integer = cboSection.Items.IndexOf(info.Code)
            If c > 0 Then cboSection.SelectedIndex = c
            cboViewMode.SelectedIndex = 0      ' switch to the class cards
        Catch ex As Exception
            Msg.Fail("show the classes", ex)
        End Try
    End Sub

    ' ---------- edit / delete / placement ----------
    Private Sub EditSection(sectionId As Integer)
        Try
            Dim info = SectionService.GetInfo(sectionId)
            If info Is Nothing Then Return
            SectionService.EnsureEditable(info)
            Using dlg As New SectionDialog(sectionId)
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then AfterSectionChange()
            End Using
        Catch ex As InvalidOperationException
            Msg.Warn(ex.Message)
        Catch ex As Exception
            Msg.Fail("edit the section", ex)
        End Try
    End Sub

    Private Sub DeleteSection(sectionId As Integer)
        Try
            Dim info = SectionService.GetInfo(sectionId)
            If info Is Nothing Then Return
            SectionService.EnsureEditable(info)
            If Not Msg.Confirm("Delete section " & info.Code & " (" & info.SchoolYear & ")?", "Delete section", "Delete", True) Then Return
            SectionService.Delete(sectionId)
            AfterSectionChange()
        Catch ex As InvalidOperationException
            Msg.Warn(ex.Message)
        Catch ex As Exception
            Msg.Fail("delete the section", ex)
        End Try
    End Sub

    Private Sub OpenPlacement(fromSectionId As Integer, toSectionId As Integer)
        Try
            Using dlg As New StudentPlacementDialog(fromSectionId, toSectionId)
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then AfterSectionChange()
            End Using
        Catch ex As Exception
            Msg.Fail("open Student Placement", ex)
        End Try
    End Sub

End Class