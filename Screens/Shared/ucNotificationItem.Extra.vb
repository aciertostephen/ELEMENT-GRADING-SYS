Imports System.Drawing
Imports System.Windows.Forms

Partial Public Class ucNotificationItem

  Private _openLink As LinkLabel

  ' Read-only status chip (teachers see Pending / Reviewed / Notice, students see Released / Notice).
  Public Sub SetStatusOnly(text As String)
    btnAction.Text = text
    btnAction.BackColor = Color.FromArgb(220, 220, 220)
    btnAction.ForeColor = Color.Gray
    btnAction.Enabled = False
  End Sub

  ' "Open class" link next to the timestamp. Call AFTER SetData so the timestamp width is final.
  Public Sub SetOpenLink(onClick As Action)
    If _openLink Is Nothing Then
      _openLink = New LinkLabel() With {.Text = "Open class", .AutoSize = True, .Cursor = Cursors.Hand}
      _openLink.Location = New Point(lblTimestamp.Right + 16, lblTimestamp.Top)
      Controls.Add(_openLink)
    End If
    AddHandler _openLink.LinkClicked, Sub(s, e) onClick()
  End Sub
End Class
