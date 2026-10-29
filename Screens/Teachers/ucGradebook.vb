Imports ClosedXML.Excel
Imports MySqlConnector
Imports System.Text.RegularExpressions

Public Class ucGradebook

    Private _classId As Integer
    Private _period As String = "Midterm"
    Private _periodSelector As ComboBox
    Private _periodLockNotice As Label

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

    Public Sub New(classId As Integer, period As String)
        Me.New(classId)
        If period IsNot Nothing AndAlso (period.Equals("Midterm", StringComparison.OrdinalIgnoreCase) OrElse period.Equals("Final", StringComparison.OrdinalIgnoreCase)) Then
            _period = If(period.Equals("Midterm", StringComparison.OrdinalIgnoreCase), "Midterm", "Final")
        End If
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

    ' True right after Save Config: the gradebook is saved, and the next change has to be confirmed
    Private _gradebookSaved As Boolean = False

    ' Save Config look: dimmed + hover hint while the gradebook is incomplete (the button itself stays clickable
    ' so that clicking it can explain what is missing)
    Private ReadOnly saveTip As New ToolTip()
    Private _saveBtnBackColor As Color
    Private _saveBtnColorKept As Boolean = False

    ' True while the code itself refills the Weights ComboBox (its SelectedIndexChanged is then ignored)
    Private _updatingWeights As Boolean = False

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
        If Not _viewOnly AndAlso (Not SessionManager.IsTeacher OrElse Not ClassService.TeacherOwnsClass(_classId, SessionManager.CurrentUserId)) Then
            Msg.Warn("You are not the teacher of this class.")
            Me.Enabled = False
            Return
        End If
        If _viewOnly AndAlso Not SessionManager.IsAdmin Then
            Msg.Warn("Only administrators can open the read-only historical gradebook.")
            Me.Enabled = False
            Return
        End If
        If Not _viewOnly AndAlso Not ClassService.IsClassInActiveTerm(_classId) Then
            Msg.Warn("Teachers can only open classes for the active term.")
            Me.Enabled = False
            Return
        End If
        EnsurePeriodSelector()
        AddHandler pnlBottomActions.SizeChanged, AddressOf pnlBottomActions_SizeChanged
        EnsureClassRubric()
        LoadWeightsFromDatabase()
        BuildGrid()
        LoadStudents()
        RecalculateAll()
        UpdateWeightDistributionComboBox()
        _gradebookSaved = IsPeriodReleased()
        UpdatePeriodEditingState()
        UpdateSaveButtonState()
        ApplyViewMode()
    End Sub

    Private Sub EnsurePeriodSelector()
        If _periodSelector IsNot Nothing Then Return
        _periodSelector = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 150, .Height = 28,
            .Font = New Font("Century Gothic", 9.0F), .Location = New Point(710, 31), .Anchor = AnchorStyles.Top Or AnchorStyles.Left}
        _periodSelector.Items.AddRange(New Object() {"Midterm", "Final"})
        _periodSelector.SelectedItem = _period
        _periodLockNotice = New Label With {
            .AutoSize = False,
            .Font = New Font("Segoe UI", 8.5F),
            .ForeColor = Color.FromArgb(128, 64, 64),
            .Location = New Point(870, 27),
            .Size = New Size(350, 34),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Visible = False
        }
        pnlTopRubricBG.Controls.Add(_periodLockNotice)
        AddHandler _periodSelector.SelectedIndexChanged,
            Sub(sender, e)
                If _periodSelector.SelectedItem Is Nothing Then Return
                Dim selected As String = _periodSelector.SelectedItem.ToString()
                If selected = _period Then Return
                _period = selected
                EnsureClassRubric()
                LoadWeightsFromDatabase()
                BuildGrid()
                LoadStudents()
                RecalculateAll()
                UpdateWeightDistributionComboBox()
                _gradebookSaved = IsPeriodReleased()
                UpdatePeriodEditingState()
                UpdateSaveButtonState()
            End Sub
        pnlTopRubricBG.Controls.Add(_periodSelector)
    End Sub

    Private Function CurrentPeriodLockReason() As String
        If _viewOnly OrElse _classId <= 0 OrElse _period <> "Final" Then Return Nothing
        If Not GradeReleaseService.IsMidtermCompleteAndPublished(_classId) Then
            Return "Final editing is locked until the Midterm gradebook is complete and officially published."
        End If
        Return Nothing
    End Function

    Private Sub UpdatePeriodEditingState()
        Dim lockReason As String = CurrentPeriodLockReason()
        Dim finalLocked As Boolean = lockReason IsNot Nothing

        For Each col As DataGridViewColumn In dgvGradebook.Columns
            If GetInfo(col).Kind = ColKind.Raw Then col.ReadOnly = finalLocked
        Next

        If _periodLockNotice IsNot Nothing Then
            _periodLockNotice.Text = If(finalLocked, "Publish Midterm grades before editing Final.", "")
            _periodLockNotice.Visible = finalLocked
        End If

        btnAddColumn.Enabled = Not finalLocked
        btnConfigureWeights.Enabled = Not finalLocked
        btnImport.Enabled = Not finalLocked
        btnSaveChanges.Enabled = Not finalLocked
        Weights.Enabled = Not finalLocked
    End Sub

    Private Function IsPeriodReleased() As Boolean
        Dim col As String = If(_period = "Midterm", "midterm_released_at", "finals_released_at")
        Return DatabaseHelper.ScalarInt("SELECT COUNT(*) FROM classes WHERE class_id = @c AND " & col & " IS NOT NULL",
            {DatabaseHelper.P("@c", _classId)}) > 0
    End Function

    Private Sub ApplyViewMode()
        btnBack.Visible = Not _embedded
        If _viewOnly Then
            dgvGradebook.ReadOnly = True
            HideEditControls(Me)
        End If
    End Sub

    Private Sub btnSaveChanges_Click(sender As Object, e As EventArgs) Handles btnSaveChanges.Click
        If _viewOnly OrElse _classId <= 0 Then Return
        If GradebookFrozen() Then Return

        Try
            dgvGradebook.EndEdit()
            If WeightsTotal() = 100D Then
                PersistWeightsIfValid()
            End If
            RecalculateAll()
            Msg.Info("Changes saved. Scores are saved as you enter them; this action does not release grades to students.", "Changes Saved")
        Catch ex As Exception
            Msg.Fail("save gradebook changes", ex)
        End Try
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
        ' Opening a locked Final view must never create or change Final rubric data.
        If CurrentPeriodLockReason() IsNot Nothing Then Return
        Try
            Dim dtR As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT COUNT(*) FROM grading_rubric WHERE class_id = @cid AND period = @p",
                {New MySqlParameter("@cid", _classId), New MySqlParameter("@p", _period)})
            If dtR Is Nothing OrElse dtR.Rows.Count = 0 Then Return

            If Convert.ToInt32(dtR.Rows(0)(0)) = 0 Then
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO grading_rubric (class_id, period, component_name, weight_percent, is_fixed) VALUES " &
                    "(@cid, @p, 'Participation', 10, 0), (@cid, @p, 'Quizzes', 20, 0), (@cid, @p, 'Projects', 30, 0), (@cid, @p, 'Final Exam', 40, 1)",
                    {New MySqlParameter("@cid", _classId), New MySqlParameter("@p", _period)})
            End If

            LoadWeightsFromDatabase()

            Dim dtI As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT COUNT(*) FROM grade_item gi INNER JOIN grading_rubric r ON r.rubric_id = gi.rubric_id WHERE r.class_id = @cid AND r.period = @p",
                {New MySqlParameter("@cid", _classId), New MySqlParameter("@p", _period)})
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
                "WHERE class_id = @cid AND period = @p ORDER BY is_fixed ASC, rubric_id ASC",
                {New MySqlParameter("@cid", _classId), New MySqlParameter("@p", _period)})
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
        "       pg.item_id, pg.item_name, pg.score " &
        "FROM enrollment e " &
        "JOIN student_profile sp ON sp.student_id = e.student_id " &
        "LEFT JOIN (SELECT ge.enrollment_id, gi.item_id, gi.item_name, ge.score " &
        "           FROM grade_entry ge " &
        "           JOIN grade_item gi ON gi.item_id = ge.item_id " &
        "           JOIN grading_rubric rp ON rp.rubric_id = gi.rubric_id " &
        "           WHERE rp.class_id = @cid AND rp.period = @p) pg ON pg.enrollment_id = e.enrollment_id " &
        "WHERE e.class_id = @cid " &
        "ORDER BY sp.last_name, sp.first_name"

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, {New MySqlParameter("@cid", _classId), New MySqlParameter("@p", _period)})
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

    ' ==========================================
    ' 4. EXPORT TO THE EXCEL TEMPLATE (dynamic layout)
    ' ==========================================

    ' ---- Template settings ----
    Private Const TemplateFileName As String = "GradingTemplate.xlsm"   ' <app folder>\Templates\
    Private Const RosterSheetName As String = "Midterm"                ' Semestral reads the names from here
    Private Const HeaderTopRow As Integer = 12                         ' row 12 = category band, 13 = labels, 14 = max score / weight
    Private Const FirstStudentRow As Integer = 15
    Private Const LastStudentRow As Integer = 59                       ' Semestral (and the template summary) stop at row 59
    Private Const ClearToRow As Integer = 133                          ' everything from row 12 to here is rebuilt
    Private Const ClearToCol As Integer = 32                           ' ... and from column A to AF
    Private Const ExportFontSize As Double = 10                         ' font size of the exported table (rows 12 and below)
    Private Const StudentRowHeight As Double = 21                      ' height of one line of a student row (points)
    Private Const NameColWidth As Double = 34                          ' width of the NAME column (characters)

    ' Old fixed template input columns - only used to blank the sample scores of the period that is NOT exported
    Private ReadOnly OldInputColumns As String() = {"C", "D", "F", "G", "H", "J", "M", "P"}

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

    Private Function AskGradingPeriod() As String
        Using dlg As New GradebookValueDialog("Grading Period", "Choose the grading period to export:", "Midterm",
                                               New String() {"Midterm", "Final"})
            If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return Nothing
            Return dlg.Value
        End Using
    End Function

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExportExcel.Click
        Dim period As String = AskGradingPeriod()
        If period Is Nothing Then Return
        If _periodSelector IsNot Nothing Then _periodSelector.SelectedItem = period

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

                ' --- 1. Header information (rows 1-3, unchanged mapping) ---
                ' C1, F1, I1, C2, F3 live on the roster sheet (Final links to them by formula).
                SetTextOrNumber(wsRoster, "C1", header.SchoolYear)
                SetTextOrNumber(wsRoster, "F1", header.Semester)
                SetTextOrNumber(wsRoster, "I1", header.Section)      ' I1:L1 is merged - I1 is the anchor
                SetTextOrNumber(wsRoster, "C2", header.Program)
                SetTextOrNumber(wsRoster, "F3", header.Professor)
                SetTextOrNumber(wsTarget, "C3", period)              ' grading period, on the selected sheet

                ' --- 2. Rows 12+ : rebuilt from the gradebook grid, column for column ---
                Dim layout As ExportLayout = BuildGradebookSheet(wsTarget, students)

                ' --- 3. Keep the rest of the workbook consistent with the new layout ---
                ' Semestral reads the names from the roster sheet. When Final is exported the roster (Midterm)
                ' still needs the names, otherwise Semestral would show the template's sample students.
                If Not String.Equals(period, RosterSheetName, StringComparison.OrdinalIgnoreCase) Then
                    For i As Integer = 0 To capacity - 1
                        SetText(wsRoster, "B" & (FirstStudentRow + i), If(i < students.Count, GridText(students(i), "studname"), ""))
                    Next
                End If

                ' The period that is NOT exported still holds the template's sample scores: clear them
                For Each otherName As String In New String() {"Midterm", "Final"}
                    Dim wsOther As IXLWorksheet = Nothing
                    If Not String.Equals(otherName, period, StringComparison.OrdinalIgnoreCase) AndAlso
                       wb.Worksheets.TryGetWorksheet(otherName, wsOther) Then
                        For rowNo As Integer = FirstStudentRow To LastStudentRow
                            For Each colLetter As String In OldInputColumns
                                Dim sample As IXLCell = wsOther.Cell(colLetter & rowNo)
                                If Not sample.HasFormula Then sample.Clear(XLClearOptions.Contents)
                            Next
                        Next
                    End If
                Next

                RepointSummaryAndSemestral(wb, wsTarget, period, layout)

                ' Make Excel recalculate every template formula when the file is opened
                wb.FullCalculationOnLoad = True

                wb.SaveAs(savePath)
            End Using

            MessageBox.Show("Class grades have been exported successfully!", "Export Complete", MessageBoxButtons.OK)

        Catch ex As System.IO.IOException
            MessageBox.Show("The file could not be saved. If it is open in Excel, close it and try again." & vbCrLf & vbCrLf & ex.Message,
                        "Export Error", MessageBoxButtons.OK)
        Catch ex As Exception
            MessageBox.Show("An error occurred while exporting: " & ex.Message, "Export Error", MessageBoxButtons.OK)
        End Try
    End Sub

    ' ---------- Dynamic sheet builder ----------

    ' Where the columns that other parts of the workbook depend on ended up
    Private Class ExportLayout
        Public GradeCol As Integer
        Public EqCol As Integer
        Public RemarksCol As Integer
    End Class

    Private Function ColLetter(colNumber As Integer) As String
        Return XLHelper.GetColumnLetterFromNumber(colNumber)
    End Function

    ' Rebuilds rows 12+ of 'ws' so that it mirrors dgvGradebook:
    '   Excel column n  =  grid column n  (NO., NAME, DRP, [Raw, GrA]..., TOTAL, ..., GRADE)
    '   + two extra columns after GRADE: EQ and REMARKS (the template's summary and Semestral sheet need them).
    ' Adding or deleting a column in the grid therefore adds or deletes it in the export automatically.
    ' Row 12 = category band, row 13 = column label, row 14 = max score (Raw) or weight % (TOTAL).
    ' GrA, TOTAL and GRADE are Excel formulas that use the same rules as the grid, so the sheet stays live.
    Private Function BuildGradebookSheet(ws As IXLWorksheet, students As List(Of DataGridViewRow)) As ExportLayout
        Dim gridCols As List(Of DataGridViewColumn) =
            dgvGradebook.Columns.Cast(Of DataGridViewColumn)().OrderBy(Function(c) c.DisplayIndex).ToList()

        ' Excel column number of every grid column (1-based, same order)
        Dim idxByName As New Dictionary(Of String, Integer)
        For i As Integer = 0 To gridCols.Count - 1
            idxByName(gridCols(i).Name) = i + 1
        Next
        If Not idxByName.ContainsKey("studname") OrElse Not idxByName.ContainsKey("status") OrElse Not idxByName.ContainsKey("grade") Then
            Throw New InvalidOperationException("The gradebook grid is missing its name, DRP or GRADE column.")
        End If

        Dim colName As Integer = idxByName("studname")
        Dim colDrp As Integer = idxByName("status")
        Dim colGrade As Integer = idxByName("grade")
        Dim colEq As Integer = gridCols.Count + 1
        Dim colRem As Integer = gridCols.Count + 2

        ' GrA columns and the TOTAL column of each category
        Dim graByCat(4) As List(Of Integer)
        Dim totalByCat(4) As Integer
        For cat As Integer = 1 To 4
            graByCat(cat) = New List(Of Integer)
        Next
        For i As Integer = 0 To gridCols.Count - 1
            Dim info As ColInfo = GetInfo(gridCols(i))
            If info.Kind = ColKind.GrA Then graByCat(info.Category).Add(i + 1)
            If info.Kind = ColKind.CatTotal Then totalByCat(info.Category) = i + 1
        Next

        ' ---- 1. Wipe rows 12+ of the old fixed layout (merges, formulas, formats, validations) ----
        Dim oldMerges As List(Of IXLRange) =
            ws.MergedRanges.Where(Function(m) m.RangeAddress.FirstAddress.RowNumber >= HeaderTopRow).ToList()
        For Each m As IXLRange In oldMerges
            m.Unmerge()
        Next
        ws.Range(HeaderTopRow, 1, ClearToRow, Math.Max(ClearToCol, colRem)).Clear()

        Dim lastRow As Integer = FirstStudentRow - 1 + students.Count
        Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture
        Dim graBaseTxt As String = GrABase.ToString(ci)
        Dim graSpanTxt As String = (100D - GrABase).ToString(ci)

        ' Header palette = the grid's palette
        Dim cDark As XLColor = XLColor.FromArgb(33, 89, 76)
        Dim cDark2 As XLColor = XLColor.FromArgb(52, 112, 96)
        Dim cLight As XLColor = XLColor.FromArgb(228, 237, 233)
        Dim cGra As XLColor = XLColor.FromArgb(221, 235, 247)
        Dim cTotal As XLColor = XLColor.FromArgb(226, 239, 218)
        Dim cLine As XLColor = XLColor.FromArgb(196, 212, 205)

        ' ---- 2. Header rows 12-14 ----
        Dim pos As Integer = 1
        While pos <= gridCols.Count
            Dim cat As Integer = GetInfo(gridCols(pos - 1)).Category

            If cat = 0 Then
                ' NO., NAME, DRP, GRADE: one tall dark cell (rows 12-14)
                Dim tall As IXLRange = ws.Range(HeaderTopRow, pos, HeaderTopRow + 2, pos).Merge()
                tall.FirstCell().Value = gridCols(pos - 1).HeaderText
                tall.Style.Fill.BackgroundColor = cDark
                tall.Style.Font.FontColor = XLColor.White
                tall.Style.Font.Bold = True
                pos += 1
            Else
                ' Category run: merged band over all of its columns
                Dim first As Integer = pos
                While pos <= gridCols.Count AndAlso GetInfo(gridCols(pos - 1)).Category = cat
                    pos += 1
                End While
                Dim last As Integer = pos - 1

                Dim band As IXLRange = ws.Range(HeaderTopRow, first, HeaderTopRow, last).Merge()
                band.FirstCell().Value = (CategoryName(cat) & " " & CategoryWeight(cat).ToString("0.##") & "%").ToUpper()
                band.Style.Fill.BackgroundColor = If(cat Mod 2 = 1, cDark, cDark2)
                band.Style.Font.FontColor = XLColor.White
                band.Style.Font.Bold = True

                For k As Integer = first To last
                    Dim gi As ColInfo = GetInfo(gridCols(k - 1))
                    Dim lbl As IXLCell = ws.Cell(HeaderTopRow + 1, k)
                    Dim sub14 As IXLCell = ws.Cell(HeaderTopRow + 2, k)

                    Select Case gi.Kind
                        Case ColKind.Raw
                            lbl.Value = gi.ItemName.ToUpper()
                            sub14.Value = CDbl(gi.MaxScore)                     ' max score: GrA formulas read this cell
                            sub14.Style.NumberFormat.Format = """MAX: ""General"
                        Case ColKind.GrA
                            lbl.Value = "GrA"
                        Case ColKind.CatTotal
                            lbl.Value = If(cat = 4, "MAJOR EXAM", "TOTAL")
                            sub14.Value = CDbl(CategoryWeight(cat))              ' weight: TOTAL formulas read this cell
                            sub14.Style.NumberFormat.Format = "General""%"""
                    End Select
                Next

                Dim lowRng As IXLRange = ws.Range(HeaderTopRow + 1, first, HeaderTopRow + 2, last)
                lowRng.Style.Fill.BackgroundColor = cLight
                lowRng.Style.Font.FontColor = cDark
                lowRng.Style.Font.Bold = True
            End If
        End While

        ' EQ and REMARKS (extra columns): same look as the fixed dark columns
        Dim extraCols As Integer() = {colEq, colRem}
        Dim extraTexts As String() = {"EQ", "REMARKS"}
        For x As Integer = 0 To 1
            Dim tallX As IXLRange = ws.Range(HeaderTopRow, extraCols(x), HeaderTopRow + 2, extraCols(x)).Merge()
            tallX.FirstCell().Value = extraTexts(x)
            tallX.Style.Fill.BackgroundColor = cDark
            tallX.Style.Font.FontColor = XLColor.White
            tallX.Style.Font.Bold = True
        Next

        Dim hdr As IXLRange = ws.Range(HeaderTopRow, 1, HeaderTopRow + 2, colRem)
        hdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center
        hdr.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center
        hdr.Style.Alignment.WrapText = True
        ws.Row(HeaderTopRow).Height = 22
        ws.Row(HeaderTopRow + 1).Height = 30
        ws.Row(HeaderTopRow + 2).Height = 18

        ' ---- 3. Student rows ----
        For i As Integer = 0 To students.Count - 1
            Dim r As Integer = FirstStudentRow + i
            Dim g As DataGridViewRow = students(i)

            ' NO., NAME (Lastname, Firstname), DRP ("Drp" only for dropped students)
            Dim studentName As String = GridText(g, "studname")
            ws.Cell(r, 1).Value = i + 1
            ws.Cell(r, colName).Value = studentName
            Dim nameLines As Integer = Math.Max(1, CInt(Math.Ceiling(studentName.Length / (NameColWidth - 4))))
            ws.Row(r).Height = StudentRowHeight * nameLines
            If GridText(g, "status").Equals("Dropped", StringComparison.OrdinalIgnoreCase) Then ws.Cell(r, colDrp).Value = "Drp"

            For k As Integer = 1 To gridCols.Count
                Dim gi As ColInfo = GetInfo(gridCols(k - 1))
                Dim cell As IXLCell = ws.Cell(r, k)

                Select Case gi.Kind
                    Case ColKind.Raw
                        Dim v As Decimal? = GridNumber(g, gridCols(k - 1))
                        If v.HasValue Then cell.Value = CDbl(v.Value)
                        cell.Style.NumberFormat.Format = "0.00"
                        cell.Style.Protection.Locked = False        ' the sheet is protected: scores stay editable

                    Case ColKind.GrA
                        ' same rule as the grid:  raw / max * (100 - base) + base, blank when not graded
                        Dim rawRef As String = ColLetter(k - 1) & r
                        Dim maxRef As String = ColLetter(k - 1) & "$" & (HeaderTopRow + 2)
                        cell.FormulaA1 = $"IF(AND(ISNUMBER({rawRef}),{maxRef}>0),ROUND({rawRef}/{maxRef}*{graSpanTxt}+{graBaseTxt},2),"""")"
                        cell.Style.NumberFormat.Format = "0.00"
                        cell.Style.Fill.BackgroundColor = cGra
                        cell.Style.Font.FontColor = XLColor.Blue

                    Case ColKind.CatTotal
                        ' category average of the graded GrA cells x category weight / 100
                        If graByCat(gi.Category).Count > 0 Then
                            Dim refs As String = String.Join(",", graByCat(gi.Category).Select(Function(n) ColLetter(n) & r))
                            Dim wRef As String = ColLetter(k) & "$" & (HeaderTopRow + 2)
                            cell.FormulaA1 = $"IF(COUNT({refs})=0,"""",ROUND(AVERAGE({refs})*{wRef}/100,2))"
                        End If
                        cell.Style.NumberFormat.Format = "0.00"
                        cell.Style.Fill.BackgroundColor = cTotal
                        cell.Style.Font.Bold = True
                End Select
            Next

            ' GRADE = sum of the category totals ("DRP" for dropped students)
            Dim totalRefs As String = String.Join(",", Enumerable.Range(1, 4).Where(Function(n) totalByCat(n) > 0).Select(Function(n) ColLetter(totalByCat(n)) & r))
            Dim gradeRef As String = ColLetter(colGrade) & r
            ws.Cell(r, colGrade).FormulaA1 = $"IF(LOWER(${ColLetter(colDrp)}{r})=""drp"",""DRP"",ROUND(SUM({totalRefs}),2))"
            ws.Cell(r, colGrade).Style.NumberFormat.Format = "0.00"
            ws.Cell(r, colGrade).Style.Font.Bold = True

            ' EQ (grade equivalent, template's EQTable) and REMARKS (Pass / Fail / DRP, passing grade = D9)
            ws.Cell(r, colEq).FormulaA1 = $"IF(ISNUMBER({gradeRef}),VLOOKUP({gradeRef},EQTable,2),{gradeRef})"
            ws.Cell(r, colRem).FormulaA1 = $"IF(ISNUMBER({gradeRef}),IF({gradeRef}>=$D$9,""Pass"",""Fail""),{gradeRef})"
        Next

        ' ---- 4. Body formatting ----
        Dim bodyEnd As Integer = Math.Max(lastRow, HeaderTopRow + 2)
        Dim body As IXLRange = ws.Range(HeaderTopRow, 1, bodyEnd, colRem)
        body.Style.Border.OutsideBorder = XLBorderStyleValues.Thin
        body.Style.Border.InsideBorder = XLBorderStyleValues.Thin
        body.Style.Border.OutsideBorderColor = cLine
        body.Style.Border.InsideBorderColor = cLine
        body.Style.Font.FontSize = ExportFontSize

        If students.Count > 0 Then
            Dim data As IXLRange = ws.Range(FirstStudentRow, 1, lastRow, colRem)
            data.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center
            data.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center
            Dim nameRange As IXLRange = ws.Range(FirstStudentRow, colName, lastRow, colName)
            nameRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left
            nameRange.Style.Alignment.WrapText = True
        End If

        ' ---- 5. Column widths ----
        ws.Column(1).Width = 6
        ws.Column(colName).Width = NameColWidth
        ws.Column(colDrp).Width = 8
        For k As Integer = 1 To gridCols.Count
            Dim gi As ColInfo = GetInfo(gridCols(k - 1))
            Select Case gi.Kind
                Case ColKind.Raw : ws.Column(k).Width = Math.Min(22, Math.Max(11, gi.ItemName.Length + 3))
                Case ColKind.GrA : ws.Column(k).Width = 9
                Case ColKind.CatTotal : ws.Column(k).Width = 12
            End Select
        Next
        ws.Column(colGrade).Width = 10
        ws.Column(colEq).Width = 8
        ws.Column(colRem).Width = 11

        Return New ExportLayout With {.GradeCol = colGrade, .EqCol = colEq, .RemarksCol = colRem}
    End Function

    ' The template's summary box (Q6:Q9) and the Semestral sheet pointed at the OLD fixed columns
    ' (S = EQ, T = Grade, U = Remarks). Point them at the columns of the rebuilt sheet instead.
    Private Sub RepointSummaryAndSemestral(wb As XLWorkbook, wsTarget As IXLWorksheet, period As String, layout As ExportLayout)
        Dim remL As String = ColLetter(layout.RemarksCol)

        ' Summary of grades (DRP / INC / Pass / Fail counts)
        Dim counts As String() = {"drp", "inc", "Pass", "Fail"}
        For i As Integer = 0 To 3
            Dim cell As IXLCell = wsTarget.Cell("Q" & (6 + i))
            If cell.HasFormula Then
                cell.FormulaA1 = $"COUNTIF({remL}{FirstStudentRow}:{remL}{LastStudentRow},""{counts(i)}"")"
            End If
        Next

        ' Print area follows the new width
        Try
            wsTarget.PageSetup.PrintAreas.Clear()
            wsTarget.PageSetup.PrintAreas.Add("A1:" & remL & LastStudentRow)
        Catch
            ' cosmetic only
        End Try

        ' Semestral: <period>!S/T/U  ->  EQ / GRADE / REMARKS column of the rebuilt sheet
        Dim wsSem As IXLWorksheet = Nothing
        If Not wb.Worksheets.TryGetWorksheet("Semestral", wsSem) Then Return

        Dim map As New Dictionary(Of String, String) From {
            {"S", ColLetter(layout.EqCol)},
            {"T", ColLetter(layout.GradeCol)},
            {"U", remL}}
        Dim pattern As String = "(?<![A-Za-z0-9_])" & Regex.Escape(period) & "!\$?([STU])\$?(\d+)"

        For r As Integer = FirstStudentRow To LastStudentRow
            For c As Integer = 2 To 10          ' columns B..J of the Semestral table
                Dim cell As IXLCell = wsSem.Cell(r, c)
                If Not cell.HasFormula Then Continue For
                Dim f As String = cell.FormulaA1
                Dim nf As String = Regex.Replace(f, pattern, Function(m As Match) period & "!" & map(m.Groups(1).Value) & m.Groups(2).Value, RegexOptions.IgnoreCase)
                If nf <> f Then cell.FormulaA1 = nf
            Next
        Next
    End Sub

    ' ---------- Export helpers ----------

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

    ' Skips formula cells (template formulas are never overwritten) and clears the cell when there is no value
    Private Sub SetText(ws As IXLWorksheet, address As String, value As String)
        Dim cell As IXLCell = ws.Cell(address)
        If cell.HasFormula Then Return
        If String.IsNullOrEmpty(value) Then
            cell.Clear(XLClearOptions.Contents)
        Else
            cell.Value = value
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
            If Not ConfirmChangeAfterSave("changing this student's drop status") Then Return
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
            UpdateSaveButtonState()
        End If
    End Sub

    Private Sub dgvGradebook_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles dgvGradebook.CellBeginEdit
        If e.RowIndex < 0 Then Return

        If GetInfo(dgvGradebook.Columns(e.ColumnIndex)).Kind = ColKind.Raw AndAlso GradebookFrozen() Then
            e.Cancel = True
            Return
        End If

        ' After Save Config the gradebook is "saved": the first score change has to be confirmed
        If GetInfo(dgvGradebook.Columns(e.ColumnIndex)).Kind = ColKind.Raw AndAlso Not ConfirmChangeAfterSave("editing a score") Then
            e.Cancel = True
            Return
        End If

        _oldCellValue = dgvGradebook.Rows(e.RowIndex).Cells(e.ColumnIndex).Value
    End Sub

    Private Sub dgvGradebook_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGradebook.CellEndEdit
        Try
            ProcessScoreEdit(e)
        Finally
            UpdateSaveButtonState()      ' Save Config is only available when every score is filled in
        End Try
    End Sub

    Private Sub ProcessScoreEdit(e As DataGridViewCellEventArgs)
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
        If GradebookFrozen() Then Return False
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
        UpdateSaveButtonState()
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
    ' 6. CATEGORY NAMES & WEIGHTS
    ' ==========================================
    ' Weights:   click a category in the Weights ComboBox and type its new percentage.
    ' Names:     the Configure button renames the three custom categories.
    ' Major Exam is fixed at 40% and can only be read. The four weights must always total 100%.

    Private Function WeightsTotal() As Decimal
        Return customCat1Weight + customCat2Weight + customCat3Weight + weightExam
    End Function

    ' Saves names + weights to the database, but only while the total is exactly 100%
    ' (a half-finished edit is never stored). Returns True when it was saved.
    Private Function PersistWeightsIfValid() As Boolean
        If WeightsTotal() <> 100D Then Return False
        SaveActiveWeightsToDatabase(customCat1Name, customCat1Weight, customCat2Name, customCat2Weight, customCat3Name, customCat3Weight, weightExam)
        Return True
    End Function

    ' Reads the percentage typed in the pop-up. Only the text AFTER ":" counts,
    ' so "Quizzes: 25%", ": 25" and "25" are all read as 25.
    Private Function TryParseWeightInput(input As String, ByRef value As Decimal) As Boolean
        Dim s As String = If(input, "")
        Dim p As Integer = s.LastIndexOf(":"c)
        If p >= 0 Then s = s.Substring(p + 1)
        s = s.Replace("%", "").Trim()
        Return Decimal.TryParse(s, value)
    End Function

    ' Once the gradebook has been saved, the next change has to be confirmed.
    ' Yes = the gradebook becomes "unsaved" again (press Save Config to save it); No = the change is cancelled.
    Private Function ConfirmChangeAfterSave(what As String) As Boolean
        If Not _gradebookSaved Then Return True

        If Not Msg.Confirm("You now have a saved gradebook." & vbCrLf & vbCrLf &
                           "Do you want to continue with " & what & "?" & vbCrLf &
                           "This changes the saved gradebook, so you will need to confirm it by saving again.",
                           "Saved Gradebook") Then Return False

        _gradebookSaved = False
        Return True
    End Function

    ' Click a category in the ComboBox to set its percentage.
    Private Sub Weights_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Weights.SelectedIndexChanged
        If _updatingWeights OrElse Weights.SelectedIndex < 0 Then Return
        Dim picked As Integer = Weights.SelectedIndex + 1      ' 1..3 = custom categories, 4 = Major Exam

        Try
            If _viewOnly Then Return

            If picked = 4 Then
                Msg.Info("Major Exam is fixed at " & weightExam.ToString("0.##") & "% and cannot be changed.")
                Return
            End If

            If GradebookFrozen() Then Return

            Dim catName As String = CategoryName(picked)
            Dim oldWeight As Decimal = CategoryWeight(picked)

            Dim typed As String
            Using dlg As New GradebookValueDialog("Set Category Weight",
                "Set " & catName & " percentage (the four categories must total 100%):",
                oldWeight.ToString("0.##"), maxLength:=8)
                If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                typed = dlg.Value
            End Using

            Dim newWeight As Decimal
            If Not TryParseWeightInput(typed, newWeight) OrElse newWeight < 0D OrElse newWeight > 60D Then
                Msg.Warn("Please type a percentage between 0 and 60 after "":"" (for example " & catName & ": 25%).", "Invalid Weight")
                Return
            End If
            If newWeight = oldWeight Then Return

            If Not ConfirmChangeAfterSave("changing the weight of " & catName) Then Return

            Select Case picked
                Case 1 : customCat1Weight = newWeight
                Case 2 : customCat2Weight = newWeight
                Case 3 : customCat3Weight = newWeight
            End Select

            dgvGradebook.Invalidate()          ' header bands show the new percentage

            Dim total As Decimal = WeightsTotal()
            If total = 100D Then
                PersistWeightsIfValid()
                RecalculateAll()
                Msg.Info(catName & " is now " & newWeight.ToString("0.##") & "%. The total is 100%.")
            Else
                Msg.Warn("The total is now " & total.ToString("0.##") & "%. Change another category so that the total is exactly 100% " &
                         "(Major Exam is fixed at 40%)." & vbCrLf & vbCrLf &
                         "The saved weights and the grades stay unchanged until the total is 100%.",
                         "Weight Distribution Mismatch")
            End If
        Finally
            UpdateWeightDistributionComboBox()
            UpdateSaveButtonState()
        End Try
    End Sub

    ' The Configure button only renames the three custom categories. Major Exam and every percentage stay as they are.
    Private Sub btnConfigureWeights_Click(sender As Object, e As EventArgs) Handles btnConfigureWeights.Click
        If GradebookFrozen() Then Return
        Try
            Dim oldNames() As String = {"", customCat1Name, customCat2Name, customCat3Name}
            Dim newNames() As String = {"", "", "", ""}

            For i As Integer = 1 To 3
                Dim typedName As String
                Using dlg As New GradebookValueDialog("Rename Category " & i,
                    "Enter the new name (Final Exam is fixed and cannot be renamed):", oldNames(i), maxLength:=50)
                    If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                    typedName = dlg.Value
                End Using
                typedName = typedName.Trim()

                If typedName.Equals("Major Exam", StringComparison.OrdinalIgnoreCase) Then
                    Msg.Warn("'Major Exam' is reserved for the fixed exam category. Please choose another name.", "Invalid Name")
                    Return
                End If

                For j As Integer = 1 To i - 1
                    If newNames(j).Equals(typedName, StringComparison.OrdinalIgnoreCase) Then
                        Msg.Warn("Each category needs its own name. '" & typedName & "' is already used.", "Duplicate Name")
                        Return
                    End If
                Next

                newNames(i) = typedName
            Next

            If newNames(1) = oldNames(1) AndAlso newNames(2) = oldNames(2) AndAlso newNames(3) = oldNames(3) Then
                Msg.Info("No category name was changed.")
                Return
            End If

            If Not ConfirmChangeAfterSave("renaming the categories") Then Return

            customCat1Name = newNames(1)
            customCat2Name = newNames(2)
            customCat3Name = newNames(3)

            Dim stored As Boolean = PersistWeightsIfValid()

            dgvGradebook.Invalidate()          ' header bands show the new names
            UpdateWeightDistributionComboBox()

            Msg.Info("Category names updated:" & vbCrLf & vbCrLf &
                     "- " & customCat1Name & ": " & customCat1Weight.ToString("0.##") & "%" & vbCrLf &
                     "- " & customCat2Name & ": " & customCat2Weight.ToString("0.##") & "%" & vbCrLf &
                     "- " & customCat3Name & ": " & customCat3Weight.ToString("0.##") & "%" & vbCrLf &
                     "- Major Exam: " & weightExam.ToString("0.##") & "% (Locked)" &
                     If(stored, "", vbCrLf & vbCrLf & "The names will be stored once the weights total 100%."))

        Catch ex As Exception
            Msg.Fail("rename the categories", ex)
        End Try
    End Sub

    ' Refills the Weights ComboBox from the current names/weights.
    ' No item stays selected, so clicking any category (even the same one twice) raises SelectedIndexChanged.
    Private Sub UpdateWeightDistributionComboBox()
        If Weights Is Nothing Then Return

        _updatingWeights = True
        Try
            Weights.Items.Clear()
            Weights.Items.Add(customCat1Name & ": " & customCat1Weight.ToString("0.##") & "%")
            Weights.Items.Add(customCat2Name & ": " & customCat2Weight.ToString("0.##") & "%")
            Weights.Items.Add(customCat3Name & ": " & customCat3Weight.ToString("0.##") & "%")
            Weights.Items.Add("Major Exam: " & weightExam.ToString("0.##") & "% (Locked)")
            Weights.SelectedIndex = -1
            Weights.Text = "Weights (click a category to set it)"
        Finally
            _updatingWeights = False
        End Try
    End Sub

    ' True (and tells the teacher why) when the gradebook is frozen: past school year, locked term or
    ' encoding closed. Scores are already protected by database triggers; columns, weights and
    ' drop / un-drop were not.
    Private Function GradebookFrozen() As Boolean
        If _viewOnly OrElse _classId <= 0 Then Return False
        Dim reason As String = Nothing
        Try
            reason = FrozenReason()
        Catch ex As Exception
            Msg.Fail("check the gradebook status", ex)
            Return True
        End Try
        If reason Is Nothing Then Return False
        Msg.Warn(reason)
        Return True
    End Function

    Private Function FrozenReason() As String
        Dim info As ClassService.ClassInfo = ClassService.GetInfo(_classId)
        If info Is Nothing Then Return "Class not found."
        If Not ClassService.IsClassInActiveTerm(_classId) Then Return "Teachers can only edit classes for the active term."
        Dim periodLockReason As String = CurrentPeriodLockReason()
        If periodLockReason IsNot Nothing Then Return periodLockReason
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
        Dim categoryChoice As String
        Using dlg As New GradebookValueDialog("Select Category", "Choose the grading category:", customCat2Name,
            New String() {customCat1Name, customCat2Name, customCat3Name, "Final Exam"})
            If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
            categoryChoice = dlg.Value
        End Using

        Dim cat As Integer = ParseCategory(categoryChoice)
        If cat = 0 Then
            MessageBox.Show("Please enter 1, 2, 3, 4 or the category name.", "Invalid Category", MessageBoxButtons.OK)
            Return
        End If

        Dim columnName As String
        Using dlg As New GradebookValueDialog("Add Assessment Column", "Assessment name:", "New Assessment", maxLength:=100)
            If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
            columnName = dlg.Value.Trim()
        End Using

        Dim maxInput As String
        Using dlg As New GradebookValueDialog("Maximum Score", "Highest possible score for " & columnName & ":", "100", maxLength:=12)
            If dlg.ShowDialog(FindForm()) <> DialogResult.OK Then Return
            maxInput = dlg.Value
        End Using

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

        If Not ConfirmChangeAfterSave("adding the column '" & columnName & "'") Then Return

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
            If Not ConfirmChangeAfterSave("deleting the column '" & label & "'") Then Return

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

    ' ---------- Save Config: the gradebook must be complete ----------

    ' Everything that still stops the gradebook from being saved (empty list = ready to save).
    ' Every student who is NOT marked Dropped needs a score in every assessment column,
    ' so a blank row is only accepted for a dropped student.
    Private Function GetSaveBlockers() As List(Of String)
        Dim problems As New List(Of String)

        Dim studentRows As List(Of DataGridViewRow) =
            dgvGradebook.Rows.Cast(Of DataGridViewRow)().Where(Function(gr) Not gr.IsNewRow).ToList()
        Dim scoreCols As List(Of DataGridViewColumn) =
            dgvGradebook.Columns.Cast(Of DataGridViewColumn)().Where(Function(gc) GetInfo(gc).Kind = ColKind.Raw).ToList()

        If studentRows.Count = 0 Then
            problems.Add("There are no students in this class.")
            Return problems
        End If

        If WeightsTotal() <> 100D Then
            problems.Add("The weights total " & WeightsTotal().ToString("0.##") & "% instead of 100%.")
        End If

        For cat As Integer = 1 To 4
            Dim catNo As Integer = cat
            If CategoryWeight(cat) > 0D AndAlso Not scoreCols.Any(Function(gc) GetInfo(gc).Category = catNo) Then
                problems.Add(CategoryName(cat) & " has no assessment column yet.")
            End If
        Next

        For Each gridRow As DataGridViewRow In studentRows
            If GridText(gridRow, "status").Equals("Dropped", StringComparison.OrdinalIgnoreCase) Then Continue For

            Dim missing As Integer = 0
            For Each scoreCol As DataGridViewColumn In scoreCols
                If String.IsNullOrWhiteSpace(GridText(gridRow, scoreCol.Name)) Then missing += 1
            Next

            If missing > 0 Then
                problems.Add(GridText(gridRow, "studname") & ": " & missing & " score(s) missing (or mark the student as Dropped).")
            End If
        Next

        Return problems
    End Function

    ' While the gradebook is incomplete the Save Config button is dimmed and its hover hint says why.
    ' It is NOT disabled: a disabled button can't be clicked, so it could never show the explanation message.
    Private Sub UpdateSaveButtonState()
        If btnSaveConfig Is Nothing OrElse _viewOnly Then Return

        If _gradebookSaved Then
            btnSaveConfig.Text = "Update Published " & _period & " Grades"
            btnSaveConfig.Size = New Size(228, btnSaveConfig.Height)
        Else
            btnSaveConfig.Text = "Publish " & _period & " Grades"
            btnSaveConfig.Size = New Size(184, btnSaveConfig.Height)
        End If
        AlignPublishButton()

        If Not _saveBtnColorKept Then
            _saveBtnBackColor = btnSaveConfig.BackColor
            _saveBtnColorKept = True
        End If

        Dim periodLockReason As String = CurrentPeriodLockReason()
        If periodLockReason IsNot Nothing Then
            btnSaveConfig.Enabled = False
            btnSaveConfig.BackColor = Color.Silver
            saveTip.SetToolTip(btnSaveConfig, periodLockReason)
            Return
        End If

        Dim blockers As List(Of String) = GetSaveBlockers()
        btnSaveConfig.Enabled = True

        If blockers.Count = 0 Then
            btnSaveConfig.BackColor = _saveBtnBackColor
            saveTip.SetToolTip(btnSaveConfig, "Publish the " & _period & " gradebook")
        Else
            btnSaveConfig.BackColor = Color.Silver
            saveTip.SetToolTip(btnSaveConfig, "Not ready yet: " & blockers(0))
        End If
    End Sub

    Private Sub pnlBottomActions_SizeChanged(sender As Object, e As EventArgs)
        AlignPublishButton()
    End Sub

    Private Sub AlignPublishButton()
        If btnSaveConfig Is Nothing OrElse btnSaveConfig.Parent Is Nothing Then Return
        Const rightMargin As Integer = 42
        btnSaveConfig.Left = Math.Max(8, btnSaveConfig.Parent.ClientSize.Width - btnSaveConfig.Width - rightMargin)
    End Sub

    Private Sub btnSaveConfig_Click(sender As Object, e As EventArgs) Handles btnSaveConfig.Click
        If GradebookFrozen() Then Return
        Try
            ' Only a complete gradebook can be saved (blank rows are only allowed for dropped students)
            Dim blockers As List(Of String) = GetSaveBlockers()
            If blockers.Count > 0 Then
                UpdateSaveButtonState()
                Msg.Warn("Save Config can't be activated yet because the gradebook is incomplete:" & vbCrLf & vbCrLf &
                         String.Join(vbCrLf, blockers.Take(10).Select(Function(problem) "- " & problem)) &
                         If(blockers.Count > 10, vbCrLf & "...and " & (blockers.Count - 10) & " more.", ""),
                         "Gradebook Incomplete")
                Return
            End If

            Dim totalSum As Decimal = customCat1Weight + customCat2Weight + customCat3Weight + weightExam

            If totalSum <> 100D Then
                Msg.Warn($"The total distribution must equal exactly 100%. Your current total is {totalSum}%." & vbCrLf & vbCrLf &
                     "Note: Major Exam is fixed at 40%, so your custom categories must combine to equal 60%.",
                     "Weight Distribution Mismatch")
                Return
            End If

            If Not Msg.Confirm("Publish the official " & _period & " period grades now?" & vbCrLf &
                               "Students can already see encoded scores; publishing records the official period grade for transcripts." & vbCrLf &
                               If(_period = "Final", "The semester average is calculated after both Midterm and Final grades are published. ", "") &
                               "The admin will be notified for review. The class list can no longer be changed after publication.",
                               "Publish grades") Then Return

            SaveActiveWeightsToDatabase(customCat1Name, customCat1Weight, customCat2Name, customCat2Weight,
                                    customCat3Name, customCat3Weight, weightExam)

            dgvGradebook.Invalidate()
            RecalculateAll()   ' only refreshes the grid on screen now

            Dim n As Integer = GradeReleaseService.SaveAndRelease(_classId, _period)

            _gradebookSaved = True
            UpdateSaveButtonState()
            Msg.Info("The official " & _period & " period grades were published." & vbCrLf &
                     n & " student(s) have a published " & _period & " period grade. The admin was notified for review." &
                     If(_period = "Final", " A semester average appears after both Midterm and Final grades are published.", "") & vbCrLf & vbCrLf &
                     "Later score edits save and remain visible live. Publish again to update the official period grade.",
                     "Grades Published")
        Catch ex As Exception
            Msg.Fail("save the gradebook", ex)
        End Try
    End Sub

End Class
