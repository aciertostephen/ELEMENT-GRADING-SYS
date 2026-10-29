Imports MySqlConnector
Imports System.Text.RegularExpressions

' =============================================================================
' ucAcademicManagement - SECTIONS VIEW + ADD SECTION
' This is a second file of the SAME class (Partial). Add it to the project next to
' ucAcademicManagement.vb. Nothing in your original file was removed.
' =============================================================================
Partial Public Class ucAcademicManagement

    ' --- state for the "Classes (Cards)" / "Sections (List)" switcher ---
    Private showingSections As Boolean = False
    Private cboViewMode As ComboBox
    Private dgvSections As DataGridView
    Private addButtonDefaultText As String = ""

    ' =========================================================================
    ' A. VIEW SWITCHER (dropdown + sections table), built in code
    ' =========================================================================
    Private Sub InitViewSwitcher()
        If DesignMode Then Return
        If cboViewMode IsNot Nothing Then Return

        Dim host As Control = flowCourses.Parent

        ' --- make flowCourses a normal anchored control so it can be moved down ---
        If flowCourses.Dock <> DockStyle.None Then
            Dim b As Rectangle = flowCourses.Bounds
            flowCourses.Dock = DockStyle.None
            flowCourses.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom
            flowCourses.Bounds = b
        End If

        ' --- the dropdown: sits in the red box (top-left, above the cards/table) ---
        cboViewMode = New ComboBox() With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Font = New Font("Century Gothic", 9, FontStyle.Regular),
            .Width = 160
        }
        cboViewMode.Items.AddRange(New Object() {"Classes (Cards)", "Sections (List)"})
        cboViewMode.SelectedIndex = 0
        cboViewMode.Anchor = AnchorStyles.Top Or AnchorStyles.Left

        ' ADJUST THESE TWO NUMBERS to nudge the dropdown
        Const offsetX As Integer = 15   ' lines up with the card's left margin
        Const offsetY As Integer = 0
        cboViewMode.Location = New Point(flowCourses.Left + offsetX, flowCourses.Top + offsetY)
        host.Controls.Add(cboViewMode)
        cboViewMode.BringToFront()

        ' --- push the cards area down to make room for the dropdown ---
        Dim shift As Integer = cboViewMode.Height + 8
        flowCourses.Bounds = New Rectangle(flowCourses.Left, flowCourses.Top + shift,
                                           flowCourses.Width, flowCourses.Height - shift)

        ' --- the sections table: takes exactly the (shifted) place of flowCourses ---
        dgvSections = New DataGridView() With {
            .Name = "dgvSections",
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .MultiSelect = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .RowHeadersVisible = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            .EnableHeadersVisualStyles = False,
            .Font = New Font("Century Gothic", 9, FontStyle.Regular),
            .Visible = False
        }
        dgvSections.RowTemplate.Height = 32
        dgvSections.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        dgvSections.ColumnHeadersHeight = 36
        dgvSections.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 243, 246)
        dgvSections.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 243, 246)
        dgvSections.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black
        dgvSections.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black
        dgvSections.ColumnHeadersDefaultCellStyle.Font = New Font("Century Gothic", 9, FontStyle.Bold)
        dgvSections.DefaultCellStyle.SelectionBackColor = Color.FromArgb(34, 90, 74)
        dgvSections.DefaultCellStyle.SelectionForeColor = Color.White

        dgvSections.Anchor = flowCourses.Anchor

        ' Add 15px margins on both the left and right sides
        dgvSections.Bounds = New Rectangle(
            flowCourses.Left + 15,
            flowCourses.Top,
            flowCourses.Width - 30,
            flowCourses.Height
        )

        host.Controls.Add(dgvSections)
        host.Controls.SetChildIndex(dgvSections, host.Controls.GetChildIndex(flowCourses))

        AddHandler cboViewMode.SelectedIndexChanged, AddressOf OnViewModeChanged
        addButtonDefaultText = btnAddCourse.Text
        InitSectionMenu()   ' right-click Edit/Delete + Student Placement button
        ApplyViewMode()
    End Sub

    Private Sub OnViewModeChanged(sender As Object, e As EventArgs)
        showingSections = (cboViewMode.SelectedIndex = 1)
        ApplyViewMode()
    End Sub

    Private Sub ApplyViewMode()
        flowCourses.Visible = Not showingSections
        dgvSections.Visible = showingSections
        If btnPlacement IsNot Nothing Then btnPlacement.Visible = showingSections

        btnAddSection.Visible = True
        btnAddSection.UseWaitCursor = False
        btnAddCourse.Text = addButtonDefaultText

        RefreshCurrentView()
    End Sub

    ' Reloads whichever view is on screen (filters + stats stay where they are)
    Private Sub RefreshCurrentView()
        If showingSections Then
            LoadSectionsFromDatabase()
        Else
            LoadCoursesFromDatabase()
        End If
    End Sub

    ' =========================================================================
    ' B. LOAD SECTIONS (uses the same filter dropdowns as the class cards)
    ' =========================================================================
    Private Sub LoadSectionsFromDatabase()
        If dgvSections Is Nothing Then Return

        Dim ps As New List(Of MySqlParameter)()
        Dim filters As String = ""
        Dim semFilter As String = ""

        If cboAY IsNot Nothing AndAlso cboAY.SelectedIndex > 0 Then
            filters &= " AND s.school_year = @sy "
            ps.Add(New MySqlParameter("@sy", cboAY.SelectedItem.ToString()))
        End If

        If cboYearLevel IsNot Nothing AndAlso cboYearLevel.SelectedIndex > 0 Then
            Dim yl As Integer
            If Integer.TryParse(cboYearLevel.SelectedItem.ToString().Substring(0, 1), yl) Then
                filters &= " AND s.year_level = @yl "
                ps.Add(New MySqlParameter("@yl", yl))
            End If
        End If

        If cboSection IsNot Nothing AndAlso cboSection.SelectedIndex > 0 Then
            filters &= " AND s.section_code = @sc "
            ps.Add(New MySqlParameter("@sc", cboSection.SelectedItem.ToString()))
        End If

        ' A section has no semester of its own, so this filter only narrows the "Classes" count
        If cboSemester IsNot Nothing AndAlso cboSemester.SelectedIndex > 0 Then
            Dim semVal As Integer = 1
            If cboSemester.SelectedItem.ToString().Contains("2nd") Then semVal = 2
            If cboSemester.SelectedItem.ToString().Contains("Summer") Then semVal = 3
            semFilter = " AND t.semester = @sem "
            ps.Add(New MySqlParameter("@sem", semVal))
        End If

        Dim sql As String =
            "SELECT s.section_id, s.section_code, p.program_code, " &
            "  CASE s.year_level WHEN 1 THEN '1st Year' WHEN 2 THEN '2nd Year' WHEN 3 THEN '3rd Year' " &
            "       ELSE CONCAT(s.year_level, 'th Year') END AS year_label, " &
            "  s.school_year, COALESCE(s.batch, '') AS batch, " &
            "  (SELECT COUNT(DISTINCT x.student_id) FROM (SELECT student_id, section_id FROM student_section_history " &
            "     UNION SELECT student_id, section_id FROM student_profile) x WHERE x.section_id = s.section_id) AS student_count, " &
            "  (SELECT COUNT(*) FROM classes c INNER JOIN academic_term t ON t.term_id = c.term_id " &
            "     WHERE c.section_id = s.section_id " & semFilter & ") AS class_count " &
            "FROM section s " &
            "INNER JOIN program p ON p.program_id = s.program_id " &
            "WHERE 1=1 " & filters &
            "ORDER BY s.school_year DESC, s.year_level ASC, s.section_code ASC;"

        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, ps.ToArray())
            dgvSections.DataSource = dt

            SetSectionColumn("section_id", "ID", False)
            SetSectionColumn("section_code", "Section", True)
            SetSectionColumn("program_code", "Program", True)
            SetSectionColumn("year_label", "Year Level", True)
            SetSectionColumn("school_year", "School Year", True)
            SetSectionColumn("batch", "Batch", True)
            SetSectionColumn("student_count", "Students (ever placed)", True)
            SetSectionColumn("class_count", "Classes", True)

            For Each colName As String In {"student_count", "class_count"}
                If dgvSections.Columns.Contains(colName) Then
                    dgvSections.Columns(colName).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                    dgvSections.Columns(colName).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
                End If
            Next
        Catch ex As Exception
            MessageBox.Show("Error loading sections: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.None)
        End Try
    End Sub

    Private Sub SetSectionColumn(name As String, header As String, isVisible As Boolean)
        If Not dgvSections.Columns.Contains(name) Then Return
        dgvSections.Columns(name).HeaderText = header
        dgvSections.Columns(name).Visible = isVisible
    End Sub

    ' =========================================================================
    ' C. ADD SECTION
    ' Called by btnAddCourse while the Sections view is showing. If you have a
    ' separate "Add Section" button, just call ShowAddSectionDialog() from its Click.
    ' =========================================================================
    Private Sub ShowAddSectionDialog()
        Dim dtPrograms As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT program_id, program_code, CONCAT(program_code, ' - ', program_name) AS display FROM program ORDER BY program_code ASC;")
        If dtPrograms Is Nothing OrElse dtPrograms.Rows.Count = 0 Then
            MessageBox.Show("No programs found. Please add a program first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.None)
            Return
        End If

        Dim dtYears As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT school_year FROM academic_term UNION SELECT school_year FROM section ORDER BY school_year DESC;")

        ' Default to the active term's school year: Add Class only offers sections of that year
        Dim defaultSY As String = ""
        Dim dtActive As DataTable = DatabaseHelper.ExecuteQuery("SELECT school_year FROM academic_term WHERE is_active = 1 LIMIT 1;")
        If dtActive IsNot Nothing AndAlso dtActive.Rows.Count > 0 Then defaultSY = dtActive.Rows(0)(0).ToString()

        Using f As New Form()
            f.Text = "Add New Section"
            f.Size = New Size(450, 370)
            f.StartPosition = FormStartPosition.CenterParent
            f.FormBorderStyle = FormBorderStyle.FixedDialog
            f.MaximizeBox = False
            f.MinimizeBox = False
            f.ShowIcon = False
            f.Font = SystemFonts.MessageBoxFont

            Dim boldFont As Font = SystemFonts.MessageBoxFont

            Dim lblProgram As New Label() With {.Text = "Program:", .Location = New Point(20, 20), .AutoSize = True, .Font = boldFont}
            Dim cboProgram As New ComboBox() With {.Location = New Point(20, 42), .Width = 390, .DropDownStyle = ComboBoxStyle.DropDownList}
            cboProgram.DataSource = dtPrograms
            cboProgram.DisplayMember = "display"
            cboProgram.ValueMember = "program_id"

            Dim lblSY As New Label() With {.Text = "School Year:", .Location = New Point(20, 80), .AutoSize = True, .Font = boldFont}
            Dim cboSY As New ComboBox() With {.Location = New Point(20, 102), .Width = 190, .DropDownStyle = ComboBoxStyle.DropDown}
            If dtYears IsNot Nothing Then
                For Each r As DataRow In dtYears.Rows
                    cboSY.Items.Add(r(0).ToString())
                Next
            End If
            cboSY.Text = defaultSY

            Dim lblYL As New Label() With {.Text = "Year Level:", .Location = New Point(220, 80), .AutoSize = True, .Font = boldFont}
            Dim cboYL As New ComboBox() With {.Location = New Point(220, 102), .Width = 190, .DropDownStyle = ComboBoxStyle.DropDownList}
            cboYL.Items.AddRange(New Object() {"1st Year", "2nd Year", "3rd Year", "4th Year"})
            cboYL.SelectedIndex = 0

            Dim lblCode As New Label() With {.Text = "Section Code:", .Location = New Point(20, 140), .AutoSize = True, .Font = boldFont}
            Dim txtCode As New TextBox() With {.Location = New Point(20, 162), .Width = 190, .MaxLength = 15}

            Dim lblBatch As New Label() With {.Text = "Batch (optional):", .Location = New Point(220, 140), .AutoSize = True, .Font = boldFont}
            Dim txtBatch As New TextBox() With {.Location = New Point(220, 162), .Width = 190, .MaxLength = 9}

            Dim lblHint As New Label() With {
                .Text = "Section code is pre-filled from the program and year level (e.g. BSCS2) - just add the letter. Batch is the school year the students entered.",
                .Location = New Point(20, 200),
                .MaximumSize = New Size(390, 0),
                .AutoSize = True,
                .ForeColor = Color.Gray,
                .Font = SystemFonts.MessageBoxFont
            }

            Dim btnSave As New Button() With {.Text = "Save Section", .Location = New Point(190, 270), .Width = 110, .Height = 35, .Font = boldFont}
            Dim btnCancel As New Button() With {.Text = "Cancel", .Location = New Point(310, 270), .Width = 100, .Height = 35, .DialogResult = DialogResult.Cancel}

            f.Controls.AddRange({lblProgram, cboProgram, lblSY, cboSY, lblYL, cboYL, lblCode, txtCode, lblBatch, txtBatch, lblHint, btnSave, btnCancel})
            f.AcceptButton = btnSave
            f.CancelButton = btnCancel

            ' --- auto-suggest section code + batch (user can still edit both) ---
            Dim autoPrefix As String = ""
            Dim autoBatch As String = ""
            Dim syncSuggestions As Action =
                Sub()
                    Dim drv As DataRowView = TryCast(cboProgram.SelectedItem, DataRowView)
                    If drv Is Nothing Then Return
                    Dim yl As Integer = cboYL.SelectedIndex + 1

                    Dim newPrefix As String = drv("program_code").ToString() & yl.ToString()
                    If txtCode.Text = "" OrElse txtCode.Text.StartsWith(autoPrefix) Then
                        Dim suffix As String = ""
                        If txtCode.Text.StartsWith(autoPrefix) Then suffix = txtCode.Text.Substring(autoPrefix.Length)
                        txtCode.Text = newPrefix & suffix
                        txtCode.SelectionStart = txtCode.Text.Length
                    End If
                    autoPrefix = newPrefix

                    Dim m As Match = Regex.Match(cboSY.Text.Trim(), "^(\d{4})-(\d{4})$")
                    If m.Success AndAlso (txtBatch.Text = "" OrElse txtBatch.Text = autoBatch) Then
                        Dim startYr As Integer = Integer.Parse(m.Groups(1).Value) - (yl - 1)
                        autoBatch = startYr.ToString() & "-" & (startYr + 1).ToString()
                        txtBatch.Text = autoBatch
                    End If
                End Sub

            AddHandler cboProgram.SelectedIndexChanged, Sub(s, ev) syncSuggestions()
            AddHandler cboYL.SelectedIndexChanged, Sub(s, ev) syncSuggestions()
            AddHandler cboSY.TextChanged, Sub(s, ev) syncSuggestions()
            syncSuggestions()

            ' --- validate BEFORE closing, so the dialog stays open on a mistake ---
            AddHandler btnSave.Click,
                Sub(s, ev)
                    Dim problem As String = ValidateNewSection(
                        Convert.ToInt32(cboProgram.SelectedValue), cboSY.Text.Trim(), cboYL.SelectedIndex + 1,
                        txtCode.Text.Trim(), txtBatch.Text.Trim())
                    If problem <> "" Then
                        MessageBox.Show(problem, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.None)
                        Return
                    End If
                    f.DialogResult = DialogResult.OK
                End Sub

            If f.ShowDialog() <> DialogResult.OK Then Return

            Dim programId As Integer = Convert.ToInt32(cboProgram.SelectedValue)
            Dim schoolYear As String = cboSY.Text.Trim()
            Dim yearLevel As Integer = cboYL.SelectedIndex + 1
            Dim code As String = txtCode.Text.Trim()
            Dim batch As String = txtBatch.Text.Trim()

            Try
                Dim sectionId As Integer = SectionService.Create(programId, schoolYear, yearLevel, code, batch)
                Msg.Info("Section " & code & " was added.", "Section created")

                If Msg.Confirm("Would you like to import the student list for this section now? New student accounts will be created by the administrator.",
                               "Import section students") Then
                    Try
                        AdminSectionStudentImportDialog.ChooseAndImport(sectionId, FindForm())
                    Catch ex As InvalidOperationException
                        Msg.Warn(ex.Message)
                    Catch ex As Exception
                        Msg.Fail("import the section student list", ex)
                    End Try
                End If

                ' new section can add a new year level / section code to the filters, and changes the stat card
                PopulateDropdowns()
                LoadAcademicStats()
                RefreshCurrentView()
            Catch ex As Exception
                Msg.Fail("add the section", ex)
            End Try
        End Using
    End Sub

    ' Returns "" when OK, otherwise the message to show
    Private Function ValidateNewSection(programId As Integer, schoolYear As String, yearLevel As Integer,
                                        code As String, batch As String) As String
        If code = "" Then Return "Section code is required."
        If code.Length > 15 Then Return "Section code can be at most 15 characters."

        Dim m As Match = Regex.Match(schoolYear, "^(\d{4})-(\d{4})$")
        If Not m.Success Then Return "School year must look like 2026-2027."
        If Not AcademicContext.IsEditableYear(schoolYear) Then
            Return "School year " & schoolYear & " is past history and is read-only. Sections can only be added to the active or an upcoming year."
        End If
        If Integer.Parse(m.Groups(2).Value) <> Integer.Parse(m.Groups(1).Value) + 1 Then
            Return "School year must be two consecutive years, e.g. 2026-2027."
        End If

        If batch <> "" AndAlso Not Regex.IsMatch(batch, "^\d{4}-\d{4}$") Then
            Return "Batch must look like 2025-2026, or be left blank."
        End If
        If yearLevel < 1 OrElse yearLevel > 4 Then Return "Year level must be between 1st and 4th."

        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(
            "SELECT COUNT(*) FROM section WHERE program_id = @p AND school_year = @sy AND year_level = @yl AND section_code = @sc;", {
            New MySqlParameter("@p", programId),
            New MySqlParameter("@sy", schoolYear),
            New MySqlParameter("@yl", yearLevel),
            New MySqlParameter("@sc", code)
        })
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso Convert.ToInt32(dt.Rows(0)(0)) > 0 Then
            Return "Section " & code & " already exists for that program, school year and year level."
        End If

        Return ""
    End Function

End Class
