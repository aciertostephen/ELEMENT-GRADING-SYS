Imports MySqlConnector
Imports ScottPlot

Public Class ucAnalytics

    Private _studentId As String

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(studentId As String)
        Me.New()
        _studentId = studentId
    End Sub

    Private Sub ucAnalytics_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadGradeStandingChart()
        LoadComponentAnalyticsChart()
        LoadAverageTrendChart()
    End Sub

    Private Sub LoadGradeStandingChart()
        Dim labelsList As New List(Of String)()
        Dim valuesList As New List(Of Double)()

        Try
            ' Query the student's actual courses, course codes, and final grades for the active term
            Dim query As String = "SELECT co.course_code, co.course_name, COALESCE(e.final_grade, 0) AS final_grade " &
                                 "FROM enrollment e " &
                                 "JOIN classes c ON e.class_id = c.class_id " &
                                 "JOIN course co ON c.course_id = co.course_id " &
                                 "JOIN academic_term t ON c.term_id = t.term_id " &
                                 "WHERE e.student_id = @sid AND t.is_active = 1"

            ' Fallback to a default student ID (e.g., Julius San Gaspar: '25-00817') if not passed
            Dim targetStudentId As String = If(String.IsNullOrEmpty(_studentId), "25-00817", _studentId)
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(query, {New MySqlParameter("@sid", targetStudentId)})

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For Each row As DataRow In dt.Rows
                    Dim code As String = row("course_code").ToString()
                    Dim name As String = row("course_name").ToString()
                    Dim grade As Double = Convert.ToDouble(row("final_grade"))

                    ' Combine course code and course name for the chart axis
                    labelsList.Add($"{code}")
                    valuesList.Add(grade)
                Next
            Else
                ' Fallback defaults if query returns empty
                labelsList.AddRange({"GE 008", "CS 103"})
                valuesList.AddRange({92.6, 90.2})
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading analytics courses: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        Dim labels() As String = labelsList.ToArray()
        Dim values() As Double = valuesList.ToArray()

        FormsPlot1.Plot.Clear()
        If values.Length > 0 Then
            Dim bar = FormsPlot1.Plot.Add.Bars(values)
            FormsPlot1.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, labels.Length).Select(Function(i) CDbl(i)).ToArray(), labels)
            FormsPlot1.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45 ' Rotate labels for better readability
        End If
        FormsPlot1.Refresh()
    End Sub

    Private Sub LoadComponentAnalyticsChart()
        Dim labels() As String = {"PARTICIPATION", "QUIZZES", "PROJECT", "MAJOR EXAM"}
        Dim values() As Double = {100, 75, 88, 85}

        FormsPlot2.Plot.Clear()
        FormsPlot2.Plot.Add.Bars(values)
        FormsPlot2.Plot.Axes.Bottom.SetTicks(Enumerable.Range(0, labels.Length).Select(Function(i) CDbl(i)).ToArray(), labels)
        FormsPlot2.Refresh()
    End Sub

    Private Sub LoadAverageTrendChart()
        Dim xs() As Double = {1, 2, 3, 4}
        Dim ys() As Double = {95, 85, 90, 82}

        FormsPlot3.Plot.Clear()
        FormsPlot3.Plot.Add.Scatter(xs, ys)
        FormsPlot3.Refresh()
    End Sub

    Private Sub FormsPlot2_Load(sender As Object, e As EventArgs) Handles FormsPlot2.Load

    End Sub

End Class