Imports MySqlConnector

Public Class ucTranscript

    Private _userId As Integer

    Public Sub New()
        InitializeComponent()
        _userId = 14 ' Default fallback user ID (e.g., Julius San Gaspar from your database dump)
    End Sub

    Public Sub New(userId As Integer)
        Me.New()
        _userId = userId
    End Sub

    Private Sub ucTranscript_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadStudentTranscript()
    End Sub

    Private Sub LoadStudentTranscript()
        dgvTranscript.Rows.Clear()
        Try
            ' Query student details, section, enrolled courses, units, final grades, grade equivalents, and professor names
            Dim query As String = "SELECT sp.first_name, sp.middle_name, sp.last_name, s.section_code, " &
                                 "co.course_code, co.course_name, co.units, e.final_grade, e.grade_equivalent, " &
                                 "tp.last_name AS prof_last, tp.first_name AS prof_first " &
                                 "FROM student_profile sp " &
                                 "JOIN section s ON sp.section_id = s.section_id " &
                                 "JOIN enrollment e ON sp.student_id = e.student_id " &
                                 "JOIN classes c ON e.class_id = c.class_id " &
                                 "JOIN course co ON c.course_id = co.course_id " &
                                 "JOIN teacher_profile tp ON c.teacher_id = tp.teacher_id " &
                                 "WHERE sp.user_id = @uid"

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
                    Dim courseCode As String = row("course_code").ToString()
                    Dim courseName As String = row("course_name").ToString()
                    Dim units As Decimal = Convert.ToDecimal(row("units"))

                    ' Grades from the enrollment table
                    Dim finalGrade As Decimal? = If(IsDBNull(row("final_grade")), Nothing, CType(row("final_grade"), Decimal?))
                    Dim gradeEquiv As Decimal = If(IsDBNull(row("grade_equivalent")), 0.0D, Convert.ToDecimal(row("grade_equivalent")))

                    ' Format Professor's Name (Last Name, First Name)
                    Dim professorName As String = $"{row("prof_last")}, {row("prof_first")}"

                    ' Grade shown in the grid: the computed grade equivalent (same scale as the GWA), or "IP" if not graded yet.
                    ' If your dashboard shows the percentage instead, swap to the line below:
                    ' Dim displayGrade As String = If(finalGrade.HasValue, finalGrade.Value.ToString("0.00"), "IP")
                    Dim displayGrade As String = If(gradeEquiv > 0, gradeEquiv.ToString("0.00"), "IP")

                    ' Add row to DataGridView (Columns: Code, Subjects, Professor, Grade)
                    ' Units is not passed because the grid has no Units column; it is still used for the GWA below.
                    dgvTranscript.Rows.Add(courseCode, courseName, professorName, displayGrade)

                    If gradeEquiv > 0 Then
                        weightedSum += (gradeEquiv * units)
                        totalUnits += units
                    End If
                Next

                ' Calculate General Weighted Average (GWA)
                If totalUnits > 0 Then
                    Dim gwa As Decimal = Math.Round(weightedSum / totalUnits, 2)
                    lblGWA.Text = $"GWA : {gwa.ToString("0.00")}"
                Else
                    lblGWA.Text = "GWA : N/A"
                End If
            Else
                lblStudentInfo.Text = "Student: Record Not Found"
                lblGWA.Text = "GWA : N/A"
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading transcript: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class