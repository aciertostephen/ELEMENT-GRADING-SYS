Public Class SessionManager

    Public Shared Property CurrentUserId As Integer = 0
    Public Shared Property CurrentUsername As String = String.Empty
    Public Shared Property CurrentRole As String = String.Empty
    Public Shared Property CurrentFullName As String = String.Empty
    Public Shared Property IsLoggedIn As Boolean = False

    Public Shared Sub StartSession(userId As Integer, username As String, role As String, fullName As String)
        CurrentUserId = userId
        CurrentUsername = username
        CurrentRole = role
        CurrentFullName = fullName
        IsLoggedIn = True
    End Sub

    Public Shared Sub EndSession()
        CurrentUserId = 0
        CurrentUsername = String.Empty
        CurrentRole = String.Empty
        CurrentFullName = String.Empty
    End Sub

    Public Shared Function AuthorizeRole(ParamArray allowedRoles As String()) As Boolean
        If Not IsLoggedIn Then Return False

        For Each role As String In allowedRoles
            If String.Equals(CurrentRole, role, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next

        Return False
    End Function

End Class