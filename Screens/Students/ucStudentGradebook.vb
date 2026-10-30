Imports ClosedXML.Excel
Imports MySqlConnector
Imports System.Text.RegularExpressions

Public Class ucStudentGradebook

    Private _classId As Integer
    Private _enrollmentId As String
    Private _period As String = "Midterm"
    Private _periodSelector As ComboBox
    Private _periodNotice As Label
    Private _isPredictiveMode As Boolean = False
    Private _realGrades As New List(Of Object())
    Private _predictionPanel As Panel
    Private _predictionGrid As DataGridView
    Private _predictionTitle As Label
    Private _liveGradeRefreshTimer As Timer

    ' grading_rubric.rubric_id for each category 1..4 of this class (index 0 unused)
    Private rubricIdByCat(4) As Integer

    Public Sub New()
        InitializeComponent()
        ' Match the teacher gradebook's quiet active-cell highlight instead of the default blue selection.
        dgvGradebook.DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 236, 236)
        dgvGradebook.DefaultCellStyle.SelectionForeColor = Color.Black
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
        Public ItemId As Integer
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

    ' Header palette: one hue (grayish blue), different shades
    ' Header palette: everything is the same light gray (matches the STUDENT NAME header)
    Private ReadOnly HdrFixed As Color = Color.LightGray
    Private ReadOnly HdrSubRow As Color = Color.LightGray

    Private Function CategoryColor(cat As Integer) As Color
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
        If Not SessionManager.IsStudent OrElse
           Not ClassService.StudentOwnsActiveEnrollment(_classId, _enrollmentId, SessionManager.CurrentUserId) Then
            Msg.Warn("This class is not available in your active-term gradebook.")
            Me.Enabled = False
            Return
        End If
        Try
            _period = If(GradeReleaseService.IsMidtermCompleteAndPublished(_classId), "Final", "Midterm")
        Catch ex As Exception
            Msg.Fail("choose the default grading period", ex)
            Me.Enabled = False
            Return
        End Try
        EnsurePeriodSelector()
        LoadWeightsFromDatabase()
        BuildStudentGrid()
        LoadStudentGrades()
        RecalculateAll()
        SetGridReadOnly(True)
        EnsurePredictionGrid()
        StartLiveGradeRefresh()
        AddHandler btnPredictive.MouseEnter, AddressOf PredictiveButtonMouseEnter
        AddHandler btnPredictive.MouseLeave, AddressOf PredictiveButtonMouseLeave
    End Sub

    Private Sub EnsurePeriodSelector()
        If _periodSelector IsNot Nothing Then Return
        _periodSelector = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = 150,
            .Height = 28,
            .Font = New Font("Century Gothic", 9.0F),
            .Location = New Point(250, 26)
        }
        _periodSelector.Items.AddRange(New Object() {"Midterm", "Final"})
        _periodSelector.SelectedItem = _period
        _periodNotice = New Label With {
            .AutoSize = True,
            .Font = New Font("Century Gothic", 9.0F),
            .ForeColor = Color.FromArgb(90, 90, 90),
            .Location = New Point(420, 31)
        }
        AddHandler _periodSelector.SelectedIndexChanged,
            Sub(sender, e)
                If _periodSelector.SelectedItem Is Nothing Then Return
                Dim selected As String = _periodSelector.SelectedItem.ToString()
                If selected = _period Then Return
                _period = selected
                LoadWeightsFromDatabase()
                BuildStudentGrid()
                LoadStudentGrades()
                RecalculateAll()
                UpdatePeriodNotice()
                If _isPredictiveMode Then RefreshPredictionGrid()
            End Sub
        pnlTopRubricBG.Controls.Add(_periodSelector)
        pnlTopRubricBG.Controls.Add(_periodNotice)
        UpdatePeriodNotice()
    End Sub

    Private Function IsPeriodReleased() As Boolean
        Dim col As String = If(_period = "Midterm", "midterm_released_at", "finals_released_at")
        Return DatabaseHelper.ScalarInt("SELECT COUNT(*) FROM classes WHERE class_id = @c AND " & col & " IS NOT NULL",
            {DatabaseHelper.P("@c", _classId)}) > 0
    End Function

    Private Sub UpdatePeriodNotice()
        If _periodNotice Is Nothing Then Return
        Try
            _periodNotice.Text = If(IsPeriodReleased(),
                "Published " & _period & " grades · new score updates appear live",
                "Scores update live as your teacher enters them · official " & _period & " grades are not published yet")
        Catch
            _periodNotice.Text = "Scores refresh automatically while this gradebook is open."
        End Try
    End Sub

    Private Sub PredictiveButtonMouseEnter(sender As Object, e As EventArgs)
        btnPredictive.BackColor = If(_isPredictiveMode, Color.FromArgb(185, 45, 58), Color.FromArgb(255, 220, 90))
    End Sub

    Private Sub PredictiveButtonMouseLeave(sender As Object, e As EventArgs)
        btnPredictive.BackColor = If(_isPredictiveMode, Color.FromArgb(220, 53, 69), Color.FromArgb(241, 196, 15))
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
            .ColumnHeadersDefaultCellStyle.BackColor = HdrFixed
            .ColumnHeadersDefaultCellStyle.ForeColor = Color.Black
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            .DefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 236, 236)
            .DefaultCellStyle.SelectionForeColor = Color.Black
        End With

        ' Fixed left columns matching teacher grid layout
        Dim cNo As New DataGridViewTextBoxColumn() With {.Name = "number", .HeaderText = "NO.", .ReadOnly = True, .Tag = New ColInfo()}
        Dim cName As New DataGridViewTextBoxColumn() With {.Name = "studname", .HeaderText = "STUDENT NAME", .ReadOnly = True, .Tag = New ColInfo()}
        cName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        Dim cDrp As New DataGridViewTextBoxColumn() With {.Name = "status", .HeaderText = "DRP", .ReadOnly = True, .Tag = New ColInfo()}
        dgvGradebook.Columns.AddRange(cNo, cName, cDrp)

        ' Dynamically build assessment columns from this class's grading components and items
        Try
            For cat As Integer = 1 To 4
                If rubricIdByCat(cat) > 0 Then
                    Dim itemDt As DataTable = DatabaseHelper.ExecuteQuery(
                        "SELECT item_id, item_name, max_score FROM grade_item WHERE rubric_id = @rid ORDER BY item_id",
                        {New MySqlParameter("@rid", rubricIdByCat(cat))})

                    If itemDt IsNot Nothing Then
                        For Each iRow As DataRow In itemDt.Rows
                            Dim itemId As Integer = Convert.ToInt32(iRow("item_id"))
                            Dim itemName As String = iRow("item_name").ToString()
                            Dim maxScore As Decimal = Convert.ToDecimal(iRow("max_score"))
                            Dim internalName As String = "col_" & Regex.Replace(itemName, "[^A-Za-z0-9]", "_") & "_" & itemId

                            Dim pair = CreateAssessmentColumns(itemName, internalName, cat, maxScore, itemId)
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
            MessageBox.Show("Error building student grid columns: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.None)
        End Try

        ' Final Grade Column
        Dim cGrade As New DataGridViewTextBoxColumn() With {.Name = "grade", .HeaderText = "GRADE", .ReadOnly = True, .Tag = New ColInfo()}
        cGrade.DefaultCellStyle.Font = New Font(dgvGradebook.Font, FontStyle.Bold)
        dgvGradebook.Columns.Add(cGrade)

        For Each c As DataGridViewColumn In dgvGradebook.Columns
            c.SortMode = DataGridViewColumnSortMode.NotSortable
        Next
    End Sub

    Private Function CreateAssessmentColumns(itemName As String, colName As String, cat As Integer, maxScore As Decimal, Optional itemId As Integer = 0) As (Raw As DataGridViewTextBoxColumn, GrA As DataGridViewTextBoxColumn)
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
            Using br As New SolidBrush(HdrFixed)
                g.FillRectangle(br, b)
            End Using
            g.DrawRectangle(Pens.Black, b.X, b.Y, b.Width - 1, b.Height - 1)
            TextRenderer.DrawText(g, CStr(e.FormattedValue), baseFont, b, Color.Black, txtFlags)
            Return
        End If

        Dim h As Integer = b.Height \ 2
        Dim bandRect As New Rectangle(b.X, b.Y, b.Width, h)
        Dim lowRect As New Rectangle(b.X, b.Y + h, b.Width, b.Height - h)

        Using br As New SolidBrush(HdrSubRow)
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
        g.IntersectClip(bandRect)
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
    Private Sub LoadStudentGrades(Optional quiet As Boolean = False)
        Try
            Dim sql As String = "SELECT e.enrollment_id, sp.last_name, sp.first_name, sp.middle_name, e.status, pg.item_id, pg.score " &
                                "FROM enrollment e " &
                                "JOIN student_profile sp ON sp.student_id = e.student_id " &
                                "LEFT JOIN (SELECT ge.enrollment_id, gi.item_id, ge.score " &
                                "           FROM grade_entry ge " &
                                "           JOIN grade_item gi ON gi.item_id = ge.item_id " &
                                "           JOIN grading_rubric r ON r.rubric_id = gi.rubric_id " &
                                "           WHERE r.class_id = @classId AND r.period = @period) pg ON pg.enrollment_id = e.enrollment_id " &
                                "WHERE e.enrollment_id = @enrId AND e.class_id = @classId " &
                                "AND sp.user_id = @userId AND e.status <> 'Dropped'"

            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, {
                New MySqlParameter("@enrId", _enrollmentId),
                New MySqlParameter("@classId", _classId),
                New MySqlParameter("@userId", SessionManager.CurrentUserId),
                New MySqlParameter("@period", _period)})

            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                dgvGradebook.Rows.Clear()
                _realGrades.Clear()
                Return
            End If

            Dim row As DataGridViewRow
            Dim changed As Boolean = False
            If dgvGradebook.Rows.Count = 0 Then
                Dim idx As Integer = dgvGradebook.Rows.Add()
                row = dgvGradebook.Rows(idx)
                changed = True
            Else
                row = dgvGradebook.Rows(0)
            End If
            row.Tag = _enrollmentId
            If Convert.ToString(row.Cells("number").Value) <> "1" Then
                row.Cells("number").Value = 1
                changed = True
            End If

            Dim r0 As DataRow = dt.Rows(0)
            Dim mi As String = If(IsDBNull(r0("middle_name")), "", r0("middle_name").ToString().Trim())
            Dim fullName As String = $"{r0("last_name")}, {r0("first_name")}" & If(mi = "", "", " " & mi)
            Dim rowStatus As String = If(r0("status").ToString() = "Dropped", "Dropped", "")
            If Not String.Equals(Convert.ToString(row.Cells("studname").Value), fullName, StringComparison.Ordinal) Then
                row.Cells("studname").Value = fullName
                changed = True
            End If
            If Not String.Equals(Convert.ToString(row.Cells("status").Value), rowStatus, StringComparison.Ordinal) Then
                row.Cells("status").Value = rowStatus
                changed = True
            End If

            ' Raw scores are visible as soon as the teacher finishes an edit; publishing is a separate action.
            Dim scores As New Dictionary(Of Integer, String)()
            For Each r As DataRow In dt.Rows
                If Not IsDBNull(r("item_id")) AndAlso Not IsDBNull(r("score")) Then
                    scores(Convert.ToInt32(r("item_id"))) = Convert.ToDecimal(r("score")).ToString("0.00")
                End If
            Next
            For Each col As DataGridViewColumn In dgvGradebook.Columns
                Dim info As ColInfo = GetInfo(col)
                If info.Kind <> ColKind.Raw Then Continue For
                Dim value As Object = Nothing
                If scores.ContainsKey(info.ItemId) Then value = scores(info.ItemId)
                If Not String.Equals(Convert.ToString(row.Cells(col.Index).Value), Convert.ToString(value), StringComparison.Ordinal) Then
                    row.Cells(col.Index).Value = value
                    changed = True
                End If
            Next

            If changed Then
                RecalculateRow(row)
                Dim backupRow(dgvGradebook.Columns.Count - 1) As Object
                For cellIdx As Integer = 0 To row.Cells.Count - 1
                    backupRow(cellIdx) = row.Cells(cellIdx).Value
                Next
                _realGrades.Clear()
                _realGrades.Add(backupRow)
            End If
        Catch ex As Exception
            If Not quiet Then MessageBox.Show("Error loading student grades: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.None)
        End Try
    End Sub

    Private Sub StartLiveGradeRefresh()
        If _liveGradeRefreshTimer IsNot Nothing Then Return
        _liveGradeRefreshTimer = New Timer() With {.Interval = 4000}
        AddHandler _liveGradeRefreshTimer.Tick,
            Sub(sender, e)
                If IsDisposed OrElse Disposing OrElse Not Visible OrElse _isPredictiveMode Then Return
                LoadStudentGrades(quiet:=True)
                UpdatePeriodNotice()
            End Sub
        AddHandler Disposed,
            Sub(sender, e)
                If _liveGradeRefreshTimer IsNot Nothing Then
                    _liveGradeRefreshTimer.Stop()
                    _liveGradeRefreshTimer.Dispose()
                    _liveGradeRefreshTimer = Nothing
                End If
            End Sub
        _liveGradeRefreshTimer.Start()
    End Sub

    ' ==========================================
    ' 5. GRADING VALIDATION & CALCULATION
    ' ==========================================
    Private Function ToGrA(raw As Decimal, maxScore As Decimal) As Decimal
        If maxScore <= 0 Then Return 0D
        Return GradeCalculator.ToGrA(raw, maxScore)
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
                Dim weighted As Decimal = GradeCalculator.CategoryTotal(sums(cat), counts(cat), CategoryWeight(cat))
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
        Dim grid As DataGridView = TryCast(sender, DataGridView)
        If Not _isPredictiveMode OrElse grid Is Nothing OrElse grid IsNot _predictionGrid Then Return

        Dim info As ColInfo = GetInfo(grid.Columns(e.ColumnIndex))
        If info.Kind <> ColKind.Raw Then Return

        Dim editedRow As DataGridViewRow = grid.Rows(e.RowIndex)
        Dim editedCell As DataGridViewCell = editedRow.Cells(e.ColumnIndex)
        Dim inputString As String = If(editedCell.Value IsNot Nothing, editedCell.Value.ToString(), "")

        If String.IsNullOrWhiteSpace(inputString) Then
            editedCell.Value = Nothing
            RecalculatePredictionRow(editedRow)
            Return
        End If

        Dim numericValue As Decimal
        If Decimal.TryParse(inputString, numericValue) Then
            If numericValue < 0 OrElse numericValue > info.MaxScore Then
                MessageBox.Show("Please enter a valid score between 0 and " & info.MaxScore.ToString("0.##") & ".", "Invalid Score", MessageBoxButtons.OK, MessageBoxIcon.None)
                editedCell.Value = "0.00"
            Else
                editedCell.Value = numericValue.ToString("0.00")
            End If
        Else
            MessageBox.Show("Please enter a numeric value.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.None)
            editedCell.Value = "0.00"
        End If

        RecalculatePredictionRow(editedRow)
    End Sub

    Private Sub SetGridReadOnly(isReadOnly As Boolean)
        ' The published gradebook always remains read-only; prediction edits live in the separate grid.
        dgvGradebook.ReadOnly = True
        dgvGradebook.BackgroundColor = Color.White
    End Sub

    Private Sub EnsurePredictionGrid()
        If _predictionPanel IsNot Nothing Then Return
        _predictionPanel = New Panel With {.Dock = DockStyle.Fill, .Visible = False, .Padding = New Padding(20, 8, 20, 12), .BackColor = Color.White}
        _predictionTitle = New Label With {.Dock = DockStyle.Top, .Height = 28, .Font = New Font("Century Gothic", 9.0F, FontStyle.Bold), .ForeColor = Color.FromArgb(34, 90, 74)}
        _predictionGrid = New DataGridView With {.Dock = DockStyle.Fill, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False, .AllowUserToOrderColumns = False, .RowHeadersVisible = False, .AutoSizeColumnsMode = dgvGradebook.AutoSizeColumnsMode, .BackgroundColor = Color.White, .BorderStyle = BorderStyle.None, .GridColor = Color.FromArgb(224, 230, 227), .SelectionMode = DataGridViewSelectionMode.CellSelect, .EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2, .EnableHeadersVisualStyles = False}
        _predictionGrid.ColumnHeadersDefaultCellStyle = dgvGradebook.ColumnHeadersDefaultCellStyle.Clone()
        _predictionGrid.DefaultCellStyle = dgvGradebook.DefaultCellStyle.Clone()
        _predictionGrid.ColumnHeadersHeight = dgvGradebook.ColumnHeadersHeight
        _predictionGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(242, 247, 244)
        _predictionPanel.Controls.Add(_predictionGrid)
        _predictionPanel.Controls.Add(_predictionTitle)
        tblMainRubric.RowCount = 3
        tblMainRubric.RowStyles.Clear()
        tblMainRubric.RowStyles.Add(New RowStyle(SizeType.Absolute, 125.0F))
        tblMainRubric.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tblMainRubric.RowStyles.Add(New RowStyle(SizeType.Absolute, 0.0F))
        tblMainRubric.Controls.Add(_predictionPanel, 0, 2)
        AddHandler _predictionGrid.CellEndEdit, AddressOf dgvGradebook_CellEndEdit
    End Sub

    Private Sub RefreshPredictionGrid()
        If _predictionGrid Is Nothing Then Return
        _predictionTitle.Text = "PREDICTIVE COPY — " & _period.ToUpperInvariant() & " · edits here are temporary and never change published grades"
        _predictionGrid.Columns.Clear()
        _predictionGrid.Rows.Clear()
        For Each sourceColumn As DataGridViewColumn In dgvGradebook.Columns
            Dim col As DataGridViewColumn = DirectCast(sourceColumn.Clone(), DataGridViewColumn)
            Dim info As ColInfo = GetInfo(sourceColumn)
            col.Tag = sourceColumn.Tag
            col.ReadOnly = (info.Kind <> ColKind.Raw)
            _predictionGrid.Columns.Add(col)
        Next
        For Each sourceRow As DataGridViewRow In dgvGradebook.Rows
            If sourceRow.IsNewRow Then Continue For
            Dim index As Integer = _predictionGrid.Rows.Add()
            For i As Integer = 0 To sourceRow.Cells.Count - 1
                _predictionGrid.Rows(index).Cells(i).Value = sourceRow.Cells(i).Value
            Next
            _predictionGrid.Rows(index).Tag = sourceRow.Tag
        Next
        RecalculatePredictionAll()
    End Sub

    Private Sub RecalculatePredictionAll()
        If _predictionGrid Is Nothing Then Return
        For Each row As DataGridViewRow In _predictionGrid.Rows
            RecalculatePredictionRow(row)
        Next
    End Sub

    Private Sub RecalculatePredictionRow(row As DataGridViewRow)
        If row Is Nothing OrElse row.IsNewRow Then Return
        Dim sums(4) As Decimal
        Dim counts(4) As Integer
        For Each col As DataGridViewColumn In _predictionGrid.Columns
            Dim info As ColInfo = GetInfo(col)
            If info.Kind <> ColKind.Raw Then Continue For
            Dim raw As Decimal
            Dim value As Object = row.Cells(col.Index).Value
            If value IsNot Nothing AndAlso Decimal.TryParse(value.ToString(), raw) Then
                row.Cells(col.Name & "_gra").Value = ToGrA(raw, info.MaxScore).ToString("0.00")
                sums(info.Category) += ToGrA(raw, info.MaxScore)
                counts(info.Category) += 1
            Else
                row.Cells(col.Name & "_gra").Value = ""
            End If
        Next
        Dim finalGrade As Decimal = 0D
        For cat As Integer = 1 To 4
            Dim name As String = "cat" & cat & "_total"
            If Not _predictionGrid.Columns.Contains(name) Then Continue For
            If counts(cat) > 0 Then
                Dim weighted As Decimal = GradeCalculator.CategoryTotal(sums(cat), counts(cat), CategoryWeight(cat))
                row.Cells(name).Value = weighted.ToString("0.00")
                finalGrade += weighted
            Else
                row.Cells(name).Value = ""
            End If
        Next
        row.Cells("grade").Value = If(Convert.ToString(row.Cells("status").Value).Equals("Dropped", StringComparison.OrdinalIgnoreCase), "DRP", finalGrade.ToString("0.00"))
    End Sub

    ' ==========================================
    ' PREDICTIVE MODE TOGGLE
    ' ==========================================
    Private Sub btnPredictive_Click(sender As Object, e As EventArgs) Handles btnPredictive.Click
        _isPredictiveMode = Not _isPredictiveMode

        If _isPredictiveMode Then
            btnPredictive.Text = "Exit predictive"
            btnPredictive.BackColor = Color.FromArgb(220, 53, 69)
            btnPredictive.ForeColor = Color.White
            btnPredictive.TextAlign = ContentAlignment.MiddleCenter
            btnPredictive.Padding = Padding.Empty
            btnPredictive.Invalidate()
            btnPredictive.TextAlign = ContentAlignment.MiddleCenter
            RefreshPredictionGrid()
            Dim usedHeight As Integer = dgvGradebook.ColumnHeadersHeight + dgvGradebook.Rows.Cast(Of DataGridViewRow)().Where(Function(r) Not r.IsNewRow).Sum(Function(r) r.Height) + 4
            Dim maxTopHeight As Integer = Math.Max(180, (tblMainRubric.ClientSize.Height - 125) \ 2)
            tblMainRubric.RowStyles(1).SizeType = SizeType.Absolute
            tblMainRubric.RowStyles(1).Height = Math.Min(usedHeight, maxTopHeight)
            tblMainRubric.RowStyles(2).SizeType = SizeType.Percent
            tblMainRubric.RowStyles(2).Height = 50.0F
            _predictionPanel.Visible = True
            _predictionGrid.ReadOnly = False
            dgvGradebook.ReadOnly = True
        Else
            btnPredictive.Text = "Predictive Mode"
            btnPredictive.BackColor = Color.FromArgb(241, 196, 15)
            btnPredictive.ForeColor = Color.Black
            btnPredictive.Invalidate()
            LoadStudentGrades(quiet:=True)
            _predictionPanel.Visible = False
            tblMainRubric.RowStyles(1).SizeType = SizeType.Percent
            tblMainRubric.RowStyles(1).Height = 100.0F
            tblMainRubric.RowStyles(2).SizeType = SizeType.Absolute
            tblMainRubric.RowStyles(2).Height = 0.0F
            dgvGradebook.ReadOnly = True
        End If
    End Sub

End Class
