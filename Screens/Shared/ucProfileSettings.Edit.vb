Imports System.Windows.Forms

Partial Public Class ucProfileSettings

  ''' <summary>"Edit info" link was not wired to anything before.</summary>
  Private Sub linkEditInfo_EditContact(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkEditInfo.LinkClicked
    Using dlg As New EditContactDialog(lblEmail.Text, lblPhone.Text)
      If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
        lblEmail.Text = dlg.Email
        lblPhone.Text = dlg.Phone
        Msg.Info("Your contact information was updated.")
      End If
    End Using
  End Sub

  ''' <summary>Shows the saved profile picture when the page opens.</summary>
  Private Sub LoadSavedAvatar(sender As Object, e As EventArgs) Handles MyBase.Load
    Try
      If Not SessionManager.IsLoggedIn Then Return
      Dim p = ProfileService.Load(SessionManager.CurrentUserId)
      Dim img = ProfileService.LoadPicture(p.PicturePath)
      If img IsNot Nothing Then picAvatar.Image = img
    Catch
      ' a missing picture must never block the page
    End Try
  End Sub
End Class
