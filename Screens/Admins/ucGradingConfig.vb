Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports MySqlConnector

' =============================================================================
' Semester / Grading Settings
'
' Workflow (each step unlocks the next):
'   1. SCHOOL YEAR   start and end of the whole school year      -> school_year_calendar
'   2. SEMESTERS     start/end of the 1st and 2nd semester       -> academic_term
'                    (the semester break is the gap between them, computed, never typed)
'   3. GRADE ENCODING encoding opens automatically on the semester start date;
'                    the admin only sets the deadline (max. 1 month after the semester ends)
'
' Controls used from the designer:
'   cboSchoolYear, lblSYStartValue, lblSYEndValue, btnNewSchoolYear, btnEditSchoolYear
'   Label12 (1st semester value), lblSem2Value, lblBreakValue, btnEditSemesters
'   cboEncodingTerm, blEncodingStartValue, lblDeadlineValue, lblLockStatusValue,
'   lblActiveTermValue, btnEditEncoding, btnSetActive
'   RoundedPanel2 (grading scale card) and EditGradingScale
' The grading scale table itself is created in code (BuildGradeScaleTable).
' =============================================================================
Public Class ucGradingConfig

    '========================================
    ' STATE
    '========================================

    Friend NotInheritable Class TermInfo
        Public TermId As Integer
        Public Semester As Integer
        Public StartDate As DateTime
        Public EndDate As DateTime
        Public EncodingStart As DateTime
        Public Deadline As DateTime
        Public IsActive As Boolean
        Public IsLocked As Boolean
    End Class

    Private _loading As Boolean = False
    Private _syStart As DateTime? = Nothing
    Private _syEnd As DateTime? = Nothing
    Private _term1 As TermInfo = Nothing
    Private _term2 As TermInfo = Nothing

    Private Sub ucGradingConfig_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If DesignMode Then Return

        BuildGradeScaleTable()
        LoadGradeScale()

        ' Only admin (the dean) can see the Edit button for the grading scale.
        EditGradingScale.Visible = SessionManager.AuthorizeRole("admin")

        _loading = True
        cboSchoolYear.DropDownStyle = ComboBoxStyle.DropDownList
        cboEncodingTerm.DropDownStyle = ComboBoxStyle.DropDownList
        cboEncodingTerm.Items.Clear()
        cboEncodingTerm.Items.Add("1st Semester")
        cboEncodingTerm.Items.Add("2nd Semester")
        cboEncodingTerm.SelectedIndex = 0
        _loading = False

        ReloadSchoolYears(Nothing)
    End Sub

    '========================================
    ' SMALL HELPERS
    '========================================

    Private ReadOnly Property SelectedSchoolYear As String
        Get
            If cboSchoolYear.SelectedItem Is Nothing Then Return ""
            Return cboSchoolYear.SelectedItem.ToString()
        End Get
    End Property

    Private Function SelectedTerm() As TermInfo
        Return If(cboEncodingTerm.SelectedIndex = 1, _term2, _term1)
    End Function

    Private Shared Function SemesterName(sem As Integer) As String
        Return If(sem = 1, "1st Semester", "2nd Semester")
    End Function

    Private Shared Function TermRangeText(t As TermInfo) As String
        If t Is Nothing Then Return "Not Set"
        Return t.StartDate.ToString("MM/dd/yyyy") & " - " & t.EndDate.ToString("MM/dd/yyyy")
    End Function

    ' The semester break = the gap between the end of semester 1 and the start of semester 2
    Private Shared Function BreakText(sem1End As DateTime, sem2Start As DateTime) As String
        Dim days As Integer = CInt((sem2Start.Date - sem1End.Date).TotalDays) - 1
        If days <= 0 Then Return "None"
        Return sem1End.Date.AddDays(1).ToString("MM/dd/yyyy") & " - " &
               sem2Start.Date.AddDays(-1).ToString("MM/dd/yyyy") &
               " (" & days.ToString() & If(days = 1, " day)", " days)")
    End Function

    Private Shared Function DefaultDeadline(semEnd As DateTime) As DateTime
        Return semEnd.Date.AddDays(14).AddHours(23).AddMinutes(59)
    End Function

    Private Shared Function MaxDeadline(semEnd As DateTime) As DateTime
        ' Same rule as the database trigger: end of the day, 1 month after the semester ends
        Return semEnd.Date.AddMonths(1).AddDays(1).AddSeconds(-1)
    End Function

    Private Shared Function MakeLabel(text As String, x As Integer, y As Integer) As Label
        Return New Label() With {.Text = text, .AutoSize = True, .Location = New Point(x, y)}
    End Function

    Private Shared Function MakeButton(text As String, x As Integer, y As Integer, primary As Boolean) As Button
        Dim b As New Button() With {
            .Text = text,
            .Size = New Size(90, 32),
            .Location = New Point(x, y),
            .FlatStyle = FlatStyle.Flat,
            .ForeColor = Color.White,
            .BackColor = If(primary, Color.FromArgb(30, 86, 62), Color.Gray),
            .DialogResult = If(primary, DialogResult.OK, DialogResult.Cancel)
        }
        b.FlatAppearance.BorderSize = 0
        Return b
    End Function

    Private Shared Sub ExecTx(conn As System.Data.Common.DbConnection,
                              tx As System.Data.Common.DbTransaction,
                              sql As String,
                              ParamArray ps() As MySqlParameter)
        Using cmd = conn.CreateCommand()
            cmd.Transaction = tx
            cmd.CommandText = sql
            For Each p As MySqlParameter In ps
                cmd.Parameters.Add(p)
            Next
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    '========================================
    ' LOADING
    '========================================

    ' Fills the school year dropdown; selects selectYear, else the active term's year, else the newest
    Private Sub ReloadSchoolYears(selectYear As String)
        _loading = True
        Try
            cboSchoolYear.Items.Clear()
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT school_year FROM school_year_calendar ORDER BY school_year DESC;")
            If dt IsNot Nothing Then
                For Each r As DataRow In dt.Rows
                    cboSchoolYear.Items.Add(r(0).ToString())
                Next
            End If

            If String.IsNullOrEmpty(selectYear) Then
                Dim dtA As DataTable = DatabaseHelper.ExecuteQuery(
                    "SELECT school_year FROM academic_term WHERE is_active = 1 LIMIT 1;")
                If dtA IsNot Nothing AndAlso dtA.Rows.Count > 0 Then selectYear = dtA.Rows(0)(0).ToString()
            End If

            Dim idx As Integer = -1
            If Not String.IsNullOrEmpty(selectYear) Then idx = cboSchoolYear.Items.IndexOf(selectYear)
            If idx < 0 AndAlso cboSchoolYear.Items.Count > 0 Then idx = 0
            cboSchoolYear.SelectedIndex = idx
        Finally
            _loading = False
        End Try

        LoadSelectedSchoolYear()
    End Sub

    Private Sub cboSchoolYear_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSchoolYear.SelectedIndexChanged
        If _loading Then Return
        LoadSelectedSchoolYear()
    End Sub

    Private Sub cboEncodingTerm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboEncodingTerm.SelectedIndexChanged
        If _loading Then Return
        RefreshUI()
    End Sub

    ' Reads the selected school year and its semesters from the database
    Private Sub LoadSelectedSchoolYear()
        _syStart = Nothing
        _syEnd = Nothing
        _term1 = Nothing
        _term2 = Nothing

        Dim sy As String = SelectedSchoolYear
        If sy <> "" Then
            Dim dtSY As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT start_date, end_date FROM school_year_calendar WHERE school_year = @sy;",
                {New MySqlParameter("@sy", sy)})
            If dtSY IsNot Nothing AndAlso dtSY.Rows.Count > 0 Then
                _syStart = Convert.ToDateTime(dtSY.Rows(0)("start_date"))
                _syEnd = Convert.ToDateTime(dtSY.Rows(0)("end_date"))
            End If

            Dim dtT As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT term_id, semester, start_date, end_date, encoding_start, encoding_deadline, is_active, is_locked " &
                "FROM academic_term WHERE school_year = @sy ORDER BY semester;",
                {New MySqlParameter("@sy", sy)})
            If dtT IsNot Nothing Then
                For Each r As DataRow In dtT.Rows
                    Dim t As New TermInfo()
                    t.TermId = Convert.ToInt32(r("term_id"))
                    t.Semester = Convert.ToInt32(r("semester"))
                    t.StartDate = Convert.ToDateTime(r("start_date"))
                    t.EndDate = Convert.ToDateTime(r("end_date"))
                    t.EncodingStart = Convert.ToDateTime(r("encoding_start"))
                    t.Deadline = Convert.ToDateTime(r("encoding_deadline"))
                    t.IsActive = Convert.ToBoolean(r("is_active"))
                    t.IsLocked = Convert.ToBoolean(r("is_locked"))
                    If t.Semester = 1 Then _term1 = t
                    If t.Semester = 2 Then _term2 = t
                Next
            End If
        End If

        RefreshUI()
    End Sub

    '========================================
    ' UI REFRESH & WORKFLOW GATES
    '========================================

    Private Sub RefreshUI()
        ' ---- Step 1: school year ----
        Dim hasSY As Boolean = _syStart.HasValue AndAlso _syEnd.HasValue
        lblSYStartValue.Text = If(hasSY, _syStart.Value.ToString("MM/dd/yyyy"), "Not Set")
        lblSYEndValue.Text = If(hasSY, _syEnd.Value.ToString("MM/dd/yyyy"), "Not Set")
        btnEditSchoolYear.Enabled = hasSY

        ' ---- Step 2: semesters (needs a school year) ----
        btnEditSemesters.Enabled = hasSY
        Label12.Text = TermRangeText(_term1)
        lblSem2Value.Text = TermRangeText(_term2)
        If _term1 IsNot Nothing AndAlso _term2 IsNot Nothing Then
            lblBreakValue.Text = BreakText(_term1.EndDate, _term2.StartDate)
        Else
            lblBreakValue.Text = "Set both semesters to compute"
        End If

        ' ---- Step 3: grade encoding of the selected semester (needs that semester) ----
        Dim t As TermInfo = SelectedTerm()
        If t Is Nothing Then
            blEncodingStartValue.Text = "Not Set"
            lblDeadlineValue.Text = "Not Set"
            lblLockStatusValue.Text = "-"
            lblLockStatusValue.ForeColor = Color.Gray
            btnSetActive.Text = "Set as Active Term"
        Else
            blEncodingStartValue.Text = t.EncodingStart.ToString("MM/dd/yyyy hh:mm tt")
            lblDeadlineValue.Text = t.Deadline.ToString("MM/dd/yyyy hh:mm tt")
            ShowEncodingStatus(t)
            btnSetActive.Text = If(t.IsActive, "Active Term", "Set as Active Term")
        End If
        btnEditEncoding.Enabled = (t IsNot Nothing)
        btnSetActive.Enabled = (t IsNot Nothing AndAlso Not t.IsActive)

        ' ---- Active term (global: classes are created under it) ----
        Dim dtA As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT school_year, semester FROM academic_term WHERE is_active = 1 LIMIT 1;")
        If dtA IsNot Nothing AndAlso dtA.Rows.Count > 0 Then
            lblActiveTermValue.Text = dtA.Rows(0)("school_year").ToString() & " - " &
                                      SemesterName(Convert.ToInt32(dtA.Rows(0)("semester")))
        Else
            lblActiveTermValue.Text = "None"
        End If
    End Sub

    Private Sub ShowEncodingStatus(t As TermInfo)
        Dim current As DateTime = DateTime.Now
        If t.IsLocked Then
            lblLockStatusValue.Text = "LOCKED"
            lblLockStatusValue.ForeColor = Color.Red
        ElseIf current < t.EncodingStart Then
            lblLockStatusValue.Text = "NOT YET OPEN"
            lblLockStatusValue.ForeColor = Color.DarkOrange
        ElseIf current > t.Deadline Then
            lblLockStatusValue.Text = "CLOSED"
            lblLockStatusValue.ForeColor = Color.Red
        Else
            lblLockStatusValue.Text = "OPEN"
            lblLockStatusValue.ForeColor = Color.Green
        End If
    End Sub

    '========================================
    ' STEP 1: SCHOOL YEAR
    '========================================

    Private Sub btnNewSchoolYear_Click(sender As Object, e As EventArgs) Handles btnNewSchoolYear.Click
        Dim existing As New List(Of String)
        For Each o As Object In cboSchoolYear.Items
            existing.Add(o.ToString())
        Next

        ' Suggest the year after the newest one on file
        Dim suggested As Integer = DateTime.Today.Year
        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT MAX(CAST(SUBSTRING(school_year, 1, 4) AS UNSIGNED)) FROM school_year_calendar;")
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso Not IsDBNull(dt.Rows(0)(0)) Then
            suggested = Convert.ToInt32(dt.Rows(0)(0)) + 1
        End If

        Using dlg As New SchoolYearDialog(True, suggested, Nothing, Nothing, existing, Nothing, Nothing)
            If dlg.ShowDialog(Me.FindForm()) <> DialogResult.OK Then Return

            Try
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO school_year_calendar (school_year, start_date, end_date) VALUES (@sy, @s, @e);",
                    {New MySqlParameter("@sy", dlg.SchoolYear),
                     New MySqlParameter("@s", dlg.StartDate),
                     New MySqlParameter("@e", dlg.EndDate)})
            Catch ex As Exception
                MessageBox.Show("Could not create the school year: " & ex.Message, "Database Error", MessageBoxButtons.OK)
                Return
            End Try

            ReloadSchoolYears(dlg.SchoolYear)

            If MessageBox.Show("School year " & dlg.SchoolYear & " was created." & vbCrLf & vbCrLf &
                               "Next step: set up its 1st and 2nd semester. Do it now?",
                               "Next Step", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                btnEditSemesters_Click(Me, EventArgs.Empty)
            End If
        End Using
    End Sub

    Private Sub btnEditSchoolYear_Click(sender As Object, e As EventArgs) Handles btnEditSchoolYear.Click
        If Not _syStart.HasValue OrElse Not _syEnd.HasValue Then Return

        ' The school year must keep covering the semesters that are already set up
        Dim minCover As DateTime? = Nothing
        Dim maxCover As DateTime? = Nothing
        For Each t As TermInfo In New TermInfo() {_term1, _term2}
            If t Is Nothing Then Continue For
            If Not minCover.HasValue OrElse t.StartDate < minCover.Value Then minCover = t.StartDate
            If Not maxCover.HasValue OrElse t.EndDate > maxCover.Value Then maxCover = t.EndDate
        Next

        Dim sy As String = SelectedSchoolYear
        Dim startYear As Integer = Integer.Parse(sy.Substring(0, 4))

        Using dlg As New SchoolYearDialog(False, startYear, _syStart, _syEnd, New List(Of String)(), minCover, maxCover)
            If dlg.ShowDialog(Me.FindForm()) <> DialogResult.OK Then Return

            Try
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE school_year_calendar SET start_date = @s, end_date = @e WHERE school_year = @sy;",
                    {New MySqlParameter("@s", dlg.StartDate),
                     New MySqlParameter("@e", dlg.EndDate),
                     New MySqlParameter("@sy", sy)})
            Catch ex As Exception
                MessageBox.Show("Could not update the school year: " & ex.Message, "Database Error", MessageBoxButtons.OK)
                Return
            End Try

            LoadSelectedSchoolYear()
            MessageBox.Show("School year " & sy & " updated.", "Success", MessageBoxButtons.OK)
        End Using
    End Sub

    '========================================
    ' STEP 2: SEMESTERS (the break is computed from the gap)
    '========================================

    Private Sub btnEditSemesters_Click(sender As Object, e As EventArgs) Handles btnEditSemesters.Click
        If Not _syStart.HasValue OrElse Not _syEnd.HasValue Then
            MessageBox.Show("Set up a school year first.", "School Year Required", MessageBoxButtons.OK)
            Return
        End If

        Dim sy As String = SelectedSchoolYear

        Using dlg As New SemesterDialog(sy, _syStart.Value, _syEnd.Value, _term1, _term2)
            If dlg.ShowDialog(Me.FindForm()) <> DialogResult.OK Then Return

            Dim notes As New List(Of String)
            Try
                Using conn = DatabaseHelper.GetConnection()
                    conn.Open()
                    Using tx = conn.BeginTransaction()
                        SaveTerm(conn, tx, sy, 1, dlg.Sem1Start, dlg.Sem1End, _term1, notes)
                        SaveTerm(conn, tx, sy, 2, dlg.Sem2Start, dlg.Sem2End, _term2, notes)
                        tx.Commit()
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Could not save the semesters: " & ex.Message, "Database Error", MessageBoxButtons.OK)
                Return
            End Try

            EnsureActiveTerm()
            LoadSelectedSchoolYear()

            Dim msg As String = "Semesters saved for " & sy & "." & vbCrLf & vbCrLf &
                                "Semester break: " & BreakText(dlg.Sem1End, dlg.Sem2Start) & vbCrLf &
                                "Grade encoding opens automatically on each semester's start date."
            If notes.Count > 0 Then
                msg &= vbCrLf & vbCrLf & String.Join(vbCrLf, notes)
            End If
            MessageBox.Show(msg, "Success", MessageBoxButtons.OK)
        End Using
    End Sub

    ' Inserts or updates one semester. Encoding opens on the semester start; an existing deadline is
    ' kept when it is still valid for the new dates, otherwise it is reset (and the admin is told).
    Private Sub SaveTerm(conn As System.Data.Common.DbConnection,
                         tx As System.Data.Common.DbTransaction,
                         sy As String, sem As Integer,
                         startD As DateTime, endD As DateTime,
                         existing As TermInfo, notes As List(Of String))
        Dim encStart As DateTime = startD.Date
        Dim deadline As DateTime

        If existing IsNot Nothing AndAlso existing.Deadline > encStart AndAlso existing.Deadline <= MaxDeadline(endD) Then
            deadline = existing.Deadline
        Else
            deadline = DefaultDeadline(endD)
            If existing IsNot Nothing Then
                notes.Add("The " & SemesterName(sem) & " encoding deadline no longer fit the new dates and was reset to " &
                          deadline.ToString("MM/dd/yyyy hh:mm tt") & ".")
            End If
        End If

        ExecTx(conn, tx,
               "INSERT INTO academic_term (school_year, semester, start_date, end_date, encoding_start, encoding_deadline) " &
               "VALUES (@sy, @sem, @s, @e, @es, @d) " &
               "ON DUPLICATE KEY UPDATE start_date = VALUES(start_date), end_date = VALUES(end_date), " &
               "encoding_start = VALUES(encoding_start), encoding_deadline = VALUES(encoding_deadline);",
               New MySqlParameter("@sy", sy),
               New MySqlParameter("@sem", sem),
               New MySqlParameter("@s", startD.Date),
               New MySqlParameter("@e", endD.Date),
               New MySqlParameter("@es", encStart),
               New MySqlParameter("@d", deadline))
    End Sub

    ' The system needs exactly one active term. If none is active yet, pick the current / next semester.
    Private Sub EnsureActiveTerm()
        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery("SELECT COUNT(*) FROM academic_term WHERE is_active = 1;")
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso Convert.ToInt32(dt.Rows(0)(0)) > 0 Then Return

            Dim dtPick As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT term_id FROM academic_term " &
                "ORDER BY (end_date >= CURDATE()) DESC, " &
                "         CASE WHEN end_date >= CURDATE() THEN start_date END ASC, " &
                "         start_date DESC LIMIT 1;")
            If dtPick IsNot Nothing AndAlso dtPick.Rows.Count > 0 Then
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE academic_term SET is_active = 1 WHERE term_id = @id;",
                    {New MySqlParameter("@id", Convert.ToInt32(dtPick.Rows(0)(0)))})
            End If
        Catch ex As Exception
            ' Not fatal: the admin can still press "Set as Active Term"
        End Try
    End Sub

    '========================================
    ' STEP 3: GRADE ENCODING (opens on semester start; admin sets the deadline)
    '========================================

    Private Sub btnEditEncoding_Click(sender As Object, e As EventArgs) Handles btnEditEncoding.Click
        Dim t As TermInfo = SelectedTerm()
        If t Is Nothing Then
            MessageBox.Show("Set up this semester's schedule first (Step 2) before setting its grade encoding deadline.",
                            "Semester Required", MessageBoxButtons.OK)
            Return
        End If

        Using dlg As New DeadlineDialog(t, SemesterName(t.Semester))
            If dlg.ShowDialog(Me.FindForm()) <> DialogResult.OK Then Return

            Try
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE academic_term SET encoding_deadline = @d, is_locked = @l WHERE term_id = @id;",
                    {New MySqlParameter("@d", dlg.NewDeadline),
                     New MySqlParameter("@l", If(dlg.IsLocked, 1, 0)),
                     New MySqlParameter("@id", t.TermId)})
            Catch ex As Exception
                MessageBox.Show("Could not save the encoding schedule: " & ex.Message, "Database Error", MessageBoxButtons.OK)
                Return
            End Try

            LoadSelectedSchoolYear()
            MessageBox.Show("Grade encoding schedule updated.", "Success", MessageBoxButtons.OK)
        End Using
    End Sub

    Private Sub btnSetActive_Click(sender As Object, e As EventArgs) Handles btnSetActive.Click
        Dim t As TermInfo = SelectedTerm()
        If t Is Nothing OrElse t.IsActive Then Return

        If MessageBox.Show("Make " & SelectedSchoolYear & " - " & SemesterName(t.Semester) & " the active term?" & vbCrLf & vbCrLf &
                           "New classes are created under the active term.",
                           "Set Active Term", MessageBoxButtons.YesNo) <> DialogResult.Yes Then
            Return
        End If

        Try
            Using conn = DatabaseHelper.GetConnection()
                conn.Open()
                Using tx = conn.BeginTransaction()
                    ' Only one term may be active (unique key), so switch the old one off first
                    ExecTx(conn, tx, "UPDATE academic_term SET is_active = 0 WHERE is_active = 1;")
                    ExecTx(conn, tx, "UPDATE academic_term SET is_active = 1 WHERE term_id = @id;",
                           New MySqlParameter("@id", t.TermId))
                    tx.Commit()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Could not change the active term: " & ex.Message, "Database Error", MessageBoxButtons.OK)
            Return
        End Try

        LoadSelectedSchoolYear()
    End Sub

    '========================================
    ' GRADING SCALE AND GWA (DISPLAY)
    '========================================

    ' The grading scale table is built here in code, inside the grading scale card (RoundedPanel2).
    ' Nothing to drag onto the designer; its rows come from the grade_scale table (see LoadGradeScale).
    Private dgvGradingScale As DataGridView
    Private lblScaleTitle As Label

    Private Sub BuildGradeScaleTable()
        If dgvGradingScale IsNot Nothing Then Return

        lblScaleTitle = New Label() With {
            .Text = "GRADING SCALE AND GWA",
            .AutoSize = True,
            .Font = New Font("Century Gothic", 12.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(34, 90, 74),
            .BackColor = Color.Transparent,
            .Location = New Point(30, 20)
        }

        dgvGradingScale = New DataGridView() With {
            .Name = "dgvGradingScale",
            .Location = New Point(30, 62),
            .Size = New Size(1100, 400),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left,
            .TabStop = False,
            .Font = New Font("Century Gothic", 10.2F, FontStyle.Regular)
        }

        With dgvGradingScale
            .Columns.Add("colRange", "Percentage Range")
            .Columns.Add("colGwa", "GWA")
            .Columns("colRange").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            .Columns("colGwa").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            .ReadOnly = True
            .AllowUserToAddRows = False
            .AllowUserToDeleteRows = False
            .RowHeadersVisible = False
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect
            .MultiSelect = False
            .AllowUserToResizeRows = False
            .AllowUserToResizeColumns = False
            .BorderStyle = BorderStyle.None
            .BackgroundColor = Color.White
            .GridColor = Color.Gainsboro
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            .RowTemplate.Height = 30
            .ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            .ColumnHeadersHeight = 34

            ' Selection looks identical to a normal row (no blue)
            .DefaultCellStyle.BackColor = Color.White
            .DefaultCellStyle.ForeColor = Color.Black
            .DefaultCellStyle.SelectionBackColor = Color.White
            .DefaultCellStyle.SelectionForeColor = Color.Black
            .EnableHeadersVisualStyles = False
            .ColumnHeadersDefaultCellStyle.BackColor = Color.White
            .ColumnHeadersDefaultCellStyle.ForeColor = Color.Black
            .ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White
            .ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black
        End With

        RoundedPanel2.Controls.Add(lblScaleTitle)
        RoundedPanel2.Controls.Add(dgvGradingScale)
    End Sub

    Private Sub LoadGradeScale()
        dgvGradingScale.Rows.Clear()

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT min_percent, max_percent, grade_equivalent " &
            "FROM grade_scale ORDER BY min_percent DESC")

        If dt Is Nothing Then Return

        For Each r As DataRow In dt.Rows
            Dim range As String = $"{Convert.ToDecimal(r("min_percent")):0.00} - {Convert.ToDecimal(r("max_percent")):0.00}"
            Dim gwa As String = Convert.ToDecimal(r("grade_equivalent")).ToString("0.00")
            dgvGradingScale.Rows.Add(range, gwa)
        Next

        dgvGradingScale.ClearSelection()
    End Sub

    '========================================
    ' EDIT GRADING SCALE (DEAN ONLY)
    '========================================

    Private Sub EditGradingScale_Click(sender As Object, e As EventArgs) Handles EditGradingScale.Click
        If Not SessionManager.AuthorizeRole("admin") Then
            MessageBox.Show("Only the Dean can edit the grading scale.", "Access Denied", MessageBoxButtons.OK)
            Return
        End If

        Using dlg As New GradeScaleDialog()
            If dlg.ShowDialog(Me.FindForm()) = DialogResult.OK Then
                LoadGradeScale()
                MessageBox.Show("Grading scale updated successfully.", "Success", MessageBoxButtons.OK)
            End If
        End Using
    End Sub

    '========================================
    ' NESTED SCHOOL YEAR DIALOG
    '========================================

    Private NotInheritable Class SchoolYearDialog
        Inherits Form

        Private ReadOnly _isNew As Boolean
        Private ReadOnly _existingYears As List(Of String)
        Private ReadOnly _minCover As DateTime?
        Private ReadOnly _maxCover As DateTime?

        Private ReadOnly nudStartYear As New NumericUpDown() With {.Width = 110, .Minimum = 2000, .Maximum = 2100}
        Private ReadOnly lblYearValue As New Label() With {.AutoSize = True, .Font = New Font("Century Gothic", 11, FontStyle.Bold)}
        Private ReadOnly dtpStart As New DateTimePicker() With {.Width = 310, .Format = DateTimePickerFormat.Short}
        Private ReadOnly dtpEnd As New DateTimePicker() With {.Width = 310, .Format = DateTimePickerFormat.Short}
        Private ReadOnly lblWarning As New Label() With {.Width = 310, .Height = 50, .Font = New Font("Century Gothic", 8, FontStyle.Regular)}
        Private ReadOnly btnOk As Button
        Private ReadOnly btnCancel As Button

        Public ReadOnly Property SchoolYear As String
            Get
                Dim y As Integer = CInt(nudStartYear.Value)
                Return y.ToString() & "-" & (y + 1).ToString()
            End Get
        End Property

        Public ReadOnly Property StartDate As DateTime
            Get
                Return dtpStart.Value.Date
            End Get
        End Property

        Public ReadOnly Property EndDate As DateTime
            Get
                Return dtpEnd.Value.Date
            End Get
        End Property

        Public Sub New(isNew As Boolean, startYear As Integer, startDt As DateTime?, endDt As DateTime?,
                       existingYears As List(Of String), minCover As DateTime?, maxCover As DateTime?)
            Text = If(isNew, "New School Year", "Edit School Year")
            Font = New Font("Century Gothic", 9, FontStyle.Regular)
            Size = New Size(370, 350)
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False

            _isNew = isNew
            _existingYears = existingYears
            _minCover = minCover
            _maxCover = maxCover

            btnOk = MakeButton("Save", 130, 265, True)
            btnCancel = MakeButton("Cancel", 230, 265, False)

            Dim pnl As New Panel() With {.Dock = DockStyle.Fill}

            pnl.Controls.Add(MakeLabel("School year starting in:", 20, 15))
            nudStartYear.Location = New Point(20, 37)
            nudStartYear.Value = Math.Max(2000, Math.Min(2100, startYear))
            nudStartYear.Enabled = isNew
            lblYearValue.Location = New Point(150, 38)
            pnl.Controls.Add(nudStartYear)
            pnl.Controls.Add(lblYearValue)

            pnl.Controls.Add(MakeLabel("School Year Start:", 20, 78))
            dtpStart.Location = New Point(20, 98)
            dtpStart.Value = If(startDt.HasValue, startDt.Value, New DateTime(startYear, 8, 1))
            pnl.Controls.Add(dtpStart)

            pnl.Controls.Add(MakeLabel("School Year End:", 20, 138))
            dtpEnd.Location = New Point(20, 158)
            dtpEnd.Value = If(endDt.HasValue, endDt.Value, New DateTime(startYear + 1, 5, 31))
            pnl.Controls.Add(dtpEnd)

            lblWarning.Location = New Point(20, 195)
            pnl.Controls.Add(lblWarning)

            pnl.Controls.Add(btnOk)
            pnl.Controls.Add(btnCancel)
            Controls.Add(pnl)
            AcceptButton = btnOk
            CancelButton = btnCancel

            AddHandler nudStartYear.ValueChanged, AddressOf OnYearChanged
            AddHandler dtpStart.ValueChanged, AddressOf OnAnyChanged
            AddHandler dtpEnd.ValueChanged, AddressOf OnAnyChanged

            UpdateYearLabel()
            ValidateInputs()
        End Sub

        Private Sub UpdateYearLabel()
            lblYearValue.Text = "= " & SchoolYear
        End Sub

        Private Sub OnYearChanged(sender As Object, e As EventArgs)
            ' A new school year restarts the date suggestions; the admin can still change them
            Dim y As Integer = CInt(nudStartYear.Value)
            dtpStart.Value = New DateTime(y, 8, 1)
            dtpEnd.Value = New DateTime(y + 1, 5, 31)
            UpdateYearLabel()
            ValidateInputs()
        End Sub

        Private Sub OnAnyChanged(sender As Object, e As EventArgs)
            ValidateInputs()
        End Sub

        Private Sub ValidateInputs()
            Dim y As Integer = CInt(nudStartYear.Value)
            Dim sDate As DateTime = dtpStart.Value.Date
            Dim eDate As DateTime = dtpEnd.Value.Date
            Dim msg As String = ""

            If _isNew AndAlso _existingYears.Contains(SchoolYear) Then
                msg = "School year " & SchoolYear & " already exists."
            ElseIf sDate.Year <> y Then
                msg = "The start date must be in " & y.ToString() & " (a " & SchoolYear & " school year starts in " & y.ToString() & ")."
            ElseIf eDate.Year <> y + 1 Then
                msg = "The end date must be in " & (y + 1).ToString() & "."
            ElseIf eDate <= sDate Then
                msg = "The end date must be after the start date."
            ElseIf _minCover.HasValue AndAlso sDate > _minCover.Value Then
                msg = "A semester already starts on " & _minCover.Value.ToString("MM/dd/yyyy") & ". The school year cannot start after it."
            ElseIf _maxCover.HasValue AndAlso eDate < _maxCover.Value Then
                msg = "A semester already ends on " & _maxCover.Value.ToString("MM/dd/yyyy") & ". The school year cannot end before it."
            End If

            If msg = "" Then
                lblWarning.ForeColor = Color.Green
                lblWarning.Text = "Valid school year. Next step: set up its semesters."
                btnOk.Enabled = True
            Else
                lblWarning.ForeColor = Color.Red
                lblWarning.Text = msg
                btnOk.Enabled = False
            End If
        End Sub
    End Class

    '========================================
    ' NESTED SEMESTERS DIALOG (semester break = the gap, shown live)
    '========================================

    Private NotInheritable Class SemesterDialog
        Inherits Form

        Private ReadOnly dtpS1Start As New DateTimePicker() With {.Width = 200, .Format = DateTimePickerFormat.Short}
        Private ReadOnly dtpS1End As New DateTimePicker() With {.Width = 200, .Format = DateTimePickerFormat.Short}
        Private ReadOnly dtpS2Start As New DateTimePicker() With {.Width = 200, .Format = DateTimePickerFormat.Short}
        Private ReadOnly dtpS2End As New DateTimePicker() With {.Width = 200, .Format = DateTimePickerFormat.Short}
        Private ReadOnly lblBreak As New Label() With {.Width = 440, .Height = 34, .ForeColor = Color.DimGray}
        Private ReadOnly lblWarning As New Label() With {.Width = 440, .Height = 40, .Font = New Font("Century Gothic", 8, FontStyle.Regular)}
        Private ReadOnly btnOk As Button
        Private ReadOnly btnCancel As Button

        Public ReadOnly Property Sem1Start As DateTime
            Get
                Return dtpS1Start.Value.Date
            End Get
        End Property

        Public ReadOnly Property Sem1End As DateTime
            Get
                Return dtpS1End.Value.Date
            End Get
        End Property

        Public ReadOnly Property Sem2Start As DateTime
            Get
                Return dtpS2Start.Value.Date
            End Get
        End Property

        Public ReadOnly Property Sem2End As DateTime
            Get
                Return dtpS2End.Value.Date
            End Get
        End Property

        ' Value first (while the picker's range is still wide open), then restrict it to the school year
        Private Shared Sub InitPicker(dtp As DateTimePicker, value As DateTime, lo As DateTime, hi As DateTime)
            dtp.Value = If(value < lo, lo, If(value > hi, hi, value))
            dtp.MinDate = lo
            dtp.MaxDate = hi
        End Sub

        Public Sub New(sy As String, syStart As DateTime, syEnd As DateTime, t1 As TermInfo, t2 As TermInfo)
            Text = "Edit Semesters - " & sy
            Font = New Font("Century Gothic", 9, FontStyle.Regular)
            Size = New Size(500, 380)
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False

            btnOk = MakeButton("Save", 290, 295, True)
            btnCancel = MakeButton("Cancel", 390, 295, False)

            Dim s1s As DateTime = If(t1 IsNot Nothing, t1.StartDate, syStart)
            Dim s1e As DateTime = If(t1 IsNot Nothing, t1.EndDate, syStart.AddMonths(4).AddDays(-1))
            Dim s2s As DateTime = If(t2 IsNot Nothing, t2.StartDate, s1e.AddDays(30))
            Dim s2e As DateTime = If(t2 IsNot Nothing, t2.EndDate, syEnd)

            InitPicker(dtpS1Start, s1s, syStart, syEnd)
            InitPicker(dtpS1End, s1e, syStart, syEnd)
            InitPicker(dtpS2Start, s2s, syStart, syEnd)
            InitPicker(dtpS2End, s2e, syStart, syEnd)

            Dim pnl As New Panel() With {.Dock = DockStyle.Fill}

            Dim lblInfo As Label = MakeLabel("School year " & sy & ":  " & syStart.ToString("MM/dd/yyyy") & " - " & syEnd.ToString("MM/dd/yyyy"), 20, 12)
            lblInfo.ForeColor = Color.DimGray
            pnl.Controls.Add(lblInfo)

            Dim h1 As Label = MakeLabel("1st Semester", 20, 42)
            h1.Font = New Font("Century Gothic", 9, FontStyle.Bold)
            Dim h2 As Label = MakeLabel("2nd Semester", 250, 42)
            h2.Font = New Font("Century Gothic", 9, FontStyle.Bold)
            pnl.Controls.Add(h1)
            pnl.Controls.Add(h2)

            pnl.Controls.Add(MakeLabel("Start:", 20, 70))
            dtpS1Start.Location = New Point(20, 90)
            pnl.Controls.Add(dtpS1Start)
            pnl.Controls.Add(MakeLabel("End:", 20, 130))
            dtpS1End.Location = New Point(20, 150)
            pnl.Controls.Add(dtpS1End)

            pnl.Controls.Add(MakeLabel("Start:", 250, 70))
            dtpS2Start.Location = New Point(250, 90)
            pnl.Controls.Add(dtpS2Start)
            pnl.Controls.Add(MakeLabel("End:", 250, 130))
            dtpS2End.Location = New Point(250, 150)
            pnl.Controls.Add(dtpS2End)

            lblBreak.Location = New Point(20, 195)
            lblWarning.Location = New Point(20, 235)
            pnl.Controls.Add(lblBreak)
            pnl.Controls.Add(lblWarning)

            pnl.Controls.Add(btnOk)
            pnl.Controls.Add(btnCancel)
            Controls.Add(pnl)
            AcceptButton = btnOk
            CancelButton = btnCancel

            AddHandler dtpS1Start.ValueChanged, AddressOf OnAnyChanged
            AddHandler dtpS1End.ValueChanged, AddressOf OnAnyChanged
            AddHandler dtpS2Start.ValueChanged, AddressOf OnAnyChanged
            AddHandler dtpS2End.ValueChanged, AddressOf OnAnyChanged

            ValidateInputs()
        End Sub

        Private Sub OnAnyChanged(sender As Object, e As EventArgs)
            ValidateInputs()
        End Sub

        Private Sub ValidateInputs()
            Dim msg As String = ""

            If Sem1End <= Sem1Start Then
                msg = "The 1st semester must end after it starts."
            ElseIf Sem2Start <= Sem1End Then
                msg = "The 2nd semester must start after the 1st semester ends. The gap between them is the semester break."
            ElseIf Sem2End <= Sem2Start Then
                msg = "The 2nd semester must end after it starts."
            End If

            If Sem2Start > Sem1End Then
                lblBreak.Text = "Semester break (computed): " & BreakText(Sem1End, Sem2Start)
            Else
                lblBreak.Text = "Semester break (computed): -"
            End If

            If msg = "" Then
                lblWarning.ForeColor = Color.Green
                lblWarning.Text = "Valid. Grade encoding opens on each semester's start date."
                btnOk.Enabled = True
            Else
                lblWarning.ForeColor = Color.Red
                lblWarning.Text = msg
                btnOk.Enabled = False
            End If
        End Sub
    End Class

    '========================================
    ' NESTED DEADLINE DIALOG
    '========================================

    Private NotInheritable Class DeadlineDialog
        Inherits Form

        Private ReadOnly dtpDeadline As New DateTimePicker() With {.Width = 310, .Format = DateTimePickerFormat.Custom, .CustomFormat = "MM/dd/yyyy hh:mm tt"}
        Private ReadOnly chkLockSystem As New CheckBox() With {.Text = "Force Lock (close encoding for this semester now)", .AutoSize = True}
        Private ReadOnly lblOpens As New Label() With {.Width = 310, .Height = 36, .ForeColor = Color.DimGray}
        Private ReadOnly lblWarning As New Label() With {.Width = 310, .Height = 45, .Font = New Font("Century Gothic", 8, FontStyle.Regular)}
        Private ReadOnly btnOk As Button
        Private ReadOnly btnCancel As Button

        Private ReadOnly encStart As DateTime
        Private ReadOnly termEnd As DateTime
        Private ReadOnly maxAllowed As DateTime

        Public ReadOnly Property NewDeadline As DateTime
            Get
                Dim v As DateTime = dtpDeadline.Value
                Return New DateTime(v.Year, v.Month, v.Day, v.Hour, v.Minute, 0)
            End Get
        End Property

        Public ReadOnly Property IsLocked As Boolean
            Get
                Return chkLockSystem.Checked
            End Get
        End Property

        Public Sub New(t As TermInfo, semName As String)
            Text = "Grade Encoding - " & semName
            Font = New Font("Century Gothic", 9, FontStyle.Regular)
            Size = New Size(370, 330)
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False

            encStart = t.EncodingStart
            termEnd = t.EndDate.Date
            maxAllowed = MaxDeadline(t.EndDate)

            btnOk = MakeButton("Save", 130, 225, True)
            btnCancel = MakeButton("Cancel", 230, 225, False)

            Dim pnl As New Panel() With {.Dock = DockStyle.Fill}

            lblOpens.Text = "Encoding opens: " & encStart.ToString("MM/dd/yyyy hh:mm tt") & vbCrLf & "(automatic - the semester start date)"
            lblOpens.Location = New Point(20, 15)
            pnl.Controls.Add(lblOpens)

            pnl.Controls.Add(MakeLabel("Encoding Deadline:", 20, 62))
            dtpDeadline.Location = New Point(20, 82)
            dtpDeadline.Value = t.Deadline
            pnl.Controls.Add(dtpDeadline)

            lblWarning.Location = New Point(20, 115)
            pnl.Controls.Add(lblWarning)

            chkLockSystem.Checked = t.IsLocked
            chkLockSystem.Location = New Point(20, 170)
            pnl.Controls.Add(chkLockSystem)

            pnl.Controls.Add(btnOk)
            pnl.Controls.Add(btnCancel)
            Controls.Add(pnl)
            AcceptButton = btnOk
            CancelButton = btnCancel

            AddHandler dtpDeadline.ValueChanged, AddressOf OnDeadlineChanged
            ValidateSelectedDate(dtpDeadline.Value)
        End Sub

        Private Sub OnDeadlineChanged(sender As Object, e As EventArgs)
            ValidateSelectedDate(dtpDeadline.Value)
        End Sub

        Private Sub ValidateSelectedDate(selected As DateTime)
            If selected <= encStart Then
                lblWarning.ForeColor = Color.Red
                lblWarning.Text = "The deadline must be after encoding opens (" & encStart.ToString("MM/dd/yyyy") & ")."
                btnOk.Enabled = False
            ElseIf selected > maxAllowed Then
                lblWarning.ForeColor = Color.Red
                lblWarning.Text = "The deadline can be at most 1 month after the semester ends (until " & maxAllowed.ToString("MM/dd/yyyy") & ")."
                btnOk.Enabled = False
            Else
                If selected.Date > termEnd Then
                    lblWarning.ForeColor = Color.DarkOrange
                    lblWarning.Text = "Falls in the post-semester grace period (the semester ends " & termEnd.ToString("MM/dd/yyyy") & ")."
                Else
                    lblWarning.ForeColor = Color.Green
                    lblWarning.Text = "Valid encoding window."
                End If
                btnOk.Enabled = True
            End If
        End Sub
    End Class

    '========================================
    ' NESTED GRADING SCALE DIALOG
    '========================================

    Private NotInheritable Class GradeScaleDialog
        Inherits Form

        Private _table As New DataTable()
        Private ReadOnly dgv As New DataGridView()
        Private ReadOnly lblWarning As New Label() With {.AutoSize = False, .ForeColor = Color.DimGray}
        ' Standard Windows buttons (same look as a normal message box)
        Private ReadOnly btnOk As New Button() With {.Text = "Save", .Size = New Size(110, 38), .FlatStyle = FlatStyle.System, .UseVisualStyleBackColor = True, .Font = New Font("Segoe UI", 10, FontStyle.Regular)}
        Private ReadOnly btnCancel As New Button() With {.Text = "Cancel", .Size = New Size(110, 38), .FlatStyle = FlatStyle.System, .UseVisualStyleBackColor = True, .Font = New Font("Segoe UI", 10, FontStyle.Regular), .DialogResult = DialogResult.Cancel}

        Public Sub New()
            Text = "Edit Grading Scale and GWA"
            Font = New Font("Century Gothic", 9, FontStyle.Regular)
            Size = New Size(520, 540)
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False

            dgv.Location = New Point(20, 15)
            dgv.Size = New Size(465, 350)
            dgv.AllowUserToAddRows = False
            dgv.AllowUserToDeleteRows = False
            dgv.RowHeadersVisible = False
            dgv.BackgroundColor = Color.White

            lblWarning.Location = New Point(20, 372)
            lblWarning.Size = New Size(465, 40)
            lblWarning.Text = "Ranges must run from 0.00 to 100.00 with no gaps or overlaps (each range starts 0.01 below the one above)."

            ' Right-aligned with the grid, like a standard dialog
            btnCancel.Location = New Point(dgv.Right - btnCancel.Width, 428)
            btnOk.Location = New Point(btnCancel.Left - 14 - btnOk.Width, 428)

            Controls.Add(dgv)
            Controls.Add(lblWarning)
            Controls.Add(btnOk)
            Controls.Add(btnCancel)
            CancelButton = btnCancel

            AddHandler btnOk.Click, AddressOf OnSave
            AddHandler dgv.DataError, AddressOf OnGridError
            AddHandler Me.Load, AddressOf OnDialogLoad
        End Sub

        Private Sub OnDialogLoad(sender As Object, e As EventArgs)
            _table = DatabaseHelper.ExecuteQuery(
                "SELECT scale_id, min_percent, max_percent, grade_equivalent " &
                "FROM grade_scale ORDER BY min_percent DESC")

            If _table Is Nothing OrElse _table.Rows.Count = 0 Then
                MessageBox.Show("No grading scale found to edit.", "Error", MessageBoxButtons.OK)
                DialogResult = DialogResult.Cancel
                Close()
                Return
            End If

            dgv.DataSource = _table
            dgv.Columns("scale_id").Visible = False
            dgv.Columns("min_percent").HeaderText = "Min %"
            dgv.Columns("max_percent").HeaderText = "Max %"
            dgv.Columns("grade_equivalent").HeaderText = "GWA"
            For Each c As DataGridViewColumn In dgv.Columns
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            Next
        End Sub

        Private Sub OnGridError(sender As Object, e As DataGridViewDataErrorEventArgs)
            MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButtons.OK)
            e.ThrowException = False
        End Sub

        Private Function ValidateScale(ByRef msg As String) As Boolean
            dgv.EndEdit()
            Me.BindingContext(_table).EndCurrentEdit()

            Dim rows = _table.Rows.Cast(Of DataRow)().ToList()
            If rows.Count = 0 Then
                msg = "The grading scale is empty."
                Return False
            End If

            For Each r In rows
                If IsDBNull(r("min_percent")) OrElse IsDBNull(r("max_percent")) OrElse IsDBNull(r("grade_equivalent")) Then
                    msg = "All fields must be filled in."
                    Return False
                End If
            Next

            Dim sorted = rows.OrderByDescending(Function(r) Convert.ToDecimal(r("min_percent"))).ToList()

            For i As Integer = 0 To sorted.Count - 1
                Dim minP As Decimal = Math.Round(Convert.ToDecimal(sorted(i)("min_percent")), 2)
                Dim maxP As Decimal = Math.Round(Convert.ToDecimal(sorted(i)("max_percent")), 2)
                Dim gwa As Decimal = Convert.ToDecimal(sorted(i)("grade_equivalent"))

                If minP < 0D OrElse maxP > 100D Then
                    msg = $"Range {minP:0.00}-{maxP:0.00}: values must be between 0 and 100."
                    Return False
                End If
                If minP > maxP Then
                    msg = $"Range {minP:0.00}-{maxP:0.00}: Min % cannot be greater than Max %."
                    Return False
                End If
                If gwa <= 0D Then
                    msg = $"Range {minP:0.00}-{maxP:0.00}: GWA must be greater than 0."
                    Return False
                End If
                If i = 0 AndAlso maxP <> 100D Then
                    msg = "The highest range must end at 100.00."
                    Return False
                End If
                If i > 0 Then
                    Dim prevMin As Decimal = Math.Round(Convert.ToDecimal(sorted(i - 1)("min_percent")), 2)
                    If prevMin - maxP <> 0.01D Then
                        msg = $"Gap or overlap between {minP:0.00}-{maxP:0.00} and the range above it."
                        Return False
                    End If
                End If
                If i = sorted.Count - 1 AndAlso minP <> 0D Then
                    msg = "The lowest range must start at 0.00."
                    Return False
                End If
            Next
            Return True
        End Function

        Private Sub OnSave(sender As Object, e As EventArgs)
            Dim msg As String = ""
            If Not ValidateScale(msg) Then
                lblWarning.ForeColor = Color.Red
                lblWarning.Text = "⚠️ " & msg
                Return
            End If

            If MessageBox.Show("Save changes to the grading scale?", "Confirm",
                               MessageBoxButtons.YesNo) <> DialogResult.Yes Then
                Return
            End If

            Try
                Using conn = DatabaseHelper.GetConnection()
                    conn.Open()
                    Using tx = conn.BeginTransaction()
                        For Each r As DataRow In _table.Rows
                            Using cmd = conn.CreateCommand()
                                cmd.Transaction = tx
                                cmd.CommandText =
                                    "UPDATE grade_scale SET min_percent=@min, max_percent=@max, " &
                                    "grade_equivalent=@gwa WHERE scale_id=@id"
                                cmd.Parameters.AddWithValue("@min", r("min_percent"))
                                cmd.Parameters.AddWithValue("@max", r("max_percent"))
                                cmd.Parameters.AddWithValue("@gwa", r("grade_equivalent"))
                                cmd.Parameters.AddWithValue("@id", r("scale_id"))
                                cmd.ExecuteNonQuery()
                            End Using
                        Next
                        tx.Commit()
                    End Using
                End Using
                DialogResult = DialogResult.OK
                Close()
            Catch ex As Exception
                MessageBox.Show("Failed to save: " & ex.Message, "Error", MessageBoxButtons.OK)
            End Try
        End Sub
    End Class

End Class