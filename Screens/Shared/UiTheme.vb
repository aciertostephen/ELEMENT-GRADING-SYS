Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

''' <summary>Neutral helpers; screens retain their designer-defined appearance.</summary>
Public Module UiTheme
    Public ReadOnly Danger As Color = Color.Firebrick
    Public ReadOnly Muted As Color = SystemColors.GrayText
    Private ReadOnly HookedButtons As New ConditionalWeakTable(Of ReaLTaiizor.Controls.Button, Object)

    Public Function UiFont(Optional size As Single = 9, Optional style As FontStyle = FontStyle.Regular) As Font
        Return New Font(SystemFonts.MessageBoxFont.FontFamily, size, style)
    End Function

    ''' <summary>Restore the original hover color after actions to avoid a persistent hover state.</summary>
    Public Sub HookButtonReset(root As Control)
        If root Is Nothing Then Return
        For Each control As Control In root.Controls
            If TypeOf control Is ReaLTaiizor.Controls.Button Then
                Dim button = DirectCast(control, ReaLTaiizor.Controls.Button)
                Dim marker As Object = Nothing
                If Not HookedButtons.TryGetValue(button, marker) Then
                    HookedButtons.Add(button, New Object())
                    Dim hoverColor As Color = button.EnteredColor
                    AddHandler button.Click,
                        Sub(sender, e)
                            If button.IsDisposed Then Return
                            button.EnteredColor = button.InactiveColor
                            button.Invalidate()
                        End Sub
                    AddHandler button.MouseLeave,
                        Sub(sender, e)
                            If button.IsDisposed Then Return
                            button.EnteredColor = hoverColor
                            button.Invalidate()
                        End Sub
                End If
            End If
            If control.HasChildren Then HookButtonReset(control)
        Next
    End Sub
End Module
