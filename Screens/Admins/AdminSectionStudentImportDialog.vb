Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

''' <summary>Admin imports the matching section sheet after creating the section.</summary>
Public Class AdminSectionStudentImportDialog
  Inherits DialogBase

  Private Class RowItem
    Public Property Student As ClassListRow
    Public Overrides Function ToString() As String
      Return Student.StudentNo.PadRight(12) & Student.FullName & If(Student.Note = "", "", "  [" & Student.Note & "]")
    End Function
  End Class

  Private ReadOnly _section As SectionService.SectionInfo
  Private ReadOnly _reader As ClassListExcelReader
  Private ReadOnly _sheet As ClassListSheet
  Private ReadOnly _rows As List(Of ClassListRow)
  Private ReadOnly clb As New CheckedListBox() With {.CheckOnClick = True, .Height = 280, .IntegralHeight = False, .HorizontalScrollbar = True}
  Private ReadOnly chkAll As New CheckBox() With {.Text = "Select all valid rows", .AutoSize = True}
  Private _loading As Boolean

  Public Sub New(sectionId As Integer, reader As ClassListExcelReader, sheet As ClassListSheet)
    MyBase.New("Import Students Into Section", 700)
    _section = SectionService.GetInfo(sectionId)
    If _section Is Nothing Then Throw New InvalidOperationException("Section not found.")
    _reader = reader
    _sheet = sheet
    _rows = reader.ReadStudents(sheet)
    AddNote("Section: " & _section.Code & " (" & _section.SchoolYear & "). New student accounts will be created here. Existing students in another section will be skipped for Student Placement.")
    AddField("Students from " & sheet.SheetName & ":", clb)
    AddField("", chkAll)
    FinishLayout("Import")
    For Each student As ClassListRow In _rows
      clb.Items.Add(New RowItem With {.Student = student}, student.Include)
    Next
    AddHandler clb.ItemCheck, AddressOf ItemCheck
    AddHandler chkAll.Click, Sub() SetAll(chkAll.Checked)
    UpdateAllState()
  End Sub

  ''' <summary>Selects the matching sheet and opens the preview. Returns True if import completed.</summary>
  Public Shared Function ChooseAndImport(sectionId As Integer, owner As IWin32Window) As Boolean
    Dim section = SectionService.GetInfo(sectionId)
    If section Is Nothing Then Throw New InvalidOperationException("Section not found.")
    Using picker As New OpenFileDialog() With {
      .Title = "Select the section class list (Excel)",
      .Filter = "Excel workbook (*.xlsx;*.xlsm)|*.xlsx;*.xlsm"}
      If picker.ShowDialog(owner) <> DialogResult.OK Then Return False
      Using reader As ClassListExcelReader = ClassListExcelReader.Open(picker.FileName)
        Dim candidates = reader.Sheets.Where(Function(s) _
          String.Equals(s.SchoolYear, section.SchoolYear, StringComparison.OrdinalIgnoreCase) AndAlso
          String.Equals(s.ProgramCode, section.ProgramCode, StringComparison.OrdinalIgnoreCase) AndAlso
          s.YearLevel = section.YearLevel).ToList()
        If candidates.Count = 0 Then
          Throw New InvalidOperationException("This workbook has no class-list sheet for " & section.ProgramCode & " " &
            section.YearLevel.ToString() & " year, school year " & section.SchoolYear & ". Check the section's program, year level, and school year.")
        End If

        Dim sectionCode As String = NormalizeSectionCode(section.Code)
        Dim sheet As ClassListSheet = candidates.FirstOrDefault(
          Function(s) String.Equals(NormalizeSectionCode(s.SectionCode), sectionCode, StringComparison.OrdinalIgnoreCase))
        If sheet Is Nothing AndAlso candidates.Count = 1 Then sheet = candidates(0)
        If sheet Is Nothing Then
          Using chooseSheet As New SheetPickerDialog(section, candidates)
            If chooseSheet.ShowDialog(owner) <> DialogResult.OK Then Return False
            sheet = chooseSheet.SelectedSheet
          End Using
        End If

        Using dlg As New AdminSectionStudentImportDialog(sectionId, reader, sheet)
          Return dlg.ShowDialog(owner) = DialogResult.OK
        End Using
      End Using
    End Using
  End Function

  Private Sub ItemCheck(sender As Object, e As ItemCheckEventArgs)
    If _loading OrElse e.Index < 0 Then Return
    Dim item As RowItem = TryCast(clb.Items(e.Index), RowItem)
    If item Is Nothing Then Return
    If e.NewValue = CheckState.Checked AndAlso Not item.Student.CanInclude Then
      e.NewValue = e.CurrentValue
      Return
    End If
    item.Student.Include = (e.NewValue = CheckState.Checked)
    BeginInvoke(New Action(AddressOf UpdateAllState))
  End Sub

  Private Sub SetAll(value As Boolean)
    _loading = True
    For i As Integer = 0 To clb.Items.Count - 1
      Dim item As RowItem = DirectCast(clb.Items(i), RowItem)
      If item.Student.CanInclude Then
        item.Student.Include = value
        clb.SetItemChecked(i, value)
      End If
    Next
    _loading = False
    UpdateAllState()
  End Sub

  Private Sub UpdateAllState()
    If Disposing OrElse IsDisposed Then Return
    Dim total As Integer = _rows.Where(Function(r) r.CanInclude).Count()
    Dim checked As Integer = _rows.Where(Function(r) r.CanInclude AndAlso r.Include).Count()
    _loading = True
    chkAll.Checked = total > 0 AndAlso checked = total
    _loading = False
  End Sub

  Protected Overrides Function OnSubmit() As Boolean
    Dim selected As Integer = _rows.Where(Function(r) r.CanInclude AndAlso r.Include).Count()
    If selected = 0 Then
      Msg.Warn("Select at least one valid student row.")
      Return False
    End If
    If Not Msg.Confirm("Import " & selected & " student row(s) from worksheet """ & _sheet.SheetName &
                       """ into section " & _section.Code & "? New accounts use the student number as the initial password.",
                       "Confirm section import") Then Return False
    Try
      Dim result = AdminSectionStudentImportService.ImportToSection(_section.SectionId, _sheet, _rows)
      Dim summary As New StringBuilder()
      summary.AppendLine("Section roster import completed.")
      summary.AppendLine("New accounts created: " & result.AccountsCreated)
      summary.AppendLine("Existing students placed here: " & result.StudentsPlaced)
      summary.AppendLine("Already in this section: " & result.AlreadyInSection)
      If result.Skipped.Count > 0 Then
        summary.AppendLine()
        summary.AppendLine("Skipped:")
        For Each message As String In result.Skipped.Take(10)
          summary.AppendLine("- " & message)
        Next
        If result.Skipped.Count > 10 Then summary.AppendLine("...and " & (result.Skipped.Count - 10) & " more")
      End If
      Msg.Info(summary.ToString().TrimEnd(), "Import complete")
      Return True
    Catch ex As InvalidOperationException
      Msg.Warn(ex.Message)
    Catch ex As Exception
      Msg.Fail("import students into the section", ex)
    End Try
    Return False
  End Function

  Private Shared Function NormalizeSectionCode(value As String) As String
    Return Regex.Replace(If(value, ""), "[^A-Za-z0-9]", "").ToUpperInvariant()
  End Function

  Private NotInheritable Class SheetPickerDialog
    Inherits DialogBase

    Private ReadOnly _sheetCombo As New ComboBox() With {.DropDownStyle = ComboBoxStyle.DropDownList}

    Public ReadOnly Property SelectedSheet As ClassListSheet
      Get
        Return TryCast(_sheetCombo.SelectedItem, ClassListSheet)
      End Get
    End Property

    Public Sub New(section As SectionService.SectionInfo, candidates As IEnumerable(Of ClassListSheet))
      MyBase.New("Choose Class-List Sheet", 520)
      AddNote("No worksheet name exactly matches section " & section.Code & ". Choose the correct sheet for " &
              section.ProgramCode & " " & section.YearLevel.ToString() & " year, " & section.SchoolYear & ".")
      AddField("Worksheet:", _sheetCombo)
      For Each sheet As ClassListSheet In candidates
        _sheetCombo.Items.Add(sheet)
      Next
      If _sheetCombo.Items.Count > 0 Then _sheetCombo.SelectedIndex = 0
      FinishLayout("Continue")
    End Sub

    Protected Overrides Function OnSubmit() As Boolean
      If SelectedSheet Is Nothing Then
        Msg.Warn("Choose a worksheet to continue.")
        Return False
      End If
      Return True
    End Function
  End Class
End Class
