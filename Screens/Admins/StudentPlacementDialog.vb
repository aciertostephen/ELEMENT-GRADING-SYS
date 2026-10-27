Imports System.Windows.Forms

''' <summary>
''' Yearly placement - the admin decides WHO goes WHERE (nothing is random):
'''   1. "Students currently in": pick last year's section (everyone is ticked) or
'''      "All students" (nobody is ticked, you tick individuals - irregulars, repeaters, transferees).
'''   2. Search by name / student no. and untick anyone who should NOT move.
'''   3. Pick the new section (active or upcoming school year) and press Move.
'''   Run it again for the next group (e.g. BSCS1A -> BSCS2A for some, BSCS2B for the others).
''' Ticks are remembered while you search, so you can find students one by one.
''' </summary>
Public Class StudentPlacementDialog
  Inherits DialogBase

  Private Class StudentItem
    Public Property StudentId As String
    Public Property FullName As String
    Public Property SectionLabel As String
    Public Overrides Function ToString() As String
      Return StudentId & "   " & FullName & "   [" & SectionLabel & "]"
    End Function
  End Class

  Private ReadOnly cboFrom As ComboBox
  Private ReadOnly cboTo As ComboBox
  Private ReadOnly txtSearch As TextBox = MakeText(60)
  Private ReadOnly clb As New CheckedListBox() With {.Height = 190, .CheckOnClick = True, .IntegralHeight = False}
  Private ReadOnly chkShown As New CheckBox() With {.Text = "Tick / untick everyone shown in the list", .AutoSize = True}
  Private ReadOnly lblCount As New Label() With {.AutoSize = True}
  Private ReadOnly _all As New List(Of StudentItem)
  Private ReadOnly _checked As New HashSet(Of String)
  Private _rebuilding As Boolean = True

  Public Property MovedCount As Integer

  Public Sub New(Optional fromSectionId As Integer = 0, Optional toSectionId As Integer = 0)
    MyBase.New("Student Placement", 540)

    Dim dtFrom As DataTable = SectionService.SectionsForPlacement(False)
    Dim allRow As DataRow = dtFrom.NewRow()
    allRow("section_id") = 0
    allRow("label") = "All students (I will tick them one by one)"
    dtFrom.Rows.InsertAt(allRow, 0)

    cboFrom = MakeCombo(dtFrom, "label", "section_id")
    cboTo = MakeCombo(SectionService.SectionsForPlacement(True), "label", "section_id")

    AddNote("You choose exactly who moves. Pick where the students are now, untick anyone who should stay behind, " &
            "then choose their new section. Repeat for each new section.")
    AddField("Students currently in:", cboFrom)
    AddField("Search (name or student no.):", txtSearch)
    AddField("Tick the students to move:", clb)
    AddField("", chkShown)
    AddField("", lblCount)
    AddField("Move the ticked students TO (active or upcoming school year only):", cboTo)
    FinishLayout("Move")

    If fromSectionId > 0 Then cboFrom.SelectedValue = fromSectionId
    If toSectionId > 0 Then cboTo.SelectedValue = toSectionId

    AddHandler cboFrom.SelectedIndexChanged, Sub() ReloadSource()
    AddHandler txtSearch.TextChanged, Sub() RebuildList()
    AddHandler chkShown.Click, Sub() ToggleShown()
    AddHandler clb.ItemCheck, AddressOf OnItemCheck
    ReloadSource()
  End Sub

  Private Function SelectedFromId() As Integer
    If cboFrom.SelectedValue Is Nothing Then Return 0
    Return Convert.ToInt32(cboFrom.SelectedValue)
  End Function

  Private Sub ReloadSource()
    _all.Clear()
    _checked.Clear()
    Dim fromId As Integer = SelectedFromId()
    For Each r As DataRow In SectionService.StudentsForPlacement(fromId).Rows
      Dim item As New StudentItem With {
        .StudentId = r("student_id").ToString(), .FullName = r("full_name").ToString(), .SectionLabel = r("section_label").ToString()}
      _all.Add(item)
      If fromId > 0 Then _checked.Add(item.StudentId)      ' a whole section starts fully ticked
    Next
    If txtSearch.Text <> "" Then
      txtSearch.Text = ""                                    ' triggers RebuildList
    Else
      RebuildList()
    End If
  End Sub

  Private Sub RebuildList()
    _rebuilding = True
    clb.BeginUpdate()
    clb.Items.Clear()
    Dim q As String = txtSearch.Text.Trim()
    For Each s As StudentItem In _all
      If q = "" OrElse s.StudentId.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
         s.FullName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 Then
        clb.Items.Add(s, _checked.Contains(s.StudentId))
      End If
    Next
    clb.EndUpdate()
    _rebuilding = False
    UpdateCount()
  End Sub

  Private Sub OnItemCheck(sender As Object, e As ItemCheckEventArgs)
    If _rebuilding Then Return
    Dim s As StudentItem = TryCast(clb.Items(e.Index), StudentItem)
    If s Is Nothing Then Return
    If e.NewValue = CheckState.Checked Then _checked.Add(s.StudentId) Else _checked.Remove(s.StudentId)
    UpdateCount()
  End Sub

  Private Sub ToggleShown()
    Dim state As Boolean = chkShown.Checked
    _rebuilding = True
    For i As Integer = 0 To clb.Items.Count - 1
      clb.SetItemChecked(i, state)
      Dim s As StudentItem = DirectCast(clb.Items(i), StudentItem)
      If state Then _checked.Add(s.StudentId) Else _checked.Remove(s.StudentId)
    Next
    _rebuilding = False
    UpdateCount()
  End Sub

  Private Sub UpdateCount()
    lblCount.Text = _checked.Count & " ticked of " & _all.Count & " students  (" & clb.Items.Count & " shown)"
  End Sub

  Protected Overrides Function OnSubmit() As Boolean
    If cboTo.SelectedValue Is Nothing Then
      Msg.Warn("Please choose the section the students will move to.")
      Return False
    End If
    Dim toId As Integer = Convert.ToInt32(cboTo.SelectedValue)
    If _checked.Count = 0 Then
      Msg.Warn("Tick at least one student to move.")
      Return False
    End If
    If SelectedFromId() = toId Then
      Msg.Warn("The source and destination sections are the same.")
      Return False
    End If

    If Not Msg.Confirm("Move " & _checked.Count & " student(s) to:" & vbCrLf & cboTo.Text & vbCrLf & vbCrLf &
                       "Each student is notified, and their old section stays in the section history.", "Confirm placement") Then
      Return False
    End If

    Try
      MovedCount = SectionService.MoveStudents(_checked.ToList(), toId)
      Msg.Info(MovedCount & " student(s) were moved.")
      Return True
    Catch ex As InvalidOperationException
      Msg.Warn(ex.Message)
      Return False
    End Try
  End Function
End Class
