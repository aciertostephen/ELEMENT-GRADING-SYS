Imports ClosedXML.Excel
Imports MySqlConnector
Imports System.Text.RegularExpressions

Public Class ucGradebook

    Private _classId As Integer

    Public Sub New()
        InitializeComponent()

        ' Selected row = very light gray (replaces the default blue highlight)
        dgvGradebook.DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 236, 236)
        dgvGradebook.DefaultCellStyle.SelectionForeColor = Color.Black
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
        Public ItemId As Integer        ' grade_item.item_id (Raw columns only)
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

    ' grade_item.item_id -> DataGridView column Name (built from the database)
    Private ReadOnly itemIdToColumn As New Dictionary(Of Integer, String)

    ' grading_rubric.rubric_id for each category 1..4 of this class (index 0 unused)
    Private rubricIdByCat(4) As Integer

    ' EmbeddedMode: hosted inside another screen (Back button hidden, the host handles it)
    ' ViewOnly:     look but don't touch (admin view). Set both BEFORE adding the control to a form.
    Private _embedded As Boolean = False
    Private _viewOnly As Boolean = False

    Public Property EmbeddedMode As Boolean
        Get
            Return _embedded
        End Get
        Set(value As Boolean)
            _embedded = value
        End Set
    End Property

    Public Property ViewOnly As Boolean
        Get
            Return _viewOnly
        End Get
        Set(value As Boolean)
            _viewOnly = value
        End Set
    End Property

    ' Value of the cell before the user started editing (used to undo a failed save)
    Private _oldCellValue As Object = Nothing

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

    ' Header palette: one hue (grayish blue), different shades
    Private ReadOnly HdrFixed As Color = Color.LightGray
    Private ReadOnly HdrSubRow As Color = Color.LightGray

    Private Function CategoryColor(cat As Integer) As Color
        Return Color.LightGray
    End Function

    ' Header colours (same green family as the title bar and buttons)
    Private ReadOnly HdrDark As Color = Color.FromArgb(33, 89, 76)
    Private ReadOnly HdrDark2 As Color = Color.FromArgb(52, 112, 96)
    Private ReadOnly HdrLight As Color = Color.FromArgb(228, 237, 233)
    Private ReadOnly HdrAccent As Color = Color.FromArgb(240, 224, 120)

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
        If Not _viewOnly AndAlso Not ClassService.TeacherOwnsClass(_classId, SessionManager.CurrentUserId) Then
            Msg.Warn("You are not the teacher of this class.")
            Me.Enabled = False
            Return
        End If
        EnsureClassRubric()
        LoadWeightsFromDatabase()
        BuildGrid()
        LoadStudents()
        RecalculateAll()
        ApplyViewMode()
    End Sub

    Private Sub ApplyViewMode()
        btnBack.Visible = Not _embedded
        If _viewOnly Then
            dgvGradebook.ReadOnly = True
            HideEditControls(Me)
        End If
    End Sub

    ' Read-only view: remove every control that changes data (Add Column, Set Weights, Save ...).
    ' Checks ALL controls (not just standard Buttons, since the green ones may be custom controls),
    ' by name and by caption. Export Excel is kept because it only reads.
    Private Sub HideEditControls(parent As Control)
        For Each c As Control In parent.Controls
            Dim caption As String = If(c.Text, "").Trim().ToLower()
            If c Is btnAddColumn OrElse c Is btnConfigureWeights OrElse c Is btnSaveConfig OrElse c Is btnImport _
               OrElse caption.Contains("save") OrElse caption.Contains("add column") OrElse caption.Contains("weights") Then
                c.Visible = False
            ElseIf c.HasChildren Then
                HideEditControls(c)
            End If
        Next
    End Sub

    ' A class always has 4 grading_rubric rows (3 custom + the fixed Final Exam).
    ' If it has none (e.g. created before sp_create_class existed) create them, and give an
    ' empty class the starter assessment columns. Teachers only: the admin view never writes.
    Private Sub EnsureClassRubric()
        If _classId <= 0 OrElse _viewOnly Then Return
        Try
            Dim dtR As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT COUNT(*) FROM grading_rubric WHERE class_id = @cid",
                {New MySqlParameter("@cid", _classId)})
            If dtR Is Nothing OrElse dtR.Rows.Count = 0 Then Return

            If Convert.ToInt32(dtR.Rows(0)(0)) = 0 Then
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO grading_rubric (class_id, component_name, weight_percent, is_fixed) VALUES " &
                    "(@cid, 'Participation', 10, 0), (@cid, 'Quizzes', 20, 0), (@cid, 'Projects', 30, 0), (@cid, 'Final Exam', 40, 1)",
                    {New MySqlParameter("@cid", _classId)})
            End If

            LoadWeightsFromDatabase()

            Dim dtI As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT COUNT(*) FROM grade_item gi INNER JOIN grading_rubric r ON r.rubric_id = gi.rubric_id WHERE r.class_id = @cid",
                {New MySqlParameter("@cid", _classId)})
            If dtI IsNot Nothing AndAlso dtI.Rows.Count > 0 AndAlso Convert.ToInt32(dtI.Rows(0)(0)) = 0 Then
                For Each it In defaultItems
                    Dim rid As Integer = rubricIdByCat(it.Category)
                    If rid > 0 Then
                        DatabaseHelper.ExecuteNonQuery(
                            "INSERT INTO grade_item (rubric_id, item_name, max_score) VALUES (@r, @n, @m)",
                            {New MySqlParameter("@r", rid),
                             New MySqlParameter("@n", it.ItemName),
                             New MySqlParameter("@m", it.MaxScore)})
                    End If
                Next
            End If
        Catch ex As Exception
            ' Leave the grid empty rather than blocking the screen
        End Try
    End Sub

    ' Category names and weights live in grading_rubric (per class).
    ' Categories 1..3 = the non-fixed components in creation order, category 4 = the fixed Final Exam.
    Private Sub LoadWeightsFromDatabase()
        Array.Clear(rubricIdByCat, 0, rubricIdByCat.Length)
        If _classId <= 0 Then Return

        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT rubric_id, component_name, weight_percent, is_fixed FROM grading_rubric " &
                "WHERE class_id = @cid ORDER BY is_fixed ASC, rubric_id ASC",
                {New MySqlParameter("@cid", _classId)})
            If dt Is Nothing Then Return

            Dim custom As Integer = 0
            For Each row As DataRow In dt.Rows
                Dim cat As Integer
                If Convert.ToInt32(row("is_fixed")) = 1 Then
                    cat = 4
                Else
                    custom += 1
                    If custom > 3 Then Continue For
                    cat = custom
                End If

                rubricIdByCat(cat) = Convert.ToInt32(row("rubric_id"))
                Dim nm As String = row("component_name").ToString()
                Dim w As Decimal = Convert.ToDecimal(row("weight_percent"))

                Select Case cat
                    Case 1 : customCat1Name = nm : customCat1Weight = w
                    Case 2 : customCat2Name = nm : customCat2Weight = w
                    Case 3 : customCat3Name = nm : customCat3Weight = w
                    Case 4 : weightExam = w
                End Select
            Next
        Catch ex As Exception
            ' Falls back silently to the defaults defined at the class level
        End Try
    End Sub

    ' Saves the category names/weights back into this class's grading_rubric rows
    Private Sub SaveActiveWeightsToDatabase(cat1Name As String, cat1Val As Decimal, cat2Name As String, cat2Val As Decimal, cat3Name As String, cat3Val As Decimal, examVal As Decimal)
        Try
            Dim names() As String = {"", cat1Name, cat2Name, cat3Name, "Final Exam"}
            Dim vals() As Decimal = {0D, cat1Val, cat2Val, cat3Val, examVal}

            For cat As Integer = 1 To 4
                If rubricIdByCat(cat) <= 0 Then Continue For
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE grading_rubric SET component_name = @n, weight_percent = @w WHERE rubric_id = @id",
                    {New MySqlParameter("@n", names(cat)),
                     New MySqlParameter("@w", vals(cat)),
                     New MySqlParameter("@id", rubricIdByCat(cat))})
            Next
        Catch ex As Exception
            MessageBox.Show("Error saving weights to database: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
    End Sub

    ' grade_item rows of one category, in creation order
    Private Function LoadItemsForCategory(cat As Integer) As List(Of (ItemId As Integer, ItemName As String, ColumnName As String, MaxScore As Decimal))
        Dim items As New List(Of (ItemId As Integer, ItemName As String, ColumnName As String, MaxScore As Decimal))
        If cat < 1 OrElse cat > 4 OrElse rubricIdByCat(cat) <= 0 Then Return items

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT item_id, item_name, max_score FROM grade_item WHERE rubric_id = @rid ORDER BY item_id",
            {New MySqlParameter("@rid", rubricIdByCat(cat))})
        If dt Is Nothing Then Return items

        For Each r As DataRow In dt.Rows
            Dim id As Integer = Convert.ToInt32(r("item_id"))
            Dim nm As String = r("item_name").ToString()
            items.Add((id, nm, "col_" & Regex.Replace(nm, "[^A-Za-z0-9]", "_") & "_" & id, Convert.ToDecimal(r("max_score"))))
        Next
        Return items
    End Function

    ' ==========================================
    ' 1. GRID LAYOUT (replaces designer columns)
    ' ==========================================
    Private Sub BuildGrid()
        itemIdToColumn.Clear()

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
            .ColumnHeadersDefaultCellStyle.BackColor = HdrFixed
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
            For Each it In LoadItemsForCategory(cat)
                Dim pair = CreateAssessmentColumns(it.ItemName, it.ColumnName, cat, it.MaxScore, it.ItemId)
                dgvGradebook.Columns.Add(pair.Raw)
                dgvGradebook.Columns.Add(pair.GrA)
                itemIdToColumn(it.ItemId) = it.ColumnName
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
                                             cat As Integer, maxScore As Decimal,
                                             Optional itemId As Integer = 0) As (Raw As DataGridViewTextBoxColumn, GrA As DataGridViewTextBoxColumn)
        Dim raw As New DataGridViewTextBoxColumn()
        raw.Name = colName
        raw.HeaderText = (itemName & " (" & maxScore.ToString("0.##") & ")").ToUpper()
        raw.Width = 80
        raw.ReadOnly = False
        raw.Tag = New ColInfo() With {.Category = cat, .Kind = ColKind.Raw, .MaxScore = maxScore, .ItemName = itemName, .ItemId = itemId}

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
        Dim cellText As String = CStr(e.FormattedValue)

        ' Columns without a category (NO., NAME, DRP, GRADE): one tall dark cell
        If cat = 0 Then
            Using br As New SolidBrush(HdrDark)
                g.FillRectangle(br, b)
            End Using
            Using pen As New Pen(Color.White)
                g.DrawLine(pen, b.Right - 1, b.Top, b.Right - 1, b.Bottom - 1)
            End Using
            Using bold As New Font(baseFont, FontStyle.Bold)
                TextRenderer.DrawText(g, cellText, bold, b, Color.White, txtFlags)
            End Using
            Using br As New SolidBrush(HdrAccent)
                g.FillRectangle(br, b.X, b.Bottom - 2, b.Width, 2)
            End Using
            Return
        End If

        Dim h As Integer = b.Height \ 2
        Dim bandRect As New Rectangle(b.X, b.Y, b.Width, h)
        Dim lowRect As New Rectangle(b.X, b.Y + h, b.Width, b.Height - h)

        ' Bottom half: pale mint, dark green text
        Using br As New SolidBrush(HdrLight)
            g.FillRectangle(br, lowRect)
        End Using
        Using pen As New Pen(Color.FromArgb(196, 212, 205))
            g.DrawLine(pen, b.Right - 1, lowRect.Top, b.Right - 1, lowRect.Bottom - 1)
        End Using
        TextRenderer.DrawText(g, cellText, baseFont, lowRect, HdrDark, txtFlags)

        ' Top half: merged band across the category's columns
        Dim first, last As Integer
        GetCategoryRun(e.ColumnIndex, first, last)

        Using br As New SolidBrush(If(cat Mod 2 = 1, HdrDark, HdrDark2))
            g.FillRectangle(br, bandRect)
        End Using
        If e.ColumnIndex = last Then
            Using pen As New Pen(Color.White)
                g.DrawLine(pen, b.Right - 1, b.Top, b.Right - 1, b.Top + h)
            End Using
        End If

        ' Gold line along the bottom of the header
        Using br As New SolidBrush(HdrAccent)
            g.FillRectangle(br, b.X, b.Bottom - 2, b.Width, 2)
        End Using

        ' Band text: centered across the whole span, clipped to this cell
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
            TextRenderer.DrawText(g, bandText, bold, spanRect, Color.White,
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
        "       gi.item_id, gi.item_name, ge.score " &
        "FROM enrollment e " &
        "JOIN student_profile sp ON sp.student_id = e.student_id " &
        "LEFT JOIN grade_entry ge ON ge.enrollment_id = e.enrollment_id " &
        "LEFT JOIN grade_item gi ON gi.item_id = ge.item_id " &
        "WHERE e.class_id = @cid " &
        "ORDER BY sp.last_name, sp.first_name"

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@cid", _classId)})
        If dt Is Nothing Then Return

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

            If Not IsDBNull(r("item_id")) AndAlso Not IsDBNull(r("score")) Then
                Dim colName As String = Nothing
                If itemIdToColumn.TryGetValue(Convert.ToInt32(r("item_id")), colName) _
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

        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT t.school_year, t.semester, s.section_code, co.course_code, co.course_name, " &
                "       UPPER(CONCAT(tp.last_name, ', ', tp.first_name, " &
                "            IF(tp.middle_name IS NULL OR tp.middle_name = '', '', CONCAT(' ', tp.middle_name)))) AS professor_name " &
                "FROM classes c " &
                "INNER JOIN academic_term t ON t.term_id = c.term_id " &
                "INNER JOIN section s ON s.section_id = c.section_id " &
                "INNER JOIN course co ON co.course_id = c.course_id " &
                "INNER JOIN teacher_profile tp ON tp.teacher_id = c.teacher_id " &
                "WHERE c.class_id = @cid",
                {New MySqlParameter("@cid", _classId)})

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                info.SchoolYear = dt.Rows(0)("school_year").ToString()
                info.Semester = dt.Rows(0)("semester").ToString()
                info.Section = dt.Rows(0)("section_code").ToString()
                info.Program = (dt.Rows(0)("course_code").ToString() & " " & dt.Rows(0)("course_name").ToString()).ToUpper()   ' template C2 = Course
                info.Professor = dt.Rows(0)("professor_name").ToString()
            End If
        Catch ex As Exception
            ' Blank header values keep the template's existing text
        End Try

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

            MessageBox.Show("Please enter 1, 2, 3 or the name of the grading period.", "Invalid Grading Period", MessageBoxButtons.OK)
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
                        "Semestral Not Exportable", MessageBoxButtons.OK)
            Return
        End If

        ' --- Template must exist ---
        Dim templatePath As String = System.IO.Path.Combine(Application.StartupPath, "Templates", TemplateFileName)
        If Not System.IO.File.Exists(templatePath) Then
            MessageBox.Show("The Excel template was not found:" & vbCrLf & templatePath,
                        "Template Missing", MessageBoxButtons.OK)
            Return
        End If

        ' --- Students currently shown in the gradebook ---
        Dim students As List(Of DataGridViewRow) =
        dgvGradebook.Rows.Cast(Of DataGridViewRow)().Where(Function(r) Not r.IsNewRow).ToList()

        Dim capacity As Integer = LastStudentRow - FirstStudentRow + 1
        If students.Count > capacity Then
            MessageBox.Show($"The template holds at most {capacity} students (rows {FirstStudentRow}-{LastStudentRow}), " &
                        $"but this class has {students.Count}.",
                        "Too Many Students", MessageBoxButtons.OK)
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
                    MessageBox.Show($"The template has no worksheet named '{period}'.", "Worksheet Not Found", MessageBoxButtons.OK)
                    Return
                End If

                Dim wsRoster As IXLWorksheet = Nothing
                If Not wb.Worksheets.TryGetWorksheet(RosterSheetName, wsRoster) Then
                    MessageBox.Show($"The template has no worksheet named '{RosterSheetName}'.", "Worksheet Not Found", MessageBoxButtons.OK)
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

                ' --- 2b. The period that is NOT exported still holds the template's sample scores: clear them ---
                For Each otherName As String In New String() {"Midterm", "Final"}
                    Dim wsOther As IXLWorksheet = Nothing
                    If Not String.Equals(otherName, period, StringComparison.OrdinalIgnoreCase) AndAlso
                       wb.Worksheets.TryGetWorksheet(otherName, wsOther) Then
                        For rowNo As Integer = FirstStudentRow To LastStudentRow
                            For Each colLetter As String In New String() {ColDrp, ColAbsences, ColExtra, ColCW, ColQuiz1Raw, ColQuiz2Raw, ColProject, ColExamRaw}
                                Dim sample As IXLCell = wsOther.Cell(colLetter & rowNo)
                                If Not sample.HasFormula Then sample.Clear(XLClearOptions.Contents)
                            Next
                        Next
                    End If
                Next

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
                        If pcts.Count > 0 Then projectPct = Math.Round(pcts.Average(), 2, MidpointRounding.AwayFromZero)
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
            MessageBox.Show(msg, "Export Complete", MessageBoxButtons.OK)

        Catch ex As System.IO.IOException
            MessageBox.Show("The file could not be saved. If it is open in Excel, close it and try again." & vbCrLf & vbCrLf & ex.Message,
                        "Export Error", MessageBoxButtons.OK)
        Catch ex As Exception
            MessageBox.Show("An error occurred while exporting: " & ex.Message, "Export Error", MessageBoxButtons.OK)
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
    ' IMPORT CLASS LIST (Excel -> this class's list of students)
    ' ==========================================
    ' The Excel parsing lives in ClassListExcelReader, the database work in ClassListImportService
    ' and the pop-up in ImportClassListDialog; this screen only starts it and refreshes the grid.
    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        If _viewOnly OrElse _classId <= 0 Then Return

        Dim path As String
        Using ofd As New OpenFileDialog() With {
            .Title = "Select the class list (Excel)",
            .Filter = "Excel workbook (*.xlsx;*.xlsm)|*.xlsx;*.xlsm"}
            If ofd.ShowDialog(Me.FindForm()) <> DialogResult.OK Then Return
            path = ofd.FileName
        End Using

        Try
            Cursor.Current = Cursors.WaitCursor
            Using reader As ClassListExcelReader = ClassListExcelReader.Open(path)
                If reader.Sheets.Count = 0 Then
                    Msg.Warn("No class list sheet was found in this file." & vbCrLf &
                             "Sheet names must look like: BSIT 2A 2026-2027")
                    Return
                End If

                Using dlg As New ImportClassListDialog(_classId, reader)
                    If dlg.ShowDialog(Me.FindForm()) = DialogResult.OK Then
                        LoadStudents()
                        RecalculateAll()
                    End If
                End Using
            End Using
        Catch ex As Exception
            Msg.Fail("import the class list", ex)
        End Try
    End Sub

    ' ==========================================
    ' 5. GRADING INPUT, VALIDATION & CALCULATION
    ' ==========================================

    ' Click-to-Toggle for DRP Column with Confirmation Prompt
    Private Sub dgvGradebook_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGradebook.CellClick
        If _viewOnly Then Return

        If e.RowIndex >= 0 AndAlso dgvGradebook.Columns(e.ColumnIndex).Name = "status" Then
            If GradebookFrozen() Then Return
            Dim cell As DataGridViewCell = dgvGradebook.Rows(e.RowIndex).Cells(e.ColumnIndex)
            Dim currentVal As String = If(cell.Value IsNot Nothing, cell.Value.ToString(), "")

            If String.IsNullOrEmpty(currentVal) Then
                Dim result As DialogResult = MessageBox.Show(
                    "Are you sure you want to mark this student as Dropped?",
                    "Confirm Student Drop",
                    MessageBoxButtons.YesNo)

                If result = DialogResult.Yes Then
                    If SaveEnrollmentStatus(dgvGradebook.Rows(e.RowIndex), "Dropped") Then
                        cell.Value = "Dropped"
                    End If
                End If
            Else
                If SaveEnrollmentStatus(dgvGradebook.Rows(e.RowIndex), "Enrolled") Then
                    cell.Value = ""
                End If
            End If

            RecalculateRow(dgvGradebook.Rows(e.RowIndex))
            dgvGradebook.InvalidateRow(e.RowIndex)
        End If
    End Sub

    Private Sub dgvGradebook_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles dgvGradebook.CellBeginEdit
        If e.RowIndex >= 0 Then _oldCellValue = dgvGradebook.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
    End Sub

    Private Sub dgvGradebook_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGradebook.CellEndEdit
        If e.RowIndex < 0 Then Return
        Dim info As ColInfo = GetInfo(dgvGradebook.Columns(e.ColumnIndex))
        If info.Kind <> ColKind.Raw Then Return ' only raw-score columns are editable

        Dim editedRow As DataGridViewRow = dgvGradebook.Rows(e.RowIndex)
        Dim editedCell As DataGridViewCell = editedRow.Cells(e.ColumnIndex)
        Dim inputString As String = If(editedCell.Value IsNot Nothing, editedCell.Value.ToString(), "")

        If String.IsNullOrWhiteSpace(inputString) Then
            editedCell.Value = Nothing
        Else
            Dim numericValue As Decimal
            If Decimal.TryParse(inputString, numericValue) Then
                If numericValue < 0 OrElse numericValue > info.MaxScore Then
                    MessageBox.Show("Please enter a valid score between 0 and " & info.MaxScore.ToString("0.##") & ".", "Invalid Score", MessageBoxButtons.OK)
                    editedCell.Value = _oldCellValue          ' keep the previous score (it used to be overwritten with 0.00)
                    RecalculateRow(editedRow)
                    Return
                Else
                    editedCell.Value = numericValue.ToString("0.00")
                End If
            Else
                MessageBox.Show("Please enter a numeric value.", "Invalid Input", MessageBoxButtons.OK)
                editedCell.Value = _oldCellValue              ' keep the previous score (it used to be overwritten with 0.00)
                RecalculateRow(editedRow)
                Return
            End If
        End If

        ' Write the score to grade_entry; if the database refuses it (encoding closed, term locked,
        ' score above the maximum) put the old value back
        If Not SaveScoreToDatabase(editedRow, info, editedCell.Value) Then
            editedCell.Value = _oldCellValue
        End If

        RecalculateRow(editedRow)
    End Sub

    ' ---------- Database writes ----------

    ' Upsert one score into grade_entry. The table triggers enforce the encoding window,
    ' the term lock and the item's maximum score. The saved value is read back to confirm it.
    Private Function SaveScoreToDatabase(row As DataGridViewRow, info As ColInfo, value As Object) As Boolean
        Dim enrId As String = TryCast(row.Tag, String)
        If String.IsNullOrEmpty(enrId) OrElse info.ItemId <= 0 Then Return True

        Dim scoreValue As Object = DBNull.Value
        Dim d As Decimal
        If value IsNot Nothing AndAlso Decimal.TryParse(value.ToString(), d) Then scoreValue = d

        Try
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO grade_entry (enrollment_id, item_id, score) VALUES (@e, @i, @s) " &
                "ON DUPLICATE KEY UPDATE score = @s",
                {New MySqlParameter("@e", enrId),
                 New MySqlParameter("@i", info.ItemId),
                 New MySqlParameter("@s", scoreValue)})
        Catch ex As Exception
            MessageBox.Show("The score was not saved: " & ex.Message, "Save Failed", MessageBoxButtons.OK)
            Return False
        End Try

        Dim chk As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT score FROM grade_entry WHERE enrollment_id = @e AND item_id = @i",
            {New MySqlParameter("@e", enrId), New MySqlParameter("@i", info.ItemId)})

        Dim saved As Boolean = False
        If chk IsNot Nothing Then
            If scoreValue Is DBNull.Value Then
                saved = (chk.Rows.Count = 0) OrElse IsDBNull(chk.Rows(0)(0))
            Else
                saved = chk.Rows.Count > 0 AndAlso Not IsDBNull(chk.Rows(0)(0)) AndAlso Convert.ToDecimal(chk.Rows(0)(0)) = d
            End If
        End If

        If Not saved Then
            MessageBox.Show("The score was not saved. Encoding may be closed or the term locked.", "Save Failed", MessageBoxButtons.OK)
        End If
        Return saved
    End Function

    Private Function SaveEnrollmentStatus(row As DataGridViewRow, newStatus As String) As Boolean
        Dim enrId As String = TryCast(row.Tag, String)
        If String.IsNullOrEmpty(enrId) Then Return True
        Try
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE enrollment SET status = @s, " &
                "  final_grade = IF(@s = 'Dropped', NULL, final_grade), " &
                "  grade_equivalent = IF(@s = 'Dropped', NULL, grade_equivalent) WHERE enrollment_id = @e",
                {New MySqlParameter("@s", newStatus), New MySqlParameter("@e", enrId)})
            Return True
        Catch ex As Exception
            MessageBox.Show("Could not update the student's status: " & ex.Message, "Database Error", MessageBoxButtons.OK)
            Return False
        End Try
    End Function

    ' Creates the grade_item row for a new column and returns its item_id (0 on failure)
    Private Function InsertGradeItem(rubricId As Integer, itemName As String, maxScore As Decimal) As Integer
        Try
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO grade_item (rubric_id, item_name, max_score) VALUES (@r, @n, @m)",
                {New MySqlParameter("@r", rubricId),
                 New MySqlParameter("@n", itemName),
                 New MySqlParameter("@m", maxScore)})

            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT item_id FROM grade_item WHERE rubric_id = @r AND item_name = @n",
                {New MySqlParameter("@r", rubricId), New MySqlParameter("@n", itemName)})
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then Return Convert.ToInt32(dt.Rows(0)(0))
        Catch ex As Exception
            MessageBox.Show("Could not add the column: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
        Return 0
    End Function

    ' Deleting a grade_item also removes its scores (ON DELETE CASCADE)
    Private Function DeleteGradeItem(itemId As Integer) As Boolean
        Try
            DatabaseHelper.ExecuteNonQuery(
                "DELETE FROM grade_item WHERE item_id = @id",
                {New MySqlParameter("@id", itemId)})
            Return True
        Catch ex As Exception
            MessageBox.Show("Could not delete the column: " & ex.Message, "Database Error", MessageBoxButtons.OK)
            Return False
        End Try
    End Function

    Private Function ToGrA(raw As Decimal, maxScore As Decimal) As Decimal
        Return GradeCalculator.ToGrA(raw, maxScore)
    End Function

    Private Sub RecalculateAll()
        For Each row As DataGridViewRow In dgvGradebook.Rows
            Dim fg As Decimal? = RecalculateRow(row)
            SaveFinalGrade(row, fg)
        Next
    End Sub

    ' Per row:  GrA per assessment -> category average -> x weight -> category total -> final grade
    ' Returns the final grade, or Nothing when the student is dropped / has no scores yet
    Private Function RecalculateRow(row As DataGridViewRow) As Decimal?
        If row.IsNewRow Then Return Nothing

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
        Dim anyScore As Boolean = False
        For cat As Integer = 1 To 4
            Dim totalCol As String = "cat" & cat & "_total"
            If Not dgvGradebook.Columns.Contains(totalCol) Then Continue For

            If counts(cat) > 0 Then
                anyScore = True
                Dim weighted As Decimal = GradeCalculator.CategoryTotal(sums(cat), counts(cat), CategoryWeight(cat))
                row.Cells(totalCol).Value = weighted.ToString("0.00")
                finalGrade += weighted
            Else
                row.Cells(totalCol).Value = ""
            End If
        Next

        Dim status As String = If(row.Cells("status").Value IsNot Nothing, row.Cells("status").Value.ToString(), "")
        Dim isDropped As Boolean = status.Equals("Dropped", StringComparison.OrdinalIgnoreCase)
        row.Cells("grade").Value = If(isDropped, "DRP", finalGrade.ToString("0.00"))

        If isDropped OrElse Not anyScore Then Return Nothing
        Return Math.Min(100D, finalGrade)
    End Function

    ' Writes final_grade and grade_equivalent (looked up in grade_scale) to the enrollment row.
    ' Teachers only: the admin's view-only gradebook never writes.
    Private Sub SaveFinalGrade(row As DataGridViewRow, finalGrade As Decimal?)
        Return
    End Sub

    ' ==========================================
    ' 6. CUSTOM CATEGORY & WEIGHT CONFIGURATION WORKFLOW
    ' ==========================================
    Private Sub btnConfigureWeights_Click(sender As Object, e As EventArgs) Handles btnConfigureWeights.Click
        If GradebookFrozen() Then Return
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
                MessageBox.Show("Please enter a valid non-negative number for " & name1 & ".", "Invalid Input", MessageBoxButtons.OK)
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
                MessageBox.Show("Please enter a valid non-negative number for " & name2 & ".", "Invalid Input", MessageBoxButtons.OK)
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
                MessageBox.Show("Please enter a valid non-negative number for " & name3 & ".", "Invalid Input", MessageBoxButtons.OK)
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
                               MessageBoxButtons.OK)
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
                            MessageBoxButtons.OK)

        Catch ex As Exception
            MessageBox.Show("An unexpected error occurred while configuring weights: " & ex.Message, "System Error", MessageBoxButtons.OK)
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



    ' True (and tells the teacher why) when the gradebook is frozen: past school year, locked term or
    ' encoding closed. Scores are already protected by database triggers; columns, weights and
    ' drop / un-drop were not.
    Private Function GradebookFrozen() As Boolean
        If _viewOnly OrElse _classId <= 0 Then Return False
        Dim reason As String = Nothing
        Try
            reason = FrozenReason()
        Catch
            Return False
        End Try
        If reason Is Nothing Then Return False
        Msg.Warn(reason)
        Return True
    End Function

    Private Function FrozenReason() As String
        Dim info As ClassService.ClassInfo = ClassService.GetInfo(_classId)
        If info Is Nothing Then Return "Class not found."
        If Not AcademicContext.IsEditableYear(info.SchoolYear) Then
            Return "School year " & info.SchoolYear & " is past history and is read-only."
        End If
        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT is_locked, (NOW() < encoding_start) AS not_open, (NOW() > encoding_deadline) AS is_closed " &
            "FROM academic_term WHERE term_id = @t",
            {New MySqlParameter("@t", info.TermId)})
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            If Convert.ToInt32(dt.Rows(0)("is_locked")) = 1 Then Return "This term is locked, so the gradebook can no longer change."
            If Convert.ToInt32(dt.Rows(0)("not_open")) = 1 Then Return "Grade encoding for this term has not opened yet."
            If Convert.ToInt32(dt.Rows(0)("is_closed")) = 1 Then Return "The grade encoding deadline has passed."
        End If
        Return Nothing
    End Function

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
        If GradebookFrozen() Then Return
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
            MessageBox.Show("Please enter 1, 2, 3, 4 or the category name.", "Invalid Category", MessageBoxButtons.OK)
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
            MessageBox.Show("Please enter a number greater than 0.", "Invalid Score", MessageBoxButtons.OK)
            Return
        End If

        ' The same assessment name can't be used twice in one gradebook
        For Each c As DataGridViewColumn In dgvGradebook.Columns
            Dim ci As ColInfo = GetInfo(c)
            If ci.Kind = ColKind.Raw AndAlso String.Equals(ci.ItemName, columnName, StringComparison.OrdinalIgnoreCase) Then
                MessageBox.Show("A column with that name already exists.", "Duplicate Column", MessageBoxButtons.OK)
                Return
            End If
        Next

        Dim rubricId As Integer = rubricIdByCat(cat)
        If rubricId <= 0 Then
            MessageBox.Show("This class has no grading component for '" & CategoryName(cat) & "'.", "Cannot Add Column", MessageBoxButtons.OK)
            Return
        End If

        Dim newItemId As Integer = InsertGradeItem(rubricId, columnName, maxScore)
        If newItemId <= 0 Then Return

        Dim internalName As String = "col_" & Regex.Replace(columnName, "[^A-Za-z0-9]", "_") & "_" & newItemId
        itemIdToColumn(newItemId) = internalName

        ' Insert [Raw][GrA] just before the category's TOTAL column
        Dim totalName As String = "cat" & cat & "_total"
        Dim insertAt As Integer = dgvGradebook.Columns(totalName).Index

        Dim pair = CreateAssessmentColumns(columnName, internalName, cat, maxScore, newItemId)
        dgvGradebook.Columns.Insert(insertAt, pair.Raw)
        dgvGradebook.Columns.Insert(insertAt + 1, pair.GrA)

        RecalculateAll()
        dgvGradebook.Invalidate()

        MessageBox.Show("Column '" & columnName.ToUpper() & "' added successfully under " & CategoryName(cat).ToUpper() & "!",
                        "Column Added", MessageBoxButtons.OK)
    End Sub

    ' ==========================================
    ' 8. DYNAMIC COLUMN DELETION
    ' ==========================================
    Private clickedRawColumnName As String = Nothing

    Private Sub dgvGradebook_ColumnHeaderMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvGradebook.ColumnHeaderMouseClick
        If _viewOnly Then Return

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
        If GradebookFrozen() Then Return

        Dim label As String = dgvGradebook.Columns(clickedRawColumnName).HeaderText

        Dim result As DialogResult = MessageBox.Show(
            "Are you sure you want to delete the column '" & label & "'? All entered grades in this column will be lost.",
            "Confirm Column Deletion",
            MessageBoxButtons.YesNo)

        If result = DialogResult.Yes Then
            ' Remove the grade_item (and its scores) from the database first
            Dim itemId As Integer = GetInfo(dgvGradebook.Columns(clickedRawColumnName)).ItemId
            If itemId > 0 Then
                If Not DeleteGradeItem(itemId) Then Return
                itemIdToColumn.Remove(itemId)
            End If

            ' Remove the GrA column and its raw column together
            Dim graName As String = clickedRawColumnName & "_gra"
            If dgvGradebook.Columns.Contains(graName) Then dgvGradebook.Columns.Remove(graName)
            dgvGradebook.Columns.Remove(clickedRawColumnName)
            clickedRawColumnName = Nothing

            RecalculateAll()
            dgvGradebook.Invalidate()
            MessageBox.Show("Column deleted successfully.", "Column Removed", MessageBoxButtons.OK)
        End If
    End Sub

    Private Sub btnSaveConfig_Click(sender As Object, e As EventArgs) Handles btnSaveConfig.Click
        If GradebookFrozen() Then Return
        Try
            Dim totalSum As Decimal = customCat1Weight + customCat2Weight + customCat3Weight + weightExam

            If totalSum <> 100D Then
                Msg.Warn($"The total distribution must equal exactly 100%. Your current total is {totalSum}%." & vbCrLf & vbCrLf &
                     "Note: Major Exam is fixed at 40%, so your custom categories must combine to equal 60%.",
                     "Weight Distribution Mismatch")
                Return
            End If

            If Not Msg.Confirm("Saving the gradebook computes the final grades and makes them visible to the students." & vbCrLf &
                               "The admin is notified, and the class list can no longer be changed (no more importing)." & vbCrLf & vbCrLf &
                               "Save and release the grades now?", "Save and release grades") Then Return

            SaveActiveWeightsToDatabase(customCat1Name, customCat1Weight, customCat2Name, customCat2Weight,
                                    customCat3Name, customCat3Weight, weightExam)

            dgvGradebook.Invalidate()
            RecalculateAll()   ' only refreshes the grid on screen now

            Dim n As Integer = GradeReleaseService.SaveAndRelease(_classId)
            Msg.Info("Saved. " & n & " student(s) can now see their final grades. The admin was notified.")
        Catch ex As Exception
            Msg.Fail("save the gradebook", ex)
        End Try
    End Sub

End Class