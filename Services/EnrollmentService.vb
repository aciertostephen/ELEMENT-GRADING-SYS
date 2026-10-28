Imports MySqlConnector

''' <summary>
''' enrollment.enrollment_id is VARCHAR(16) with no default, and the database CHECK
''' chk_enroll_id_format only accepts the pattern  enr-YY-NNNNN  (for example enr-26-01303).
''' The id is NOT built from the class id or the student id (the same student is enrolled in many
''' classes, and a student number can be 4 or 5 digits), so it is a running number:
''' the IdAllocator reads the highest number in use INSIDE the caller's transaction (locking the
''' table while it does) and hands out the next ones.
''' </summary>
Public Class EnrollmentService

  Private Const IdPrefix As String = "enr-"
  Private Const MaxSeq As Integer = 99999

  ''' <summary>"2026-2027" gives "26" (the 2 digits used inside the enrollment id).</summary>
  Public Shared Function YearCode(schoolYear As String) As String
    If Not String.IsNullOrEmpty(schoolYear) AndAlso schoolYear.Length >= 4 Then
      Dim yy As String = schoolYear.Substring(2, 2)
      If Char.IsDigit(yy(0)) AndAlso Char.IsDigit(yy(1)) Then Return yy
    End If
    Return (DateTime.Today.Year Mod 100).ToString("00")
  End Function

  Public Shared Function FormatId(yearCode As String, seq As Integer) As String
    Return IdPrefix & yearCode & "-" & seq.ToString("00000")
  End Function

  ''' <summary>Hands out enrollment ids one after another. Create it INSIDE RunInTransaction.</summary>
  Friend NotInheritable Class IdAllocator
    Private ReadOnly _yearCode As String
    Private _next As Integer

    Friend Sub New(tx As DbTx, schoolYear As String)
      _yearCode = YearCode(schoolYear)
      ' FOR UPDATE: a second import running at the same moment waits instead of taking the same numbers
      Dim highest As Object = tx.Scalar(
        "SELECT COALESCE(MAX(CAST(SUBSTRING(enrollment_id, 8) AS UNSIGNED)), 0) FROM enrollment " &
        "WHERE enrollment_id REGEXP '^enr-[0-9]{2}-[0-9]{5}$' FOR UPDATE")
      _next = Convert.ToInt32(highest) + 1
    End Sub

    Friend Function NextId() As String
      If _next > MaxSeq Then
        Throw New InvalidOperationException("No enrollment numbers are left (the format allows 99,999). Ask the administrator.")
      End If
      Dim id As String = FormatId(_yearCode, _next)
      _next += 1
      Return id
    End Function
  End Class

  ''' <summary>Adds a student to a class roster (or re-enrolls a Dropped one). Returns True if new.</summary>
  Public Shared Function AddStudent(classId As Integer, studentId As String) As Boolean
    Dim info As ClassService.ClassInfo = ClassService.GetInfo(classId)
    If info Is Nothing Then Throw New InvalidOperationException("Class not found.")
    If Not SessionManager.IsTeacher OrElse Not ClassService.TeacherOwnsClass(classId, SessionManager.CurrentUserId) Then
      Throw New InvalidOperationException("Only the class teacher can update this class roster.")
    End If
    Dim blocked As String = ClassService.CannotChangeGradebookReason(classId)
    If blocked IsNot Nothing Then Throw New InvalidOperationException(blocked)

    Dim added As Boolean = False
    DatabaseHelper.RunInTransaction(
      Sub(tx)
        Dim exists As Integer = Convert.ToInt32(tx.Scalar(
          "SELECT COUNT(*) FROM student_profile WHERE student_id = @s", DatabaseHelper.P("@s", studentId)))
        If exists = 0 Then Throw New InvalidOperationException("Student " & studentId & " does not exist.")

        Dim status As Object = tx.Scalar(
          "SELECT status FROM enrollment WHERE class_id = @c AND student_id = @s",
          DatabaseHelper.P("@c", classId), DatabaseHelper.P("@s", studentId))

        If status Is Nothing Then
          Dim ids As New IdAllocator(tx, info.SchoolYear)
          tx.Exec("INSERT INTO enrollment (enrollment_id, student_id, class_id, status) VALUES (@e, @s, @c, 'Enrolled')",
                  DatabaseHelper.P("@e", ids.NextId()), DatabaseHelper.P("@s", studentId), DatabaseHelper.P("@c", classId))
          NotificationService.NotifyStudentAssigned(tx, classId, studentId)
          added = True
        ElseIf status.ToString() = "Dropped" Then
          tx.Exec("UPDATE enrollment SET status = 'Enrolled' WHERE class_id = @c AND student_id = @s",
                  DatabaseHelper.P("@c", classId), DatabaseHelper.P("@s", studentId))
          NotificationService.NotifyStudentAssigned(tx, classId, studentId)
        End If
      End Sub)
    Return added
  End Function

  ''' <summary>Students with grades are marked Dropped, never deleted.</summary>
  Public Shared Sub Drop(enrollmentId As String)
    If Not SessionManager.IsTeacher Then Throw New InvalidOperationException("Only a teacher can drop a student from a class.")
    Dim classId As Integer = DatabaseHelper.ScalarInt("SELECT class_id FROM enrollment WHERE enrollment_id = @e",
                                                       {DatabaseHelper.P("@e", enrollmentId)})
    If classId <= 0 OrElse Not ClassService.TeacherOwnsClass(classId, SessionManager.CurrentUserId) Then
      Throw New InvalidOperationException("This enrollment does not belong to one of your classes.")
    End If
    Dim blocked As String = ClassService.CannotChangeGradebookReason(classId)
    If blocked IsNot Nothing Then Throw New InvalidOperationException(blocked)
    DatabaseHelper.ExecuteNonQuery("UPDATE enrollment SET status = 'Dropped', final_grade = NULL, grade_equivalent = NULL " &
                                   "WHERE enrollment_id = @e", {DatabaseHelper.P("@e", enrollmentId)})
  End Sub
End Class
