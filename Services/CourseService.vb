Imports MySqlConnector

Public Class CourseService

  Public Shared Function Create(code As String, name As String, units As Integer) As Integer
    If Not SessionManager.IsAdmin Then Throw New InvalidOperationException("Only an administrator can create a course.")
    code = If(code, "").Trim().ToUpperInvariant()
    name = If(name, "").Trim()
    If code.Length = 0 OrElse code.Length > 15 Then Throw New InvalidOperationException("Course code is required (max 15 characters).")
    If name.Length = 0 OrElse name.Length > 100 Then Throw New InvalidOperationException("Course name is required (max 100 characters).")
    If units < 1 OrElse units > 9 Then Throw New InvalidOperationException("Units must be between 1 and 9.")

    Dim exists As Integer = DatabaseHelper.ScalarInt("SELECT COUNT(*) FROM course WHERE course_code = @c",
                                                     {DatabaseHelper.P("@c", code)})
    If exists > 0 Then Throw New InvalidOperationException("A course with code " & code & " already exists.")

    Try
      DatabaseHelper.ExecuteNonQuery(
        "INSERT INTO course (course_code, course_name, units) VALUES (@c, @n, @u)",
        {DatabaseHelper.P("@c", code), DatabaseHelper.P("@n", name), DatabaseHelper.P("@u", units)})
    Catch ex As MySqlException When ex.Number = 1062
      Throw New InvalidOperationException("That course already exists.")
    End Try
    Return DatabaseHelper.ScalarInt("SELECT course_id FROM course WHERE course_code = @c", {DatabaseHelper.P("@c", code)})
  End Function

  Public Shared Function ListForCombo() As DataTable
    Return DatabaseHelper.ExecuteQuery(
      "SELECT course_id, CONCAT(course_code, ' - ', course_name) AS display FROM course ORDER BY course_code")
  End Function
End Class
