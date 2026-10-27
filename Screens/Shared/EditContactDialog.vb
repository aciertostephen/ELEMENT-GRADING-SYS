Imports System.Windows.Forms

''' <summary>Edit Info on Profile Settings: email + phone are saved to the users table.</summary>
Public Class EditContactDialog
  Inherits DialogBase

  Private ReadOnly txtEmail As TextBox = MakeText(100)
  Private ReadOnly txtPhone As TextBox = MakeText(13)
  Public Property Email As String
  Public Property Phone As String

  Public Sub New(currentEmail As String, currentPhone As String)
    MyBase.New("Edit Contact Info", 420)
    txtEmail.Text = currentEmail
    txtPhone.Text = currentPhone
    AddField("Email:", txtEmail)
    AddField("Phone (10 digits, e.g. 9171234567):", txtPhone)
    FinishLayout("Save")
  End Sub

  Protected Overrides Function OnSubmit() As Boolean
    Try
      ProfileService.SaveContact(SessionManager.CurrentUserId, txtEmail.Text, txtPhone.Text)
      Email = txtEmail.Text.Trim()
      Phone = NormalizePhone(txtPhone.Text)
      Return True
    Catch ex As InvalidOperationException
      Msg.Warn(ex.Message)
      Return False
    End Try
  End Function
End Class
