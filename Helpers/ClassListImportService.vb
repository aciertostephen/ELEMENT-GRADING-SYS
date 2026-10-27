Imports System.Collections.Concurrent
Imports System.Globalization
Imports System.Text
Imports System.Threading.Tasks
Imports BCryptHasher = BCrypt.Net.BCrypt

''' <summary>
''' Teacher imports a class list (Excel) into ONE of their classes.
'''
'''   * Students already in the system are only ENROLLED in the class (their account / section is never touched).
'''   * New students get an account: username = student number, starting password = student number,
'''     e-mail = lastname_firstname@plpasig.edu.ph. They are placed in the section named by the Excel sheet
'''     (the section row is created when it does not exist yet - that is the real section of the student).
'''   * A student who was Dropped from this class is re-enrolled (grades are kept).
'''   * Everything happens in ONE transaction: if anything fails, nothing is saved.
''' The class roster is the real roster (enrollment). The section of the class is only a label.
''' </summary>
Public Class ClassListImportService

  Public Const EmailDomain As String = "plpasig.edu.ph"
  Private Const BcryptCost As Integer = 11          ' same cost as the admin's Create User

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

    ' ---- who already has an account, and who is already in this class ----
    Dim existing As HashSet(Of String) = FindExistingStudents(chosen.Select(Function(r) r.StudentNo))

    Dim roster As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    For Each dr As DataRow In DatabaseHelper.ExecuteQuery(
        "SELECT student_id, status FROM enrollment WHERE class_id = @c", {DatabaseHelper.P("@c", classId)}).Rows
      roster(dr("student_id").ToString()) = dr("status").ToString()
    Next

    ' ---- password hashing is slow, so do it for all new students at once, in parallel ----
    Dim hashes As New ConcurrentDictionary(Of String, String)()
    Dim needHash As List(Of String) = chosen.Where(Function(r) Not existing.Contains(r.StudentNo)).
                                             Select(Function(r) r.StudentNo).ToList()
    Parallel.ForEach(Of String)(needHash,
      Sub(sn As String)
        hashes(sn) = BCryptHasher.HashPassword(sn, BcryptCost)
      End Sub)

    ' ---- write everything atomically ----
    Dim usedEmails As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    DatabaseHelper.RunInTransaction(
      Sub(tx)
        Dim sectionId As Integer = 0          ' only looked up / created when a NEW student needs it
        Dim ids As New EnrollmentService.IdAllocator(tx, info.SchoolYear)   ' enr-YY-NNNNN numbers

        For Each r As ClassListRow In chosen
          Dim isNew As Boolean = Not existing.Contains(r.StudentNo)

          If isNew Then
            If Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM users WHERE username = @u",
                                         DatabaseHelper.P("@u", r.StudentNo))) > 0 Then
              result.Skipped.Add(r.StudentNo & " " & r.FullName & " (username already used by another account)")
              Continue For
            End If
            If sectionId = 0 Then sectionId = EnsureSection(tx, sheet, result)
            CreateStudentAccount(tx, r, sectionId, sheet.YearLevel, hashes(r.StudentNo), usedEmails)
            result.NewAccounts += 1
          Else
            result.ExistingAccounts += 1
          End If

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

  ''' <summary>The section named by the sheet in the class's school year; created when it does not exist yet.</summary>
  Private Shared Function EnsureSection(tx As DbTx, sheet As ClassListSheet, result As ImportResult) As Integer
    Dim prog As Object = tx.Scalar("SELECT program_id FROM program WHERE program_code = @c",
                                   DatabaseHelper.P("@c", sheet.ProgramCode))
    If prog Is Nothing Then
      Throw New InvalidOperationException("Program " & sheet.ProgramCode & " does not exist. Ask the admin to add it first.")
    End If
    Dim programId As Integer = Convert.ToInt32(prog)

    Dim sec As Object = tx.Scalar(
      "SELECT section_id FROM section WHERE program_id = @p AND school_year = @sy AND year_level = @yl AND section_code = @c",
      DatabaseHelper.P("@p", programId), DatabaseHelper.P("@sy", sheet.SchoolYear),
      DatabaseHelper.P("@yl", sheet.YearLevel), DatabaseHelper.P("@c", sheet.SectionCode))
    If sec IsNot Nothing Then Return Convert.ToInt32(sec)

    If sheet.SectionCode.Length > 15 Then
      Throw New InvalidOperationException("Section code " & sheet.SectionCode & " is longer than 15 characters.")
    End If
    tx.Exec("INSERT INTO section (section_code, program_id, school_year, year_level) VALUES (@c, @p, @sy, @yl)",
            DatabaseHelper.P("@c", sheet.SectionCode), DatabaseHelper.P("@p", programId),
            DatabaseHelper.P("@sy", sheet.SchoolYear), DatabaseHelper.P("@yl", sheet.YearLevel))
    result.SectionsCreated.Add(sheet.SectionCode & " (" & sheet.SchoolYear & ")")
    Return Convert.ToInt32(tx.Scalar("SELECT LAST_INSERT_ID()"))
  End Function

  ''' <summary>users + student_profile. The DB triggers set year_level and write the section history.</summary>
  Private Shared Sub CreateStudentAccount(tx As DbTx, r As ClassListRow, sectionId As Integer, yearLevel As Integer,
                                          hash As String, usedEmails As HashSet(Of String))
    Dim email As String = UniqueEmail(tx, r.LastName, r.FirstName, usedEmails)

    tx.Exec("INSERT INTO users (username, email, password_hash, role, status, phone_no) " &
            "VALUES (@u, @e, @h, 'Student', 'Active', NULL)",
            DatabaseHelper.P("@u", r.StudentNo), DatabaseHelper.P("@e", email), DatabaseHelper.P("@h", hash))
    Dim userId As Integer = Convert.ToInt32(tx.Scalar("SELECT LAST_INSERT_ID()"))

    tx.Exec("INSERT INTO student_profile (student_id, user_id, first_name, middle_name, last_name, section_id, year_level) " &
            "VALUES (@id, @uid, @f, @m, @l, @sec, @yl)",
            DatabaseHelper.P("@id", r.StudentNo), DatabaseHelper.P("@uid", userId),
            DatabaseHelper.P("@f", Clip(r.FirstName, 50)), DatabaseHelper.P("@m", Clip(r.MiddleName, 50)),
            DatabaseHelper.P("@l", Clip(r.LastName, 50)), DatabaseHelper.P("@sec", sectionId),
            DatabaseHelper.P("@yl", yearLevel.ToString()))
  End Sub

  ''' <summary>Adds the student to the class roster, or re-enrolls a Dropped one.</summary>
  Private Shared Sub EnrollInClass(tx As DbTx, classId As Integer, studentNo As String,
                                   roster As Dictionary(Of String, String), result As ImportResult,
                                   ids As EnrollmentService.IdAllocator)
    Dim status As String = Nothing
    If roster.TryGetValue(studentNo, status) Then
      If status = "Dropped" Then
        tx.Exec("UPDATE enrollment SET status = 'Enrolled' WHERE class_id = @c AND student_id = @s",
                DatabaseHelper.P("@c", classId), DatabaseHelper.P("@s", studentNo))
        result.Reenrolled += 1
      Else
        result.AlreadyInClass += 1
      End If
    Else
      tx.Exec("INSERT INTO enrollment (enrollment_id, student_id, class_id, status) VALUES (@e, @s, @c, 'Enrolled')",
              DatabaseHelper.P("@e", ids.NextId()),
              DatabaseHelper.P("@s", studentNo), DatabaseHelper.P("@c", classId))
      roster(studentNo) = "Enrolled"
      result.Enrolled += 1
    End If
  End Sub

  ' ------------------------------------------------------------------
  ' E-mail: lastname_firstname@plpasig.edu.ph (same style as the registrar's sheet)
  ' ------------------------------------------------------------------

  Private Shared Function UniqueEmail(tx As DbTx, last As String, first As String, used As HashSet(Of String)) As String
    Dim localPart As String = Slug(last) & "_" & Slug(first)
    If localPart = "_" Then localPart = "student"
    If localPart.Length > 60 Then localPart = localPart.Substring(0, 60)

    Dim n As Integer = 0
    Do
      Dim candidate As String = localPart & If(n = 0, "", n.ToString()) & "@" & EmailDomain
      If Not used.Contains(candidate) AndAlso
         Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM users WHERE email = @e", DatabaseHelper.P("@e", candidate))) = 0 Then
        used.Add(candidate)
        Return candidate
      End If
      n += 1
    Loop
  End Function

  ''' <summary>"CAÑARES" -> "canares", "DELA CRUZ" -> "delacruz" (letters, digits and hyphen only).</summary>
  Private Shared Function Slug(s As String) As String
    Dim sb As New StringBuilder()
    For Each ch As Char In If(s, "").Normalize(NormalizationForm.FormD)
      If CharUnicodeInfo.GetUnicodeCategory(ch) = UnicodeCategory.NonSpacingMark Then Continue For
      Dim lc As Char = Char.ToLowerInvariant(ch)
      If (lc >= "a"c AndAlso lc <= "z"c) OrElse (lc >= "0"c AndAlso lc <= "9"c) OrElse lc = "-"c Then sb.Append(lc)
    Next
    Return sb.ToString()
  End Function

  Private Shared Function Clip(s As String, maxLength As Integer) As String
    s = If(s, "").Trim()
    Return If(s.Length > maxLength, s.Substring(0, maxLength), s)
  End Function
End Class
