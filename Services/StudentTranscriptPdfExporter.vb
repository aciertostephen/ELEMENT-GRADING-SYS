Imports System.IO
Imports System.Text

''' <summary>Writes a small, dependency-free PDF transcript with no watermark.</summary>
Public NotInheritable Class StudentTranscriptPdfExporter
  Private Const PageWidth As Integer = 612
  Private Const PageHeight As Integer = 792
  Private Const LeftMargin As Integer = 32
  Private Const RightMargin As Integer = 32

  Private Sub New()
  End Sub

  Public Shared Sub Export(path As String, studentInfo As String, gwa As String,
                           headers As IReadOnlyList(Of String), rows As IReadOnlyList(Of String()))
    If String.IsNullOrWhiteSpace(path) Then Throw New ArgumentException("Choose a PDF file path.", NameOf(path))
    If headers Is Nothing OrElse headers.Count <> 4 Then Throw New InvalidOperationException("The transcript table must have four columns.")
    Dim pages As New List(Of Byte())
    Dim rowIndex As Integer = 0
    Dim pageNumber As Integer = 1
    Do
      Dim firstPage As Boolean = (pageNumber = 1)
      Dim tableTop As Integer = If(firstPage, 220, 100)
      Dim available As Integer = PageHeight - 48 - tableTop - 28
      Dim rowCapacity As Integer = Math.Max(1, available \ 26)
      Dim take As Integer = Math.Min(rowCapacity, Math.Max(0, rows.Count - rowIndex))
      pages.Add(BuildPage(studentInfo, gwa, headers, rows, rowIndex, take, pageNumber, firstPage, tableTop))
      rowIndex += take
      pageNumber += 1
    Loop While rowIndex < rows.Count

    WritePdf(path, pages)
  End Sub

  Private Shared Function BuildPage(studentInfo As String, gwa As String, headers As IReadOnlyList(Of String),
                                    rows As IReadOnlyList(Of String()), startIndex As Integer, rowCount As Integer,
                                    pageNumber As Integer, firstPage As Boolean, tableTop As Integer) As Byte()
    Dim content As New StringBuilder()
    If firstPage Then
      DrawText(content, "ELEMENT GRADING SYSTEM", LeftMargin, 44, "F2", 18, PageWidth - LeftMargin - RightMargin)
      DrawText(content, "STUDENT TRANSCRIPT", LeftMargin, 76, "F2", 11, PageWidth - LeftMargin - RightMargin)
      Dim lines = If(studentInfo, "").Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
      Dim infoY As Integer = 102
      For Each infoLine As String In lines.Take(2)
        DrawText(content, infoLine, LeftMargin, infoY, "F1", 10, PageWidth - LeftMargin - RightMargin)
        infoY += 17
      Next
      DrawText(content, gwa, LeftMargin, 151, "F2", 10, PageWidth - LeftMargin - RightMargin)
      DrawText(content, "Generated " & Date.Now.ToString("yyyy-MM-dd"), LeftMargin, 172, "F1", 8, PageWidth - LeftMargin - RightMargin)
    Else
      DrawText(content, "STUDENT TRANSCRIPT (continued)", LeftMargin, 44, "F2", 11, PageWidth - LeftMargin - RightMargin)
    End If

    Dim widths As Single() = {74, 220, 190, 64}
    Dim tableWidth As Single = widths.Sum()
    Dim y As Single = tableTop
    DrawRect(content, LeftMargin, y, tableWidth, 28, True, True)
    Dim x As Single = LeftMargin
    For col As Integer = 0 To headers.Count - 1
      DrawText(content, headers(col), x + 5, y + 8, "F2", 8, widths(col) - 10)
      x += widths(col)
    Next
    y += 28

    For rowOffset As Integer = 0 To rowCount - 1
      Dim values As String() = rows(startIndex + rowOffset)
      Dim fill As Boolean = (rowOffset Mod 2 = 1)
      DrawRect(content, LeftMargin, y, tableWidth, 26, fill, True)
      x = LeftMargin
      For col As Integer = 0 To headers.Count - 1
        Dim value As String = If(values IsNot Nothing AndAlso col < values.Length, values(col), "")
        DrawText(content, value, x + 5, y + 8, "F1", 8, widths(col) - 10)
        x += widths(col)
      Next
      y += 26
    Next
    DrawText(content, "Page " & pageNumber.ToString(), PageWidth - RightMargin - 50, PageHeight - 34, "F1", 8, 50)
    Return Encoding.ASCII.GetBytes(content.ToString())
  End Function

  Private Shared Sub DrawText(content As StringBuilder, value As String, x As Single, top As Single,
                               font As String, size As Integer, maxWidth As Single)
    Dim maxChars As Integer = Math.Max(1, CInt(Math.Floor(maxWidth / (size * 0.55))))
    Dim safe As String = If(value, "").Replace(vbCr, " ").Replace(vbLf, " ").Trim()
    If safe.Length > maxChars Then safe = safe.Substring(0, Math.Max(1, maxChars - 3)) & "..."
    Dim bytes As Byte() = Encoding.Latin1.GetBytes(safe)
    Dim hex As String = Convert.ToHexString(bytes)
    Dim baseline As Single = PageHeight - top - size
    content.Append("0 0 0 rg" & vbLf)
    content.Append("BT /").Append(font).Append(" ").Append(size).Append(" Tf 1 0 0 1 ")
    content.Append(x.ToString("0.##", Globalization.CultureInfo.InvariantCulture)).Append(" ")
    content.Append(baseline.ToString("0.##", Globalization.CultureInfo.InvariantCulture))
    content.Append(" Tm <").Append(hex).Append("> Tj ET" & vbLf)
  End Sub

  Private Shared Sub DrawRect(content As StringBuilder, x As Single, top As Single, width As Single,
                              height As Single, fill As Boolean, stroke As Boolean)
    If fill Then content.Append("0.92 0.94 0.93 rg" & vbLf)
    If stroke Then content.Append("0.55 0.58 0.56 RG 0.6 w" & vbLf)
    content.Append(x.ToString("0.##", Globalization.CultureInfo.InvariantCulture)).Append(" ")
    content.Append((PageHeight - top - height).ToString("0.##", Globalization.CultureInfo.InvariantCulture)).Append(" ")
    content.Append(width.ToString("0.##", Globalization.CultureInfo.InvariantCulture)).Append(" ")
    content.Append(height.ToString("0.##", Globalization.CultureInfo.InvariantCulture)).Append(" re ")
    content.Append(If(fill AndAlso stroke, "B", If(fill, "f", "S"))).Append(vbLf)
  End Sub

  Private Shared Sub WritePdf(path As String, pageStreams As IReadOnlyList(Of Byte()))
    Dim objects As New List(Of Byte())
    objects.Add(Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"))
    Dim kids As New List(Of Integer)
    For i As Integer = 0 To pageStreams.Count - 1
      kids.Add(5 + i * 2)
    Next
    objects.Add(Encoding.ASCII.GetBytes("<< /Type /Pages /Kids [" & String.Join(" ", kids.Select(Function(id) id & " 0 R")) & "] /Count " & pageStreams.Count & " >>"))
    objects.Add(Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"))
    objects.Add(Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"))
    For i As Integer = 0 To pageStreams.Count - 1
      Dim pageId As Integer = 5 + i * 2
      Dim contentId As Integer = pageId + 1
      objects.Add(Encoding.ASCII.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " & PageWidth & " " & PageHeight & "] " &
                                          "/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents " & contentId & " 0 R >>"))
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
      xref.Append("xref" & vbLf & "0 " & (objects.Count + 1) & vbLf)
      xref.Append("0000000000 65535 f " & vbLf)
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
