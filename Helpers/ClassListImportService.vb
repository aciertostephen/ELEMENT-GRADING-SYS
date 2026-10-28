''' <summary>
''' Teacher imports a class list (Excel) into ONE of their classes.
'''
'''   * Only students who already have an account can be enrolled by a teacher.
'''   * A teacher import never creates an account or changes a student's section.
'''   * A student who was Dropped from this class is re-enrolled (grades are kept).
'''   * Everything happens in ONE transaction: if anything fails, nothing is saved.
''' The class roster is the real roster (enrollment). The section of the class is only a label.
''' </summary>
Public Class ClassListImportService

  Public Class ImportResult
    Public Property NewAccounts As Integer          ' accounts created
    Public Property ExistingAccounts As Integer     ' already had an account
    Public Property Enrolled As Integer             ' newly added to this class
    Public Property Reenrolled As Integer           ' were Dropped, now Enrolled again
    Public Property AlreadyInClass As Integer       ' already on this class list
    Public ReadOnly Property SectionsCreated As New List(Of String)
    Public ReadOnly Property Skipped As New List(Of String)
  End Class

  ''' <summary>Why this teacher cannot import into this class right now, or Nothing when it is allowed.</summary>
  Public Shared Function CannotImportReason(info As ClassService.ClassInfo, teacherUserId As Integer) As String
    If info Is Nothing Then Return "Class not found."
    If Not ClassService.TeacherOwnsClass(info.ClassId, teacherUserId) Then Return "You are not the teacher of this class."
    If Not AcademicContext.IsEditableYear(info.SchoolYear) Then
      Return "School year " & info.SchoolYear & " is past history and is read-only."
    End If
    If info.IsReleased Then Return "Grades for this class were already released, so its class list can no longer change."
    If DatabaseHelper.ScalarInt("SELECT is_locked FROM academic_term WHERE term_id = @t",
                                {DatabaseHelper.P("@t", info.TermId)}) > 0 Then
      Return "This term is locked, so the class list can no longer change."
    End If
    Return Nothing
  End Function

  Public Shared Function ImportIntoClass(classId As Integer, teacherUserId As Integer,
                                         sheet As ClassListSheet, rows As IEnumerable(Of ClassListRow)) As ImportResult
    Dim info As ClassService.ClassInfo = ClassService.GetInfo(classId)
    Dim blocked As String = CannotImportReason(info, teacherUserId)
    If blocked IsNot Nothing Then Throw New InvalidOperationException(blocked)

    If sheet.SchoolYear <> info.SchoolYear Then
      Throw New InvalidOperationException("This list is for school year " & sheet.SchoolYear &
                                          " but the class belongs to " & info.SchoolYear & ".")
    End If

    Dim result As New ImportResult()

    ' ---- the students to import (valid rows only, each student number once) ----
    Dim chosen As New List(Of ClassListRow)
    Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    For Each r As ClassListRow In rows
      If Not (r.Include AndAlso r.CanInclude) Then Continue For
      If Not seen.Add(r.StudentNo) Then
        result.Skipped.Add(r.StudentNo & " (listed twice in the sheet)")
        Continue For
      End If
      chosen.Add(r)
    Next
    If chosen.Count = 0 Then Throw New InvalidOperationException("There is no student to import.")

    ' Teachers may enroll existing accounts only. Account creation and section placement
    ' belong to the administrator's user/placement workflows.
    Dim existing As HashSet(Of String) = FindExistingStudents(chosen.Select(Function(r) r.StudentNo))
    Dim eligible As New List(Of ClassListRow)
    For Each r As ClassListRow In chosen
      If existing.Contains(r.StudentNo) Then
        eligible.Add(r)
      Else
        result.Skipped.Add(r.StudentNo & " " & r.FullName & " (student account not found; ask an administrator to create it)")
      End If
    Next
    chosen = eligible
    If chosen.Count = 0 Then
      Throw New InvalidOperationException("None of the selected student numbers have an existing student account. Ask an administrator to create the accounts, then import the list again.")
    End If

    Dim roster As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    For Each dr As DataRow In DatabaseHelper.ExecuteQuery(
        "SELECT student_id, status FROM enrollment WHERE class_id = @c", {DatabaseHelper.P("@c", classId)}).Rows
      roster(dr("student_id").ToString()) = dr("status").ToString()
    Next

    ' ---- write everything atomically ----
    DatabaseHelper.RunInTransaction(
      Sub(tx)
        Dim ids As New EnrollmentService.IdAllocator(tx, info.SchoolYear)   ' enr-YY-NNNNN numbers

        For Each r As ClassListRow In chosen
          result.ExistingAccounts += 1
          EnrollInClass(tx, classId, r.StudentNo, roster, result, ids)
        Next
      End Sub)

    Return result
  End Function

  ' ------------------------------------------------------------------
  ' Steps
  ' ------------------------------------------------------------------

  Private Shared Function FindExistingStudents(studentNos As IEnumerable(Of String)) As HashSet(Of String)
    Dim found As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Dim ps As New List(Of MySqlConnector.MySqlParameter)
    Dim names As New List(Of String)
    For Each sn As String In studentNos
      names.Add("@p" & ps.Count)
      ps.Add(DatabaseHelper.P("@p" & ps.Count, sn))
    Next
    If ps.Count = 0 Then Return found
    Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
      "SELECT student_id FROM student_profile WHERE student_id IN (" & String.Join(",", names) & ")", ps.ToArray())
    For Each dr As DataRow In dt.Rows
      found.Add(dr("student_id").ToString())
    Next
    Return found
  End Function

  ''' <summary>Adds the student to the class roster, or re-enrolls a Dropped one.</summary>
  Private Shared Sub EnrollInClass(tx As DbTx, classId As Integer, studentNo As String,
                                   roster As Dictionary(Of String, String), result As ImportResult,
                                   ids As EnrollmentService.IdAllocator)
    Dim status As String = Nothing
    If roster.TryGetValue(studentNo, status) Then
      If status = "Dropped" Then
        tx.Exec("UPDATE enrollment SET status = 'Enrolled' WHERE class_id = @c AND student_id = @s",
                DatabaseHelper.P("@c", classId), DatabaseHelper.P("@s", studentNo))
        NotificationService.NotifyStudentAssigned(tx, classId, studentNo)
        result.Reenrolled += 1
      Else
        result.AlreadyInClass += 1
      End If
    Else
      tx.Exec("INSERT INTO enrollment (enrollment_id, student_id, class_id, status) VALUES (@e, @s, @c, 'Enrolled')",
              DatabaseHelper.P("@e", ids.NextId()),
              DatabaseHelper.P("@s", studentNo), DatabaseHelper.P("@c", classId))
      roster(studentNo) = "Enrolled"
      NotificationService.NotifyStudentAssigned(tx, classId, studentNo)
      result.Enrolled += 1
    End If
  End Sub

End Class
