Imports System.IO
Imports System.Text.RegularExpressions
Imports ClosedXML.Excel

''' <summary>Why a row of the class list can or cannot be imported.</summary>
Public Enum ClassListRowStatus
  Ready              ' valid, will be imported
  LooksNotEnrolled   ' yellow row / marked withdrawn, LOA, no show ... (teacher may still tick it)
  MissingStudentNo   ' no usable student number (cannot be imported)
  MissingName        ' name is not "LAST, FIRST" (cannot be imported)
End Enum

''' <summary>One class-list sheet such as "BSIT 2A 2026-2027" or "BSIT 1D-BAP 2026-2027".</summary>
Public Class ClassListSheet
  Public Property SheetName As String
  Public Property ProgramCode As String      ' BSIT
  Public Property YearLevel As Integer       ' 2
  Public Property SectionCode As String      ' BSIT2A  (same style as section.section_code)
  Public Property SchoolYear As String       ' 2026-2027

  Public Overrides Function ToString() As String
    Return SheetName.Trim()
  End Function
End Class

''' <summary>One student row read from a sheet.</summary>
Public Class ClassListRow
  Public Property ExcelRow As Integer
  Public Property StudentNo As String = ""
  Public Property LastName As String = ""
  Public Property FirstName As String = ""
  Public Property MiddleName As String = ""
  Public Property Status As ClassListRowStatus = ClassListRowStatus.Ready
  Public Property Note As String = ""
  Public Property Include As Boolean

  ''' <summary>Rows without a student number or a proper name can never be imported.</summary>
  Public ReadOnly Property CanInclude As Boolean
    Get
      Return Status = ClassListRowStatus.Ready OrElse Status = ClassListRowStatus.LooksNotEnrolled
    End Get
  End Property

  Public ReadOnly Property FullName As String
    Get
      Return LastName & ", " & FirstName & If(MiddleName = "", "", " " & MiddleName)
    End Get
  End Property
End Class

