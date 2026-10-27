Imports System.Windows.Forms

''' <summary>
''' Every message in the program goes through here. These are the plain Windows message boxes
''' (title + text + button, NO warning/info icons), same look as the "No User Selected" box.
''' </summary>
Public Module Msg
    Public Sub Info(text As String, Optional title As String = "Information")
        MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.None)
    End Sub

    Public Sub Warn(text As String, Optional title As String = "Please check")
        MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.None)
    End Sub

    Public Sub [Error](text As String, Optional title As String = "Error")
        MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.None)
    End Sub

    ''' <summary>Standard "could not do X" message for a caught exception.</summary>
    Public Sub Fail(action As String, ex As Exception)
        MessageBox.Show("Could not " & action & "." & vbCrLf & vbCrLf & ex.Message,
                        "Action failed", MessageBoxButtons.OK, MessageBoxIcon.None)
    End Sub

    ''' <summary>Yes / No box. okText is kept so older calls compile; the buttons are the standard Yes / No.
    ''' When danger = True, "No" is the default button so Enter cannot delete by accident.</summary>
    Public Function Confirm(prompt As String, Optional title As String = "Confirm",
                            Optional okText As String = "Confirm", Optional danger As Boolean = False) As Boolean
        Dim defBtn As MessageBoxDefaultButton = If(danger, MessageBoxDefaultButton.Button2, MessageBoxDefaultButton.Button1)
        Return MessageBox.Show(prompt, title, MessageBoxButtons.YesNo, MessageBoxIcon.None, defBtn) = DialogResult.Yes
    End Function
End Module

''' <summary>Compatibility shim so existing ConfirmationDialogBox(...) calls keep compiling.</summary>
Public Module ConfirmationDialogBox
    Public Function ConfirmationDialogBox(prompt As String, title As String) As Boolean
        Return Msg.Confirm(prompt, title)
    End Function
End Module