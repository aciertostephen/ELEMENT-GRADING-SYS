Imports System.Drawing.Drawing2D
Imports System.ComponentModel

Public Class RoundedPanel
    Inherits Panel

    Private _radius As Integer = 20
    Private _borderColor As Color = Color.FromArgb(209, 213, 219)
    Private _borderThickness As Integer = 1
    Private _activeBorderColor As Color = Color.FromArgb(34, 90, 74)

    '========================================================
    ' RADIUS
    '========================================================

    <Category("Appearance")>
    <Description("The corner radius of the panel.")>
    Public Property Radius As Integer
        Get
            Return _radius
        End Get

        Set(value As Integer)
            _radius = Math.Max(0, value)
            UpdateRegion()
            Me.Invalidate()
        End Set
    End Property


    '========================================================
    ' BORDER COLOR
    '========================================================

    <Category("Appearance")>
    <Description("The normal border color.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get

        Set(value As Color)
            _borderColor = value
            Me.Invalidate()
        End Set
    End Property


    '========================================================
    ' BORDER THICKNESS
    '========================================================

    <Category("Appearance")>
    <Description("The thickness of the border.")>
    Public Property BorderThickness As Integer
        Get
            Return _borderThickness
        End Get

        Set(value As Integer)
            _borderThickness = Math.Max(0, value)
            Me.Invalidate()
        End Set
    End Property


    '========================================================
    ' ACTIVE BORDER COLOR
    '========================================================

    <Category("Appearance")>
    <Description("The border color when the panel or a control inside it has focus.")>
    Public Property ActiveBorderColor As Color
        Get
            Return _activeBorderColor
        End Get

        Set(value As Color)
            _activeBorderColor = value
            Me.Invalidate()
        End Set
    End Property


    '========================================================
    ' CONSTRUCTOR
    '========================================================

    Public Sub New()

        Me.DoubleBuffered = True
        Me.ResizeRedraw = True

    End Sub


    '========================================================
    ' RESIZE
    '========================================================

    Protected Overrides Sub OnResize(e As EventArgs)

        MyBase.OnResize(e)

        UpdateRegion()

    End Sub


    '========================================================
    ' CONTROL ADDED
    '========================================================

    Protected Overrides Sub OnControlAdded(e As ControlEventArgs)

        MyBase.OnControlAdded(e)

        AddHandler e.Control.Enter, AddressOf ChildControlEnter
        AddHandler e.Control.Leave, AddressOf ChildControlLeave

        Me.Invalidate()

    End Sub


    '========================================================
    ' CONTROL REMOVED
    '========================================================

    Protected Overrides Sub OnControlRemoved(e As ControlEventArgs)

        RemoveHandler e.Control.Enter, AddressOf ChildControlEnter
        RemoveHandler e.Control.Leave, AddressOf ChildControlLeave

        MyBase.OnControlRemoved(e)

        Me.Invalidate()

    End Sub


    Private Sub ChildControlEnter(sender As Object, e As EventArgs)

        Me.Invalidate()

    End Sub


    Private Sub ChildControlLeave(sender As Object, e As EventArgs)

        Me.Invalidate()

    End Sub


    '========================================================
    ' PAINT
    '========================================================

    Protected Overrides Sub OnPaint(e As PaintEventArgs)

        MyBase.OnPaint(e)

        ' Nothing to draw when the panel is collapsed
        If Me.Width <= 1 OrElse Me.Height <= 1 Then
            Return
        End If

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality

        Dim thickness As Single = _borderThickness

        'Keep border inside the panel
        Dim rect As New RectangleF(
            thickness / 2,
            thickness / 2,
            Me.Width - thickness,
            Me.Height - thickness
        )

        ' Too thin to round (e.g. a 1px divider): fill it flat instead of drawing arcs
        If rect.Width < 2 OrElse rect.Height < 2 Then
            Using brush As New SolidBrush(Me.BackColor)
                e.Graphics.FillRectangle(brush, 0, 0, Me.Width, Me.Height)
            End Using
            Return
        End If

        Dim diameter As Single = Math.Min(
            _radius * 2.0F,
            Math.Min(rect.Width, rect.Height)
        )

        ' Radius of 0 (or too small): draw a plain rectangle
        If diameter < 1.0F Then
            Using brush As New SolidBrush(Me.BackColor)
                e.Graphics.FillRectangle(brush, rect)
            End Using

            If _borderThickness > 0 Then
                Dim flatColor As Color = If(Me.ContainsFocus, _activeBorderColor, _borderColor)
                Using pen As New Pen(flatColor, _borderThickness)
                    e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
                End Using
            End If
            Return
        End If

        Using path As New GraphicsPath()

            'Top-left
            path.AddArc(
                rect.X,
                rect.Y,
                diameter,
                diameter,
                180,
                90
            )

            'Top-right
            path.AddArc(
                rect.Right - diameter,
                rect.Y,
                diameter,
                diameter,
                270,
                90
            )

            'Bottom-right
            path.AddArc(
                rect.Right - diameter,
                rect.Bottom - diameter,
                diameter,
                diameter,
                0,
                90
            )

            'Bottom-left
            path.AddArc(
                rect.X,
                rect.Bottom - diameter,
                diameter,
                diameter,
                90,
                90
            )

            path.CloseFigure()


            '================================================
            ' FILL
            '================================================

            Using brush As New SolidBrush(Me.BackColor)

                e.Graphics.FillPath(
                    brush,
                    path
                )

            End Using


            '================================================
            ' BORDER
            '================================================

            If _borderThickness > 0 Then

                Dim currentBorderColor As Color = _borderColor

                'Change border when the panel or
                'a control inside it has focus
                If Me.ContainsFocus Then
                    currentBorderColor = _activeBorderColor
                End If

                Using pen As New Pen(
                    currentBorderColor,
                    _borderThickness
                )

                    pen.Alignment = PenAlignment.Inset

                    e.Graphics.DrawPath(
                        pen,
                        path
                    )

                End Using

            End If

        End Using

    End Sub


    '========================================================
    ' UPDATE REGION
    '========================================================

    Private Sub UpdateRegion()

        ' Too small to round: remove any rounded region
        If Me.Width < 2 OrElse Me.Height < 2 Then
            Me.Region = Nothing
            Return
        End If

        Dim rect As New Rectangle(
            0,
            0,
            Me.Width,
            Me.Height
        )

        Dim diameter As Integer = Math.Min(
            _radius * 2,
            Math.Min(
                rect.Width,
                rect.Height
            )
        )

        If diameter < 1 Then
            Me.Region = Nothing
            Return
        End If

        Using path As New GraphicsPath()

            path.AddArc(
                rect.X,
                rect.Y,
                diameter,
                diameter,
                180,
                90
            )

            path.AddArc(
                rect.Right - diameter,
                rect.Y,
                diameter,
                diameter,
                270,
                90
            )

            path.AddArc(
                rect.Right - diameter,
                rect.Bottom - diameter,
                diameter,
                diameter,
                0,
                90
            )

            path.AddArc(
                rect.X,
                rect.Bottom - diameter,
                diameter,
                diameter,
                90,
                90
            )

            path.CloseFigure()

            Me.Region = New Region(path)

        End Using

    End Sub

End Class