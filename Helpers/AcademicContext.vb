''' <summary>
''' One place that answers "which term / school year is active" and "may this year be edited".
''' Only the TERM is activated; the school year is derived from it (no second flag to drift).
''' Past school years are read-only history. Active and upcoming years are editable.
''' </summary>
Public Class AcademicContext

  Public Shared Function ActiveTermId() As Integer
    Return DatabaseHelper.ScalarInt("SELECT term_id FROM academic_term WHERE is_active = 1 LIMIT 1")
  End Function

  Public Shared Function ActiveSchoolYear() As String
    Dim r As Object = DatabaseHelper.ExecuteScalar("SELECT school_year FROM academic_term WHERE is_active = 1 LIMIT 1")
    Return If(r Is Nothing, Nothing, r.ToString())
  End Function

  Public Shared Function SemesterName(sem As Integer) As String
    Select Case sem
      Case 1 : Return "1st Semester"
      Case 2 : Return "2nd Semester"
      Case Else : Return "Summer"
    End Select
  End Function

  Public Shared Function OrdinalYear(n As Integer) As String
    Select Case n
      Case 1 : Return "1st Year"
      Case 2 : Return "2nd Year"
      Case 3 : Return "3rd Year"
      Case Else : Return n.ToString() & "th Year"
    End Select
  End Function

  ''' <summary>"2026-2027" strings sort correctly as text. No active term = nothing is locked.</summary>
  Public Shared Function IsEditableYear(schoolYear As String) As Boolean
    Dim active As String = ActiveSchoolYear()
    If String.IsNullOrEmpty(active) OrElse String.IsNullOrEmpty(schoolYear) Then Return True
    Return String.CompareOrdinal(schoolYear, active) >= 0
  End Function

End Class
