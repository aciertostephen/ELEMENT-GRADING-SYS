Imports System.Globalization
Imports System.Windows.Forms

''' <summary>Standard Windows input dialog used by the gradebook instead of InputBox.</summary>
Public NotInheritable Class GradebookValueDialog
  Inherits DialogBase

  Private ReadOnly _text As TextBox
  Private ReadOnly _choice As ComboBox

  Public ReadOnly Property Value As String
    Get
      If _choice IsNot Nothing Then Return If(_choice.SelectedItem, "").ToString()
      Return If(_text Is Nothing, "", _text.Text.Trim())
    End Get
  End Property

  Public Sub New(title As String, prompt As String, initialValue As String,
                 Optional choices As IEnumerable(Of String) = Nothing,
                 Optional maxLength As Integer = 100)
    MyBase.New(title, 440)
    If choices IsNot Nothing Then
      _choice = New ComboBox() With {.DropDownStyle = ComboBoxStyle.DropDownList}
      _choice.Items.AddRange(choices.Cast(Of Object)().ToArray())
      If _choice.Items.Count > 0 Then
        Dim index As Integer = _choice.Items.IndexOf(initialValue)
        _choice.SelectedIndex = If(index >= 0, index, 0)
      End If
      AddField(prompt, _choice)
    Else
      _text = MakeText(maxLength)
      _text.Text = initialValue
      AddField(prompt, _text)
    End If
    FinishLayout("Save")
  End Sub

  Protected Overrides Function OnSubmit() As Boolean
    If Value.Length = 0 Then
      Msg.Warn("Enter or choose a value before continuing.")
      Return False
    End If
    Return True
  End Function
End Class
