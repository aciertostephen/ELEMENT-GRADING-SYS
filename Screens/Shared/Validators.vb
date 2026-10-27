Imports System.Text.RegularExpressions

''' <summary>Email / phone rules in one place (was duplicated in user management).</summary>
Public Module Validators
  Private ReadOnly EmailRx As New Regex("^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)

  Public Function IsEmail(s As String) As Boolean
    Return Not String.IsNullOrWhiteSpace(s) AndAlso s.Length <= 100 AndAlso EmailRx.IsMatch(s.Trim())
  End Function

  ''' <summary>Philippine mobile: 10 digits starting with 9 (stored without the leading 0 / +63).</summary>
  Public Function NormalizePhone(s As String) As String
    If s Is Nothing Then Return ""
    Dim d As String = Regex.Replace(s, "\D", "")
    If d.StartsWith("63") AndAlso d.Length = 12 Then d = d.Substring(2)
    If d.StartsWith("0") AndAlso d.Length = 11 Then d = d.Substring(1)
    Return d
  End Function

  Public Function IsPhone(s As String) As Boolean
    Return Regex.IsMatch(NormalizePhone(s), "^9\d{9}$")
  End Function
End Module
