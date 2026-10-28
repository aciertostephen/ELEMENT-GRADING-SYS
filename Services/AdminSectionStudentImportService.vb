Imports System.Collections.Concurrent
Imports System.Globalization
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks
Imports BCryptHasher = BCrypt.Net.BCrypt

''' <summary>Administrator imports the roster of one new section and creates missing student accounts.</summary>
Public NotInheritable Class AdminSectionStudentImportService
  Private Const BcryptCost As Integer = 11
  Private Const EmailDomain As String = "plpasig.edu.ph"

  Public Class ImportResult
    Public Property AccountsCreated As Integer
    Public Property StudentsPlaced As Integer
    Public Property AlreadyInSection As Integer
    Public ReadOnly Property Skipped As New List(Of String)
  End Class

  Public Class BulkUserImportResult
    Public Property AccountsCreated As Integer
    Public Property ExistingStudentAccounts As Integer
    Public ReadOnly Property Skipped As New List(Of String)
  End Class

  ''' <summary>Creates student accounts from the class-list workbook without placing them in a section.</summary>
  Public Shared Function ImportUsers(rows As IEnumerable(Of ClassListRow)) As BulkUserImportResult
    If Not SessionManager.IsAdmin Then Throw New InvalidOperationException("Only an administrator can import users.")
    Dim nullablePlacementColumns As Integer = DatabaseHelper.ScalarInt(
      "SELECT COUNT(*) FROM information_schema.columns " &
      "WHERE table_schema = DATABASE() AND table_name = 'student_profile' " &
      "AND column_name IN ('section_id', 'year_level') AND is_nullable = 'YES'", Nothing)
    If nullablePlacementColumns <> 2 Then
      Throw New InvalidOperationException("Run database migration 03 first so imported students can be created before section placement.")
    End If

    Dim result As New BulkUserImportResult()
    Dim selected As New List(Of ClassListRow)
    Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    For Each row As ClassListRow In rows
      If row Is Nothing OrElse Not row.Include OrElse Not row.CanInclude Then Continue For
      If Not seen.Add(row.StudentNo) Then
        result.Skipped.Add(row.StudentNo & " (listed on multiple sheets)")
      Else
        selected.Add(row)
      End If
    Next
    If selected.Count = 0 Then Throw New InvalidOperationException("Select at least one valid student row to import.")

    Dim existingIds As HashSet(Of String) = FindExistingStudentIds(selected.Select(Function(r) r.StudentNo))
    Dim newIds As List(Of String) = selected.Where(Function(r) Not existingIds.Contains(r.StudentNo)).
      Select(Function(r) r.StudentNo).ToList()
    Dim hashes As New ConcurrentDictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Parallel.ForEach(newIds, Sub(id) hashes(id) = BCryptHasher.HashPassword(id, BcryptCost))

    DatabaseHelper.RunInTransaction(
      Sub(tx)
        Dim usedEmails As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row As ClassListRow In selected
          If existingIds.Contains(row.StudentNo) Then
            result.ExistingStudentAccounts += 1
            Continue For
          End If

          If Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM users WHERE username = @u",
                                       DatabaseHelper.P("@u", row.StudentNo))) > 0 Then
            result.Skipped.Add(row.StudentNo & " " & row.FullName & " (username is already used by a non-student account)")
            Continue For
          End If

          Dim email As String = UniqueEmail(tx, row, usedEmails)
          tx.Exec("INSERT INTO users (username, email, password_hash, role, status, phone_no) " &
                  "VALUES (@u, @e, @h, 'Student', 'Active', NULL)",
                  DatabaseHelper.P("@u", row.StudentNo), DatabaseHelper.P("@e", email),
                  DatabaseHelper.P("@h", hashes(row.StudentNo)))
          Dim userId As Integer = Convert.ToInt32(tx.Scalar("SELECT LAST_INSERT_ID()"))
          tx.Exec("INSERT INTO student_profile (student_id, user_id, first_name, middle_name, last_name, section_id, year_level) " &
                  "VALUES (@s, @u, @f, @m, @l, NULL, NULL)",
                  DatabaseHelper.P("@s", row.StudentNo), DatabaseHelper.P("@u", userId),
                  DatabaseHelper.P("@f", Clip(row.FirstName, 50)), DatabaseHelper.P("@m", Clip(row.MiddleName, 50)),
                  DatabaseHelper.P("@l", Clip(row.LastName, 50)))
          result.AccountsCreated += 1
        Next
      End Sub)

    Return result
  End Function

  Public Shared Function ImportToSection(sectionId As Integer, sheet As ClassListSheet, rows As IEnumerable(Of ClassListRow)) As ImportResult
    If Not SessionManager.IsAdmin Then Throw New InvalidOperationException("Only an administrator can import a section roster.")
    Dim section As SectionService.SectionInfo = SectionService.GetInfo(sectionId)
    If section Is Nothing Then Throw New InvalidOperationException("Section not found.")
    SectionService.EnsureEditable(section)
    If sheet Is Nothing OrElse
       Not String.Equals(sheet.SchoolYear, section.SchoolYear, StringComparison.OrdinalIgnoreCase) OrElse
       Not String.Equals(sheet.ProgramCode, section.ProgramCode, StringComparison.OrdinalIgnoreCase) OrElse
       sheet.YearLevel <> section.YearLevel Then
      Throw New InvalidOperationException("The selected class-list sheet must match the section's program, year level, and school year.")
    End If

    Dim result As New ImportResult()
    Dim selected As New List(Of ClassListRow)
    Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    For Each row As ClassListRow In rows
      If row Is Nothing OrElse Not row.Include OrElse Not row.CanInclude Then Continue For
      If Not seen.Add(row.StudentNo) Then
        result.Skipped.Add(row.StudentNo & " (listed more than once)")
      Else
        selected.Add(row)
      End If
    Next
    If selected.Count = 0 Then Throw New InvalidOperationException("Select at least one valid student row.")

    Dim accountIds As HashSet(Of String) = FindExistingStudentIds(selected.Select(Function(r) r.StudentNo))
    Dim hashes As New ConcurrentDictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Dim newIds As List(Of String) = selected.Where(Function(r) Not accountIds.Contains(r.StudentNo)).
      Select(Function(r) r.StudentNo).ToList()
    Parallel.ForEach(newIds, Sub(id) hashes(id) = BCryptHasher.HashPassword(id, BcryptCost))

    DatabaseHelper.RunInTransaction(
      Sub(tx)
        Dim usedEmails As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row As ClassListRow In selected
          If accountIds.Contains(row.StudentNo) Then
            Dim currentSection As Object = tx.Scalar(
              "SELECT section_id FROM student_profile WHERE student_id = @s FOR UPDATE", DatabaseHelper.P("@s", row.StudentNo))
            If currentSection Is Nothing OrElse Convert.IsDBNull(currentSection) Then
              tx.Exec("UPDATE student_profile SET section_id = @d, year_level = @yl WHERE student_id = @s",
                      DatabaseHelper.P("@d", sectionId), DatabaseHelper.P("@yl", section.YearLevel.ToString()),
                      DatabaseHelper.P("@s", row.StudentNo))
              result.StudentsPlaced += 1
            ElseIf Convert.ToInt32(currentSection) = sectionId Then
              result.AlreadyInSection += 1
            Else
              result.Skipped.Add(row.StudentNo & " " & row.FullName & " (already placed in another section; use Student Placement)")
            End If
            Continue For
          End If

          If Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM users WHERE username = @u",
                                       DatabaseHelper.P("@u", row.StudentNo))) > 0 Then
            result.Skipped.Add(row.StudentNo & " " & row.FullName & " (username belongs to a non-student account)")
            Continue For
          End If

          Dim email As String = UniqueEmail(tx, row, usedEmails)
          tx.Exec("INSERT INTO users (username, email, password_hash, role, status, phone_no) " &
                  "VALUES (@u, @e, @h, 'Student', 'Active', NULL)",
                  DatabaseHelper.P("@u", row.StudentNo), DatabaseHelper.P("@e", email),
                  DatabaseHelper.P("@h", hashes(row.StudentNo)))
          Dim userId As Integer = Convert.ToInt32(tx.Scalar("SELECT LAST_INSERT_ID()"))
          tx.Exec("INSERT INTO student_profile (student_id, user_id, first_name, middle_name, last_name, section_id, year_level) " &
                  "VALUES (@s, @u, @f, @m, @l, @sec, @yl)",
                  DatabaseHelper.P("@s", row.StudentNo), DatabaseHelper.P("@u", userId),
                  DatabaseHelper.P("@f", Clip(row.FirstName, 50)), DatabaseHelper.P("@m", Clip(row.MiddleName, 50)),
                  DatabaseHelper.P("@l", Clip(row.LastName, 50)), DatabaseHelper.P("@sec", sectionId),
                  DatabaseHelper.P("@yl", section.YearLevel.ToString()))
          result.AccountsCreated += 1
        Next
      End Sub)
    Return result
  End Function

  Private Shared Function FindExistingStudentIds(ids As IEnumerable(Of String)) As HashSet(Of String)
    Dim result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Dim parameters As New List(Of MySqlConnector.MySqlParameter)
    Dim names As New List(Of String)
    For Each id As String In ids
      Dim name As String = "@s" & parameters.Count.ToString(CultureInfo.InvariantCulture)
      names.Add(name)
      parameters.Add(DatabaseHelper.P(name, id))
    Next
    If names.Count = 0 Then Return result
    Dim dt = DatabaseHelper.ExecuteQuery("SELECT student_id FROM student_profile WHERE student_id IN (" & String.Join(",", names) & ")", parameters.ToArray())
    For Each row As DataRow In dt.Rows
      result.Add(row("student_id").ToString())
    Next
    Return result
  End Function

  Private Shared Function UniqueEmail(tx As DbTx, row As ClassListRow, used As HashSet(Of String)) As String
    Dim local As String = Slug(row.LastName) & "_" & Slug(row.FirstName)
    If local = "_" Then local = "student"
    If local.Length > 55 Then local = local.Substring(0, 55)
    Dim suffix As String = Regex.Replace(row.StudentNo, "\D", "")
    Dim candidate As String = local & "@" & EmailDomain
    If used.Contains(candidate) OrElse Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM users WHERE email = @e", DatabaseHelper.P("@e", candidate))) > 0 Then
      candidate = local & suffix & "@" & EmailDomain
      Dim counter As Integer = 1
      While used.Contains(candidate) OrElse Convert.ToInt32(tx.Scalar("SELECT COUNT(*) FROM users WHERE email = @e", DatabaseHelper.P("@e", candidate))) > 0
        candidate = local & suffix & counter.ToString(CultureInfo.InvariantCulture) & "@" & EmailDomain
        counter += 1
      End While
    End If
    used.Add(candidate)
    Return candidate
  End Function

  Private Shared Function Slug(value As String) As String
    Dim builder As New StringBuilder()
    For Each ch As Char In If(value, "").Normalize(NormalizationForm.FormD)
      If CharUnicodeInfo.GetUnicodeCategory(ch) = UnicodeCategory.NonSpacingMark Then Continue For
      Dim lower As Char = Char.ToLowerInvariant(ch)
      If (lower >= "a"c AndAlso lower <= "z"c) OrElse (lower >= "0"c AndAlso lower <= "9"c) OrElse lower = "-"c Then builder.Append(lower)
    Next
    Return builder.ToString()
  End Function

  Private Shared Function Clip(value As String, length As Integer) As String
    value = If(value, "").Trim()
    Return If(value.Length > length, value.Substring(0, length), value)
  End Function
End Class
