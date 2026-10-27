Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

''' <summary>Shared control helpers (replaces the two different DisposeAndClear versions).</summary>
Public Module ControlExtensions

  ''' <summary>Disposes every child control, then clears the container.</summary>
  <Extension()>
  Public Sub DisposeAndClear(container As Control)
    If container Is Nothing Then Return
    container.SuspendLayout()
    For i As Integer = container.Controls.Count - 1 To 0 Step -1
      Dim c As Control = container.Controls(i)
      container.Controls.RemoveAt(i)
      c.Dispose()
    Next
    container.ResumeLayout()
  End Sub

  ''' <summary>
  ''' Right-click works only on the control it is assigned to, and a card is made of many child
  ''' labels, so the menu is attached to the control and every child recursively.
  ''' </summary>
  <Extension()>
  Public Sub AttachContextMenu(ctrl As Control, menu As ContextMenuStrip)
    ctrl.ContextMenuStrip = menu
    For Each child As Control In ctrl.Controls
      child.AttachContextMenu(menu)
    Next
  End Sub
End Module
