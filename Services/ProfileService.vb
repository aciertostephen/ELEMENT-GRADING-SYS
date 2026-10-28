Imports System.Data
Imports System.Drawing
Imports System.IO
Imports MySqlConnector
Imports BCryptHasher = BCrypt.Net.BCrypt

''' <summary>Real data for Profile Settings (replaces the "Department / Email / Phone Number" placeholders).</summary>
Public Class ProfileService

  Public Class ProfileInfo
    Public Property FullName As String
    Public Property RoleLabel As String
    Public Property Department As String
    Public Property Email As String
    Public Property Phone As String
    Public Property PicturePath As String
  End Class

  Public Shared Function Load(userId As Integer) As ProfileInfo
    Dim u = DatabaseHelper.ExecuteQuery("SELECT role, email, phone_no, profile_picture FROM users WHERE user_id = @u",
                                        {DatabaseHelper.P("@u", userId)})
    If u.Rows.Count = 0 Then Return New ProfileInfo()
    Dim role As String = u.Rows(0)("role").ToString()
    Dim p As New ProfileInfo With {
      .FullName = SessionManager.CurrentFullName,
      .Email = u.Rows(0)("email").ToString(),
      .Phone = u.Rows(0)("phone_no").ToString(),
      .PicturePath = If(IsDBNull(u.Rows(0)("profile_picture")), Nothing, u.Rows(0)("profile_picture").ToString())}

    Select Case role
      Case "Teacher"
        p.RoleLabel = "Professor"
        p.Department = Convert.ToString(DatabaseHelper.ExecuteScalar(
          "SELECT department FROM teacher_profile WHERE user_id = @u", {DatabaseHelper.P("@u", userId)}))
      Case "Student"
        p.RoleLabel = "Student"
        Dim d = DatabaseHelper.ExecuteQuery(
          "SELECT pr.program_name, s.section_code FROM student_profile sp JOIN section s ON s.section_id = sp.section_id " &
          "JOIN program pr ON pr.program_id = s.program_id WHERE sp.user_id = @u", {DatabaseHelper.P("@u", userId)})
        p.Department = If(d.Rows.Count > 0, d.Rows(0)("program_name").ToString() & " - " & d.Rows(0)("section_code").ToString(), "")
      Case Else
        p.RoleLabel = "Administrator"
        p.Department = "Administration"
    End Select
    Return p
  End Function

  Public Shared Sub SaveContact(userId As Integer, email As String, phone As String)
    EnsureSelfUser(userId)
    email = If(email, "").Trim()
    If Not IsEmail(email) Then Throw New InvalidOperationException("Please enter a valid email address.")
    phone = If(phone, "").Trim()
    If phone <> "" AndAlso Not IsPhone(phone) Then Throw New InvalidOperationException("Phone number must be a 10-digit mobile number starting with 9.")
    Try
      DatabaseHelper.ExecuteNonQuery("UPDATE users SET email = @e, phone_no = @p WHERE user_id = @u",
        {DatabaseHelper.P("@e", email), DatabaseHelper.P("@p", If(phone = "", CType(DBNull.Value, Object), NormalizePhone(phone))), DatabaseHelper.P("@u", userId)})
    Catch ex As MySqlException When ex.Number = 1062
      Throw New InvalidOperationException("That email is already used by another account.")
    End Try
  End Sub

  ''' <summary>Save contact details and, when requested, change the user's password atomically.</summary>
  Public Shared Sub SaveSettings(userId As Integer, email As String, phone As String,
                                 currentPassword As String, newPassword As String, confirmPassword As String)
    EnsureSelfUser(userId)
    email = If(email, "").Trim()
    phone = If(phone, "").Trim()
    currentPassword = If(currentPassword, "")
    newPassword = If(newPassword, "")
    confirmPassword = If(confirmPassword, "")

    If Not IsEmail(email) Then Throw New InvalidOperationException("Please enter a valid email address.")
    If phone <> "" AndAlso Not IsPhone(phone) Then Throw New InvalidOperationException("Phone number must be a 10-digit mobile number starting with 9.")

    Dim changePassword As Boolean = currentPassword <> "" OrElse newPassword <> "" OrElse confirmPassword <> ""
    Dim newHash As String = Nothing
    If changePassword Then
      If currentPassword = "" Then Throw New InvalidOperationException("Enter your current password to change it.")
      If newPassword.Length < 8 Then Throw New InvalidOperationException("The new password must be at least 8 characters.")
      If newPassword.Contains(" "c) Then Throw New InvalidOperationException("The new password cannot contain spaces.")
      If newPassword <> confirmPassword Then Throw New InvalidOperationException("The new passwords do not match.")

      Dim stored As Object = DatabaseHelper.ExecuteScalar(
        "SELECT password_hash FROM users WHERE user_id = @u", {DatabaseHelper.P("@u", userId)})
      If stored Is Nothing OrElse stored Is DBNull.Value Then Throw New InvalidOperationException("The account password could not be verified.")
      Dim storedHash As String = If(TypeOf stored Is Byte(), System.Text.Encoding.UTF8.GetString(DirectCast(stored, Byte())), Convert.ToString(stored))
      Dim valid As Boolean = False
      Try
        valid = BCryptHasher.Verify(currentPassword, storedHash.Trim())
      Catch
        valid = False
      End Try
      If Not valid Then Throw New InvalidOperationException("The current password is incorrect.")
      newHash = BCryptHasher.HashPassword(newPassword, 11)
    End If

    Try
      DatabaseHelper.RunInTransaction(
        Sub(tx)
          Dim sql As String = "UPDATE users SET email = @e, phone_no = @p"
          If changePassword Then sql &= ", password_hash = @h"
          sql &= " WHERE user_id = @u"
          If changePassword Then
            tx.Exec(sql, DatabaseHelper.P("@e", email), DatabaseHelper.P("@p", If(phone = "", CType(DBNull.Value, Object), NormalizePhone(phone))),
                    DatabaseHelper.P("@h", newHash), DatabaseHelper.P("@u", userId))
          Else
            tx.Exec(sql, DatabaseHelper.P("@e", email), DatabaseHelper.P("@p", If(phone = "", CType(DBNull.Value, Object), NormalizePhone(phone))),
                    DatabaseHelper.P("@u", userId))
          End If
        End Sub)
    Catch ex As MySqlException When ex.Number = 1062
      Throw New InvalidOperationException("That email is already used by another account.")
    End Try
  End Sub

  Private Shared Function AvatarFolder() As String
    Dim dir As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GradebookApp", "avatars")
    Directory.CreateDirectory(dir)
    Return dir
  End Function

  Public Shared Sub SavePicture(userId As Integer, img As Image)
    EnsureSelfUser(userId)
    If img Is Nothing Then Throw New InvalidOperationException("Choose a profile image first.")
    Dim path As String = System.IO.Path.Combine(AvatarFolder(), userId & ".png")
    img.Save(path, Imaging.ImageFormat.Png)
    DatabaseHelper.ExecuteNonQuery("UPDATE users SET profile_picture = @p WHERE user_id = @u",
      {DatabaseHelper.P("@p", path), DatabaseHelper.P("@u", userId)})
  End Sub

  Private Shared Sub EnsureSelfUser(userId As Integer)
    If Not SessionManager.IsLoggedIn OrElse userId <> SessionManager.CurrentUserId Then
      Throw New InvalidOperationException("You can only update your own profile.")
    End If
  End Sub

  ''' <summary>Returns a copy of the saved picture (no file lock), or Nothing.</summary>
  Public Shared Function LoadPicture(path As String) As Image
    If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then Return Nothing
    Using fs As New FileStream(path, FileMode.Open, FileAccess.Read)
      Using src As Image = Image.FromStream(fs)
        Return New Bitmap(src)
      End Using
    End Using
  End Function
End Class
