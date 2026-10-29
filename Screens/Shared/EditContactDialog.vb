Imports System.Windows.Forms

''' <summary>Edit Info on Profile Settings: email + phone are saved to the users table.</summary>
Public Class EditContactDialog
  Inherits DialogBase

  Private ReadOnly txtEmail As TextBox = MakeText(100)
  Private ReadOnly txtPhone As TextBox = MakeText(13)
  Private ReadOnly txtCurrentPassword As TextBox = MakeText(100)
  Private ReadOnly txtNewPassword As TextBox = MakeText(100)
  Private ReadOnly txtConfirmPassword As TextBox = MakeText(100)
  Public Property Email As String
  Public Property Phone As String

  Public Sub New(currentEmail As String, currentPhone As String)
    MyBase.New("Edit Contact Info", 420)
    txtEmail.Text = currentEmail
    txtPhone.Text = currentPhone
    txtCurrentPassword.UseSystemPasswordChar = True
    txtNewPassword.UseSystemPasswordChar = True
    txtConfirmPassword.UseSystemPasswordChar = True
    AddField("Email:", txtEmail)
    AddField("Phone (10 digits, e.g. 9171234567):", txtPhone)
    AddNote("To change your password, enter your current password and the new password twice. Leave these fields blank to keep it unchanged.")
    AddField("Current password:", txtCurrentPassword)
    AddField("New password (at least 8 characters):", txtNewPassword)
    AddField("Confirm new password:", txtConfirmPassword)
    FinishLayout("Save")
  End Sub

  Protected Overrides Function OnSubmit() As Boolean
    Try
      ProfileService.SaveSettings(SessionManager.CurrentUserId, txtEmail.Text, txtPhone.Text,
                                  txtCurrentPassword.Text, txtNewPassword.Text, txtConfirmPassword.Text)
      Email = txtEmail.Text.Trim()
      Phone = NormalizePhone(txtPhone.Text)
      Return True
    Catch ex As InvalidOperationException
      Msg.Warn(ex.Message)
      Return False
    End Try
  End Function
End Class
