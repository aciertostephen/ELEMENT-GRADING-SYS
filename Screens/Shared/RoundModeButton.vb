Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>A compact, rounded, owner-drawn button for mode toggles.</summary>
Public Class RoundModeButton
    Inherits Button

    Private _pressed As Boolean
    Public Property CornerRadius As Integer = 12

    Public Sub New()
        FlatStyle = FlatStyle.Flat
        FlatAppearance.BorderSize = 0
        UseVisualStyleBackColor = False
        TextAlign = ContentAlignment.MiddleCenter
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        _pressed = True
        Invalidate()
        MyBase.OnMouseDown(e)
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        _pressed = False
        Invalidate()
        MyBase.OnMouseUp(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        _pressed = False
        Invalidate()
        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Dim bounds As Rectangle = ClientRectangle
        bounds.Inflate(-1, -1)
        Dim diameter As Integer = Math.Min(CornerRadius * 2, Math.Min(bounds.Width, bounds.Height))
        Using path As New GraphicsPath()
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90)
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90)
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90)
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90)
            path.CloseFigure()
            Dim fill As Color = BackColor
            If Not Enabled Then
                fill = Color.LightGray
            ElseIf _pressed Then
                fill = ControlPaint.Dark(BackColor, 0.08F)
            End If
            Using brush As New SolidBrush(fill)
                e.Graphics.FillPath(brush, path)
            End Using
        End Using
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
    End Sub
End Class
