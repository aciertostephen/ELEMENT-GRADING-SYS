Imports ClosedXML.Excel
Imports MySqlConnector
Imports System.Text.RegularExpressions

Public Class ucGradebook

    Private _classId As Integer

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(classId As Integer)
        Me.New()
        _classId = classId
    End Sub

    ' ==========================================
    ' 0. COLUMN MODEL
    ' ==========================================
    Private Enum ColKind
        None        ' No, Student Name, DRP, Grade
        Raw         ' editable raw score
        GrA         ' computed grade (read-only)
        CatTotal    ' computed weighted category total (read-only)
    End Enum

    ' Stored in DataGridViewColumn.Tag. Drives the header band, validation and calculations.
    Private Class ColInfo
        Public Category As Integer      ' 0 = none, 1..4 = category (4 = Major Exam)
        Public Kind As ColKind
        Public MaxScore As Decimal      ' Raw columns only
        Public ItemName As String       ' grade_item.item_name (Raw columns only)
        Public PairName As String       ' For a GrA column: name of its Raw column
    End Class

    ' GrA = raw / max * (100 - GrABase) + GrABase   (e.g. 30/35 -> 92.86 with base 50)
    Private Const GrABase As Decimal = 50D

    ' Default assessments: item name (grade_item.item_name), column name, category, max score
    Private ReadOnly defaultItems As New List(Of (ItemName As String, ColName As String, Category As Integer, MaxScore As Decimal)) From {
        ("Participation 1", "part", 1, 100D),
        ("Q1", "quiz1", 2, 30D),
        ("Q2", "quiz2", 2, 35D),
        ("Project 1", "project", 3, 100D),
        ("Final Exam", "exam", 4, 50D)
    }

    ' grade_item.item_name -> DataGridView column Name (built from defaultItems)
    Private ReadOnly itemToColumn As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    ' Custom Categories & Weights (Exam is permanently locked at 40%)
    Private customCat1Name As String = "Class Participation"
    Private customCat1Weight As Decimal = 10

    Private customCat2Name As String = "Quizzes"
    Private customCat2Weight As Decimal = 20

    Private customCat3Name As String = "Projects"
    Private customCat3Weight As Decimal = 30

    Private weightExam As Decimal = 40

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
    ' LOAD
    ' ==========================================
    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Dim mainForm As MainForm = TryCast(Me.FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            mainForm.ShowPage(New ucTeacherDashboard())
        End If
    End Sub

    Private Sub ucGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadWeightsFromDatabase()
        BuildGrid()
        LoadStudents()
        RecalculateAll()
    End Sub

    ' --- Place LoadWeightsFromDatabase here ---
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

    ' --- Place SaveActiveWeightsToDatabase here ---
    Private Sub SaveActiveWeightsToDatabase(cat1Name As String, cat1Val As Decimal, cat2Name As String, cat2Val As Decimal, cat3Name As String, cat3Val As Decimal, examVal As Decimal)
        Try
            Dim query As String = "INSERT INTO gradebook_weights (id, cat1_name, cat1_val, cat2_name, cat2_val, cat3_name, cat3_val, exam_val) VALUES (1, @c1n, @c1v, @c2n, @c2v, @c3n, @c3v, @ev) " &
                                  "ON DUPLICATE KEY UPDATE cat1_name=@c1n, cat1_val=@c1v, cat2_name=@c2n, cat2_val=@c2v, cat3_name=@c3n, cat3_val=@c3v, exam_val=@ev"

            Dim params As MySqlParameter() = {
                New MySqlParameter("@c1n", cat1Name),
                New MySqlParameter("@c1v", cat1Val),
                New MySqlParameter("@c2n", cat2Name),
                New MySqlParameter("@c2v", cat2Val),
                New MySqlParameter("@c3n", cat3Name),
                New MySqlParameter("@c3v", cat3Val),
                New MySqlParameter("@ev", examVal)
            }

            DatabaseHelper.ExecuteNonQuery(query, params)
        Catch ex As Exception
            MessageBox.Show("Error saving weights to database: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ==========================================
    ' 1. GRID LAYOUT (replaces designer columns)
    ' ==========================================
    Private Sub BuildGrid()
        itemToColumn.Clear()
        For Each it In defaultItems
            itemToColumn(it.ItemName) = it.ColName
        Next

        With dgvGradebook
            .Columns.Clear()
            .AllowUserToAddRows = False
            .AllowUserToOrderColumns = False
            .RowHeadersVisible = False

            ' Set auto-sizing mode to AllCells so columns expand to fit all text on one line
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            .AllowUserToResizeColumns = True

            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 60
            .EnableHeadersVisualStyles = False

            ' Disable text wrapping globally for headers and cells to enforce single-line layout
            .ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False
            .DefaultCellStyle.WrapMode = DataGridViewTriState.False

            .ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray
            .ColumnHeadersDefaultCellStyle.ForeColor = Color.Black
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End With

        ' Fixed left columns
        Dim cNo As New DataGridViewTextBoxColumn() With {.Name = "number", .HeaderText = "NO.", .ReadOnly = True, .Tag = New ColInfo()}
        Dim cName As New DataGridViewTextBoxColumn() With {.Name = "studname", .HeaderText = "STUDENT NAME", .ReadOnly = True, .Tag = New ColInfo()}
        cName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        Dim cDrp As New DataGridViewTextBoxColumn() With {.Name = "status", .HeaderText = "DRP", .ReadOnly = True, .Tag = New ColInfo()}

        dgvGradebook.Columns.AddRange(cNo, cName, cDrp)

        ' Categories 1..4, each: [Raw, GrA]... then a weighted total
        For cat As Integer = 1 To 4
            For Each it In defaultItems.Where(Function(x) x.Category = cat)
                Dim pair = CreateAssessmentColumns(it.ItemName, it.ColName, cat, it.MaxScore)
                dgvGradebook.Columns.Add(pair.Raw)
                dgvGradebook.Columns.Add(pair.GrA)
            Next

            Dim total As New DataGridViewTextBoxColumn()
            total.Name = "cat" & cat & "_total"
            total.HeaderText = If(cat = 4, "MAJOR EXAM", "TOTAL")
            total.ReadOnly = True
            total.Tag = New ColInfo() With {.Category = cat, .Kind = ColKind.CatTotal}
            total.DefaultCellStyle.BackColor = Color.FromArgb(226, 239, 218)
            total.DefaultCellStyle.Font = New Font(dgvGradebook.Font, FontStyle.Bold)
            dgvGradebook.Columns.Add(total)
        Next

        ' Final grade
        Dim cGrade As New DataGridViewTextBoxColumn() With {.Name = "grade", .HeaderText = "GRADE", .ReadOnly = True, .Tag = New ColInfo()}
        cGrade.DefaultCellStyle.Font = New Font(dgvGradebook.Font, FontStyle.Bold)
        dgvGradebook.Columns.Add(cGrade)

        For Each c As DataGridViewColumn In dgvGradebook.Columns
            c.SortMode = DataGridViewColumnSortMode.NotSortable
        Next
    End Sub

    ' Creates the Raw column and its GrA column (not yet added to the grid)
    Private Function CreateAssessmentColumns(itemName As String, colName As String,
                                             cat As Integer, maxScore As Decimal) As (Raw As DataGridViewTextBoxColumn, GrA As DataGridViewTextBoxColumn)
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
    ' Finds the contiguous run of columns that share the same category as column 'index'
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

        ' Columns without a category: one tall cell
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

        ' Bottom half: normal header text
        Using br As New SolidBrush(Color.LightGray)
            g.FillRectangle(br, lowRect)
        End Using
        g.DrawRectangle(Pens.Black, lowRect.X, lowRect.Y, lowRect.Width - 1, lowRect.Height - 1)
        TextRenderer.DrawText(g, CStr(e.FormattedValue), baseFont, lowRect, Color.Black, txtFlags)

        ' Top half: merged band across the category's run of columns
        Dim first, last As Integer
        GetCategoryRun(e.ColumnIndex, first, last)

        Using br As New SolidBrush(CategoryColor(cat))
            g.FillRectangle(br, bandRect)
        End Using
        g.DrawLine(Pens.Black, b.Left, b.Top, b.Right - 1, b.Top)                 ' top
        g.DrawLine(Pens.Black, b.Left, b.Top + h, b.Right - 1, b.Top + h)         ' bottom of band
        If e.ColumnIndex = first Then g.DrawLine(Pens.Black, b.Left, b.Top, b.Left, b.Top + h)
        If e.ColumnIndex = last Then g.DrawLine(Pens.Black, b.Right - 1, b.Top, b.Right - 1, b.Top + h)

        ' Band text: centered across the whole span, clipped to this cell (so it also works while scrolling)
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
        g.IntersectClip(bandRect)
        Using bold As New Font(baseFont, FontStyle.Bold)
            TextRenderer.DrawText(g, bandText, bold, spanRect, Color.Black,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
        End Using
        g.Restore(state)
    End Sub

    Private Sub dgvGradebook_ColumnWidthChanged(sender As Object, e As DataGridViewColumnEventArgs) Handles dgvGradebook.ColumnWidthChanged
        dgvGradebook.Invalidate()
    End Sub

    ' ==========================================
    ' 3. LOAD STUDENTS
    ' ==========================================
    Private Sub LoadStudents()
        dgvGradebook.Rows.Clear()

        Dim sql As String =
        "SELECT e.enrollment_id, sp.last_name, sp.first_name, sp.middle_name, e.status, " &
        "       gi.item_name, ge.score " &
        "FROM enrollment e " &
        "JOIN student_profile sp ON sp.student_id = e.student_id " &
        "LEFT JOIN grade_entry ge ON ge.enrollment_id = e.enrollment_id " &
        "LEFT JOIN grade_item gi ON gi.item_id = ge.item_id " &
        "WHERE e.class_id = @cid " &
        "ORDER BY sp.last_name, sp.first_name"

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@cid", _classId)})

        Dim rowsByEnrollment As New Dictionary(Of String, DataGridViewRow)
        Dim n As Integer = 0

        For Each r As DataRow In dt.Rows
            Dim enrId As String = r("enrollment_id").ToString()

            If Not rowsByEnrollment.ContainsKey(enrId) Then
                n += 1
                Dim idx As Integer = dgvGradebook.Rows.Add()
                Dim row As DataGridViewRow = dgvGradebook.Rows(idx)
                row.Tag = enrId                       ' needed later for saving
                row.Cells("number").Value = n

                Dim mi As String = r("middle_name").ToString().Trim()
                row.Cells("studname").Value =
                $"{r("last_name")}, {r("first_name")}" & If(mi = "", "", " " & mi)

                row.Cells("status").Value = If(r("status").ToString() = "Dropped", "Dropped", "")
                rowsByEnrollment(enrId) = row
            End If

            If Not IsDBNull(r("item_name")) AndAlso Not IsDBNull(r("score")) Then
                Dim colName As String = Nothing
                If itemToColumn.TryGetValue(r("item_name").ToString(), colName) _
               AndAlso dgvGradebook.Columns.Contains(colName) Then
                    rowsByEnrollment(enrId).Cells(colName).Value =
                    Convert.ToDecimal(r("score")).ToString("0.00")
                End If
            End If
        Next
    End Sub

    ' ==========================================================================
    ' REPLACES section "4. EXPORT METHOD" in ucGradebook.vb
    ' (delete the old btnExport_Click CSV code and paste everything below in its place).
    ' No new Imports are needed - ClosedXML.Excel is already imported.
    ' ==========================================================================

    ' ==========================================
    ' 4. EXPORT TO THE EXCEL TEMPLATE
    ' ==========================================

    ' ---- Template settings (adjust if you move the file) ----
    Private Const TemplateFileName As String = "GradingTemplate.xlsm"   ' put it in <app folder>\Templates\
    Private Const RosterSheetName As String = "Midterm"                ' Final/Semestral read names + header from here
    Private Const FirstStudentRow As Integer = 15
    Private Const LastStudentRow As Integer = 59                       ' template formulas/print area stop at row 59

    ' ---- INPUT columns of the template (everything else is a formula and is never touched) ----
    Private Const ColName As String = "B"      ' Student name  (Lastname, Firstname)
    Private Const ColDrp As String = "C"       ' "Drp" for dropped
    Private Const ColAbsences As String = "D"  ' Absences (Part / Absences)
    Private Const ColExtra As String = "F"     ' Extra in (%)
    Private Const ColCW As String = "G"        ' CW / SW in (%)
    Private Const ColQuiz1Raw As String = "H"  ' Quiz 1 raw score   (max score in I12)
    Private Const ColQuiz2Raw As String = "J"  ' Quiz 2 raw score   (max score in K12)
    Private Const ColProject As String = "M"   ' Project in (%)
    Private Const ColExamRaw As String = "P"   ' Exam raw score     (max score in Q12)

    Private Class ExportHeaderInfo
        Public SchoolYear As String = ""   ' -> C1
        Public Semester As String = ""     ' -> F1
        Public Section As String = ""      ' -> I1
        Public Program As String = ""      ' -> C2
        Public Professor As String = ""    ' -> F3
    End Class

    ' Fill this in from wherever your system stores the class details.
    ' Blank values are NOT written, so the template's existing text is kept.
    Private Function GetExportHeaderInfo() As ExportHeaderInfo
        Dim info As New ExportHeaderInfo()

        ' Example only - change table/column names to your schema, then uncomment:
        ' Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
        '     "SELECT school_year, semester, section, program, professor_name FROM class WHERE class_id = @cid",
        '     {New MySqlParameter("@cid", _classId)})
        ' If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
        '     info.SchoolYear = dt.Rows(0)("school_year").ToString()
        '     info.Semester = dt.Rows(0)("semester").ToString()
        '     info.Section = dt.Rows(0)("section").ToString()
        '     info.Program = dt.Rows(0)("program").ToString()
        '     info.Professor = dt.Rows(0)("professor_name").ToString()
        ' End If

        Return info
    End Function

    ' The gradebook has no absence data. Return the student's number of absences here
    ' (e.g. from an attendance table). Nothing = leave the cell blank (counts as 0 absences).
    Private Function GetAbsences(gridRow As DataGridViewRow) As Decimal?
        Return Nothing
    End Function

    Private Function AskGradingPeriod() As String
        Do
            Dim input As String = Microsoft.VisualBasic.InputBox(
            "Select the grading period to export:" & vbCrLf &
            "1. Midterm" & vbCrLf &
            "2. Final" & vbCrLf &
            "3. Semestral",
            "Grading Period", "Midterm")

            If String.IsNullOrWhiteSpace(input) Then Return Nothing

            Select Case input.Trim().TrimEnd("."c).ToLower()
                Case "1", "midterm" : Return "Midterm"
                Case "2", "final" : Return "Final"
                Case "3", "semestral" : Return "Semestral"
            End Select

            MessageBox.Show("Please enter 1, 2, 3 or the name of the grading period.", "Invalid Grading Period", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Loop
    End Function

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExportExcel.Click
        Dim period As String = AskGradingPeriod()
        If period Is Nothing Then Return

        Using sfd As New SaveFileDialog()
            sfd.Filter = "Excel Workbook|*.xlsx"
            sfd.Title = "Export Class Grades"
            sfd.FileName = "Class_Grades_" & period & "_" & DateTime.Now.ToString("yyyyMMdd") & ".xlsx"

            If sfd.ShowDialog() = DialogResult.OK Then
                ExportToTemplate(sfd.FileName, period, GetExportHeaderInfo())
            End If
        End Using
    End Sub

    Private Sub ExportToTemplate(savePath As String, period As String, header As ExportHeaderInfo)
        ' --- Semestral has no input cells: every cell is a formula over Midterm and Final ---
        If period = "Semestral" Then
            MessageBox.Show("The Semestral sheet is fully calculated from the Midterm and Final sheets." & vbCrLf &
                        "Export the Midterm and Final periods instead.",
                        "Semestral Not Exportable", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' --- Template must exist ---
        Dim templatePath As String = System.IO.Path.Combine(Application.StartupPath, "Templates", TemplateFileName)
        If Not System.IO.File.Exists(templatePath) Then
            MessageBox.Show("The Excel template was not found:" & vbCrLf & templatePath,
                        "Template Missing", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ' --- Students currently shown in the gradebook ---
        Dim students As List(Of DataGridViewRow) =
        dgvGradebook.Rows.Cast(Of DataGridViewRow)().Where(Function(r) Not r.IsNewRow).ToList()

        Dim capacity As Integer = LastStudentRow - FirstStudentRow + 1
        If students.Count > capacity Then
            MessageBox.Show($"The template holds at most {capacity} students (rows {FirstStudentRow}-{LastStudentRow}), " &
                        $"but this class has {students.Count}.",
                        "Too Many Students", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' --- Which gradebook columns feed which template columns ---
        Dim quizCols As List(Of DataGridViewColumn) = RawColumnsOf(2)
        Dim projectCols As List(Of DataGridViewColumn) = RawColumnsOf(3)
        Dim examCols As List(Of DataGridViewColumn) = RawColumnsOf(4)

        Dim notes As New List(Of String)
        If RawColumnsOf(1).Count > 0 Then notes.Add(customCat1Name & " scores (the template uses attendance/absences instead)")
        If quizCols.Count > 2 Then notes.Add("Quiz columns after the first two (template has only Q1 and Q2)")
        If examCols.Count > 1 Then notes.Add("Exam columns after the first one")

        Try
            Using wb As New XLWorkbook(templatePath)

                ' --- Worksheet lookups ---
                Dim wsTarget As IXLWorksheet = Nothing
                If Not wb.Worksheets.TryGetWorksheet(period, wsTarget) Then
                    MessageBox.Show($"The template has no worksheet named '{period}'.", "Worksheet Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If

                Dim wsRoster As IXLWorksheet = Nothing
                If Not wb.Worksheets.TryGetWorksheet(RosterSheetName, wsRoster) Then
                    MessageBox.Show($"The template has no worksheet named '{RosterSheetName}'.", "Worksheet Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If

                ' --- 1. Header information ---
                ' C1, F1, I1, C2, F3 live on the roster sheet (Final links to them by formula).
                SetTextOrNumber(wsRoster, "C1", header.SchoolYear)
                SetTextOrNumber(wsRoster, "F1", header.Semester)
                SetTextOrNumber(wsRoster, "I1", header.Section)      ' I1:L1 is merged - I1 is the anchor
                SetTextOrNumber(wsRoster, "C2", header.Program)
                SetTextOrNumber(wsRoster, "F3", header.Professor)
                SetTextOrNumber(wsTarget, "C3", period)              ' grading period, on the selected sheet

                ' --- 2. Maximum scores the template formulas divide by (row 12) ---
                If quizCols.Count >= 1 Then SetNumber(wsTarget, "I12", GetInfo(quizCols(0)).MaxScore)
                If quizCols.Count >= 2 Then SetNumber(wsTarget, "K12", GetInfo(quizCols(1)).MaxScore)
                If examCols.Count >= 1 Then SetNumber(wsTarget, "Q12", GetInfo(examCols(0)).MaxScore)

                ' --- 3. Student rows (15..59). Unused rows are cleared of template sample data ---
                For i As Integer = 0 To capacity - 1
                    Dim r As Integer = FirstStudentRow + i
                    Dim hasStudent As Boolean = (i < students.Count)
                    Dim g As DataGridViewRow = If(hasStudent, students(i), Nothing)

                    ' B  Student name (Lastname, Firstname) - written to the roster sheet
                    SetText(wsRoster, ColName & r, If(hasStudent, GridText(g, "studname"), ""))

                    ' C  Drp only when dropped, otherwise blank
                    Dim isDropped As Boolean = hasStudent AndAlso GridText(g, "status").Equals("Dropped", StringComparison.OrdinalIgnoreCase)
                    SetText(wsTarget, ColDrp & r, If(isDropped, "Drp", ""))

                    ' D  Absences / F  Extra / G  CW  (no source in the gradebook for F and G: left blank)
                    SetNumber(wsTarget, ColAbsences & r, If(hasStudent, GetAbsences(g), Nothing))
                    SetNumber(wsTarget, ColExtra & r, Nothing)
                    SetNumber(wsTarget, ColCW & r, Nothing)

                    ' H, J  Quiz 1 and Quiz 2 raw scores
                    SetNumber(wsTarget, ColQuiz1Raw & r, If(hasStudent AndAlso quizCols.Count >= 1, GridNumber(g, quizCols(0)), Nothing))
                    SetNumber(wsTarget, ColQuiz2Raw & r, If(hasStudent AndAlso quizCols.Count >= 2, GridNumber(g, quizCols(1)), Nothing))

                    ' M  Project in (%): average of raw/max across the graded project columns
                    Dim projectPct As Decimal? = Nothing
                    If hasStudent Then
                        Dim pcts As New List(Of Decimal)
                        For Each pc As DataGridViewColumn In projectCols
                            Dim v As Decimal? = GridNumber(g, pc)
                            Dim mx As Decimal = GetInfo(pc).MaxScore
                            If v.HasValue AndAlso mx > 0 Then pcts.Add(v.Value / mx * 100D)
                        Next
                        If pcts.Count > 0 Then projectPct = Math.Round(pcts.Average(), 2)
                    End If
                    SetNumber(wsTarget, ColProject & r, projectPct)

                    ' P  Exam raw score
                    SetNumber(wsTarget, ColExamRaw & r, If(hasStudent AndAlso examCols.Count >= 1, GridNumber(g, examCols(0)), Nothing))
                Next

                ' Make Excel recalculate every template formula when the file is opened
                wb.FullCalculationOnLoad = True

                wb.SaveAs(savePath)
            End Using

            Dim msg As String = "Class grades have been exported successfully!"
            If notes.Count > 0 Then
                msg &= vbCrLf & vbCrLf & "Not exported (no matching template column):" & vbCrLf & "• " & String.Join(vbCrLf & "• ", notes)
            End If
            MessageBox.Show(msg, "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As System.IO.IOException
            MessageBox.Show("The file could not be saved. If it is open in Excel, close it and try again." & vbCrLf & vbCrLf & ex.Message,
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            MessageBox.Show("An error occurred while exporting: " & ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ---------- Export helpers ----------

    ' Raw (editable) gradebook columns of a category, in on-screen order
    Private Function RawColumnsOf(cat As Integer) As List(Of DataGridViewColumn)
        Return dgvGradebook.Columns.Cast(Of DataGridViewColumn)().
        Where(Function(c) GetInfo(c).Kind = ColKind.Raw AndAlso GetInfo(c).Category = cat).
        OrderBy(Function(c) c.DisplayIndex).ToList()
    End Function

    Private Function GridText(row As DataGridViewRow, colName As String) As String
        Dim v As Object = row.Cells(colName).Value
        Return If(v Is Nothing, "", v.ToString().Trim())
    End Function

    Private Function GridNumber(row As DataGridViewRow, col As DataGridViewColumn) As Decimal?
        Dim v As Object = row.Cells(col.Index).Value
        Dim d As Decimal
        If v IsNot Nothing AndAlso Decimal.TryParse(v.ToString(), d) Then Return d
        Return Nothing
    End Function

    ' Every setter skips formula cells (so template formulas are never overwritten)
    ' and clears the cell when there is no value (so template sample data never leaks through).
    Private Sub SetText(ws As IXLWorksheet, address As String, value As String)
        Dim cell As IXLCell = ws.Cell(address)
        If cell.HasFormula Then Return
        If String.IsNullOrEmpty(value) Then
            cell.Clear(XLClearOptions.Contents)
        Else
            cell.Value = value
        End If
    End Sub

    Private Sub SetNumber(ws As IXLWorksheet, address As String, value As Decimal?)
        Dim cell As IXLCell = ws.Cell(address)
        If cell.HasFormula Then Return
        If value.HasValue Then
            cell.Value = CDbl(value.Value)
        Else
            cell.Clear(XLClearOptions.Contents)
        End If
    End Sub

    ' Header cells: blank input is ignored (template text stays); numeric text becomes a number
    Private Sub SetTextOrNumber(ws As IXLWorksheet, address As String, value As String)
        If String.IsNullOrWhiteSpace(value) Then Return
        Dim cell As IXLCell = ws.Cell(address)
        If cell.HasFormula Then Return

        Dim n As Double
        If Double.TryParse(value.Trim(), n) Then
            cell.Value = n
        Else
            cell.Value = value.Trim()
        End If
    End Sub

    ' ==========================================
    ' 5. GRADING INPUT, VALIDATION & CALCULATION
    ' ==========================================

    ' Click-to-Toggle for DRP Column with Confirmation Prompt
    Private Sub dgvGradebook_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGradebook.CellClick
        If e.RowIndex >= 0 AndAlso dgvGradebook.Columns(e.ColumnIndex).Name = "status" Then
            Dim cell As DataGridViewCell = dgvGradebook.Rows(e.RowIndex).Cells(e.ColumnIndex)
            Dim currentVal As String = If(cell.Value IsNot Nothing, cell.Value.ToString(), "")

            If String.IsNullOrEmpty(currentVal) Then
                Dim result As DialogResult = MessageBox.Show(
                    "Are you sure you want to mark this student as Dropped?",
                    "Confirm Student Drop",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                )

                If result = DialogResult.Yes Then
                    cell.Value = "Dropped"
                End If
            Else
                cell.Value = ""
            End If

            RecalculateRow(dgvGradebook.Rows(e.RowIndex))
            dgvGradebook.InvalidateRow(e.RowIndex)
        End If
    End Sub

    Private Sub dgvGradebook_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGradebook.CellEndEdit
        If e.RowIndex < 0 Then Return

        Dim info As ColInfo = GetInfo(dgvGradebook.Columns(e.ColumnIndex))
        If info.Kind <> ColKind.Raw Then Return   ' only raw-score columns are editable

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
                MessageBox.Show("Please enter a valid score between 0 and " & info.MaxScore.ToString("0.##") & ".",
                                "Invalid Score", MessageBoxButtons.OK, MessageBoxIcon.Warning)
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

    Private Function ToGrA(raw As Decimal, maxScore As Decimal) As Decimal
        If maxScore <= 0 Then Return 0D
        Return Math.Round(raw / maxScore * (100D - GrABase) + GrABase, 2)
    End Function

    Private Sub RecalculateAll()
        For Each row As DataGridViewRow In dgvGradebook.Rows
            RecalculateRow(row)
        Next
    End Sub

    ' Per row:  GrA per assessment -> category average -> x weight -> category total -> final grade
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
                graCell.Value = ""    ' blank score = not graded yet (ignored in the average)
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
        row.Cells("grade").Value = If(status.Equals("Dropped", StringComparison.OrdinalIgnoreCase),
                                      "DRP", finalGrade.ToString("0.00"))
    End Sub

    ' Highlight the DRP cell in red when marked as "Dropped"
    Private Sub dgvGradebook_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvGradebook.CellFormatting
        If e.RowIndex >= 0 AndAlso dgvGradebook.Columns(e.ColumnIndex).Name = "status" Then
            If e.Value IsNot Nothing AndAlso e.Value.ToString().Equals("Dropped", StringComparison.OrdinalIgnoreCase) Then
                e.CellStyle.BackColor = Color.LightCoral
                e.CellStyle.ForeColor = Color.DarkRed
                e.CellStyle.SelectionBackColor = Color.IndianRed
                e.CellStyle.SelectionForeColor = Color.White
            Else
                e.CellStyle.BackColor = Color.White
                e.CellStyle.ForeColor = Color.Black
            End If
        End If
    End Sub

    ' ==========================================
    ' 6. CUSTOM CATEGORY & WEIGHT CONFIGURATION WORKFLOW
    ' ==========================================
    Private Sub btnConfigureWeights_Click(sender As Object, e As EventArgs) Handles btnConfigureWeights.Click
        Try
            ' 1. Custom Category 1 (Name & Weight)
            Dim name1 As String = Microsoft.VisualBasic.InputBox(
                "Enter the custom name for Category 1:",
                "Configure Custom Categories",
                customCat1Name)
            If String.IsNullOrWhiteSpace(name1) Then Return

            Dim weight1Input As String = Microsoft.VisualBasic.InputBox(
                "Enter the weight percentage for " & name1.Trim() & ":",
                "Configure Category Weight",
                customCat1Weight.ToString())
            If String.IsNullOrEmpty(weight1Input) Then Return

            Dim w1 As Decimal
            If Not Decimal.TryParse(weight1Input.Trim(), w1) OrElse w1 < 0 Then
                MessageBox.Show("Please enter a valid non-negative number for " & name1 & ".", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 2. Custom Category 2 (Name & Weight)
            Dim name2 As String = Microsoft.VisualBasic.InputBox(
                "Enter the custom name for Category 2:",
                "Configure Custom Categories",
                customCat2Name)
            If String.IsNullOrWhiteSpace(name2) Then Return

            Dim weight2Input As String = Microsoft.VisualBasic.InputBox(
                "Enter the weight percentage for " & name2.Trim() & ":",
                "Configure Category Weight",
                customCat2Weight.ToString())
            If String.IsNullOrEmpty(weight2Input) Then Return

            Dim w2 As Decimal
            If Not Decimal.TryParse(weight2Input.Trim(), w2) OrElse w2 < 0 Then
                MessageBox.Show("Please enter a valid non-negative number for " & name2 & ".", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 3. Custom Category 3 (Name & Weight)
            Dim name3 As String = Microsoft.VisualBasic.InputBox(
                "Enter the custom name for Category 3:",
                "Configure Custom Categories",
                customCat3Name)
            If String.IsNullOrWhiteSpace(name3) Then Return

            Dim weight3Input As String = Microsoft.VisualBasic.InputBox(
                "Enter the weight percentage for " & name3.Trim() & ":",
                "Configure Category Weight",
                customCat3Weight.ToString())
            If String.IsNullOrEmpty(weight3Input) Then Return

            Dim w3 As Decimal
            If Not Decimal.TryParse(weight3Input.Trim(), w3) OrElse w3 < 0 Then
                MessageBox.Show("Please enter a valid non-negative number for " & name3 & ".", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 4. Major Exam is permanently locked at 40%
            Dim examVal As Decimal = 40D

            ' 5. Strict 100% Total Validation Check
            Dim totalSum As Decimal = w1 + w2 + w3 + examVal

            If totalSum <> 100D Then
                MessageBox.Show($"The total distribution must equal exactly 100%. Your current total is {totalSum}%." & vbCrLf & vbCrLf &
                               "Note: Major Exam is fixed at 40%, meaning your custom categories must combine to equal 60%.",
                               "Weight Distribution Mismatch",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning)
                Return
            End If

            ' 6. Commit Custom State
            customCat1Name = name1.Trim()
            customCat1Weight = w1
            customCat2Name = name2.Trim()
            customCat2Weight = w2
            customCat3Name = name3.Trim()
            customCat3Weight = w3
            weightExam = examVal

            ' Inside your Save/Configure Weights button click event:
            SaveActiveWeightsToDatabase(customCat1Name, customCat1Weight, customCat2Name, customCat2Weight, customCat3Name, customCat3Weight, weightExam)

            ' Refresh the header bands, recompute every student's totals with the new weights, and update the ComboBox
            dgvGradebook.Invalidate()
            RecalculateAll()
            UpdateWeightDistributionComboBox()

            ' 7. Reflect the set weights and custom categories directly in the success message box
            MessageBox.Show("Gradebook Configuration Saved Successfully!" & vbCrLf & vbCrLf &
                            $"• {customCat1Name}: {customCat1Weight}%" & vbCrLf &
                            $"• {customCat2Name}: {customCat2Weight}%" & vbCrLf &
                            $"• {customCat3Name}: {customCat3Weight}%" & vbCrLf &
                            $"• Major Exam: {weightExam}% (Locked)" & vbCrLf &
                            $"Total Gradebook Weight: {totalSum}%",
                            "Custom Gradebook Configured",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("An unexpected error occurred while configuring weights: " & ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Helper method to update the weights ComboBox control
    Private Sub UpdateWeightDistributionComboBox()
        If Weights IsNot Nothing Then
            Weights.Items.Clear()
            Weights.Items.Add($"{customCat1Name}: {customCat1Weight}%")
            Weights.Items.Add($"{customCat2Name}: {customCat2Weight}%")
            Weights.Items.Add($"{customCat3Name}: {customCat3Weight}%")
            Weights.Items.Add($"Major Exam: {weightExam}% (Locked)")

            If Weights.Items.Count > 0 Then
                Weights.SelectedIndex = 0
            End If
        End If
    End Sub



    ' ==========================================
    ' 7. CATEGORY-AWARE DYNAMIC COLUMN ADDITION
    ' ==========================================
    Private Function ParseCategory(input As String) As Integer
        Dim s As String = input.Trim().TrimEnd("."c).ToLower()
        Select Case s
            Case "1", customCat1Name.ToLower() : Return 1
            Case "2", customCat2Name.ToLower() : Return 2
            Case "3", customCat3Name.ToLower() : Return 3
            Case "4", "final exam", "exam", "major exam" : Return 4
        End Select
        Return 0
    End Function

    Private Sub btnAddColumn_Click(sender As Object, e As EventArgs) Handles btnAddColumn.Click
        Dim categoryChoice As String = Microsoft.VisualBasic.InputBox(
            "Select the category for this column:" & vbCrLf &
            "1. " & customCat1Name & vbCrLf &
            "2. " & customCat2Name & vbCrLf &
            "3. " & customCat3Name & vbCrLf &
            "4. Final Exam",
            "Select Category",
            "2")

        If String.IsNullOrWhiteSpace(categoryChoice) Then Return

        Dim cat As Integer = ParseCategory(categoryChoice)
        If cat = 0 Then
            MessageBox.Show("Please enter 1, 2, 3, 4 or the category name.", "Invalid Category", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim columnName As String = Microsoft.VisualBasic.InputBox(
            "Enter the name of the new assessment column (e.g., Quiz 3, Activity 4):",
            "Add Assessment Column",
            "New Assessment")

        If String.IsNullOrWhiteSpace(columnName) Then Return
        columnName = columnName.Trim()

        Dim maxInput As String = Microsoft.VisualBasic.InputBox(
            "Enter the total / highest possible score for '" & columnName & "':",
            "Maximum Score",
            "100")
        If String.IsNullOrWhiteSpace(maxInput) Then Return

        Dim maxScore As Decimal
        If Not Decimal.TryParse(maxInput.Trim(), maxScore) OrElse maxScore <= 0 Then
            MessageBox.Show("Please enter a number greater than 0.", "Invalid Score", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim internalName As String = "col_" & Regex.Replace(columnName, "[^A-Za-z0-9]", "_")
        If dgvGradebook.Columns.Contains(internalName) Then
            MessageBox.Show("A column with that name already exists.", "Duplicate Column", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Insert [Raw][GrA] just before the category's TOTAL column
        Dim totalName As String = "cat" & cat & "_total"
        Dim insertAt As Integer = dgvGradebook.Columns(totalName).Index

        Dim pair = CreateAssessmentColumns(columnName, internalName, cat, maxScore)
        dgvGradebook.Columns.Insert(insertAt, pair.Raw)
        dgvGradebook.Columns.Insert(insertAt + 1, pair.GrA)

        RecalculateAll()
        dgvGradebook.Invalidate()

        MessageBox.Show("Column '" & columnName.ToUpper() & "' added successfully under " & CategoryName(cat).ToUpper() & "!",
                        "Column Added", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ' ==========================================
    ' 8. DYNAMIC COLUMN DELETION
    ' ==========================================
    Private clickedRawColumnName As String = Nothing

    Private Sub dgvGradebook_ColumnHeaderMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvGradebook.ColumnHeaderMouseClick
        If e.Button = MouseButtons.Right AndAlso e.RowIndex = -1 Then
            Dim col As DataGridViewColumn = dgvGradebook.Columns(e.ColumnIndex)
            Dim info As ColInfo = GetInfo(col)

            ' Only assessment columns (Raw or its GrA) can be deleted.
            ' No., Student Name, DRP, category TOTALs, MAJOR EXAM total and GRADE are protected.
            If info.Kind <> ColKind.Raw AndAlso info.Kind <> ColKind.GrA Then Return

            clickedRawColumnName = If(info.Kind = ColKind.Raw, col.Name, info.PairName)
            Dim label As String = dgvGradebook.Columns(clickedRawColumnName).HeaderText.Trim()

            Dim menu As New ContextMenuStrip()
            Dim deleteItem As ToolStripMenuItem = menu.Items.Add("Delete Column '" & label & "'")

            AddHandler deleteItem.Click, AddressOf DeleteSelectedColumn_Click
            menu.Show(Cursor.Position)
        End If
    End Sub

    Private Sub DeleteSelectedColumn_Click(sender As Object, e As EventArgs)
        If clickedRawColumnName Is Nothing OrElse Not dgvGradebook.Columns.Contains(clickedRawColumnName) Then Return

        Dim label As String = dgvGradebook.Columns(clickedRawColumnName).HeaderText

        Dim result As DialogResult = MessageBox.Show(
            "Are you sure you want to delete the column '" & label & "'? All entered grades in this column will be lost.",
            "Confirm Column Deletion",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning
        )

        If result = DialogResult.Yes Then
            ' Remove the GrA column and its raw column together
            Dim graName As String = clickedRawColumnName & "_gra"
            If dgvGradebook.Columns.Contains(graName) Then dgvGradebook.Columns.Remove(graName)
            dgvGradebook.Columns.Remove(clickedRawColumnName)
            clickedRawColumnName = Nothing

            RecalculateAll()
            dgvGradebook.Invalidate()
            MessageBox.Show("Column deleted successfully.", "Column Removed", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub btnSaveConfig_Click(sender As Object, e As EventArgs) Handles btnSaveConfig.Click
        Try
            ' 1. Prompt or read the current category names and weights from your UI/inputs
            ' (If you are using InputBoxes like before, keep them here; otherwise, read from your configuration controls)

            ' Example validation for 100% total (Major Exam locked at 40%)
            Dim totalSum As Decimal = customCat1Weight + customCat2Weight + customCat3Weight + weightExam

            If totalSum <> 100D Then
                MessageBox.Show($"The total distribution must equal exactly 100%. Your current total is {totalSum}%." & vbCrLf & vbCrLf &
                           "Note: Major Exam is fixed at 40%, meaning your custom categories must combine to equal 60%.",
                           "Weight Distribution Mismatch",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Warning)
                Return
            End If

            ' 2. Save the active configuration to the MySQL database
            SaveActiveWeightsToDatabase(customCat1Name, customCat1Weight, customCat2Name, customCat2Weight, customCat3Name, customCat3Weight, weightExam)

            ' 3. Refresh grid header bands and recalculate grades
            dgvGradebook.Invalidate()
            RecalculateAll()

            MessageBox.Show("Gradebook configuration saved successfully and persisted!", "Configuration Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("An error occurred while saving configuration: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class