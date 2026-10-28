Imports MySqlConnector

Public Class ucAcademicManagement

    Private Sub ucAcademicManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitViewSwitcher()          ' NEW
        PopulateDropdowns()
        LoadAcademicStats()
        LoadCoursesFromDatabase()
        ' The dean opens on the active year and can select historical years and semesters.
        lblAY.Visible = True
        cboAY.Visible = True
    End Sub

    ' =========================================================================
    ' 1. POPULATE DROPDOWNS FROM DATABASE
    ' =========================================================================
    Private Sub PopulateDropdowns()
        RemoveHandler cboAY.SelectedIndexChanged, AddressOf OnAcademicYearFilterChanged
        RemoveHandler cboSemester.SelectedIndexChanged, AddressOf OnFilterChanged
        RemoveHandler cboYearLevel.SelectedIndexChanged, AddressOf OnYearLevelFilterChanged
        RemoveHandler cboSection.SelectedIndexChanged, AddressOf OnFilterChanged

        ' --- Year Levels ---
        cboYearLevel.Items.Clear()
        cboYearLevel.Items.Add("All Year Levels")
        Try
            Dim dtYL As DataTable = DatabaseHelper.ExecuteQuery("SELECT DISTINCT year_level FROM section WHERE year_level IS NOT NULL ORDER BY year_level ASC;")
            If dtYL IsNot Nothing AndAlso dtYL.Rows.Count > 0 Then
                For Each row As DataRow In dtYL.Rows
                    cboYearLevel.Items.Add(AcademicContext.OrdinalYear(Convert.ToInt32(row(0))))
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
        ' --- Academic Years ---
        cboAY.Items.Clear()
        cboAY.Items.Add("All Academic Years")
        Try
            Dim dtAY As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT school_year FROM school_year_calendar UNION SELECT school_year FROM academic_term " &
                "UNION SELECT school_year FROM section ORDER BY school_year DESC;")
            If dtAY IsNot Nothing AndAlso dtAY.Rows.Count > 0 Then
                For Each row As DataRow In dtAY.Rows
                    If Not IsDBNull(row(0)) Then cboAY.Items.Add(row(0).ToString())
                Next
            End If
        Catch
        End Try
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
        Dim activeYear As String = ""
        Dim activeSemester As Integer = 0
        Try
            Dim dtActive As DataTable = DatabaseHelper.ExecuteQuery(
                "SELECT school_year, semester FROM academic_term WHERE is_active = 1 LIMIT 1;")
            If dtActive IsNot Nothing AndAlso dtActive.Rows.Count > 0 Then
                activeYear = dtActive.Rows(0)("school_year").ToString()
                activeSemester = Convert.ToInt32(dtActive.Rows(0)("semester"))
            End If
        Catch
        End Try
        Dim yearIndex As Integer = cboAY.Items.IndexOf(activeYear)
        cboAY.SelectedIndex = If(yearIndex > 0, yearIndex, 0)
        Dim activeSemesterLabel As String = If(activeSemester = 1, "1st Semester",
            If(activeSemester = 2, "2nd Semester", If(activeSemester = 3, "Summer", "")))
        Dim semesterIndex As Integer = cboSemester.Items.IndexOf(activeSemesterLabel)
        cboSemester.SelectedIndex = If(semesterIndex > 0, semesterIndex, 0)
        cboSection.SelectedIndex = 0

        AddHandler cboAY.SelectedIndexChanged, AddressOf OnAcademicYearFilterChanged
        AddHandler cboSemester.SelectedIndexChanged, AddressOf OnFilterChanged
        AddHandler cboYearLevel.SelectedIndexChanged, AddressOf OnYearLevelFilterChanged
        AddHandler cboSection.SelectedIndexChanged, AddressOf OnFilterChanged
        RefreshSectionFilter()
    End Sub

    ' =========================================================================
    ' 2. LOAD COURSES AND APPLY FILTERS DYNAMICALLY
    ' =========================================================================
    Private Sub LoadCoursesFromDatabase()
        If flowCourses Is Nothing Then Return
        flowCourses.DisposeAndClear()

        Dim sql As String = "SELECT cl.class_id, " &
                             "co.course_code, " &
                             "co.course_name, " &
                             "COALESCE(s.section_code, 'N/A') AS sec_name, " &
                             "CONCAT(tp.first_name, ' ', tp.last_name) AS instructor, " &
                             "COUNT(DISTINCT CASE WHEN e.status = 'Dropped' THEN NULL ELSE e.enrollment_id END) AS student_count, " &
                             "(SELECT COUNT(*) FROM grade_item gi INNER JOIN grading_rubric gr ON gr.rubric_id = gi.rubric_id " &
                             "  WHERE gr.class_id = cl.class_id) AS item_count, " &
                             "(SELECT COUNT(*) FROM grade_entry ge INNER JOIN enrollment e2 ON e2.enrollment_id = ge.enrollment_id " &
                             "  WHERE e2.class_id = cl.class_id AND e2.status <> 'Dropped' AND ge.score IS NOT NULL) AS filled_count " &
                             "FROM classes cl " &
                             "INNER JOIN course co ON cl.course_id = co.course_id " &
                             "LEFT JOIN section s ON cl.section_id = s.section_id " &
                             "LEFT JOIN academic_term t ON cl.term_id = t.term_id " &
                             "LEFT JOIN teacher_profile tp ON cl.teacher_id = tp.teacher_id " &
                             "LEFT JOIN enrollment e ON cl.class_id = e.class_id " &
                             "WHERE 1=1 "

        Dim parameters As New System.Collections.Generic.List(Of MySqlParameter)()

        If cboAY IsNot Nothing AndAlso cboAY.SelectedIndex > 0 Then
            sql &= " AND t.school_year = @school_year "
            parameters.Add(New MySqlParameter("@school_year", cboAY.SelectedItem.ToString()))
        End If

        If cboYearLevel IsNot Nothing AndAlso cboYearLevel.SelectedIndex > 0 Then
            Dim yl As Integer
            If Integer.TryParse(cboYearLevel.SelectedItem.ToString().Substring(0, 1), yl) Then
                sql &= " AND s.year_level = " & yl & " "
            End If
        End If

        If cboSemester IsNot Nothing AndAlso cboSemester.SelectedIndex > 0 Then
            Dim semVal As Integer = 0
            If cboSemester.SelectedItem.ToString().Contains("1st") Then semVal = 1
            If cboSemester.SelectedItem.ToString().Contains("2nd") Then semVal = 2
            If cboSemester.SelectedItem.ToString().Contains("Summer") Then semVal = 3
            If semVal > 0 Then
                sql &= " AND t.semester = @semester "
                parameters.Add(New MySqlParameter("@semester", semVal))
            End If
        End If

        If cboSection IsNot Nothing AndAlso cboSection.SelectedIndex > 0 Then
            sql &= " AND s.section_code = @section_code "
            parameters.Add(New MySqlParameter("@section_code", cboSection.SelectedItem.ToString()))
        End If

        sql &= " GROUP BY cl.class_id, co.course_code, co.course_name, s.section_code, tp.first_name, tp.last_name " &
               " ORDER BY co.course_code ASC;"

        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, parameters.ToArray())

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For Each row As DataRow In dt.Rows
                    Dim classId As Integer = Convert.ToInt32(row("class_id"))
                    Dim subjectTitle As String = $"{row("course_code")} - {row("course_name")}"
                    Dim sectionCode As String = row("sec_name").ToString()
                    Dim instructor As String = row("instructor").ToString()
                    Dim studentCount As String = row("student_count").ToString()

                    ' Encoding progress = scores entered / (enrolled students x grade items)
                    Dim expectedScores As Integer = Convert.ToInt32(row("student_count")) * Convert.ToInt32(row("item_count"))
                    Dim filledScores As Integer = Convert.ToInt32(row("filled_count"))
                    Dim progress As Integer = 0
                    If expectedScores > 0 Then
                        progress = CInt(Math.Min(100D, Math.Round(filledScores * 100D / expectedScores)))
                    End If

                    Dim card As New ucCourseCard()
                    card.SetData(subjectTitle, sectionCode, instructor, studentCount, progress)
                    card.Margin = New Padding(15)

                    AddHandler card.CardClicked, Sub()
                                                     ShowGradebookPOV(classId, subjectTitle, sectionCode, instructor)
                                                 End Sub

                    flowCourses.Controls.Add(card)
                    AttachClassMenu(card, classId)
                Next
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading classes: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
    End Sub

    Private Function SafeSql(val As String) As String
        Return val.Replace("'", "''")
    End Function

    Private Sub OnFilterChanged(sender As Object, e As EventArgs)
        RefreshCurrentView()
    End Sub

    Private Sub OnYearLevelFilterChanged(sender As Object, e As EventArgs)
        RefreshSectionFilter()
        RefreshCurrentView()
    End Sub

    Private Sub OnAcademicYearFilterChanged(sender As Object, e As EventArgs)
        RefreshSectionFilter()
        RefreshCurrentView()
    End Sub

    Private Sub RefreshSectionFilter()
        Dim selected As String = If(cboSection.SelectedItem Is Nothing, "All Sections", cboSection.SelectedItem.ToString())
        Dim selectedYear As Integer = 0
        If cboYearLevel.SelectedIndex > 0 Then Integer.TryParse(cboYearLevel.SelectedItem.ToString().Substring(0, 1), selectedYear)
        RemoveHandler cboSection.SelectedIndexChanged, AddressOf OnFilterChanged
        cboSection.Items.Clear()
        cboSection.Items.Add("All Sections")
        Dim filters As New System.Collections.Generic.List(Of String)()
        Dim parameters As New System.Collections.Generic.List(Of MySqlParameter)()
        If selectedYear > 0 Then
            filters.Add("year_level = @yl")
            parameters.Add(New MySqlParameter("@yl", selectedYear))
        End If
        If cboAY.SelectedIndex > 0 Then
            filters.Add("school_year = @sy")
            parameters.Add(New MySqlParameter("@sy", cboAY.SelectedItem.ToString()))
        End If
        Dim sql As String = "SELECT DISTINCT section_code FROM section"
        If filters.Count > 0 Then sql &= " WHERE " & String.Join(" AND ", filters)
        sql &= " ORDER BY section_code ASC"
        Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql, parameters.ToArray())
        If dt IsNot Nothing Then
            For Each row As DataRow In dt.Rows
                cboSection.Items.Add(row(0).ToString())
            Next
        End If
        Dim index As Integer = cboSection.Items.IndexOf(selected)
        cboSection.SelectedIndex = If(index >= 0, index, 0)
        AddHandler cboSection.SelectedIndexChanged, AddressOf OnFilterChanged
    End Sub

    ' =========================================================================
    ' 3. ADD COURSE / INSTANTIATE CLASS MODAL
    ' =========================================================================
    Private Sub btnAddCourse_Click(sender As Object, e As EventArgs) Handles btnAddCourse.Click
        ShowAddClassDialog()
    End Sub

    Private Sub btnAddSection_Click(sender As Object, e As EventArgs) Handles btnAddSection.Click
        ShowAddSectionDialog()
    End Sub

    ' =========================================================================
    ' 4. FLASH GRADEBOOK FROM TEACHER POV (COMPLETE ASSESSMENT MATRIX)
    ' =========================================================================
    Private Sub ShowGradebookPOV(classId As Integer, courseTitle As String, sectionCode As String, teacherName As String)
        ' If a gradebook is already open, replace it instead of stacking another one
        For i As Integer = Me.Controls.Count - 1 To 0 Step -1
            If Me.Controls(i).Name = "pnlGradebookView" Then
                Dim stale As Control = Me.Controls(i)
                Me.Controls.RemoveAt(i)
                stale.Dispose()
            End If
        Next

        Dim pnlGradebook As New Panel() With {
            .Name = "pnlGradebookView",
            .Dock = DockStyle.Fill,
            .BackColor = Color.White
        }

        Dim pnlTopBar As New Panel() With {
            .Dock = DockStyle.Top,
            .Height = 55,
            .BackColor = Color.FromArgb(240, 243, 246)
        }

        ' REFINED DYNAMIC-SIZED BACK BUTTON
        Dim normalColor As Color = Color.FromArgb(34, 90, 74)
        Dim hoverColor As Color = Color.FromArgb(23, 61, 50)
        Dim isHovered As Boolean = False

        Dim btnFont As New Font("Century Gothic", 8.5F, FontStyle.Regular)
        Dim buttonText As String = "← Back to Classes"

        ' Dynamically measure text width to prevent clipping under any DPI scaling
        Dim measuredSize As Size = TextRenderer.MeasureText(buttonText, btnFont)
        Dim btnWidth As Integer = measuredSize.Width + 28 ' 14px horizontal padding on each side
        Dim btnHeight As Integer = 30

        Dim btnBack As New Button() With {
            .Text = buttonText,
            .Size = New Size(btnWidth, btnHeight),
            .Location = New Point(15, (pnlTopBar.Height - btnHeight) \ 2), ' Perfectly centered vertically
            .FlatStyle = FlatStyle.Flat,
            .ForeColor = Color.White,
            .Font = btnFont,
            .Cursor = Cursors.Hand,
            .BackColor = Color.Transparent
        }
        btnBack.FlatAppearance.BorderSize = 0
        btnBack.FlatAppearance.MouseDownBackColor = Color.Transparent
        btnBack.FlatAppearance.MouseOverBackColor = Color.Transparent

        AddHandler btnBack.MouseEnter, Sub()
                                           isHovered = True
                                           btnBack.Invalidate()
                                       End Sub

        AddHandler btnBack.MouseLeave, Sub()
                                           isHovered = False
                                           btnBack.Invalidate()
                                       End Sub

        AddHandler btnBack.Paint, Sub(sender As Object, pe As PaintEventArgs)
                                      Dim g As Graphics = pe.Graphics
                                      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias
                                      g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit

                                      Dim rect As New Rectangle(0, 0, btnBack.Width - 1, btnBack.Height - 1)
                                      Dim cornerRadius As Integer = 12

                                      Using path As New System.Drawing.Drawing2D.GraphicsPath()
                                          path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90)
                                          path.AddArc(rect.Right - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90)
                                          path.AddArc(rect.Right - cornerRadius, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90)
                                          path.AddArc(rect.X, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90)
                                          path.CloseAllFigures()

                                          Dim fillColor As Color = If(isHovered, hoverColor, normalColor)
                                          Using brush As New SolidBrush(fillColor)
                                              g.FillPath(brush, path)
                                          End Using
                                      End Using

                                      TextRenderer.DrawText(g, btnBack.Text, btnBack.Font, rect, btnBack.ForeColor, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
                                  End Sub

        AddHandler btnBack.Click, Sub()
                                      Me.Controls.Remove(pnlGradebook)
                                      pnlGradebook.Dispose()
                                      LoadCoursesFromDatabase()
                                  End Sub

        ' Dynamic positioning for title label relative to button
        Dim lblTitle As New Label() With {
            .Text = $"{courseTitle} — Section: {sectionCode} (Teacher: {teacherName})",
            .Font = New Font("Century Gothic", 10.5F, FontStyle.Bold),
            .AutoSize = True
        }
        lblTitle.Location = New Point(btnBack.Right + 18, (pnlTopBar.Height - lblTitle.PreferredHeight) \ 2)

        pnlTopBar.Controls.Add(btnBack)
        pnlTopBar.Controls.Add(lblTitle)

        ' The real gradebook for this class.
        Dim gradebook As New ucGradebook(classId) With {.Dock = DockStyle.Fill}
        gradebook.EmbeddedMode = True
        gradebook.ViewOnly = True

        pnlGradebook.Controls.Add(gradebook)
        pnlGradebook.Controls.Add(pnlTopBar)

        Me.Controls.Add(pnlGradebook)
        pnlGradebook.BringToFront()
    End Sub

    ' =========================================================================
    ' 5. ACADEMIC STATS CARDS
    ' =========================================================================
    Private Sub LoadAcademicStats()
        If flowStats Is Nothing Then Return
        flowStats.DisposeAndClear()

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

End Class
