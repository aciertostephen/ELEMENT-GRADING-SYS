Public Class ucAcademicManagement

    Private Sub ucAcademicManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PopulateDropdowns()
        LoadAcademicStats()
        LoadCoursesFromDatabase()
    End Sub

    ' =========================================================================
    ' 1. POPULATE DROPDOWNS FROM DATABASE
    ' =========================================================================
    Private Sub PopulateDropdowns()
        RemoveHandler cboAY.SelectedIndexChanged, AddressOf OnFilterChanged
        RemoveHandler cboSemester.SelectedIndexChanged, AddressOf OnFilterChanged
        RemoveHandler cboYearLevel.SelectedIndexChanged, AddressOf OnFilterChanged
        RemoveHandler cboSection.SelectedIndexChanged, AddressOf OnFilterChanged

        ' --- Year Levels ---
        cboYearLevel.Items.Clear()
        cboYearLevel.Items.Add("All Year Levels")
        Try
            Dim dtYL As DataTable = DatabaseHelper.ExecuteQuery("SELECT DISTINCT year_level FROM student_profile WHERE year_level IS NOT NULL AND year_level <> '' ORDER BY year_level ASC;")
            If dtYL IsNot Nothing AndAlso dtYL.Rows.Count > 0 Then
                For Each row As DataRow In dtYL.Rows
                    cboYearLevel.Items.Add(row(0).ToString() & "nd Year")
                Next
            Else
                cboYearLevel.Items.AddRange(New String() {"1st Year", "2nd Year", "3rd Year", "4th Year"})
            End If
        Catch
            cboYearLevel.Items.AddRange(New String() {"1st Year", "2nd Year", "3rd Year", "4th Year"})
        End Try
        cboYearLevel.SelectedIndex = 0

        ' --- Semesters ---
        cboSemester.Items.Clear()
        cboSemester.Items.Add("All Semesters")
        Try
            Dim dtSem As DataTable = DatabaseHelper.ExecuteQuery("SELECT DISTINCT semester FROM academic_term ORDER BY semester ASC;")
            If dtSem IsNot Nothing AndAlso dtSem.Rows.Count > 0 Then
                For Each row As DataRow In dtSem.Rows
                    Dim semNum As String = row(0).ToString()
                    Select Case semNum
                        Case "1" : cboSemester.Items.Add("1st Semester")
                        Case "2" : cboSemester.Items.Add("2nd Semester")
                        Case "3" : cboSemester.Items.Add("Summer")
                        Case Else : cboSemester.Items.Add("Semester " & semNum)
                    End Select
                Next
            Else
                cboSemester.Items.AddRange(New String() {"1st Semester", "2nd Semester", "Summer"})
            End If
        Catch
            cboSemester.Items.AddRange(New String() {"1st Semester", "2nd Semester", "Summer"})
        End Try
        cboSemester.SelectedIndex = 0

        ' --- Academic Years ---
        cboAY.Items.Clear()
        cboAY.Items.Add("All Academic Years")
        Try
            Dim dtAY As DataTable = DatabaseHelper.ExecuteQuery("SELECT DISTINCT school_year FROM academic_term ORDER BY school_year DESC;")
            If dtAY IsNot Nothing AndAlso dtAY.Rows.Count > 0 Then
                For Each row As DataRow In dtAY.Rows
                    If Not IsDBNull(row(0)) Then cboAY.Items.Add(row(0).ToString())
                Next
            End If
        Catch
        End Try
        cboAY.SelectedIndex = 0

        ' --- Sections ---
        cboSection.Items.Clear()
        cboSection.Items.Add("All Sections")
        Try
            Dim dtSec As DataTable = DatabaseHelper.ExecuteQuery("SELECT DISTINCT section_code FROM section ORDER BY section_code ASC;")
            If dtSec IsNot Nothing AndAlso dtSec.Rows.Count > 0 Then
                For Each row As DataRow In dtSec.Rows
                    If Not IsDBNull(row(0)) Then cboSection.Items.Add(row(0).ToString())
                Next
            End If
        Catch
        End Try
        cboSection.SelectedIndex = 0

        AddHandler cboAY.SelectedIndexChanged, AddressOf OnFilterChanged
        AddHandler cboSemester.SelectedIndexChanged, AddressOf OnFilterChanged
        AddHandler cboYearLevel.SelectedIndexChanged, AddressOf OnFilterChanged
        AddHandler cboSection.SelectedIndexChanged, AddressOf OnFilterChanged
    End Sub

    ' =========================================================================
    ' 2. LOAD COURSES AND APPLY FILTERS DYNAMICALLY
    ' =========================================================================
    Private Sub LoadCoursesFromDatabase()
        If flowCourses Is Nothing Then Return
        DisposeAndClear(flowCourses)

        Dim sql As String = "SELECT cl.class_id, " &
                             "co.course_code, " &
                             "co.course_name, " &
                             "COALESCE(s.section_code, 'N/A') AS sec_name, " &
                             "CONCAT(tp.first_name, ' ', tp.last_name) AS instructor, " &
                             "COUNT(e.student_id) AS student_count " &
                             "FROM classes cl " &
                             "INNER JOIN course co ON cl.course_id = co.course_id " &
                             "LEFT JOIN section s ON cl.section_id = s.section_id " &
                             "LEFT JOIN academic_term t ON cl.term_id = t.term_id " &
                             "LEFT JOIN teacher_profile tp ON cl.teacher_id = tp.teacher_id " &
                             "LEFT JOIN enrollment e ON cl.class_id = e.class_id " &
                             "WHERE 1=1 "

        If cboAY IsNot Nothing AndAlso cboAY.SelectedIndex > 0 Then
            sql &= " AND t.school_year = '" & SafeSql(cboAY.SelectedItem.ToString()) & "' "
        End If

        If cboSemester IsNot Nothing AndAlso cboSemester.SelectedIndex > 0 Then
            Dim semVal As String = "1"
            If cboSemester.SelectedItem.ToString().Contains("1st") Then semVal = "1"
            If cboSemester.SelectedItem.ToString().Contains("2nd") Then semVal = "2"
            If cboSemester.SelectedItem.ToString().Contains("Summer") Then semVal = "3"
            sql &= " AND t.semester = " & semVal & " "
        End If

        If cboSection IsNot Nothing AndAlso cboSection.SelectedIndex > 0 Then
            sql &= " AND s.section_code = '" & SafeSql(cboSection.SelectedItem.ToString()) & "' "
        End If

        sql &= " GROUP BY cl.class_id, co.course_code, co.course_name, s.section_code, tp.first_name, tp.last_name " &
               " ORDER BY co.course_code ASC;"

        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql)

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For Each row As DataRow In dt.Rows
                    Dim classId As Integer = Convert.ToInt32(row("class_id"))
                    Dim subjectTitle As String = $"{row("course_code")} - {row("course_name")}"
                    Dim sectionCode As String = row("sec_name").ToString()
                    Dim instructor As String = row("instructor").ToString()
                    Dim studentCount As String = row("student_count").ToString()

                    Dim card As New ucCourseCard()
                    card.SetData(subjectTitle, sectionCode, instructor, studentCount, 100)
                    card.Margin = New Padding(15)

                    AddHandler card.CardClicked, Sub()
                                                     ShowGradebookPOV(classId, subjectTitle, sectionCode, instructor)
                                                 End Sub

                    flowCourses.Controls.Add(card)
                Next
            End If

        Catch ex As Exception
        End Try
    End Sub

    Private Function SafeSql(val As String) As String
        Return val.Replace("'", "''")
    End Function

    Private Sub OnFilterChanged(sender As Object, e As EventArgs) Handles cboAY.SelectedIndexChanged,
                                                                           cboSemester.SelectedIndexChanged,
                                                                           cboYearLevel.SelectedIndexChanged,
                                                                           cboSection.SelectedIndexChanged
        LoadCoursesFromDatabase()
    End Sub

    ' =========================================================================
    ' 3. ADD COURSE / INSTANTIATE CLASS MODAL
    ' =========================================================================
    Private Sub btnAddCourse_Click(sender As Object, e As EventArgs) Handles btnAddCourse.Click
        Dim dtCourses As DataTable = DatabaseHelper.ExecuteQuery("SELECT course_id, CONCAT(course_code, ' - ', course_name) AS display FROM course ORDER BY course_code ASC;")
        Dim dtSections As DataTable = DatabaseHelper.ExecuteQuery("SELECT section_id, section_code FROM section ORDER BY section_code ASC;")
        Dim dtTeachers As DataTable = DatabaseHelper.ExecuteQuery("SELECT teacher_id, CONCAT(first_name, ' ', last_name) AS teacher_name FROM teacher_profile ORDER BY last_name ASC;")
        Dim dtActiveTerm As DataTable = DatabaseHelper.ExecuteQuery("SELECT term_id, school_year, semester FROM academic_term WHERE is_active = 1 LIMIT 1;")

        If dtActiveTerm Is Nothing OrElse dtActiveTerm.Rows.Count = 0 Then
            MessageBox.Show("No active academic term found. Please set an active semester first in Semester/Grading Settings.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim f As New Form()
        f.Text = "Add New Class Instance"
        f.Size = New Size(450, 380)
        f.StartPosition = FormStartPosition.CenterParent
        f.FormBorderStyle = FormBorderStyle.FixedDialog
        f.MaximizeBox = False
        f.MinimizeBox = False
        f.Font = New Font("Century Gothic", 9, FontStyle.Regular)

        Dim lblCourse As New Label() With {.Text = "Select Course:", .Location = New Point(20, 20), .AutoSize = True, .Font = New Font("Century Gothic", 9, FontStyle.Bold)}
        Dim cboCourses As New ComboBox() With {.Location = New Point(20, 42), .Width = 390, .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Century Gothic", 9, FontStyle.Regular)}
        cboCourses.DataSource = dtCourses
        cboCourses.DisplayMember = "display"
        cboCourses.ValueMember = "course_id"

        Dim lblSection As New Label() With {.Text = "Select Section:", .Location = New Point(20, 80), .AutoSize = True, .Font = New Font("Century Gothic", 9, FontStyle.Bold)}
        Dim cboSections As New ComboBox() With {.Location = New Point(20, 102), .Width = 390, .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Century Gothic", 9, FontStyle.Regular)}
        cboSections.DataSource = dtSections
        cboSections.DisplayMember = "section_code"
        cboSections.ValueMember = "section_id"

        Dim lblTeacher As New Label() With {.Text = "Assign Teacher:", .Location = New Point(20, 140), .AutoSize = True, .Font = New Font("Century Gothic", 9, FontStyle.Bold)}
        Dim cboTeachers As New ComboBox() With {.Location = New Point(20, 162), .Width = 390, .DropDownStyle = ComboBoxStyle.DropDownList, .Font = New Font("Century Gothic", 9, FontStyle.Regular)}
        cboTeachers.DataSource = dtTeachers
        cboTeachers.DisplayMember = "teacher_name"
        cboTeachers.ValueMember = "teacher_id"

        Dim activeTermId As Integer = Convert.ToInt32(dtActiveTerm.Rows(0)("term_id"))
        Dim activeTermLabel As String = $"{dtActiveTerm.Rows(0)("school_year")} (Sem {dtActiveTerm.Rows(0)("semester")})"
        Dim lblTermInfo As New Label() With {.Text = "Active Term: " & activeTermLabel, .Location = New Point(20, 205), .AutoSize = True, .Font = New Font("Century Gothic", 9, FontStyle.Bold)}

        Dim btnSave As New Button() With {.Text = "Save Class", .Location = New Point(200, 260), .Width = 100, .Height = 35, .DialogResult = DialogResult.OK, .Font = New Font("Century Gothic", 9, FontStyle.Bold)}
        Dim btnCancel As New Button() With {.Text = "Cancel", .Location = New Point(310, 260), .Width = 100, .Height = 35, .DialogResult = DialogResult.Cancel, .Font = New Font("Century Gothic", 9, FontStyle.Regular)}

        f.Controls.AddRange({lblCourse, cboCourses, lblSection, cboSections, lblTeacher, cboTeachers, lblTermInfo, btnSave, btnCancel})
        f.AcceptButton = btnSave

        If f.ShowDialog() = DialogResult.OK Then
            If cboCourses.SelectedValue Is Nothing OrElse cboSections.SelectedValue Is Nothing OrElse cboTeachers.SelectedValue Is Nothing Then
                MessageBox.Show("All fields must be selected.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim selectedCourseId As Integer = Convert.ToInt32(cboCourses.SelectedValue)
            Dim selectedSectionId As Integer = Convert.ToInt32(cboSections.SelectedValue)
            Dim selectedTeacherId As String = cboTeachers.SelectedValue.ToString()

            Dim insertSql As String = "INSERT INTO classes (course_id, section_id, teacher_id, term_id) VALUES " &
                                      $"({selectedCourseId}, {selectedSectionId}, '{selectedTeacherId}', {activeTermId});"

            Try
                DatabaseHelper.ExecuteNonQuery(insertSql)
                MessageBox.Show("Class successfully instantiated and assigned!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                LoadCoursesFromDatabase()
            Catch ex As Exception
                MessageBox.Show("Error adding class instance: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    ' =========================================================================
    ' 4. FLASH GRADEBOOK FROM TEACHER POV
    ' =========================================================================
    ' =========================================================================
    ' 4. FLASH GRADEBOOK FROM TEACHER POV (ROUNDED BACK BUTTON WITH HOVER)
    ' =========================================================================
    Private Sub ShowGradebookPOV(classId As Integer, courseTitle As String, sectionCode As String, teacherName As String)
        Dim sql As String = "SELECT " &
                             "sp.student_id AS `Student ID`, " &
                             "CONCAT(sp.last_name, ', ', sp.first_name) AS `Student Name`, " &
                             "e.status AS `Enrollment Status`, " &
                             "COALESCE(e.final_grade, 0.00) AS `Final Grade`, " &
                             "COALESCE(e.grade_equivalent, 0.00) AS `Equivalent` " &
                             "FROM enrollment e " &
                             "INNER JOIN student_profile sp ON e.student_id = sp.student_id " &
                             "WHERE e.class_id = " & classId & " " &
                             "ORDER BY sp.last_name ASC;"

        Dim dtRoster As DataTable = DatabaseHelper.ExecuteQuery(sql)

        Dim pnlGradebook As New Panel() With {
            .Name = "pnlGradebookView",
            .Dock = DockStyle.Fill,
            .BackColor = Color.White
        }

        Dim pnlTopBar As New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 60,
            .BackColor = Color.FromArgb(240, 243, 246)
        }

        ' ROUNDED BACK BUTTON WITH CUSTOM RGB COLORS & HOVER EFFECT
        Dim normalColor As Color = Color.FromArgb(34, 90, 74)
        Dim hoverColor As Color = Color.FromArgb(23, 61, 50)

        Dim btnBack As New Button() With {
            .Text = "← Back to Classes",
            .Size = New Size(150, 35),
            .Location = New Point(15, 12),
            .FlatStyle = FlatStyle.Flat,
            .BackColor = normalColor,
            .ForeColor = Color.White,
            .Font = New Font("Century Gothic", 9, FontStyle.Regular),
            .Cursor = Cursors.Hand
        }
        btnBack.FlatAppearance.BorderSize = 0

        ' Dynamically apply rounded corners (Radius: 15px)
        Dim cornerRadius As Integer = 15
        Dim path As New System.Drawing.Drawing2D.GraphicsPath()
        path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90)
        path.AddArc(btnBack.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90)
        path.AddArc(btnBack.Width - cornerRadius, btnBack.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90)
        path.AddArc(0, btnBack.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90)
        path.CloseAllFigures()
        btnBack.Region = New Region(path)

        ' Mouse Hover Event Handlers
        AddHandler btnBack.MouseEnter, Sub() btnBack.BackColor = hoverColor
        AddHandler btnBack.MouseLeave, Sub() btnBack.BackColor = normalColor

        ' Click Event Handler
        AddHandler btnBack.Click, Sub()
                                      Me.Controls.Remove(pnlGradebook)
                                      pnlGradebook.Dispose()
                                      LoadCoursesFromDatabase()
                                  End Sub

        Dim lblTitle As New Label() With {
            .Text = $"{courseTitle} — Section: {sectionCode} (Teacher: {teacherName})",
            .Font = New Font("Century Gothic", 11, FontStyle.Bold),
            .Location = New Point(180, 18),
            .AutoSize = True
        }

        pnlTopBar.Controls.Add(btnBack)
        pnlTopBar.Controls.Add(lblTitle)

        Dim dgvRoster As New DataGridView() With {
            .Dock = DockStyle.Fill,
            .ReadOnly = True,
            .AllowUserToAddRows = False,
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .BackgroundColor = Color.White,
            .BorderStyle = BorderStyle.None,
            .Font = New Font("Century Gothic", 9, FontStyle.Regular)
        }
        dgvRoster.ColumnHeadersDefaultCellStyle.Font = New Font("Century Gothic", 9, FontStyle.Bold)
        dgvRoster.DataSource = dtRoster

        Dim lblFooter As New Label() With {
            .Text = $"Total Enrolled Students: {If(dtRoster IsNot Nothing, dtRoster.Rows.Count, 0)}",
            .Dock = DockStyle.Bottom,
            .Height = 35,
            .TextAlign = ContentAlignment.MiddleRight,
            .Padding = New Padding(0, 0, 20, 0),
            .Font = New Font("Century Gothic", 9, FontStyle.Bold),
            .BackColor = Color.FromArgb(245, 247, 248)
        }

        pnlGradebook.Controls.Add(dgvRoster)
        pnlGradebook.Controls.Add(pnlTopBar)
        pnlGradebook.Controls.Add(lblFooter)

        Me.Controls.Add(pnlGradebook)
        pnlGradebook.BringToFront()
    End Sub

    ' =========================================================================
    ' 5. ACADEMIC STATS CARDS
    ' =========================================================================
    Private Sub LoadAcademicStats()
        If flowStats Is Nothing Then Return
        DisposeAndClear(flowStats)

        Dim activeSem As String = "1"
        Dim totalSections As String = "0"
        Dim totalCourses As String = "0"

        Try
            Dim dtSem As DataTable = DatabaseHelper.ExecuteQuery("SELECT semester FROM academic_term WHERE is_active = 1 LIMIT 1;")
            If dtSem IsNot Nothing AndAlso dtSem.Rows.Count > 0 Then activeSem = dtSem.Rows(0)(0).ToString()
        Catch
        End Try

        Try
            Dim dtSecCount As DataTable = DatabaseHelper.ExecuteQuery("SELECT COUNT(*) FROM section;")
            If dtSecCount IsNot Nothing AndAlso dtSecCount.Rows.Count > 0 Then totalSections = dtSecCount.Rows(0)(0).ToString()
        Catch
        End Try

        Try
            Dim dtCourseCount As DataTable = DatabaseHelper.ExecuteQuery("SELECT COUNT(*) FROM course;")
            If dtCourseCount IsNot Nothing AndAlso dtCourseCount.Rows.Count > 0 Then totalCourses = dtCourseCount.Rows(0)(0).ToString()
        Catch
        End Try

        AddStatBox("SEMESTER", activeSem)
        AddStatBox("TOTAL SECTIONS", totalSections)
        AddStatBox("TOTAL COURSES", totalCourses)
    End Sub

    Private Sub AddStatBox(caption As String, value As String)
        Dim box As New RoundedPanel()
        box.Size = New Size(350, 80)
        box.BackColor = Color.White
        box.Margin = New Padding(15, 15, 0, 0)

        Dim lblCaption As New Label()
        lblCaption.Text = caption
        lblCaption.ForeColor = Color.Gray
        lblCaption.Font = New Font("Century Gothic", 8, FontStyle.Regular)
        lblCaption.Location = New Point(15, 12)
        lblCaption.AutoSize = True

        Dim lblValue As New Label()
        lblValue.Text = value
        lblValue.Font = New Font("Century Gothic", 17, FontStyle.Bold)
        lblValue.Location = New Point(13, 32)
        lblValue.AutoSize = True

        box.Controls.Add(lblCaption)
        box.Controls.Add(lblValue)
        flowStats.Controls.Add(box)
    End Sub

    Private Sub DisposeAndClear(container As Control)
        For Each ctrl As Control In container.Controls
            ctrl.Dispose()
        Next
        container.Controls.Clear()
    End Sub

End Class