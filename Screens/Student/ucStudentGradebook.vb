Imports ClosedXML.Excel
Imports MySqlConnector
Imports System.Text.RegularExpressions

Public Class ucStudentGradebook

    Private _classId As Integer
    Private _enrollmentId As String
    Private _isPredictiveMode As Boolean = False
    Private _realGrades As New List(Of Object())

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(classId As Integer, enrollmentId As String)
        Me.New()
        _classId = classId
        _enrollmentId = enrollmentId
    End Sub

    ' ==========================================
    ' 0. COLUMN MODEL & CONFIGURATION
    ' ==========================================
    Private Enum ColKind
        None
        Raw
        GrA
        CatTotal
    End Enum

    Private Class ColInfo
        Public Category As Integer
        Public Kind As ColKind
        Public MaxScore As Decimal
        Public ItemName As String
        Public PairName As String
    End Class

    Private Const GrABase As Decimal = 50D

    ' Custom Categories & Weights (Synced from database)
    Private customCat1Name As String = "Class Participation"
    Private customCat1Weight As Decimal = 10D
    Private customCat2Name As String = "Quizzes"
    Private customCat2Weight As Decimal = 20D
    Private customCat3Name As String = "Projects"
    Private customCat3Weight As Decimal = 30D
    Private weightExam As Decimal = 40D

    Private Function CategoryName(cat As Integer) As String
        Select Case cat
            Case 1 : Return customCat1Name
            Case 2 : Return customCat2Name
            Case 3 : Return customCat3Name
            Case 4 : Return "Major Exam"
        End Select
        Return ""
    End Function

    Private Function CategoryWeight(cat As Integer) As Decimal
        Select Case cat
            Case 1 : Return customCat1Weight
            Case 2 : Return customCat2Weight
            Case 3 : Return customCat3Weight
            Case 4 : Return weightExam
        End Select
        Return 0D
    End Function

    Private Function CategoryColor(cat As Integer) As Color
        Select Case cat
            Case 1 : Return Color.Yellow
            Case 2 : Return Color.LightGreen
            Case 3 : Return Color.Khaki
            Case 4 : Return Color.LightGreen
        End Select
        Return Color.LightGray
    End Function

    Private Function GetInfo(col As DataGridViewColumn) As ColInfo
        Dim info As ColInfo = TryCast(col.Tag, ColInfo)
        If info Is Nothing Then info = New ColInfo() With {.Category = 0, .Kind = ColKind.None}
        Return info
    End Function

    ' ==========================================
    ' LOAD & GRID INITIALIZATION
    ' ==========================================
    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Dim mainForm = TryCast(FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            mainForm.ShowPage(New ucStudentDashboard())
        End If
    End Sub

    Private Sub ucStudentGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadWeightsFromDatabase()
        BuildStudentGrid()
        LoadStudentGrades()
        RecalculateAll()
        SetGridReadOnly(True)
    End Sub

    Private Sub LoadWeightsFromDatabase()
        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery("SELECT * FROM gradebook_weights LIMIT 1")
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                customCat1Name = row("cat1_name").ToString()
                customCat1Weight = Convert.ToDecimal(row("cat1_val"))
                customCat2Name = row("cat2_name").ToString()
                customCat2Weight = Convert.ToDecimal(row("cat2_val"))
                customCat3Name = row("cat3_name").ToString()
                customCat3Weight = Convert.ToDecimal(row("cat3_val"))
                weightExam = Convert.ToDecimal(row("exam_val"))
            End If
        Catch ex As Exception
            ' Fallbacks to defaults if uninitialized
        End Try
    End Sub

    Private Sub BuildStudentGrid()
        With dgvGradebook
            .Columns.Clear()
            .AllowUserToAddRows = False
            .AllowUserToOrderColumns = False
            .RowHeadersVisible = False
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            .AllowUserToResizeColumns = True
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 60
            .EnableHeadersVisualStyles = False
            .ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False
            .DefaultCellStyle.WrapMode = DataGridViewTriState.False
            .ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray
            .ColumnHeadersDefaultCellStyle.ForeColor = Color.Black
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End With

        ' Fixed left columns matching teacher grid layout
        Dim cNo As New DataGridViewTextBoxColumn() With {.Name = "number", .HeaderText = "NO.", .ReadOnly = True, .Tag = New ColInfo()}
        Dim cName As New DataGridViewTextBoxColumn() With {.Name = "studname", .HeaderText = "STUDENT NAME", .ReadOnly = True, .Tag = New ColInfo()}
        cName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        Dim cDrp As New DataGridViewTextBoxColumn() With {.Name = "status", .HeaderText = "DRP", .ReadOnly = True, .Tag = New ColInfo()}
        dgvGradebook.Columns.AddRange(cNo, cName, cDrp)

        ' Dynamically build assessment columns based on class rubrics and items
        Try
            Dim rubricQuery As String = "SELECT rubric_id, component_name FROM grading_rubric WHERE class_id = @cid ORDER BY rubric_id"
            Dim rubricDt As DataTable = DatabaseHelper.ExecuteQuery(rubricQuery, {New MySqlParameter("@cid", _classId)})

            For cat As Integer = 1 To 4
                Dim targetRubricId As Integer = -1
                If rubricDt IsNot Nothing Then
                    For Each rRow As DataRow In rubricDt.Rows
                        Dim compName As String = rRow("component_name").ToString()
                        If (cat = 1 AndAlso compName.Equals(customCat1Name, StringComparison.OrdinalIgnoreCase)) OrElse
                           (cat = 2 AndAlso (compName.Equals(customCat2Name, StringComparison.OrdinalIgnoreCase) OrElse compName.Equals("Quizzes", StringComparison.OrdinalIgnoreCase))) OrElse
                           (cat = 3 AndAlso (compName.Equals(customCat3Name, StringComparison.OrdinalIgnoreCase) OrElse compName.Equals("Projects", StringComparison.OrdinalIgnoreCase))) OrElse
                           (cat = 4 AndAlso compName.Equals("Final Exam", StringComparison.OrdinalIgnoreCase)) Then
                            targetRubricId = Convert.ToInt32(rRow("rubric_id"))
                            Exit For
                        End If
                    Next
                    If targetRubricId = -1 AndAlso rubricDt.Rows.Count >= cat Then
                        targetRubricId = Convert.ToInt32(rubricDt.Rows(cat - 1)("rubric_id"))
                    End If
                End If

                If targetRubricId <> -1 Then
                    Dim itemQuery As String = "SELECT item_id, item_name, max_score FROM grade_item WHERE rubric_id = @rid"
                    Dim itemDt As DataTable = DatabaseHelper.ExecuteQuery(itemQuery, {New MySqlParameter("@rid", targetRubricId)})

                    If itemDt IsNot Nothing Then
                        For Each iRow As DataRow In itemDt.Rows
                            Dim itemName As String = iRow("item_name").ToString()
                            Dim maxScore As Decimal = Convert.ToDecimal(iRow("max_score"))
                            Dim internalName As String = "col_" & Regex.Replace(itemName, "[^A-Za-z0-9]", "_") & "_" & iRow("item_id")

                            Dim pair = CreateAssessmentColumns(itemName, internalName, cat, maxScore)
                            dgvGradebook.Columns.Add(pair.Raw)
                            dgvGradebook.Columns.Add(pair.GrA)
                        Next
                    End If
                End If

                ' Category Total Column
                Dim total As New DataGridViewTextBoxColumn()
                total.Name = "cat" & cat & "_total"
                total.HeaderText = If(cat = 4, "MAJOR EXAM", "TOTAL")
                total.ReadOnly = True
                total.Tag = New ColInfo() With {.Category = cat, .Kind = ColKind.CatTotal}
                total.DefaultCellStyle.BackColor = Color.FromArgb(226, 239, 218)
                total.DefaultCellStyle.Font = New Font(dgvGradebook.Font, FontStyle.Bold)
                dgvGradebook.Columns.Add(total)
            Next
        Catch ex As Exception
            MessageBox.Show("Error building student grid columns: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        ' Final Grade Column
        Dim cGrade As New DataGridViewTextBoxColumn() With {.Name = "grade", .HeaderText = "GRADE", .ReadOnly = True, .Tag = New ColInfo()}
        cGrade.DefaultCellStyle.Font = New Font(dgvGradebook.Font, FontStyle.Bold)
        dgvGradebook.Columns.Add(cGrade)

        For Each c As DataGridViewColumn In dgvGradebook.Columns
            c.SortMode = DataGridViewColumnSortMode.NotSortable
        Next
    End Sub

    Private Function CreateAssessmentColumns(itemName As String, colName As String, cat As Integer, maxScore As Decimal) As (Raw As DataGridViewTextBoxColumn, GrA As DataGridViewTextBoxColumn)
        Dim raw As New DataGridViewTextBoxColumn()
        raw.Name = colName
        raw.HeaderText = (itemName & " (" & maxScore.ToString("0.##") & ")").ToUpper()
        raw.Width = 80
        raw.ReadOnly = False
        raw.Tag = New ColInfo() With {.Category = cat, .Kind = ColKind.Raw, .MaxScore = maxScore, .ItemName = itemName}

        Dim gra As New DataGridViewTextBoxColumn()
        gra.Name = colName & "_gra"
        gra.HeaderText = "GrA"
        gra.Width = 65
        gra.ReadOnly = True
        gra.Tag = New ColInfo() With {.Category = cat, .Kind = ColKind.GrA, .PairName = colName}
        gra.DefaultCellStyle.BackColor = Color.FromArgb(221, 235, 247)
        gra.DefaultCellStyle.ForeColor = Color.Blue

        Return (raw, gra)
    End Function

    ' ==========================================
    ' 2. TWO-LEVEL HEADER PAINTING
    ' ==========================================
    Private Sub GetCategoryRun(index As Integer, ByRef first As Integer, ByRef last As Integer)
        Dim cat As Integer = GetInfo(dgvGradebook.Columns(index)).Category
        first = index
        While first > 0 AndAlso GetInfo(dgvGradebook.Columns(first - 1)).Category = cat
            first -= 1
        End While
        last = index
        While last < dgvGradebook.Columns.Count - 1 AndAlso GetInfo(dgvGradebook.Columns(last + 1)).Category = cat
            last += 1
        End While
    End Sub

    Private Sub dgvGradebook_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles dgvGradebook.CellPainting
        If e.RowIndex <> -1 OrElse e.ColumnIndex < 0 Then Return
        e.Handled = True
        Dim g As Graphics = e.Graphics
        Dim b As Rectangle = e.CellBounds
        Dim col As DataGridViewColumn = dgvGradebook.Columns(e.ColumnIndex)
        Dim cat As Integer = GetInfo(col).Category
        Dim baseFont As Font = If(e.CellStyle.Font, dgvGradebook.Font)
        Dim txtFlags As TextFormatFlags = TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.WordBreak

        If cat = 0 Then
            Using br As New SolidBrush(Color.LightGray)
                g.FillRectangle(br, b)
            End Using
            g.DrawRectangle(Pens.Black, b.X, b.Y, b.Width - 1, b.Height - 1)
            TextRenderer.DrawText(g, CStr(e.FormattedValue), baseFont, b, Color.Black, txtFlags)
            Return
        End If

        Dim h As Integer = b.Height \ 2
        Dim bandRect As New Rectangle(b.X, b.Y, b.Width, h)
        Dim lowRect As New Rectangle(b.X, b.Y + h, b.Width, b.Height - h)

        Using br As New SolidBrush(Color.LightGray)
            g.FillRectangle(br, lowRect)
        End Using
        g.DrawRectangle(Pens.Black, lowRect.X, lowRect.Y, lowRect.Width - 1, lowRect.Height - 1)
        TextRenderer.DrawText(g, CStr(e.FormattedValue), baseFont, lowRect, Color.Black, txtFlags)

        Dim first, last As Integer
        GetCategoryRun(e.ColumnIndex, first, last)

        Using br As New SolidBrush(CategoryColor(cat))
            g.FillRectangle(br, bandRect)
        End Using

        g.DrawLine(Pens.Black, b.Left, b.Top, b.Right - 1, b.Top)
        g.DrawLine(Pens.Black, b.Left, b.Top + h, b.Right - 1, b.Top + h)
        If e.ColumnIndex = first Then g.DrawLine(Pens.Black, b.Left, b.Top, b.Left, b.Top + h)
        If e.ColumnIndex = last Then g.DrawLine(Pens.Black, b.Right - 1, b.Top, b.Right - 1, b.Top + h)

        Dim spanLeft As Integer = b.Left
        For i As Integer = first To e.ColumnIndex - 1
            spanLeft -= dgvGradebook.Columns(i).Width
        Next
        Dim spanWidth As Integer = 0
        For i As Integer = first To last
            spanWidth += dgvGradebook.Columns(i).Width
        Next
        Dim spanRect As New Rectangle(spanLeft, b.Top, spanWidth, h)
        Dim bandText As String = (CategoryName(cat) & " " & CategoryWeight(cat).ToString("0.##") & "%").ToUpper()

        Dim state As Drawing2D.GraphicsState = g.Save()
        g.IntersectClip(spanRect)
        Using bold As New Font(baseFont, FontStyle.Bold)
            TextRenderer.DrawText(g, bandText, bold, spanRect, Color.Black, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        End Using
        g.Restore(state)
    End Sub

    Private Sub dgvGradebook_ColumnWidthChanged(sender As Object, e As DataGridViewColumnEventArgs) Handles dgvGradebook.ColumnWidthChanged
        dgvGradebook.Invalidate()
    End Sub

    ' ==========================================
    ' 3. LOAD STUDENT GRADES
    ' ==========================================
    Private Sub LoadStudentGrades()
        dgvGradebook.Rows.Clear()
        Try
            Dim sql As String = "SELECT e.enrollment_id, sp.last_name, sp.first_name, sp.middle_name, e.status, gi.item_id, ge.score " &
                                "FROM enrollment e " &
                                "JOIN student_profile sp ON sp.student_id = e.student_id " &
                                "LEFT JOIN grade_entry ge ON ge.enrollment_id = e.enrollment_id " &
                                "LEFT JOIN grade_item gi ON gi.item_id = ge.item_id " &
                                "WHERE e.enrollment_id = @enrId"

            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@enrId", _enrollmentId)})

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim idx As Integer = dgvGradebook.Rows.Add()
                Dim row As DataGridViewRow = dgvGradebook.Rows(idx)
                row.Tag = _enrollmentId
                row.Cells("number").Value = 1

                Dim r0 = dt.Rows(0)
                Dim mi As String = If(IsDBNull(r0("middle_name")), "", r0("middle_name").ToString().Trim())
                row.Cells("studname").Value = $"{r0("last_name")}, {r0("first_name")}" & If(mi = "", "", " " & mi)
                row.Cells("status").Value = If(r0("status").ToString() = "Dropped", "Dropped", "")

                For Each r As DataRow In dt.Rows
                    If Not IsDBNull(r("item_id")) AndAlso Not IsDBNull(r("score")) Then
                        Dim targetItemId As Integer = Convert.ToInt32(r("item_id"))
                        For Each col As DataGridViewColumn In dgvGradebook.Columns
                            Dim info As ColInfo = GetInfo(col)
                            If info.Kind = ColKind.Raw AndAlso col.Tag IsNot Nothing Then
                                If col.Name.EndsWith("_" & targetItemId) Then
                                    row.Cells(col.Index).Value = Convert.ToDecimal(r("score")).ToString("0.00")
                                End If
                            End If
                        Next
                    End If
                Next

                ' Backup row for Predictive Mode restoration
                Dim backupRow(dgvGradebook.Columns.Count - 1) As Object
                For cellIdx As Integer = 0 To row.Cells.Count - 1
                    backupRow(cellIdx) = row.Cells(cellIdx).Value
                Next
                _realGrades.Clear()
                _realGrades.Add(backupRow)
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading student grades: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ==========================================
    ' 5. GRADING VALIDATION & CALCULATION
    ' ==========================================
    Private Function ToGrA(raw As Decimal, maxScore As Decimal) As Decimal
        If maxScore <= 0 Then Return 0D
        Return Math.Round(raw / maxScore * (100D - GrABase) + GrABase, 2)
    End Function

    Private Sub RecalculateAll()
        For Each row As DataGridViewRow In dgvGradebook.Rows
            RecalculateRow(row)
        Next
    End Sub

    Private Sub RecalculateRow(row As DataGridViewRow)
        If row.IsNewRow Then Return
        Dim sums(4) As Decimal
        Dim counts(4) As Integer

        For Each col As DataGridViewColumn In dgvGradebook.Columns
            Dim info As ColInfo = GetInfo(col)
            If info.Kind <> ColKind.Raw Then Continue For

            Dim graCell As DataGridViewCell = row.Cells(col.Name & "_gra")
            Dim raw As Decimal
            Dim v As Object = row.Cells(col.Index).Value
            If v IsNot Nothing AndAlso Decimal.TryParse(v.ToString(), raw) Then
                Dim gra As Decimal = ToGrA(raw, info.MaxScore)
                graCell.Value = gra.ToString("0.00")
                sums(info.Category) += gra
                counts(info.Category) += 1
            Else
                graCell.Value = ""
            End If
        Next

        Dim finalGrade As Decimal = 0D
        For cat As Integer = 1 To 4
            Dim totalCol As String = "cat" & cat & "_total"
            If Not dgvGradebook.Columns.Contains(totalCol) Then Continue For
            If counts(cat) > 0 Then
                Dim avg As Decimal = sums(cat) / counts(cat)
                Dim weighted As Decimal = Math.Round(avg * CategoryWeight(cat) / 100D, 2)
                row.Cells(totalCol).Value = weighted.ToString("0.00")
                finalGrade += weighted
            Else
                row.Cells(totalCol).Value = ""
            End If
        Next

        Dim status As String = If(row.Cells("status").Value IsNot Nothing, row.Cells("status").Value.ToString(), "")
        row.Cells("grade").Value = If(status.Equals("Dropped", StringComparison.OrdinalIgnoreCase), "DRP", finalGrade.ToString("0.00"))
    End Sub

    Private Sub dgvGradebook_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGradebook.CellEndEdit
        If e.RowIndex < 0 Then Return

        ' Guard: Block edits if Predictive Mode is turned off
        If Not _isPredictiveMode Then Return

        Dim info As ColInfo = GetInfo(dgvGradebook.Columns(e.ColumnIndex))
        If info.Kind <> ColKind.Raw Then Return

        Dim editedRow As DataGridViewRow = dgvGradebook.Rows(e.RowIndex)
        Dim editedCell As DataGridViewCell = editedRow.Cells(e.ColumnIndex)
        Dim inputString As String = If(editedCell.Value IsNot Nothing, editedCell.Value.ToString(), "")

        If String.IsNullOrWhiteSpace(inputString) Then
            editedCell.Value = Nothing
            RecalculateRow(editedRow)
            Return
        End If

        Dim numericValue As Decimal
        If Decimal.TryParse(inputString, numericValue) Then
            If numericValue < 0 OrElse numericValue > info.MaxScore Then
                MessageBox.Show("Please enter a valid score between 0 and " & info.MaxScore.ToString("0.##") & ".", "Invalid Score", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                editedCell.Value = "0.00"
            Else
                editedCell.Value = numericValue.ToString("0.00")
            End If
        Else
            MessageBox.Show("Please enter a numeric value.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            editedCell.Value = "0.00"
        End If

        RecalculateRow(editedRow)
    End Sub

    Private Sub SetGridReadOnly(isReadOnly As Boolean)
        dgvGradebook.ReadOnly = isReadOnly
        dgvGradebook.BackgroundColor = If(isReadOnly, Color.White, Color.FromArgb(255, 252, 230))
    End Sub

    ' ==========================================
    ' PREDICTIVE MODE TOGGLE
    ' ==========================================
    Private Sub btnPredictive_Click(sender As Object, e As EventArgs) Handles btnPredictive.Click
        _isPredictiveMode = Not _isPredictiveMode

        If _isPredictiveMode Then
            btnPredictive.Text = "Exit Predictive Mode"
            btnPredictive.BackColor = Color.FromArgb(220, 53, 69)
            btnPredictive.TextAlign = ContentAlignment.MiddleCenter
            SetGridReadOnly(False)
        Else
            btnPredictive.Text = "Predictive Mode"
            btnPredictive.BackColor = Color.FromArgb(241, 196, 15)
            SetGridReadOnly(True)

            ' Restore real grades from backup
            If _realGrades.Count > 0 AndAlso dgvGradebook.Rows.Count > 0 Then
                Dim row = dgvGradebook.Rows(0)
                Dim backup = _realGrades(0)
                For idx As Integer = 0 To backup.Length - 1
                    If idx < row.Cells.Count Then
                        row.Cells(idx).Value = backup(idx)
                    End If
                Next
                RecalculateRow(row)
            End If
        End If
    End Sub

End Class