''' <summary>
''' Reads the registrar's class-list workbook (one sheet per section, named like "BSIT 2A 2026-2027").
''' Pure Excel parsing - it never touches the database.
'''
''' The sheets are NOT identical (the header is on row 2, 3 or 4, 3rd/4th year sheets repeat the header
''' for irregular students, and the helper formula columns are unreliable), so the reader:
'''   * finds the "Student No." and "Student Name" columns by their header text,
'''   * reads the name only from the "Student Name" column ("LAST, FIRST MIDDLE"),
'''   * skips repeated header rows and section titles such as "REGULAR STUDENTS",
'''   * flags rows that are yellow or marked withdrawn / LOA / no show / not enrolled.
''' The file is copied into memory first, so it can be read while it is still open in Excel.
''' </summary>
Public NotInheritable Class ClassListExcelReader
  Implements IDisposable

  Private Shared ReadOnly SheetNamePattern As New Regex(
    "^\s*([A-Za-z]+)\s+(\d)([A-Za-z0-9\-]*)\s+(\d{4}-\d{4})\s*$", RegexOptions.Compiled)

  Private Shared ReadOnly StudentNoPattern As New Regex("^\d{2}-\d{5}$", RegexOptions.Compiled)

  Private Shared ReadOnly NotEnrolledPattern As New Regex(
    "NOT\s+ENROLLED|WITHDRAW|NO\s+SHOW|\bLOA\b|\bEXIT\b|NO\s+LONGER\s+ATTENDING|NOT\s+ATTENDING|TRANSFER\s+IN\s+OTHER|\bNE\b",
    RegexOptions.IgnoreCase Or RegexOptions.Compiled)

  Private ReadOnly _stream As MemoryStream
  Private ReadOnly _book As XLWorkbook
  Private ReadOnly _sheets As New List(Of ClassListSheet)

  Private Sub New(path As String)
    _stream = New MemoryStream()
    Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
      fs.CopyTo(_stream)
    End Using
    _stream.Position = 0
    _book = New XLWorkbook(_stream)

    For Each ws As IXLWorksheet In _book.Worksheets
      Dim sheet As ClassListSheet = ParseSheetName(ws.Name)
      If sheet IsNot Nothing Then _sheets.Add(sheet)
    Next
  End Sub

  Public Shared Function Open(path As String) As ClassListExcelReader
    Return New ClassListExcelReader(path)
  End Function

  ''' <summary>Only the sheets that look like "PROGRAM 2A 2026-2027" (other tabs such as "LIST NOT ENROLLED" are ignored).</summary>
  Public ReadOnly Property Sheets As IReadOnlyList(Of ClassListSheet)
    Get
      Return _sheets
    End Get
  End Property

  ' ------------------------------------------------------------------
  ' Sheet name  ->  program / year level / section code / school year
  ' ------------------------------------------------------------------

  Private Shared Function ParseSheetName(name As String) As ClassListSheet
    Dim m As Match = SheetNamePattern.Match(If(name, ""))
    If Not m.Success Then Return Nothing
    Dim program As String = m.Groups(1).Value.ToUpperInvariant()
    Dim level As Integer = Integer.Parse(m.Groups(2).Value)
    Dim suffix As String = m.Groups(3).Value.ToUpperInvariant()
    Return New ClassListSheet With {
      .SheetName = name, .ProgramCode = program, .YearLevel = level,
      .SectionCode = program & level.ToString() & suffix, .SchoolYear = m.Groups(4).Value}
  End Function

  ' ------------------------------------------------------------------
  ' Students of one sheet
  ' ------------------------------------------------------------------

  Public Function ReadStudents(sheet As ClassListSheet) As List(Of ClassListRow)
    Dim result As New List(Of ClassListRow)
    Dim ws As IXLWorksheet = _book.Worksheet(sheet.SheetName)

    ' 1) find the header row ("Student No." + "Student Name"; "EMAIL" tells where the status columns start)
    Dim headerRow As Integer = 0, noCol As Integer = 0, nameCol As Integer = 0, mailCol As Integer = 0
    For r As Integer = 1 To 10
      Dim cNo As Integer = 0, cName As Integer = 0, cMail As Integer = 0
      For c As Integer = 1 To 30
        Select Case Compact(CellText(ws, r, c))
          Case "studentno"
            If cNo = 0 Then cNo = c
          Case "studentname"
            If cName = 0 Then cName = c
          Case "email"
            If cMail = 0 Then cMail = c
        End Select
      Next
      If cNo > 0 AndAlso cName > 0 Then
        headerRow = r : noCol = cNo : nameCol = cName : mailCol = cMail
        Exit For
      End If
    Next
    If headerRow = 0 Then
      Throw New InvalidOperationException("Could not find the ""Student No."" and ""Student Name"" columns in sheet """ &
                                          sheet.SheetName.Trim() & """.")
    End If

    Dim statusStart As Integer = If(mailCol > 0, mailCol + 1, nameCol + 1)

    Dim lastUsed As IXLRow = ws.LastRowUsed()
    Dim lastRow As Integer = If(lastUsed Is Nothing, headerRow, lastUsed.RowNumber())
    lastRow = Math.Min(lastRow, headerRow + 500)

    ' 2) read the rows
    For r As Integer = headerRow + 1 To lastRow
      Dim sn As String = CleanSpaces(CellText(ws, r, noCol))
      Dim rawName As String = CleanSpaces(CellText(ws, r, nameCol))
      If sn = "" AndAlso rawName = "" Then Continue For                       ' blank line / section title
      If Compact(sn) = "studentno" OrElse Compact(rawName) = "studentname" Then Continue For   ' repeated header

      Dim row As New ClassListRow With {.ExcelRow = r, .StudentNo = sn}
      Dim nameOk As Boolean = ParseName(rawName, row)

      If Not StudentNoPattern.IsMatch(sn) Then
        row.Status = ClassListRowStatus.MissingStudentNo
        row.Note = If(sn = "", "no student number", "invalid student number")
      ElseIf Not nameOk Then
        row.Status = ClassListRowStatus.MissingName
        row.Note = "name must look like LAST NAME, FIRST NAME"
      Else
        Dim reason As String = NotEnrolledReason(ws, r, statusStart)
        If reason IsNot Nothing Then
          row.Status = ClassListRowStatus.LooksNotEnrolled
          row.Note = reason
        End If
      End If

      row.Include = (row.Status = ClassListRowStatus.Ready)
      result.Add(row)
    Next
    Return result
  End Function

  ' ------------------------------------------------------------------
  ' Row helpers
  ' ------------------------------------------------------------------

  ''' <summary>
  ''' "LAST, FIRST MIDDLE" -> last / first / middle.
  ''' The last word is the middle name or initial (same rule the registrar's e-mail formula uses),
  ''' unless the name ends with a lone "." placeholder, in which case there is no middle name.
  ''' </summary>
  Private Shared Function ParseName(raw As String, row As ClassListRow) As Boolean
    row.LastName = raw
    Dim comma As Integer = raw.IndexOf(","c)
    If comma <= 0 Then Return False

    Dim last As String = raw.Substring(0, comma).Trim()
    Dim given As String = raw.Substring(comma + 1).Replace(","c, " "c)
    Dim toks As List(Of String) = given.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries).ToList()

    Dim placeholderAtEnd As Boolean = toks.Count > 0 AndAlso Not HasLetterOrDigit(toks(toks.Count - 1))
    toks = toks.Where(Function(t) HasLetterOrDigit(t)).ToList()
    If last = "" OrElse toks.Count = 0 Then Return False

    Dim middle As String = ""
    If toks.Count >= 2 AndAlso Not placeholderAtEnd Then
      middle = toks(toks.Count - 1)
      toks.RemoveAt(toks.Count - 1)
    End If

    row.LastName = last
    row.FirstName = String.Join(" ", toks)
    row.MiddleName = middle
    Return True
  End Function

  ''' <summary>Returns the reason a row looks "not enrolled" (text mark or yellow highlight), or Nothing.</summary>
  Private Shared Function NotEnrolledReason(ws As IXLWorksheet, r As Integer, statusStart As Integer) As String
    For c As Integer = statusStart To statusStart + 7
      Dim t As String = CleanSpaces(CellText(ws, r, c))
      If t <> "" AndAlso NotEnrolledPattern.IsMatch(t) Then Return "marked """ & t & """"
    Next
    For c As Integer = 1 To statusStart + 5
      If IsYellow(ws.Cell(r, c)) Then Return "highlighted yellow"
    Next
    Return Nothing
  End Function

  Private Shared Function IsYellow(cell As IXLCell) As Boolean
    Try
      Dim fill As IXLFill = cell.Style.Fill
      If fill.PatternType = XLFillPatternValues.None Then Return False
      Return IsYellowColor(fill.BackgroundColor) OrElse IsYellowColor(fill.PatternColor)
    Catch
      Return False
    End Try
  End Function

  Private Shared Function IsYellowColor(xlColor As XLColor) As Boolean
    If xlColor Is Nothing OrElse xlColor.ColorType <> XLColorType.Color Then Return False
    Dim c As System.Drawing.Color = xlColor.Color
    Return c.R >= 230 AndAlso c.G >= 200 AndAlso c.B <= 120
  End Function

  Private Shared Function CellText(ws As IXLWorksheet, r As Integer, c As Integer) As String
    Try
      Return If(ws.Cell(r, c).GetString(), "")
    Catch
      Return ""
    End Try
  End Function

  Private Shared Function CleanSpaces(s As String) As String
    Return Regex.Replace(If(s, ""), "\s+", " ").Trim()
  End Function

  ''' <summary>"     Student No." -> "studentno" (for matching header cells).</summary>
  Private Shared Function Compact(s As String) As String
    Return Regex.Replace(If(s, ""), "[\s\.]", "").ToLowerInvariant()
  End Function

  Private Shared Function HasLetterOrDigit(s As String) As Boolean
    For Each ch As Char In s
      If Char.IsLetterOrDigit(ch) Then Return True
    Next
    Return False
  End Function

  Public Sub Dispose() Implements IDisposable.Dispose
    _book.Dispose()
    _stream.Dispose()
  End Sub
End Class
