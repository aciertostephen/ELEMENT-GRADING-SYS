Imports MySqlConnector

Imports System.IO

Public Class ucTranscript

    Private _userId As Integer
    Private ReadOnly _transcriptRows As New List(Of String())

    Public Sub New()
        InitializeComponent()
        _userId = SessionManager.CurrentUserId ' the logged-in user (no hardcoded student)
    End Sub

    Public Sub New(userId As Integer)
        Me.New()
        _userId = userId
    End Sub

    Private Sub ucTranscript_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadStudentTranscript()
    End Sub

    Private Sub btnExportStudent_Click(sender As Object, e As EventArgs) Handles btnExportStudent.Click
        Using picker As New SaveFileDialog() With {
            .Title = "Export Student Transcript",
            .Filter = "PDF document|*.pdf",
            .DefaultExt = "pdf",
            .AddExtension = True,
            .FileName = "Student_Transcript_" & Date.Now.ToString("yyyyMMdd")
        }
            If picker.ShowDialog(FindForm()) <> DialogResult.OK Then Return
            Try
                Dim info As String() = lblStudentInfo.Text.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
                Dim studentName As String = If(info.Length > 0, info(0).Replace("Student:", "").Trim(), "")
                Dim section As String = If(info.Length > 1, info(1).Replace("Section:", "").Trim(), "")
                Dim termGrade As String = lblGWA.Text.Replace("GWA :", "").Trim()
                Dim metadata = DatabaseHelper.ExecuteQuery(
                    "SELECT DISTINCT at.school_year, at.semester, p.program_code " &
                    "FROM student_profile sp LEFT JOIN section s ON s.section_id = sp.section_id " &
                    "LEFT JOIN program p ON p.program_id = s.program_id " &
                    "LEFT JOIN enrollment e ON e.student_id = sp.student_id " &
                    "LEFT JOIN classes c ON c.class_id = e.class_id " &
                    "LEFT JOIN academic_term at ON at.term_id = c.term_id WHERE sp.user_id = @uid",
                    {New MySqlParameter("@uid", _userId)})
                Dim schoolYears As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                Dim semesters As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                Dim program As String = ""
                If metadata IsNot Nothing Then
                    For Each m As DataRow In metadata.Rows
                        If program = "" AndAlso Not IsDBNull(m("program_code")) Then program = m("program_code").ToString()
                        If Not IsDBNull(m("school_year")) Then schoolYears.Add(m("school_year").ToString())
                        If Not IsDBNull(m("semester")) Then
                            Select Case Convert.ToInt32(m("semester"))
                                Case 1 : semesters.Add("1st Semester")
                                Case 2 : semesters.Add("2nd Semester")
                                Case Else : semesters.Add("Summer")
                            End Select
                        End If
                    Next
                End If
                Dim schoolYear As String = If(schoolYears.Count = 0, "", If(schoolYears.Count = 1, schoolYears.First(), "Multiple"))
                Dim semester As String = If(semesters.Count = 0, "", If(semesters.Count = 1, semesters.First(), "Multiple"))
                Dim template As String = Path.Combine(Application.StartupPath, "Resources", "StudentTranscriptTemplate.jpg")
                TranscriptTemplatePdfExporter.Export(picker.FileName, template, studentName, section, termGrade,
                    schoolYear, "Semestral", program, semester, _transcriptRows)
                MessageBox.Show("The transcript PDF was exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.None)
            Catch ex As Exception
                MessageBox.Show("Could not export the transcript PDF: " & ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.None)
            End Try
        End Using
    End Sub

    Private Sub LoadStudentTranscript()
        dgvTranscript.Rows.Clear()
        _transcriptRows.Clear()
        Try
            ' Query student details, section, enrolled courses, units, final grades, grade equivalents, and professor names
            Dim query As String = "SELECT sp.first_name, sp.middle_name, sp.last_name, s.section_code, " &
                                 "co.course_code, co.course_name, co.units, e.status AS enroll_status, " &
                                 "CASE WHEN c.midterm_released_at IS NOT NULL AND c.finals_released_at IS NOT NULL " &
                                 "AND e.midterm_grade IS NOT NULL AND e.finals_grade IS NOT NULL " &
                                 "THEN ROUND((e.midterm_grade + e.finals_grade) / 2, 2) ELSE NULL END AS final_grade, " &
                                 "CASE WHEN c.midterm_released_at IS NOT NULL AND c.finals_released_at IS NOT NULL " &
                                 "AND e.midterm_grade IS NOT NULL AND e.finals_grade IS NOT NULL " &
                                 "THEN (SELECT gs.grade_equivalent FROM grade_scale gs " &
                                 "WHERE gs.min_percent <= ROUND((e.midterm_grade + e.finals_grade) / 2, 2) " &
                                 "ORDER BY gs.min_percent DESC LIMIT 1) ELSE NULL END AS grade_equivalent, " &
                                 "CASE WHEN c.midterm_released_at IS NOT NULL AND c.finals_released_at IS NOT NULL " &
                                 "AND e.midterm_grade IS NOT NULL AND e.finals_grade IS NOT NULL " &
                                 "THEN (SELECT gs.remarks FROM grade_scale gs " &
                                 "WHERE gs.min_percent <= ROUND((e.midterm_grade + e.finals_grade) / 2, 2) " &
                                 "ORDER BY gs.min_percent DESC LIMIT 1) ELSE NULL END AS grade_remarks, " &
                                 "tp.last_name AS prof_last, tp.first_name AS prof_first, at.school_year AS term_school_year, at.semester AS term_semester " &
                                 "FROM student_profile sp " &
                                 "LEFT JOIN section s ON sp.section_id = s.section_id " &
                                 "LEFT JOIN enrollment e ON sp.student_id = e.student_id " &
                                 "LEFT JOIN classes c ON e.class_id = c.class_id " &
                                 "LEFT JOIN course co ON c.course_id = co.course_id " &
                                 "LEFT JOIN program p ON s.program_id = p.program_id " &
                                 "LEFT JOIN academic_term at ON c.term_id = at.term_id " &
                                 "LEFT JOIN teacher_profile tp ON c.teacher_id = tp.teacher_id " &
                                 "WHERE sp.user_id = @uid " &
                                 "ORDER BY c.term_id, co.course_code"

            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(query, {New MySqlParameter("@uid", _userId)})

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim r0 = dt.Rows(0)

                ' Middle initial: strip any existing period so we don't end up with "C.."
                Dim mi As String = If(IsDBNull(r0("middle_name")), "", r0("middle_name").ToString().Trim().TrimEnd("."c))
                Dim studentName As String = $"{r0("last_name")}, {r0("first_name")}" & If(mi = "", "", " " & mi & ".")
                Dim sectionCode As String = r0("section_code").ToString()

                ' Display student metadata
                lblStudentInfo.Text = $"Student: {studentName}" & vbCrLf & $"Section: {sectionCode}"

                Dim totalUnits As Decimal = 0
                Dim weightedSum As Decimal = 0

                ' Populate transcript courses, professor names, final grades, and compute GWA
                For Each row As DataRow In dt.Rows
                    If IsDBNull(row("course_code")) Then Continue For      ' student not enrolled in any class yet
                    Dim courseCode As String = row("course_code").ToString()
                    Dim courseName As String = row("course_name").ToString()
                    Dim units As Decimal = Convert.ToDecimal(row("units"))

                    ' Grades from the enrollment table
                    Dim finalGrade As Decimal? = If(IsDBNull(row("final_grade")), Nothing, CType(row("final_grade"), Decimal?))
                    Dim gradeEquiv As Decimal? = Nothing
                    If Not IsDBNull(row("grade_equivalent")) Then gradeEquiv = Convert.ToDecimal(row("grade_equivalent"))

                    ' Format Professor's Name (Last Name, First Name)
                    Dim professorName As String = If(IsDBNull(row("prof_last")), "Unassigned", $"{row("prof_last")}, {row("prof_first")}")

                    Dim enrollStatus As String = Convert.ToString(row("enroll_status"))
                    Dim displayGrade As String
                    If enrollStatus = "Dropped" Then
                        displayGrade = "DRP"
                    ElseIf enrollStatus = "INC" Then
                        displayGrade = "INC"
                    ElseIf finalGrade.HasValue Then
                        ' The semester percentage can be valid even when grade_scale has no matching equivalent.
                        ' Never label a completed, officially released semester as IP for that lookup failure.
                        displayGrade = If(gradeEquiv.HasValue,
                                          gradeEquiv.Value.ToString("0.00"),
                                          finalGrade.Value.ToString("0.00") & "%")
                    Else
                        displayGrade = "IP"
                    End If
                    Dim remarks As String = If(enrollStatus = "Dropped", "Dropped",
                                            If(enrollStatus = "INC", "Incomplete",
                                            If(IsDBNull(row("grade_remarks")),
                                               If(finalGrade.HasValue, "Grade-scale equivalent unavailable", "In Progress"),
                                               row("grade_remarks").ToString())))
                    _transcriptRows.Add({courseCode, courseName, professorName, displayGrade, remarks})

                    ' Add row to DataGridView (Columns: Code, Subjects, Professor, Grade)
                    ' Units is not passed because the grid has no Units column; it is still used for the GWA below.
                    dgvTranscript.Rows.Add(courseCode, courseName, professorName, displayGrade)

                    If gradeEquiv.HasValue AndAlso enrollStatus <> "Dropped" Then
                        weightedSum += (gradeEquiv.Value * units)
                        totalUnits += units
                    End If
                Next

                ' Calculate General Weighted Average (GWA)
                If totalUnits > 0 Then
                    Dim gwa As Decimal = Math.Round(weightedSum / totalUnits, 2, MidpointRounding.AwayFromZero)
                    lblGWA.Text = $"GWA : {gwa.ToString("0.00")}"
                Else
                    lblGWA.Text = "GWA : N/A"
                End If
            Else
                lblStudentInfo.Text = "Student: Record Not Found"
                lblGWA.Text = "GWA : N/A"
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading transcript: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.None)
        End Try
    End Sub

End Class
