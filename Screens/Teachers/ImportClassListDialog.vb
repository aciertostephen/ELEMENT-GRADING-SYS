Imports System.Text
Imports System.Windows.Forms

''' <summary>
''' Teacher: import a class list (Excel) into ONE class.
'''   1. Pick the sheet (the sheet that matches the class's section is pre-selected; an irregular
'''      student can be imported from any other sheet).
'''   2. Review the students. Rows that are yellow / marked withdrawn, LOA, no show ... start unticked
'''      (tick them if they really are enrolled). Rows without a student number cannot be imported.
'''   3. Press Import. Only students with an existing account are added to this class's list.
''' </summary>
Public Class ImportClassListDialog
  Inherits DialogBase

  Private Class RowItem
    Public Property Row As ClassListRow
    Public Overrides Function ToString() As String
      Dim s As String = Row.StudentNo.PadRight(9) & "  " & Row.FullName
      If Row.Note <> "" Then s &= "     [" & Row.Note & "]"
      Return s
    End Function
  End Class

  Private ReadOnly _classId As Integer
  Private ReadOnly _info As ClassService.ClassInfo
  Private ReadOnly _reader As ClassListExcelReader
  Private ReadOnly cboSheet As New ComboBox() With {.DropDownStyle = ComboBoxStyle.DropDownList}
  Private ReadOnly clb As New CheckedListBox() With {.Height = 250, .CheckOnClick = True, .IntegralHeight = False, .HorizontalScrollbar = True}
    Private ReadOnly chkAll As New CheckBox() With {.Text = "Select students to enroll", .AutoSize = True}
    Private ReadOnly lblStatus As New Label() With {.AutoSize = False, .Height = 34}
  Private _rows As New List(Of ClassListRow)
  Private _loading As Boolean = True

  ''' <summary>Filled after a successful import.</summary>
  Public Property Result As ClassListImportService.ImportResult

  Public Sub New(classId As Integer, reader As ClassListExcelReader)
    MyBase.New("Import Class List", 680)
    _classId = classId
    _reader = reader
    _info = ClassService.GetInfo(classId)
    If _info Is Nothing Then Throw New InvalidOperationException("Class not found.")

    For Each s As ClassListSheet In reader.Sheets
      cboSheet.Items.Add(s)
    Next
    If cboSheet.Items.Count > 0 Then cboSheet.SelectedIndex = DefaultSheetIndex()

    AddNote("Importing into:  " & _info.Title & "  (" & _info.SchoolYear & ")")
        AddNote("Only existing student accounts can be enrolled. Ask an administrator to create missing accounts.")
        AddField("Class list sheet:", cboSheet)
        AddField("Students in the sheet :", clb)
        AddField("", chkAll)
    AddField("", lblStatus)
    FinishLayout("Import")

    AddHandler cboSheet.SelectedIndexChanged, Sub() LoadSheet()
    AddHandler clb.ItemCheck, AddressOf OnItemCheck
    AddHandler chkAll.Click, Sub() ToggleAll()
    LoadSheet()
  End Sub

  ''' <summary>Same section + school year as the class, else same school year, else the first sheet.</summary>
  Private Function DefaultSheetIndex() As Integer
    Dim sec As SectionService.SectionInfo = SectionService.GetInfo(_info.SectionId)
    Dim best As Integer = 0
    Dim bestScore As Integer = -1
    For i As Integer = 0 To _reader.Sheets.Count - 1
      Dim s As ClassListSheet = _reader.Sheets(i)
      Dim score As Integer = 0
      If s.SchoolYear = _info.SchoolYear Then score += 1
      If sec IsNot Nothing AndAlso String.Equals(s.SectionCode, sec.Code, StringComparison.OrdinalIgnoreCase) Then score += 2
      If score > bestScore Then
        bestScore = score
        best = i
      End If
    Next
    Return best
  End Function

  Private Function SelectedSheet() As ClassListSheet
    Return TryCast(cboSheet.SelectedItem, ClassListSheet)
  End Function

  Private Sub LoadSheet()
    _rows = New List(Of ClassListRow)
    Dim sheet As ClassListSheet = SelectedSheet()
    If sheet IsNot Nothing Then
      Try
        _rows = _reader.ReadStudents(sheet)
      Catch ex As Exception
        Msg.Fail("read that sheet", ex)
      End Try
    End If

    _loading = True
    clb.BeginUpdate()
    clb.Items.Clear()
    For Each r As ClassListRow In _rows
      clb.Items.Add(New RowItem With {.Row = r}, r.Include)
    Next
    clb.EndUpdate()
    chkAll.Checked = False
    _loading = False
    UpdateStatus()
  End Sub

  Private Sub OnItemCheck(sender As Object, e As ItemCheckEventArgs)
    If _loading Then Return
    Dim item As RowItem = TryCast(clb.Items(e.Index), RowItem)
    If item Is Nothing Then Return
    If e.NewValue = CheckState.Checked AndAlso Not item.Row.CanInclude Then
      e.NewValue = e.CurrentValue          ' no student number / no proper name: cannot be ticked
      Return
    End If
    item.Row.Include = (e.NewValue = CheckState.Checked)
    UpdateStatus()
  End Sub

  Private Sub ToggleAll()
    Dim state As Boolean = chkAll.Checked
    _loading = True
    For i As Integer = 0 To clb.Items.Count - 1
      Dim item As RowItem = DirectCast(clb.Items(i), RowItem)
      If item.Row.CanInclude Then
        item.Row.Include = state
        clb.SetItemChecked(i, state)
      End If
    Next
    _loading = False
    UpdateStatus()
  End Sub

  Private Sub UpdateStatus()
    Dim picked As Integer = _rows.Where(Function(r) r.Include).Count()
    Dim includable As Integer = _rows.Where(Function(r) r.CanInclude).Count()
    chkAll.Checked = (includable > 0 AndAlso picked = includable)     ' (does not raise Click, so no loop)
    Dim text As String = picked & " student(s) will be imported, " & (_rows.Count - picked) & " skipped."
    lblStatus.ForeColor = SystemColors.ControlText

    Dim sheet As ClassListSheet = SelectedSheet()
    If sheet IsNot Nothing AndAlso sheet.SchoolYear <> _info.SchoolYear Then
      text &= vbCrLf & "This sheet is for " & sheet.SchoolYear & " but this class is " & _info.SchoolYear & ". It cannot be imported."
      lblStatus.ForeColor = SystemColors.ControlText
    End If
    lblStatus.Text = text
  End Sub

  Protected Overrides Function OnSubmit() As Boolean
    Dim sheet As ClassListSheet = SelectedSheet()
    If sheet Is Nothing Then
      Msg.Warn("Please choose the sheet to import.")
      Return False
    End If
    If sheet.SchoolYear <> _info.SchoolYear Then
      Msg.Warn("This sheet is for school year " & sheet.SchoolYear & " but the class belongs to " & _info.SchoolYear &
               ". Please choose a sheet of the same school year.")
      Return False
    End If

    Dim blocked As String = ClassListImportService.CannotImportReason(_info, SessionManager.CurrentUserId)
    If blocked IsNot Nothing Then
      Msg.Warn(blocked)
      Return False
    End If

    Dim picked As Integer = _rows.Where(Function(r) r.Include).Count()
    If picked = 0 Then
      Msg.Warn("Tick at least one student to import.")
      Return False
    End If

    If Not Msg.Confirm("Import " & picked & " student(s) from """ & sheet.SheetName.Trim() & """ into " & _info.Title & "?",
                       "Confirm Import") Then Return False

    Try
      Cursor.Current = Cursors.WaitCursor
      Result = ClassListImportService.ImportIntoClass(_classId, SessionManager.CurrentUserId, sheet, _rows)
    Catch ex As Exception
      Msg.Fail("import the class list", ex)
      Return False
    End Try

    Msg.Info(BuildSummary(Result), "Import complete")
    Return True
  End Function

  Private Function BuildSummary(r As ClassListImportService.ImportResult) As String
    Dim sb As New StringBuilder()
    sb.AppendLine("Class list imported into " & _info.Title & ".")
    sb.AppendLine()
    sb.AppendLine("Added to this class: " & (r.Enrolled + r.Reenrolled) &
                  If(r.Reenrolled > 0, "  (" & r.Reenrolled & " re-enrolled)", ""))
    sb.AppendLine("Already on this class list: " & r.AlreadyInClass)
    sb.AppendLine("Already had an account: " & r.ExistingAccounts)
    If r.SectionsCreated.Count > 0 Then
      sb.AppendLine("Section created: " & String.Join(", ", r.SectionsCreated))
    End If
    If r.Skipped.Count > 0 Then
      sb.AppendLine()
      sb.AppendLine("Skipped (" & r.Skipped.Count & "):")
      For Each s As String In r.Skipped.Take(8)
        sb.AppendLine("  - " & s)
      Next
      If r.Skipped.Count > 8 Then sb.AppendLine("  ... and " & (r.Skipped.Count - 8) & " more")
    End If
    Return sb.ToString().TrimEnd()
  End Function
End Class
