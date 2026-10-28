Imports System.Text

''' <summary>Preview and bulk-create unplaced student accounts from the registrar's class-list workbook.</summary>
Public Class AdminBulkUserImportDialog
  Inherits DialogBase

  Private Class RowItem
    Public Property SheetName As String
    Public Property Student As ClassListRow
    Public Overrides Function ToString() As String
      Return Student.StudentNo.PadRight(12) & Student.FullName & "  [" & SheetName & "]" &
        If(Student.Note = "", "", "  (" & Student.Note & ")")
    End Function
  End Class

  Private ReadOnly _reader As ClassListExcelReader
  Private ReadOnly _rows As List(Of ClassListRow)
  Private ReadOnly _items As New List(Of RowItem)
  Private ReadOnly clb As New CheckedListBox() With {.CheckOnClick = True, .Height = 300, .IntegralHeight = False, .HorizontalScrollbar = True}
  Private ReadOnly chkAll As New CheckBox() With {.Text = "Select all valid student rows", .AutoSize = True}
  Private _loading As Boolean

  Private Sub New(reader As ClassListExcelReader)
    MyBase.New("Import Student Users", 720)
    _reader = reader
    _rows = New List(Of ClassListRow)()
    For Each sheet As ClassListSheet In reader.Sheets
      For Each row As ClassListRow In reader.ReadStudents(sheet)
        _rows.Add(row)
        _items.Add(New RowItem With {.SheetName = sheet.SheetName, .Student = row})
      Next
    Next

    If _rows.Count = 0 Then Throw New InvalidOperationException("No student rows were found in the matching class-list sheets.")
    AddNote("This imports student accounts only. Students remain unplaced until an admin assigns them to a section. New accounts use the student number as the initial password.")
    AddField("Students from all recognized section sheets:", clb)
    AddField("", chkAll)
    FinishLayout("Import Users")

    For Each item As RowItem In _items
      clb.Items.Add(item, item.Student.Include)
    Next
    AddHandler clb.ItemCheck, AddressOf OnItemCheck
    AddHandler chkAll.Click, Sub() SetAll(chkAll.Checked)
    UpdateAllState()
  End Sub

  Public Shared Function ChooseAndImport(owner As IWin32Window) As Boolean
    If Not SessionManager.IsAdmin Then Throw New InvalidOperationException("Only an administrator can import users.")
    Using picker As New OpenFileDialog() With {
      .Title = "Select the student class-list workbook",
      .Filter = "Excel workbook (*.xlsx;*.xlsm)|*.xlsx;*.xlsm"}
      If picker.ShowDialog(owner) <> DialogResult.OK Then Return False
      Using reader As ClassListExcelReader = ClassListExcelReader.Open(picker.FileName)
        If reader.Sheets.Count = 0 Then Throw New InvalidOperationException("No worksheets match the expected class-list format (for example, BSCS 2A 2026-2027).")
        Using dlg As New AdminBulkUserImportDialog(reader)
          Return dlg.ShowDialog(owner) = DialogResult.OK
        End Using
      End Using
    End Using
  End Function

  Private Sub OnItemCheck(sender As Object, e As ItemCheckEventArgs)
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
    Dim valid = _items.Where(Function(i) i.Student.CanInclude).ToList()
    Dim checked = valid.Where(Function(i) i.Student.Include).Count()
    _loading = True
    chkAll.Checked = valid.Count > 0 AndAlso checked = valid.Count
    _loading = False
  End Sub

  Protected Overrides Function OnSubmit() As Boolean
    Dim selected = _items.Where(Function(i) i.Student.Include AndAlso i.Student.CanInclude).Count()
    If selected = 0 Then
      Msg.Warn("Select at least one valid student row.")
      Return False
    End If
    If Not Msg.Confirm("Create student accounts for " & selected & " selected row(s)? Existing accounts will be left unchanged.", "Confirm user import") Then Return False

    Try
      Dim result = AdminSectionStudentImportService.ImportUsers(_rows)
      Dim summary As New StringBuilder()
      summary.AppendLine("Bulk user import completed.")
      summary.AppendLine("Student accounts created: " & result.AccountsCreated)
      summary.AppendLine("Already had a student account: " & result.ExistingStudentAccounts)
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
      Msg.Fail("import student users", ex)
    End Try
    Return False
  End Function
End Class
