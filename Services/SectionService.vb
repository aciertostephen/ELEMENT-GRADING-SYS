Imports System.Data
Imports System.Text.RegularExpressions
Imports MySqlConnector

''' <summary>
''' Sections (BSCS2A in 2026-2027 is a different row from BSCS2A in 2027-2028) and the yearly
''' student placement. Past school years are read-only history.
''' Who was in a section is kept FOREVER in student_section_history, and each class keeps its
''' own class list in enrollment, so emptying a section never loses the old batch.
''' </summary>
Public Class SectionService

  Public Class SectionInfo
    Public Property SectionId As Integer
    Public Property Code As String
    Public Property ProgramId As Integer
    Public Property ProgramCode As String
    Public Property SchoolYear As String
    Public Property YearLevel As Integer
    Public Property Batch As String
    Public Property StudentCount As Integer      ' students in the section RIGHT NOW
    Public Property HistoryCount As Integer      ' students who were EVER placed in it
    Public Property ClassCount As Integer
  End Class

  Public Shared Function GetInfo(sectionId As Integer) As SectionInfo
    Dim dt = DatabaseHelper.ExecuteQuery(
      "SELECT s.section_id, s.section_code, s.program_id, p.program_code, s.school_year, s.year_level, " &
      "       COALESCE(s.batch, '') AS batch, " &
      "       (SELECT COUNT(*) FROM student_profile sp WHERE sp.section_id = s.section_id) AS stu, " &
      "       (SELECT COUNT(DISTINCT h.student_id) FROM student_section_history h WHERE h.section_id = s.section_id) AS hist, " &
      "       (SELECT COUNT(*) FROM classes c WHERE c.section_id = s.section_id) AS cls " &
      "FROM section s JOIN program p ON p.program_id = s.program_id WHERE s.section_id = @s",
      {DatabaseHelper.P("@s", sectionId)})
    If dt.Rows.Count = 0 Then Return Nothing
    Dim r = dt.Rows(0)
    Return New SectionInfo With {
      .SectionId = sectionId, .Code = r("section_code").ToString(), .ProgramId = Convert.ToInt32(r("program_id")),
      .ProgramCode = r("program_code").ToString(), .SchoolYear = r("school_year").ToString(),
      .YearLevel = Convert.ToInt32(r("year_level")), .Batch = r("batch").ToString(),
      .StudentCount = Convert.ToInt32(r("stu")), .HistoryCount = Convert.ToInt32(r("hist")),
      .ClassCount = Convert.ToInt32(r("cls"))}
  End Function

  Public Shared Sub EnsureEditable(info As SectionInfo)
    If Not AcademicContext.IsEditableYear(info.SchoolYear) Then
      Throw New InvalidOperationException("School year " & info.SchoolYear & " is past history and is read-only.")
    End If
  End Sub

  ''' <summary>Program, school year and year level define the section, so only the code and batch can change.</summary>
  Public Shared Sub Update(sectionId As Integer, code As String, batch As String)
    Dim info = GetInfo(sectionId)
    If info Is Nothing Then Throw New InvalidOperationException("Section not found.")
    EnsureEditable(info)

    code = If(code, "").Trim()
    batch = If(batch, "").Trim()
    If code.Length = 0 Then Throw New InvalidOperationException("Section code is required.")
    If code.Length > 15 Then Throw New InvalidOperationException("Section code can be at most 15 characters.")
    If batch <> "" AndAlso Not Regex.IsMatch(batch, "^\d{4}-\d{4}$") Then
      Throw New InvalidOperationException("Batch must look like 2025-2026, or be left blank.")
    End If

    Dim dup As Integer = DatabaseHelper.ScalarInt(
      "SELECT COUNT(*) FROM section WHERE program_id = @p AND school_year = @sy AND year_level = @yl " &
      "AND section_code = @c AND section_id <> @id",
      {DatabaseHelper.P("@p", info.ProgramId), DatabaseHelper.P("@sy", info.SchoolYear),
       DatabaseHelper.P("@yl", info.YearLevel), DatabaseHelper.P("@c", code), DatabaseHelper.P("@id", sectionId)})
    If dup > 0 Then Throw New InvalidOperationException("Section " & code & " already exists for that program, school year and year level.")

    DatabaseHelper.ExecuteNonQuery(
      "UPDATE section SET section_code = @c, batch = @b WHERE section_id = @id",
      {DatabaseHelper.P("@c", code), DatabaseHelper.P("@b", If(batch = "", Nothing, batch)), DatabaseHelper.P("@id", sectionId)})
  End Sub

  ''' <summary>Only a section that never had students and has no classes can be deleted.</summary>
  Public Shared Sub Delete(sectionId As Integer)
    Dim info = GetInfo(sectionId)
    If info Is Nothing Then Return
    EnsureEditable(info)
    If info.StudentCount > 0 Then
      Throw New InvalidOperationException(info.Code & " still has " & info.StudentCount &
        " student(s). Move them to another section first (Student Placement).")
    End If
    If info.HistoryCount > 0 Then
      Throw New InvalidOperationException(info.Code & " (" & info.SchoolYear & ") had " & info.HistoryCount &
        " student(s) placed in it before, so it is kept as history and cannot be deleted.")
    End If
    If info.ClassCount > 0 Then
      Throw New InvalidOperationException(info.Code & " still has " & info.ClassCount & " class(es). Delete those classes first.")
    End If
    DatabaseHelper.ExecuteNonQuery("DELETE FROM section WHERE section_id = @id", {DatabaseHelper.P("@id", sectionId)})
  End Sub

  ' ------------------------------------------------------------------
  ' Student Placement
  ' ------------------------------------------------------------------

  ''' <summary>Sections for a dropdown: section_id, label. editableOnly = only active / upcoming years.</summary>
  Public Shared Function SectionsForPlacement(editableOnly As Boolean) As DataTable
    Dim sql As String =
      "SELECT s.section_id, " &
      "  CONCAT(s.school_year, '  |  ', s.section_code, '  (', " &
      "    (SELECT COUNT(*) FROM student_profile sp WHERE sp.section_id = s.section_id), ' students now)') AS label " &
      "FROM section s "
    Dim active As String = AcademicContext.ActiveSchoolYear()
    If editableOnly AndAlso Not String.IsNullOrEmpty(active) Then
      Return DatabaseHelper.ExecuteQuery(sql & "WHERE s.school_year >= @a ORDER BY s.school_year DESC, s.year_level, s.section_code",
                                         {DatabaseHelper.P("@a", active)})
    End If
    Return DatabaseHelper.ExecuteQuery(sql & "ORDER BY s.school_year DESC, s.year_level, s.section_code")
  End Function

  ''' <summary>Students a section currently holds. sectionId = 0 -> every student (admin picks individually).</summary>
  Public Shared Function StudentsForPlacement(sectionId As Integer) As DataTable
    Dim sql As String =
      "SELECT sp.student_id, " &
      "  CONCAT(sp.last_name, ', ', sp.first_name, IF(sp.middle_name IS NULL OR sp.middle_name = '', '', CONCAT(' ', sp.middle_name))) AS full_name, " &
      "  CONCAT(s.section_code, ' ', s.school_year) AS section_label " &
      "FROM student_profile sp JOIN section s ON s.section_id = sp.section_id "
    If sectionId > 0 Then
      Return DatabaseHelper.ExecuteQuery(sql & "WHERE sp.section_id = @s ORDER BY sp.last_name, sp.first_name", {DatabaseHelper.P("@s", sectionId)})
    End If
    Return DatabaseHelper.ExecuteQuery(sql & "ORDER BY sp.last_name, sp.first_name")
  End Function

  ''' <summary>
  ''' Everyone who was ever in this section (student_section_history), so an old section such as
  ''' BSCS2A 2026-2027 still shows its batch after the students moved up.
  ''' </summary>
  Public Shared Function RosterHistory(sectionId As Integer) As DataTable
    Return DatabaseHelper.ExecuteQuery(
      "SELECT sp.student_id, " &
      "  CONCAT(sp.last_name, ', ', sp.first_name) AS full_name, " &
      "  h.effective_from, h.effective_to, " &
      "  IF(h.effective_to IS NULL, 'In this section now', 'Moved on') AS status " &
      "FROM student_section_history h JOIN student_profile sp ON sp.student_id = h.student_id " &
      "WHERE h.section_id = @s ORDER BY sp.last_name, sp.first_name, h.effective_from",
      {DatabaseHelper.P("@s", sectionId)})
  End Function

  ''' <summary>
  ''' Moves the chosen students to a new section. DB triggers close the old history row, open a new
  ''' one and refresh year_level. Each student gets a notification. Returns how many moved.
  ''' </summary>
  Public Shared Function MoveStudents(studentIds As IEnumerable(Of String), destSectionId As Integer) As Integer
    Dim dest = GetInfo(destSectionId)
    If dest Is Nothing Then Throw New InvalidOperationException("Destination section not found.")
    EnsureEditable(dest)

    Dim moved As Integer = 0
    DatabaseHelper.RunInTransaction(
      Sub(tx)
        For Each id As String In studentIds
          Dim n As Integer = tx.Exec("UPDATE student_profile SET section_id = @d WHERE student_id = @s AND section_id <> @d",
                                     DatabaseHelper.P("@d", destSectionId), DatabaseHelper.P("@s", id))
          moved += n
          If n > 0 Then
            Dim uid As Object = tx.Scalar("SELECT user_id FROM student_profile WHERE student_id = @s", DatabaseHelper.P("@s", id))
            If uid IsNot Nothing Then
              NotificationService.SendNotice(tx, Convert.ToInt32(uid), "Class Update",
                "You have been placed in section " & dest.Code & " (" & dest.SchoolYear & ", " & AcademicContext.OrdinalYear(dest.YearLevel) & ").")
            End If
          End If
        Next
      End Sub)
    Return moved
  End Function
End Class
