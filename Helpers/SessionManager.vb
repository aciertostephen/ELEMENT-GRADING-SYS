Public Class SessionManager

  Public Shared Property CurrentUserId As Integer = 0
  Public Shared Property CurrentUsername As String = String.Empty
  Public Shared Property CurrentRole As String = String.Empty
  Public Shared Property CurrentFullName As String = String.Empty
  Public Shared Property IsLoggedIn As Boolean = False

  Public Shared ReadOnly Property IsAdmin As Boolean
    Get
      Return AuthorizeRole("Admin")
    End Get
  End Property

  Public Shared ReadOnly Property IsTeacher As Boolean
    Get
      Return AuthorizeRole("Teacher")
    End Get
  End Property

  Public Shared ReadOnly Property IsStudent As Boolean
    Get
      Return AuthorizeRole("Student")
    End Get
  End Property

  Public Shared Sub StartSession(userId As Integer, username As String, role As String, fullName As String)
    CurrentUserId = userId
    CurrentUsername = username
    CurrentRole = role
    CurrentFullName = fullName
    IsLoggedIn = True
  End Sub

  ''' <summary>BUG FIX: IsLoggedIn is now reset, so AuthorizeRole fails after logout.</summary>
  Public Shared Sub EndSession()
    CurrentUserId = 0
    CurrentUsername = String.Empty
    CurrentRole = String.Empty
    CurrentFullName = String.Empty
    IsLoggedIn = False
  End Sub

  Public Shared Function AuthorizeRole(ParamArray allowedRoles As String()) As Boolean
    If Not IsLoggedIn Then Return False
    For Each role As String In allowedRoles
      If String.Equals(CurrentRole, role, StringComparison.OrdinalIgnoreCase) Then Return True
    Next
    Return False
  End Function

End Class
