Imports System.Drawing
Imports System.IO
Imports System.Text

''' <summary>Fills the supplied A4 transcript sheet and writes a native, unwatermarked PDF.</summary>
Public NotInheritable Class TranscriptTemplatePdfExporter
  Private Const PageWidth As Integer = 596
  Private Const PageHeight As Integer = 842
  Private Const TableLeft As Single = 36
  Private Const TableTop As Single = 237
  Private Const RowHeight As Single = 22
  Private Shared ReadOnly ColumnEdges As Single() = {36, 139, 242, 346, 449, 552}
  Private Const RowsPerPage As Integer = 25

  Private Sub New()
  End Sub

  Public Shared Sub Export(outputPath As String, templatePath As String, studentName As String, section As String,
                           termGrade As String, schoolYear As String, gradingPeriod As String, program As String,
                           semester As String, rows As IReadOnlyList(Of String()))
    If Not File.Exists(templatePath) Then Throw New FileNotFoundException("The transcript template image was not found.", templatePath)
    If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
    Dim pageCount As Integer = Math.Max(1, CInt(Math.Ceiling(rows.Count / CDbl(RowsPerPage))))
    Dim pageStreams As New List(Of Byte())
    For page As Integer = 0 To pageCount - 1
      Dim startIndex As Integer = page * RowsPerPage
      Dim count As Integer = Math.Min(RowsPerPage, Math.Max(0, rows.Count - startIndex))
      pageStreams.Add(BuildPage(studentName, section, termGrade, schoolYear, gradingPeriod, program, semester, rows, startIndex, count, page + 1, pageCount))
    Next
    WritePdf(outputPath, templatePath, pageStreams)
  End Sub

  Private Shared Function BuildPage(studentName As String, section As String, termGrade As String, schoolYear As String,
                                    gradingPeriod As String, program As String, semester As String,
                                    rows As IReadOnlyList(Of String()), startIndex As Integer, rowCount As Integer,
                                    pageNumber As Integer, pageCount As Integer) As Byte()
    Dim content As New StringBuilder()
    content.Append("q 596 0 0 842 0 0 cm /Im0 Do Q" & vbLf)

    ' Remove the template placeholders while retaining its labels, boxes, and both logos.
    WhiteBox(content, 70, 107, 360, 19)
    WhiteBox(content, 505, 107, 55, 19)
    WhiteBox(content, 80, 122, 145, 18)
    WhiteBox(content, 177, 138, 116, 23)
    WhiteBox(content, 435, 138, 115, 23)
    WhiteBox(content, 178, 162, 115, 23)
    WhiteBox(content, 449, 162, 101, 23)

    Text(content, studentName, 72, 110, 10, 360)
    Text(content, termGrade, 508, 110, 10, 48)
    Text(content, section, 83, 125, 10, 145)
    Text(content, schoolYear, 190, 145, 9, 100, True)
    Text(content, gradingPeriod, 440, 145, 9, 104, True)
    Text(content, program, 194, 169, 9, 98, True)
    Text(content, semester, 453, 169, 9, 94, True)

    ' Clear the template's sample data row and continue the table using its five original columns.
    WhiteBox(content, TableLeft, TableTop, ColumnEdges.Last() - TableLeft, PageHeight - TableTop - 38)
    Dim endY As Single = TableTop + (rowCount * RowHeight)
    For Each edge As Single In ColumnEdges
      Line(content, edge, TableTop, edge, endY)
    Next
    For i As Integer = 0 To rowCount
      Dim y As Single = TableTop + i * RowHeight
      Line(content, TableLeft, y, ColumnEdges.Last(), y)
    Next

    For i As Integer = 0 To rowCount - 1
      Dim values As String() = rows(startIndex + i)
      For col As Integer = 0 To 4
        Dim value As String = If(values IsNot Nothing AndAlso col < values.Length, values(col), "")
        Text(content, value, ColumnEdges(col) + 5, TableTop + i * RowHeight + 6, 8, ColumnEdges(col + 1) - ColumnEdges(col) - 10)
      Next
    Next
    If pageCount > 1 Then Text(content, "Page " & pageNumber.ToString() & " of " & pageCount.ToString(), 466, 814, 7, 85)
    Return Encoding.ASCII.GetBytes(content.ToString())
  End Function

  Private Shared Sub WhiteBox(content As StringBuilder, x As Single, top As Single, width As Single, height As Single)
    content.Append("q 1 1 1 rg ")
    content.Append(N(x)).Append(" ").Append(N(PageHeight - top - height)).Append(" ")
    content.Append(N(width)).Append(" ").Append(N(height)).Append(" re f Q" & vbLf)
  End Sub

  Private Shared Sub Text(content As StringBuilder, value As String, x As Single, top As Single,
                          size As Integer, maxWidth As Single, Optional bold As Boolean = False)
    Dim maxChars As Integer = Math.Max(1, CInt(Math.Floor(maxWidth / (size * 0.53))))
    Dim safe As String = If(value, "").Replace(vbCr, " ").Replace(vbLf, " ").Trim()
    If safe.Length > maxChars Then safe = safe.Substring(0, Math.Max(1, maxChars - 3)) & "..."
    Dim hex As String = Convert.ToHexString(Encoding.Latin1.GetBytes(safe))
    Dim font As String = If(bold, "F2", "F1")
    Dim baseline As Single = PageHeight - top - size
    content.Append("0 0 0 rg BT /").Append(font).Append(" ").Append(size).Append(" Tf 1 0 0 1 ")
    content.Append(N(x)).Append(" ").Append(N(baseline)).Append(" Tm <").Append(hex).Append("> Tj ET" & vbLf)
  End Sub

  Private Shared Sub Line(content As StringBuilder, x1 As Single, y1Top As Single, x2 As Single, y2Top As Single)
    content.Append("0 0 0 RG 0.55 w ").Append(N(x1)).Append(" ").Append(N(PageHeight - y1Top)).Append(" m ")
    content.Append(N(x2)).Append(" ").Append(N(PageHeight - y2Top)).Append(" l S" & vbLf)
  End Sub

  Private Shared Function N(value As Single) As String
    Return value.ToString("0.##", Globalization.CultureInfo.InvariantCulture)
  End Function

  Private Shared Sub WritePdf(path As String, templatePath As String, pageStreams As IReadOnlyList(Of Byte()))
    Dim imageBytes As Byte() = File.ReadAllBytes(templatePath)
    Dim imageWidth As Integer
    Dim imageHeight As Integer
    Using image As Image = Image.FromFile(templatePath)
      imageWidth = image.Width
      imageHeight = image.Height
    End Using

    Dim objects As New List(Of Byte())
    objects.Add(Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"))
    Dim kids As New List(Of Integer)
    For i As Integer = 0 To pageStreams.Count - 1
      kids.Add(6 + i * 2)
    Next
    objects.Add(Encoding.ASCII.GetBytes("<< /Type /Pages /Kids [" & String.Join(" ", kids.Select(Function(id) id & " 0 R")) & "] /Count " & pageStreams.Count & " >>"))
    objects.Add(Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"))
    objects.Add(Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"))
    Dim imageHeader = Encoding.ASCII.GetBytes("<< /Type /XObject /Subtype /Image /Width " & imageWidth & " /Height " & imageHeight &
                                             " /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " & imageBytes.Length & " >>" & vbLf & "stream" & vbLf)
    Dim imageSuffix = Encoding.ASCII.GetBytes(vbLf & "endstream")
    Using imageObject As New MemoryStream()
      imageObject.Write(imageHeader, 0, imageHeader.Length)
      imageObject.Write(imageBytes, 0, imageBytes.Length)
      imageObject.Write(imageSuffix, 0, imageSuffix.Length)
      objects.Add(imageObject.ToArray())
    End Using

    For i As Integer = 0 To pageStreams.Count - 1
      Dim pageId As Integer = 6 + i * 2
      Dim contentId As Integer = pageId + 1
      objects.Add(Encoding.ASCII.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " & PageWidth & " " & PageHeight & "] " &
        "/Resources << /Font << /F1 3 0 R /F2 4 0 R >> /XObject << /Im0 5 0 R >> >> /Contents " & contentId & " 0 R >>"))
      Dim stream As Byte() = pageStreams(i)
      Dim prefix = Encoding.ASCII.GetBytes("<< /Length " & stream.Length & " >>" & vbLf & "stream" & vbLf)
      Dim suffix = Encoding.ASCII.GetBytes(vbLf & "endstream")
      Using ms As New MemoryStream()
        ms.Write(prefix, 0, prefix.Length)
        ms.Write(stream, 0, stream.Length)
        ms.Write(suffix, 0, suffix.Length)
        objects.Add(ms.ToArray())
      End Using
    Next

    Using output As New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
      Dim header = Encoding.ASCII.GetBytes("%PDF-1.4" & vbLf)
      output.Write(header, 0, header.Length)
      Dim offsets As New List(Of Long) From {0L}
      For i As Integer = 0 To objects.Count - 1
        offsets.Add(output.Position)
        Dim start = Encoding.ASCII.GetBytes((i + 1).ToString() & " 0 obj" & vbLf)
        Dim finish = Encoding.ASCII.GetBytes(vbLf & "endobj" & vbLf)
        output.Write(start, 0, start.Length)
        output.Write(objects(i), 0, objects(i).Length)
        output.Write(finish, 0, finish.Length)
      Next
      Dim xrefPosition As Long = output.Position
      Dim xref As New StringBuilder()
      xref.Append("xref" & vbLf & "0 " & (objects.Count + 1) & vbLf).Append("0000000000 65535 f " & vbLf)
      For i As Integer = 1 To objects.Count
        xref.Append(offsets(i).ToString("0000000000", Globalization.CultureInfo.InvariantCulture)).Append(" 00000 n ").Append(vbLf)
      Next
      xref.Append("trailer" & vbLf & "<< /Size " & (objects.Count + 1) & " /Root 1 0 R >>" & vbLf)
      xref.Append("startxref" & vbLf & xrefPosition & vbLf & "%%EOF")
      Dim tail = Encoding.ASCII.GetBytes(xref.ToString())
      output.Write(tail, 0, tail.Length)
    End Using
  End Sub
End Class
