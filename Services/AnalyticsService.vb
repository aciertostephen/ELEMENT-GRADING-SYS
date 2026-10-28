Imports System.Data
''' <summary>Real numbers for the student analytics charts (two of three were hardcoded).</summary>
Public Class AnalyticsService

  ''' <summary>Available active-term classes for the student's subject filter.</summary>
  Public Shared Function ActiveCourseClasses(studentId As String) As DataTable
    Return DatabaseHelper.ExecuteQuery(
      "SELECT c.class_id, CONCAT(co.course_code, ' - ', co.course_name, ' (', s.section_code, ')') AS label " &
      "FROM enrollment e JOIN classes c ON c.class_id = e.class_id " &
      "JOIN academic_term t ON t.term_id = c.term_id AND t.is_active = 1 " &
      "JOIN course co ON co.course_id = c.course_id JOIN section s ON s.section_id = c.section_id " &
      "WHERE e.student_id = @s AND e.status <> 'Dropped' ORDER BY co.course_code, s.section_code",
      {DatabaseHelper.P("@s", studentId)})
  End Function

  ''' <summary>Average scored percent by rubric category, for all active classes or one selected class.</summary>
  Public Shared Function ComponentAverages(studentId As String, Optional classId As Integer = 0) As List(Of KeyValuePair(Of String, Double))
    Dim sql As String =
      "SELECT UPPER(r.component_name) AS label, AVG(ge.score / gi.max_score * 100) AS pct " &
      "FROM enrollment e JOIN classes c ON c.class_id = e.class_id " &
      "JOIN academic_term t ON t.term_id = c.term_id AND t.is_active = 1 " &
      "JOIN grading_rubric r ON r.class_id = c.class_id " &
      "JOIN grade_item gi ON gi.rubric_id = r.rubric_id AND gi.max_score > 0 " &
      "JOIN grade_entry ge ON ge.item_id = gi.item_id AND ge.enrollment_id = e.enrollment_id " &
      "WHERE e.student_id = @s AND e.status <> 'Dropped' AND ge.score IS NOT NULL "
    Dim ps As New List(Of MySqlConnector.MySqlParameter) From {DatabaseHelper.P("@s", studentId)}
    If classId > 0 Then
      sql &= "AND c.class_id = @c "
      ps.Add(DatabaseHelper.P("@c", classId))
    End If
    sql &= "GROUP BY UPPER(r.component_name) ORDER BY MIN(r.rubric_id)"
    Dim dt = DatabaseHelper.ExecuteQuery(sql, ps.ToArray())
    Dim list As New List(Of KeyValuePair(Of String, Double))
    For Each r As DataRow In dt.Rows
      list.Add(New KeyValuePair(Of String, Double)(r("label").ToString(), Convert.ToDouble(r("pct"))))
    Next
    Return list
  End Function

  ''' <summary>Average released final grade by release week, month or year.</summary>
  Public Shared Function GradeTrend(studentId As String, period As String) As List(Of KeyValuePair(Of String, Double))
    Dim bucketFormat As String
    Select Case If(period, "").Trim().ToLowerInvariant()
      Case "week" : bucketFormat = "%x-W%v"
      Case "year" : bucketFormat = "%Y"
      Case Else : bucketFormat = "%Y-%m"
    End Select
    Dim dt = DatabaseHelper.ExecuteQuery(
      "SELECT DATE_FORMAT(c.grades_released_at, @fmt) AS label, AVG(e.final_grade) AS avg_grade " &
      "FROM enrollment e JOIN classes c ON c.class_id = e.class_id " &
      "WHERE e.student_id = @s AND e.final_grade IS NOT NULL AND c.grades_released_at IS NOT NULL " &
      "GROUP BY DATE_FORMAT(c.grades_released_at, @fmt) ORDER BY MIN(c.grades_released_at)",
      {DatabaseHelper.P("@fmt", bucketFormat), DatabaseHelper.P("@s", studentId)})
    Dim list As New List(Of KeyValuePair(Of String, Double))
    For Each row As DataRow In dt.Rows
      list.Add(New KeyValuePair(Of String, Double)(row("label").ToString(), Convert.ToDouble(row("avg_grade"))))
    Next
    Return list
  End Function

  ''' <summary>Average released final grade per term, oldest to newest.</summary>
  Public Shared Function TermTrend(studentId As String) As List(Of KeyValuePair(Of String, Double))
    Dim dt = DatabaseHelper.ExecuteQuery(
      "SELECT CONCAT(t.school_year, ' S', t.semester) AS label, AVG(e.final_grade) AS avg_grade " &
      "FROM enrollment e JOIN classes c ON c.class_id = e.class_id " &
      "JOIN academic_term t ON t.term_id = c.term_id " &
      "WHERE e.student_id = @s AND e.final_grade IS NOT NULL AND c.grades_released_at IS NOT NULL " &
      "GROUP BY t.term_id, t.school_year, t.semester ORDER BY t.school_year, t.semester", {DatabaseHelper.P("@s", studentId)})
    Dim list As New List(Of KeyValuePair(Of String, Double))
    For Each r As DataRow In dt.Rows
      list.Add(New KeyValuePair(Of String, Double)(r("label").ToString(), Convert.ToDouble(r("avg_grade"))))
    Next
    Return list
  End Function
End Class
