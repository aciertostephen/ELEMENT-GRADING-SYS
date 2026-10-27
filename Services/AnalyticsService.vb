Imports System.Data
''' <summary>Real numbers for the student analytics charts (two of three were hardcoded).</summary>
Public Class AnalyticsService

  ''' <summary>Average % per grading component (Participation, Quizzes, ...) over the ACTIVE term.</summary>
  Public Shared Function ComponentAverages(studentId As String) As List(Of KeyValuePair(Of String, Double))
    Dim dt = DatabaseHelper.ExecuteQuery(
      "SELECT UPPER(r.component_name) AS label, AVG(ge.score / gi.max_score * 100) AS pct " &
      "FROM enrollment e JOIN classes c ON c.class_id = e.class_id " &
      "JOIN academic_term t ON t.term_id = c.term_id AND t.is_active = 1 " &
      "JOIN grading_rubric r ON r.class_id = c.class_id " &
      "JOIN grade_item gi ON gi.rubric_id = r.rubric_id AND gi.max_score > 0 " &
      "JOIN grade_entry ge ON ge.item_id = gi.item_id AND ge.enrollment_id = e.enrollment_id " &
      "WHERE e.student_id = @s AND e.status <> 'Dropped' AND ge.score IS NOT NULL " &
      "GROUP BY UPPER(r.component_name) ORDER BY MIN(r.rubric_id)", {DatabaseHelper.P("@s", studentId)})
    Dim list As New List(Of KeyValuePair(Of String, Double))
    For Each r As DataRow In dt.Rows
      list.Add(New KeyValuePair(Of String, Double)(r("label").ToString(), Convert.ToDouble(r("pct"))))
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
