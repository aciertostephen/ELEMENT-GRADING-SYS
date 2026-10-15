Imports System.Text
Imports MySqlConnector

Partial Public Class MainForm

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        PerformLogin()
    End Sub

    Private Sub PerformLogin()
        Dim usernameInput As String = txtUsername.Text.Trim()
        Dim passwordInput As String = txtPassword.Text.Trim()

        If String.IsNullOrWhiteSpace(usernameInput) OrElse String.IsNullOrWhiteSpace(passwordInput) Then
            MessageBox.Show("Please enter both username/email and password.", "Validation Error",
                            MessageBoxButtons.OK)
            Return
        End If

        Dim sql As String =
            "SELECT u.user_id, u.username, u.password_hash, u.role, u.status, " &
            "COALESCE(CONCAT(tp.first_name, ' ', tp.last_name), CONCAT(sp.first_name, ' ', sp.last_name), 'Administrator') AS full_name " &
            "FROM users u " &
            "LEFT JOIN teacher_profile tp ON u.user_id = tp.user_id " &
            "LEFT JOIN student_profile sp ON u.user_id = sp.user_id " &
            "WHERE u.username = @login OR u.email = @login " &
            "LIMIT 1;"

        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, {
                New MySqlParameter("@login", usernameInput)
            })

            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                MessageBox.Show("No account found with username/email: " & usernameInput, "Login Failed",
                                MessageBoxButtons.OK)
                Return
            End If

            Dim row As DataRow = dt.Rows(0)

            Dim status As String = Convert.ToString(row("status"))
            If String.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show("Your account is inactive. Please contact the system administrator.", "Access Denied",
                                MessageBoxButtons.OK)
                Return
            End If
            Dim storedValue As String
            If TypeOf row("password_hash") Is Byte() Then
                storedValue = Encoding.UTF8.GetString(CType(row("password_hash"), Byte()))
            Else
                storedValue = Convert.ToString(row("password_hash"))
            End If
            storedValue = If(storedValue, String.Empty).Trim()

            If Not String.Equals(passwordInput, storedValue, StringComparison.Ordinal) Then
                MessageBox.Show("Invalid password.", "Login Failed", MessageBoxButtons.OK)
                txtPassword.SelectAll()
                txtPassword.Focus()
                Return
            End If

            Dim userId As Integer = Convert.ToInt32(row("user_id"))
            Dim username As String = Convert.ToString(row("username"))
            Dim role As String = Convert.ToString(row("role"))
            Dim fullName As String = Convert.ToString(row("full_name"))

            SessionManager.StartSession(userId, username, role, fullName)
            txtPassword.Clear()
            RouteToDashboard(role)

        Catch ex As Exception
            MessageBox.Show("Database Login Error: " & ex.Message, "Database Error",
                            MessageBoxButtons.OK)
        End Try
    End Sub

    Private Sub RouteToDashboard(role As String)
        If String.IsNullOrWhiteSpace(role) Then Return

        Select Case role.Trim().ToLower()
            Case "admin"
                ShowAdminDashboard()
            Case "teacher", "faculty"
                ShowTeacherDashboard()
            Case "student"
                ShowStudentDashboard()
            Case Else
                MessageBox.Show("Unrecognized account role: " & role, "Authorization Error",
                                MessageBoxButtons.OK)
        End Select
    End Sub

End Class