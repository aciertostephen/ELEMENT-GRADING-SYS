Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Base for the input pop-ups (Create Course, Create/Edit Class, Edit Contact Info).
''' Standard Windows look: default font, default title bar, default OK / Cancel buttons.
''' Subclasses call AddField(...) for each input, FinishLayout("Save"), and override OnSubmit.
''' </summary>
Public MustInherit Class DialogBase
    Inherits Form

    Protected ReadOnly BtnOk As New Button()
    Protected ReadOnly BtnCancel As New Button()
    Private _y As Integer = 16

    Protected Sub New(title As String, Optional width As Integer = 420)
        Text = title
        Font = SystemFonts.MessageBoxFont
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        ShowIcon = False
        ClientSize = New Size(width, 100)
    End Sub

    ''' <summary>Adds a caption and the control under it. Returns the control.</summary>
    Protected Function AddField(caption As String, ctl As Control) As Control
        Dim lbl As New Label() With {.Text = caption, .AutoSize = True, .Location = New Point(16, _y)}
        ctl.Location = New Point(16, _y + 20)
        ctl.Width = ClientSize.Width - 32
        Controls.Add(lbl)
        Controls.Add(ctl)
        _y += 20 + ctl.Height + 12
        Return ctl
    End Function

    Private Function NoteHeight(text As String) As Integer
        Dim h As Integer = TextRenderer.MeasureText(text, Font, New Size(ClientSize.Width - 32, Integer.MaxValue),
                                                    TextFormatFlags.WordBreak).Height
        Return Math.Max(34, h + 6)
    End Function

    Protected Sub AddNote(text As String, Optional isWarning As Boolean = False)
        Dim lbl As New Label() With {
          .Text = text, .AutoSize = False, .Width = ClientSize.Width - 32, .Height = NoteHeight(text),
          .ForeColor = SystemColors.ControlText, .Location = New Point(16, _y)}
        Controls.Add(lbl)
        _y += lbl.Height + 6
    End Sub

    Protected Function MakeCombo(data As DataTable, display As String, value As String) As ComboBox
        Dim c As New ComboBox() With {.DropDownStyle = ComboBoxStyle.DropDownList}
        c.DataSource = data
        c.DisplayMember = display
        c.ValueMember = value
        Return c
    End Function

    Protected Function MakeText(Optional maxLength As Integer = 100) As TextBox
        Return New TextBox() With {.MaxLength = maxLength}
    End Function

    ''' <summary>Adds the OK / Cancel buttons and sizes the dialog. Call last.</summary>
    Protected Sub FinishLayout(Optional okText As String = "OK", Optional dangerOk As Boolean = False)
        BtnOk.Text = okText
        BtnCancel.Text = "Cancel"
        BtnOk.SetBounds(ClientSize.Width - 186, _y + 4, 80, 26)
        BtnCancel.SetBounds(ClientSize.Width - 96, _y + 4, 80, 26)
        BtnCancel.DialogResult = DialogResult.Cancel
        AddHandler BtnOk.Click, AddressOf OkClicked
        Controls.Add(BtnOk)
        Controls.Add(BtnCancel)
        AcceptButton = BtnOk
        CancelButton = BtnCancel
        ClientSize = New Size(ClientSize.Width, _y + 42)
    End Sub

    Private Sub OkClicked(sender As Object, e As EventArgs)
        Try
            If OnSubmit() Then DialogResult = DialogResult.OK
        Catch ex As Exception
            Msg.Fail("save", ex)
        End Try
    End Sub

    ''' <summary>Validate + save. Return True to close the dialog, False to keep it open.</summary>
    Protected MustOverride Function OnSubmit() As Boolean
End Class
