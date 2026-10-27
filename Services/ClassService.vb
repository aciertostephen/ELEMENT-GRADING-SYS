Imports MySqlConnector

''' <summary>Everything about a class = Course x Teacher x Term x Section(label only).</summary>
Public Class ClassService

  Public Class ClassInfo
    Public Property ClassId As Integer
    Public Property CourseId As Integer
    Public Property SectionId As Integer
    Public Property TeacherId As String
    Public Property TermId As Integer
    Public Property SchoolYear As String
    Public Property Title As String
    Public Property IsReleased As Boolean
    Public Property ScoreCount As Integer
    Public Property StudentCount As Integer
  End Class

  Public Shared Function GetInfo(classId As Integer) As ClassInfo
    Dim dt = DatabaseHelper.ExecuteQuery(
      "SELECT c.class_id, c.course_id, c.section_id, c.teacher_id, c.term_id, t.school_year, " &
      "       CONCAT(co.course_code, ' - ', s.section_code) AS title, c.grades_released_at, " &
      "       (SELECT COUNT(*) FROM grade_entry ge JOIN enrollment e ON e.enrollment_id = ge.enrollment_id " &
      "         WHERE e.class_id = c.class_id AND ge.score IS NOT NULL) AS score_count, " &
      "       (SELECT COUNT(*) FROM enrollment e WHERE e.class_id = c.class_id AND e.status <> 'Dropped') AS stu_count " &
      "FROM classes c JOIN course co ON co.course_id = c.course_id " &
      "JOIN section s ON s.section_id = c.section_id JOIN academic_term t ON t.term_id = c.term_id " &
      "WHERE c.class_id = @c", {DatabaseHelper.P("@c", classId)})
    If dt.Rows.Count = 0 Then Return Nothing
    Dim r = dt.Rows(0)
    Return New ClassInfo With {
      .ClassId = classId, .CourseId = Convert.ToInt32(r("course_id")), .SectionId = Convert.ToInt32(r("section_id")),
      .TeacherId = r("teacher_id").ToString(), .TermId = Convert.ToInt32(r("term_id")),
      .SchoolYear = r("school_year").ToString(), .Title = r("title").ToString(),
      .IsReleased = Not IsDBNull(r("grades_released_at")),
      .ScoreCount = Convert.ToInt32(r("score_count")), .StudentCount = Convert.ToInt32(r("stu_count"))}
  End Function

  ''' <summary>Ownership check (fixes: any teacher could open any classId).</summary>
  Public Shared Function TeacherOwnsClass(classId As Integer, userId As Integer) As Boolean
    Return DatabaseHelper.ScalarInt(
      "SELECT COUNT(*) FROM classes c JOIN teacher_profile tp ON tp.teacher_id = c.teacher_id " &
      "WHERE c.class_id = @c AND tp.user_id = @u",
      {DatabaseHelper.P("@c", classId), DatabaseHelper.P("@u", userId)}) > 0
  End Function

  Public Shared Function Create(courseId As Integer, sectionId As Integer, teacherId As String, termId As Integer) As Integer
    EnsureSectionMatchesTerm(sectionId, termId)
    Try
      Dim r As Object = DatabaseHelper.ExecuteScalar("CALL sp_create_class(@co, @se, @te, @tm)",
        {DatabaseHelper.P("@co", courseId), DatabaseHelper.P("@se", sectionId),
         DatabaseHelper.P("@te", teacherId), DatabaseHelper.P("@tm", termId)})
      Dim newId As Integer = If(r Is Nothing, 0, Convert.ToInt32(r))
      If newId > 0 Then
        NotifySafe(Sub() NotificationService.NotifyTeacher(teacherId,
                         "You were assigned to " & Label(newId) & ". Open the class to import your class list.", newId))
      End If
      Return newId
    Catch ex As MySqlException When ex.Number = 1062
      Throw New InvalidOperationException("That course already has a class for this section in this term.")
    End Try
  End Function

  Public Shared Sub Update(classId As Integer, courseId As Integer, sectionId As Integer, teacherId As String)
    Dim info = GetInfo(classId)
    If info Is Nothing Then Throw New InvalidOperationException("Class not found.")
    EnsureEditable(info)
    EnsureSectionMatchesTerm(sectionId, info.TermId)
    Dim oldLabel As String = Label(classId)
    Try
      DatabaseHelper.ExecuteNonQuery(
        "UPDATE classes SET course_id = @co, section_id = @se, teacher_id = @te WHERE class_id = @c",
        {DatabaseHelper.P("@co", courseId), DatabaseHelper.P("@se", sectionId),
         DatabaseHelper.P("@te", teacherId), DatabaseHelper.P("@c", classId)})
    Catch ex As MySqlException When ex.Number = 1062
      Throw New InvalidOperationException("That course already has a class for this section in this term.")
    End Try

    ' tell the teachers what happened (a failed notice never undoes the change)
    NotifySafe(Sub()
                 If teacherId <> info.TeacherId Then
                   NotificationService.NotifyTeacher(info.TeacherId, "You are no longer assigned to " & oldLabel & ".")
                   NotificationService.NotifyTeacher(teacherId, "You were assigned to " & Label(classId) & ". Open the class to import your class list.", classId)
                 ElseIf courseId <> info.CourseId OrElse sectionId <> info.SectionId Then
                   NotificationService.NotifyTeacher(teacherId, "Your class " & oldLabel & " was changed to " & Label(classId) & ".", classId)
                 End If
               End Sub)
  End Sub

  ''' <summary>Deletes a class and everything under it, atomically.</summary>
  Public Shared Sub Delete(classId As Integer)
    Dim info = GetInfo(classId)
    If info Is Nothing Then Return
    EnsureEditable(info)
    If info.IsReleased Then
      Throw New InvalidOperationException("Grades for this class were already released to students, so it can no longer be deleted.")
    End If
    Dim deletedLabel As String = Label(classId)
    DatabaseHelper.RunInTransaction(
      Sub(tx)
        Dim c = DatabaseHelper.P("@c", classId)
        tx.Exec("DELETE FROM grade_entry WHERE enrollment_id IN (SELECT enrollment_id FROM enrollment WHERE class_id = @c)", c)
        tx.Exec("DELETE FROM enrollment WHERE class_id = @c", DatabaseHelper.P("@c", classId))
        ' trg_rubric_fixed_delete blocks deleting the fixed Final Exam row, so unfix first
        tx.Exec("UPDATE grading_rubric SET is_fixed = 0 WHERE class_id = @c", DatabaseHelper.P("@c", classId))
        tx.Exec("DELETE FROM grading_rubric WHERE class_id = @c", DatabaseHelper.P("@c", classId))  ' grade_item cascades
        tx.Exec("DELETE FROM classes WHERE class_id = @c", DatabaseHelper.P("@c", classId))        ' notifications -> SET NULL
      End Sub)

    NotifySafe(Sub() NotificationService.NotifyTeacher(info.TeacherId, "Your class " & deletedLabel & " was deleted by the admin."))
  End Sub

  ''' <summary>
  ''' Why the gradebook structure (columns, weights, drop / un-drop) cannot change right now, or Nothing.
  ''' Same rules the database triggers apply to scores: past school year, locked term, encoding window.
  ''' </summary>
  Public Shared Function CannotChangeGradebookReason(classId As Integer) As String
    Dim info As ClassInfo = GetInfo(classId)
    If info Is Nothing Then Return "Class not found."
    If Not AcademicContext.IsEditableYear(info.SchoolYear) Then
      Return "School year " & info.SchoolYear & " is past history and is read-only."
    End If
    Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
      "SELECT is_locked, (NOW() < encoding_start) AS not_open, (NOW() > encoding_deadline) AS is_closed " &
      "FROM academic_term WHERE term_id = @t", {DatabaseHelper.P("@t", info.TermId)})
    If dt.Rows.Count > 0 Then
      If Convert.ToInt32(dt.Rows(0)("is_locked")) = 1 Then Return "This term is locked, so the gradebook can no longer change."
      If Convert.ToInt32(dt.Rows(0)("not_open")) = 1 Then Return "Grade encoding for this term has not opened yet."
      If Convert.ToInt32(dt.Rows(0)("is_closed")) = 1 Then Return "The grade encoding deadline has passed."
    End If
    Return Nothing
  End Function

  Public Shared Sub EnsureEditable(info As ClassInfo)
    If Not AcademicContext.IsEditableYear(info.SchoolYear) Then
      Throw New InvalidOperationException("School year " & info.SchoolYear & " is past history and is read-only.")
    End If
  End Sub

  Private Shared Sub EnsureSectionMatchesTerm(sectionId As Integer, termId As Integer)
    Dim ok As Integer = DatabaseHelper.ScalarInt(
      "SELECT COUNT(*) FROM section s JOIN academic_term t ON t.school_year = s.school_year " &
      "WHERE s.section_id = @s AND t.term_id = @t",
      {DatabaseHelper.P("@s", sectionId), DatabaseHelper.P("@t", termId)})
    If ok = 0 Then Throw New InvalidOperationException("The section must belong to the same school year as the class's term.")
  End Sub

  ''' <summary>"CS 103 - BSCS2A (2026-2027, 1st Semester)" for messages.</summary>
  Public Shared Function Label(classId As Integer) As String
    Dim r As Object = DatabaseHelper.ExecuteScalar(
      "SELECT CONCAT(co.course_code, ' - ', s.section_code, ' (', t.school_year, ', ', " &
      "       IF(t.semester = 1, '1st Semester', IF(t.semester = 2, '2nd Semester', 'Summer')), ')') " &
      "FROM classes c JOIN course co ON co.course_id = c.course_id JOIN section s ON s.section_id = c.section_id " &
      "JOIN academic_term t ON t.term_id = c.term_id WHERE c.class_id = @c", {DatabaseHelper.P("@c", classId)})
    Return If(r Is Nothing, "a class", r.ToString())
  End Function

  ''' <summary>A failed notice must never undo (or hide) a successful class change.</summary>
  Private Shared Sub NotifySafe(work As Action)
    Try
      work()
    Catch
    End Try
  End Sub

  Public Shared Function TeachersForCombo() As DataTable
    Return DatabaseHelper.ExecuteQuery(
      "SELECT teacher_id, CONCAT(last_name, ', ', first_name) AS teacher_name FROM teacher_profile ORDER BY last_name, first_name")
  End Function

  Public Shared Function SectionsForCombo(schoolYear As String) As DataTable
    Return DatabaseHelper.ExecuteQuery(
      "SELECT section_id, CONCAT(section_code, ' (', school_year, ')') AS section_label FROM section " &
      "WHERE school_year = @sy ORDER BY section_code", {DatabaseHelper.P("@sy", schoolYear)})
  End Function
End Class
