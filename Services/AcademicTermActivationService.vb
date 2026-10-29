Imports System.Data
Imports System.Collections.Generic
Imports MySqlConnector

''' <summary>
''' Activates an academic term and prepares its missing section/class records from the closest prior setup.
''' Score entries and publication timestamps are deliberately term-specific and are never copied.
''' </summary>
Public NotInheritable Class AcademicTermActivationService

  Public Class ActivationResult
    Public Property SectionsCreated As Integer
    Public Property ClassesCreated As Integer
    Public Property StudentsEnrolled As Integer
    Public Property SourceTermLabel As String = ""
  End Class

  Private Class TeacherNotice
    Public Property TeacherId As String
    Public Property ClassId As Integer
  End Class

  Private Sub New()
  End Sub

  Public Shared Function Activate(termId As Integer) As ActivationResult
    If Not SessionManager.IsAdmin Then Throw New InvalidOperationException("Only an administrator can activate an academic term.")
    If termId <= 0 Then Throw New InvalidOperationException("Choose a valid academic term.")

    Dim result As New ActivationResult()
    Dim teacherNotices As New List(Of TeacherNotice)()

    Using conn As MySqlConnection = DatabaseHelper.GetConnection()
      conn.Open()
      Using trans As MySqlTransaction = conn.BeginTransaction()
        Try
          Dim target As DataTable = Query(conn, trans,
            "SELECT school_year, semester, end_date, is_active FROM academic_term WHERE term_id = @id FOR UPDATE",
            DatabaseHelper.P("@id", termId))
          If target.Rows.Count = 0 Then Throw New InvalidOperationException("The selected academic term no longer exists.")

          Dim targetYear As String = target.Rows(0)("school_year").ToString()
          Dim targetSemester As Integer = Convert.ToInt32(target.Rows(0)("semester"))
          Dim targetEnd As DateTime = Convert.ToDateTime(target.Rows(0)("end_date"))
          If targetEnd.Date < Date.Today Then
            Throw New InvalidOperationException("A past term cannot be activated. Use Academic Management's year and semester filters to view history.")
          End If
          If Convert.ToBoolean(target.Rows(0)("is_active")) Then
            Throw New InvalidOperationException("This term is already active.")
          End If

          CopyMissingSectionDefinitions(conn, trans, targetYear, result)
          Dim sourceTerm As DataTable = Query(conn, trans,
            "SELECT t.term_id, t.school_year, t.semester FROM academic_term t " &
            "WHERE (t.school_year < @sy OR (t.school_year = @sy AND t.semester < @sem)) " &
            "AND EXISTS (SELECT 1 FROM classes c WHERE c.term_id = t.term_id) " &
            "ORDER BY t.school_year DESC, t.semester DESC LIMIT 1",
            DatabaseHelper.P("@sy", targetYear), DatabaseHelper.P("@sem", targetSemester))

          Exec(conn, trans, "UPDATE academic_term SET is_active = 0 WHERE is_active = 1")
          Exec(conn, trans, "UPDATE academic_term SET is_active = 1 WHERE term_id = @id", DatabaseHelper.P("@id", termId))

          If sourceTerm.Rows.Count > 0 Then
            Dim sourceTermId As Integer = Convert.ToInt32(sourceTerm.Rows(0)("term_id"))
            Dim sourceYear As String = sourceTerm.Rows(0)("school_year").ToString()
            Dim sourceSemester As Integer = Convert.ToInt32(sourceTerm.Rows(0)("semester"))
            result.SourceTermLabel = sourceYear & " - " & AcademicContext.SemesterName(sourceSemester)
            CopyMissingClasses(conn, trans, sourceTermId, sourceYear, termId, targetYear, result, teacherNotices)
          End If

          trans.Commit()
        Catch
          Try
            trans.Rollback()
          Catch
          End Try
          Throw
        End Try
      End Using
    End Using

    ' Send assignment notices after the transaction commits so a notification issue cannot undo activation.
    For Each notice As TeacherNotice In teacherNotices
      Try
        NotificationService.NotifyTeacher(notice.TeacherId,
          "You were assigned to " & ClassService.Label(notice.ClassId) & ". Open the class to review its roster and grades.", notice.ClassId)
      Catch
      End Try
    Next
    Return result
  End Function

  Private Shared Sub CopyMissingSectionDefinitions(conn As MySqlConnection, trans As MySqlTransaction,
                                                   targetYear As String, result As ActivationResult)
    Dim source As Object = Scalar(conn, trans,
      "SELECT MAX(school_year) FROM section WHERE school_year < @sy", DatabaseHelper.P("@sy", targetYear))
    If source Is Nothing Then Return

    Dim sourceYear As String = source.ToString()
    Dim sections As DataTable = Query(conn, trans,
      "SELECT program_id, section_code, year_level, batch FROM section WHERE school_year = @sy " &
      "ORDER BY program_id, year_level, section_code",
      DatabaseHelper.P("@sy", sourceYear))

    For Each section As DataRow In sections.Rows
      Dim programId As Integer = Convert.ToInt32(section("program_id"))
      Dim yearLevel As Integer = Convert.ToInt32(section("year_level"))
      Dim code As String = section("section_code").ToString()
      Dim exists As Integer = Convert.ToInt32(Scalar(conn, trans,
        "SELECT COUNT(*) FROM section WHERE school_year = @sy AND program_id = @p AND year_level = @yl AND section_code = @c",
        DatabaseHelper.P("@sy", targetYear), DatabaseHelper.P("@p", programId),
        DatabaseHelper.P("@yl", yearLevel), DatabaseHelper.P("@c", code)))
      If exists > 0 Then Continue For

      Exec(conn, trans,
        "INSERT INTO section (section_code, program_id, school_year, year_level, batch) VALUES (@c, @p, @sy, @yl, @b)",
        DatabaseHelper.P("@c", code), DatabaseHelper.P("@p", programId), DatabaseHelper.P("@sy", targetYear),
        DatabaseHelper.P("@yl", yearLevel), DatabaseHelper.P("@b", If(IsDBNull(section("batch")), Nothing, section("batch"))))
      result.SectionsCreated += 1
    Next
  End Sub

  Private Shared Sub CopyMissingClasses(conn As MySqlConnection, trans As MySqlTransaction,
                                        sourceTermId As Integer, sourceYear As String,
                                        targetTermId As Integer, targetYear As String,
                                        result As ActivationResult, teacherNotices As List(Of TeacherNotice))
    Dim sourceClasses As DataTable = Query(conn, trans,
      "SELECT c.class_id, c.course_id, c.teacher_id, s.program_id, s.section_code, s.year_level " &
      "FROM classes c JOIN section s ON s.section_id = c.section_id " &
      "WHERE c.term_id = @t ORDER BY s.program_id, s.year_level, s.section_code, c.course_id",
      DatabaseHelper.P("@t", sourceTermId))

    Dim ids As New EnrollmentService.IdAllocator(New DbTx(conn, trans), targetYear)
    For Each sourceClass As DataRow In sourceClasses.Rows
      Dim sourceClassId As Integer = Convert.ToInt32(sourceClass("class_id"))
      Dim courseId As Integer = Convert.ToInt32(sourceClass("course_id"))
      Dim teacherId As String = sourceClass("teacher_id").ToString()
      Dim targetSectionId As Object = Scalar(conn, trans,
        "SELECT section_id FROM section WHERE school_year = @sy AND program_id = @p AND year_level = @yl AND section_code = @c LIMIT 1",
        DatabaseHelper.P("@sy", targetYear), DatabaseHelper.P("@p", Convert.ToInt32(sourceClass("program_id"))),
        DatabaseHelper.P("@yl", Convert.ToInt32(sourceClass("year_level"))), DatabaseHelper.P("@c", sourceClass("section_code").ToString()))
      If targetSectionId Is Nothing Then Continue For

      Dim exists As Integer = Convert.ToInt32(Scalar(conn, trans,
        "SELECT COUNT(*) FROM classes WHERE term_id = @t AND course_id = @c AND section_id = @s",
        DatabaseHelper.P("@t", targetTermId), DatabaseHelper.P("@c", courseId), DatabaseHelper.P("@s", targetSectionId)))
      If exists > 0 Then Continue For

      Exec(conn, trans,
        "INSERT INTO classes (course_id, section_id, teacher_id, term_id) VALUES (@c, @s, @te, @t)",
        DatabaseHelper.P("@c", courseId), DatabaseHelper.P("@s", targetSectionId),
        DatabaseHelper.P("@te", teacherId), DatabaseHelper.P("@t", targetTermId))
      Dim targetClassId As Integer = Convert.ToInt32(Scalar(conn, trans, "SELECT LAST_INSERT_ID()"))
      CopyRubricsAndItems(conn, trans, sourceClassId, targetClassId)
      CopyRoster(conn, trans, sourceClassId, targetClassId, Convert.ToInt32(targetSectionId),
                 sourceYear = targetYear, targetYear, ids, result)
      teacherNotices.Add(New TeacherNotice With {.TeacherId = teacherId, .ClassId = targetClassId})
      result.ClassesCreated += 1
    Next
  End Sub

  Private Shared Sub CopyRubricsAndItems(conn As MySqlConnection, trans As MySqlTransaction,
                                         sourceClassId As Integer, targetClassId As Integer)
    Dim rubrics As DataTable = Query(conn, trans,
      "SELECT rubric_id, period, component_name, weight_percent, is_fixed FROM grading_rubric " &
      "WHERE class_id = @c ORDER BY period, rubric_id",
      DatabaseHelper.P("@c", sourceClassId))

    For Each rubric As DataRow In rubrics.Rows
      Exec(conn, trans,
        "INSERT INTO grading_rubric (class_id, period, component_name, weight_percent, is_fixed) " &
        "VALUES (@c, @p, @n, @w, @f)",
        DatabaseHelper.P("@c", targetClassId), DatabaseHelper.P("@p", rubric("period").ToString()),
        DatabaseHelper.P("@n", rubric("component_name").ToString()), DatabaseHelper.P("@w", rubric("weight_percent")),
        DatabaseHelper.P("@f", Convert.ToInt32(rubric("is_fixed"))))
      Dim targetRubricId As Integer = Convert.ToInt32(Scalar(conn, trans, "SELECT LAST_INSERT_ID()"))
      Dim items As DataTable = Query(conn, trans,
        "SELECT item_name, max_score FROM grade_item WHERE rubric_id = @r ORDER BY item_id",
        DatabaseHelper.P("@r", Convert.ToInt32(rubric("rubric_id"))))
      For Each item As DataRow In items.Rows
        Exec(conn, trans,
          "INSERT INTO grade_item (rubric_id, item_name, max_score) VALUES (@r, @n, @m)",
          DatabaseHelper.P("@r", targetRubricId), DatabaseHelper.P("@n", item("item_name").ToString()),
          DatabaseHelper.P("@m", item("max_score")))
      Next
    Next
  End Sub

  Private Shared Sub CopyRoster(conn As MySqlConnection, trans As MySqlTransaction,
                                sourceClassId As Integer, targetClassId As Integer, targetSectionId As Integer,
                                sameSchoolYear As Boolean, targetYear As String,
                                ids As EnrollmentService.IdAllocator, result As ActivationResult)
    Dim roster As DataTable
    If sameSchoolYear Then
      roster = Query(conn, trans,
        "SELECT student_id FROM enrollment WHERE class_id = @c AND status = 'Enrolled' ORDER BY student_id",
        DatabaseHelper.P("@c", sourceClassId))
    Else
      ' New-year rosters follow each student's explicit target-section placement; no year promotion is inferred.
      roster = Query(conn, trans,
        "SELECT student_id FROM student_profile WHERE section_id = @s ORDER BY student_id",
        DatabaseHelper.P("@s", targetSectionId))
    End If

    Dim dbTx As New DbTx(conn, trans)
    For Each student As DataRow In roster.Rows
      Dim studentId As String = student("student_id").ToString()
      Exec(conn, trans,
        "INSERT INTO enrollment (enrollment_id, student_id, class_id, status) VALUES (@e, @s, @c, 'Enrolled')",
        DatabaseHelper.P("@e", ids.NextId()), DatabaseHelper.P("@s", studentId), DatabaseHelper.P("@c", targetClassId))
      NotificationService.NotifyStudentAssigned(dbTx, targetClassId, studentId)
      result.StudentsEnrolled += 1
    Next
  End Sub

  Private Shared Function Query(conn As MySqlConnection, trans As MySqlTransaction, sql As String,
                                ParamArray parameters As MySqlParameter()) As DataTable
    Dim dt As New DataTable()
    Using cmd As New MySqlCommand(sql, conn, trans)
      If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters)
      Using reader As MySqlDataReader = cmd.ExecuteReader()
        dt.Load(reader)
      End Using
    End Using
    Return dt
  End Function

  Private Shared Function Scalar(conn As MySqlConnection, trans As MySqlTransaction, sql As String,
                                 ParamArray parameters As MySqlParameter()) As Object
    Using cmd As New MySqlCommand(sql, conn, trans)
      If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters)
      Dim value As Object = cmd.ExecuteScalar()
      Return If(value Is Nothing OrElse Convert.IsDBNull(value), Nothing, value)
    End Using
  End Function

  Private Shared Sub Exec(conn As MySqlConnection, trans As MySqlTransaction, sql As String,
                          ParamArray parameters As MySqlParameter())
    Using cmd As New MySqlCommand(sql, conn, trans)
      If parameters IsNot Nothing Then cmd.Parameters.AddRange(parameters)
      cmd.ExecuteNonQuery()
    End Using
  End Sub
End Class
