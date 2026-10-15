Imports System.Windows.Forms

Public Module ConfirmationDialogBox

    Public Function ConfirmationDialogBox(prompt As String, title As String) As Boolean
        Using msgBox As New Form()
            msgBox.Width = 380
            msgBox.Height = 160
            msgBox.FormBorderStyle = FormBorderStyle.FixedDialog
            msgBox.Text = title
            msgBox.StartPosition = FormStartPosition.CenterParent
            msgBox.MaximizeBox = False
            msgBox.MinimizeBox = False

            Dim lblText As New Label() With {
                .Left = 20,
                .Top = 20,
                .Width = 320,
                .Height = 40,
                .Text = prompt
            }

            Dim btnNo As New Button() With {
                .Text = "No",
                .Left = 168,
                .Top = 70,
                .Width = 85,
                .Height = 28,
                .DialogResult = DialogResult.No
            }

            Dim btnConfirm As New Button() With {
                .Text = "Confirm",
                .Left = 255,
                .Top = 70,
                .Width = 85,
                .Height = 28,
                .DialogResult = DialogResult.Yes
            }

            msgBox.Controls.Add(lblText)
            msgBox.Controls.Add(btnNo)
            msgBox.Controls.Add(btnConfirm)

            msgBox.AcceptButton = btnConfirm
            msgBox.CancelButton = btnNo

            Return msgBox.ShowDialog() = DialogResult.Yes
        End Using
    End Function

End Module