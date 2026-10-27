Imports System.Drawing

''' <summary>Neutral helpers only: dialogs use the standard Windows look (no custom colors or fonts).</summary>
Public Module UiTheme
    Public ReadOnly Danger As Color = Color.Firebrick
    Public ReadOnly Muted As Color = SystemColors.GrayText

    ''' <summary>System message font (same font as a normal MessageBox).</summary>
    Public Function UiFont(Optional size As Single = 9, Optional style As FontStyle = FontStyle.Regular) As Font
        Return New Font(SystemFonts.MessageBoxFont.FontFamily, size, style)
    End Function
End Module