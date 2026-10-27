Imports System.Data
Imports MySqlConnector

''' <summary>
''' Notifications.
'''   Requests to the admin (teacher -> admin): recipient_user_id IS NULL. Admin presses Review.
'''   Notices (admin -> teacher/student, teacher -> student grade release): recipient_user_id = the user.
'''     They carry is_read, which drives the red badge on the bell.
''' List columns: notification_id, type, message, status, created_at, related_class_id, is_read, kind
'''   kind = "Request" (sent TO the admin) or "Notice" (sent TO this user).
''' </summary>
Public Class NotificationService

  Private Const InsertNoticeSql As String =
    "INSERT INTO notification (sender_id, recipient_user_id, type, message, related_class_id, status) " &
    "VALUES (@s, @r, @t, @m, @c, 'Completed')"

  ' ---------------------------------------------------------------
  ' Sending
  ' ---------------------------------------------------------------

  ''' <summary>Called inside the transaction that releases grades: notice to the admin + one notice per student.</summary>
  Friend Shared Sub NotifyGradesSaved(tx As DbTx, classId As Integer, teacherUserId As Integer)
    Dim msg As String = Convert.ToString(tx.Scalar(
      "SELECT CONCAT(tp.first_name, ' ', tp.last_name, ' saved the final grades of ', co.course_code, ' - ', s.section_code, " &
      "' (', t.school_year, ' Sem ', t.semester, '). Grades are now visible to students.') " &
      "FROM classes c JOIN teacher_profile tp ON tp.teacher_id = c.teacher_id " &
      "JOIN course co ON co.course_id = c.course_id JOIN section s ON s.section_id = c.section_id " &
      "JOIN academic_term t ON t.term_id = c.term_id WHERE c.class_id = @c", DatabaseHelper.P("@c", classId)))

    Dim pending As Object = tx.Scalar(
      "SELECT notification_id FROM notification WHERE type = 'Grade Release' AND status = 'Pending' " &
      "AND recipient_user_id IS NULL AND related_class_id = @c LIMIT 1", DatabaseHelper.P("@c", classId))

    If pending IsNot Nothing Then
      tx.Exec("UPDATE notification SET message = @m, created_at = NOW() WHERE notification_id = @n",
              DatabaseHelper.P("@m", msg), DatabaseHelper.P("@n", pending))
    Else
      tx.Exec("INSERT INTO notification (sender_id, type, message, related_class_id) VALUES (@u, 'Grade Release', @m, @c)",
              DatabaseHelper.P("@u", teacherUserId), DatabaseHelper.P("@m", msg), DatabaseHelper.P("@c", classId))
    End If

    ' one notice per student who now has a grade (skip if they still have an unread one for this class)
    tx.Exec(
      "INSERT INTO notification (sender_id, recipient_user_id, type, message, related_class_id, status) " &
      "SELECT @t, sp.user_id, 'Grade Release', " &
      "       CONCAT('Official grades for ', co.course_code, ' - ', co.course_name, ' are now available.'), c.class_id, 'Completed' " &
      "FROM enrollment e JOIN student_profile sp ON sp.student_id = e.student_id " &
      "JOIN classes c ON c.class_id = e.class_id JOIN course co ON co.course_id = c.course_id " &
      "WHERE e.class_id = @c AND e.status <> 'Dropped' AND e.final_grade IS NOT NULL " &
      "AND NOT EXISTS (SELECT 1 FROM notification n WHERE n.recipient_user_id = sp.user_id " &
      "                AND n.related_class_id = c.class_id AND n.type = 'Grade Release' AND n.is_read = 0)",
      DatabaseHelper.P("@t", teacherUserId), DatabaseHelper.P("@c", classId))
  End Sub

  ''' <summary>Teacher sends a Class Update or Add-Drop-Transfer request to the admin.</summary>
  Public Shared Sub SendRequest(type As String, message As String, Optional classId As Integer? = Nothing)
    If Not SessionManager.IsTeacher Then Throw New InvalidOperationException("Only teachers can send requests.")
    DatabaseHelper.ExecuteNonQuery(
      "INSERT INTO notification (sender_id, type, message, related_class_id) VALUES (@u, @t, @m, @c)",
      {DatabaseHelper.P("@u", SessionManager.CurrentUserId), DatabaseHelper.P("@t", type),
       DatabaseHelper.P("@m", message), DatabaseHelper.P("@c", If(classId.HasValue, CObj(classId.Value), Nothing))})
  End Sub

  ''' <summary>Admin -> teacher notice (e.g. "you were assigned a class"). Silent if the teacher does not exist.</summary>
  Public Shared Sub NotifyTeacher(teacherId As String, message As String, Optional classId As Integer? = Nothing)
    If Not SessionManager.IsAdmin Then Return
    Dim uid As Object = DatabaseHelper.ExecuteScalar("SELECT user_id FROM teacher_profile WHERE teacher_id = @t",
                                                     {DatabaseHelper.P("@t", teacherId)})
    If uid Is Nothing Then Return
    SendNotice(Convert.ToInt32(uid), "Class Update", message, classId)
  End Sub

  Public Shared Sub SendNotice(recipientUserId As Integer, type As String, message As String, Optional classId As Integer? = Nothing)
    DatabaseHelper.ExecuteNonQuery(InsertNoticeSql,
      {DatabaseHelper.P("@s", SessionManager.CurrentUserId), DatabaseHelper.P("@r", recipientUserId),
       DatabaseHelper.P("@t", type), DatabaseHelper.P("@m", message),
       DatabaseHelper.P("@c", If(classId.HasValue, CObj(classId.Value), Nothing))})
  End Sub

  ''' <summary>Same as SendNotice but inside an existing transaction.</summary>
  Friend Shared Sub SendNotice(tx As DbTx, recipientUserId As Integer, type As String, message As String, Optional classId As Integer? = Nothing)
    tx.Exec(InsertNoticeSql,
      DatabaseHelper.P("@s", SessionManager.CurrentUserId), DatabaseHelper.P("@r", recipientUserId),
      DatabaseHelper.P("@t", type), DatabaseHelper.P("@m", message),
      DatabaseHelper.P("@c", If(classId.HasValue, CObj(classId.Value), Nothing)))
  End Sub

  ' ---------------------------------------------------------------
  ' Reading
  ' ---------------------------------------------------------------

  Public Shared Function ListForCurrentUser() As DataTable
    Dim cols As String = "notification_id, type, message, status, created_at, related_class_id, "
    Select Case SessionManager.CurrentRole
      Case "Admin"
        Return DatabaseHelper.ExecuteQuery(
          "SELECT " & cols & "1 AS is_read, 'Request' AS kind FROM notification WHERE recipient_user_id IS NULL " &
          "ORDER BY (status = 'Pending') DESC, created_at DESC LIMIT 100")
      Case "Teacher"
        Return DatabaseHelper.ExecuteQuery(
          "SELECT " & cols & "is_read, 'Notice' AS kind FROM notification WHERE recipient_user_id = @u " &
          "UNION ALL " &
          "SELECT " & cols & "1 AS is_read, 'Request' AS kind FROM notification WHERE sender_id = @u AND recipient_user_id IS NULL " &
          "ORDER BY created_at DESC LIMIT 100", {DatabaseHelper.P("@u", SessionManager.CurrentUserId)})
      Case Else
        Return DatabaseHelper.ExecuteQuery(
          "SELECT " & cols & "is_read, 'Notice' AS kind FROM notification WHERE recipient_user_id = @u " &
          "ORDER BY created_at DESC LIMIT 100", {DatabaseHelper.P("@u", SessionManager.CurrentUserId)})
    End Select
  End Function

  ''' <summary>Number on the red bell badge: admin = requests waiting for review, others = unread notices.</summary>
  Public Shared Function UnreadCount() As Integer
    If Not SessionManager.IsLoggedIn Then Return 0
    If SessionManager.IsAdmin Then
      Return DatabaseHelper.ScalarInt("SELECT COUNT(*) FROM notification WHERE recipient_user_id IS NULL AND status = 'Pending'")
    End If
    Return DatabaseHelper.ScalarInt("SELECT COUNT(*) FROM notification WHERE recipient_user_id = @u AND is_read = 0",
                                    {DatabaseHelper.P("@u", SessionManager.CurrentUserId)})
  End Function

  Public Shared Sub MarkAllRead()
    If SessionManager.IsAdmin OrElse Not SessionManager.IsLoggedIn Then Return
    DatabaseHelper.ExecuteNonQuery("UPDATE notification SET is_read = 1 WHERE recipient_user_id = @u AND is_read = 0",
                                   {DatabaseHelper.P("@u", SessionManager.CurrentUserId)})
  End Sub

  ''' <summary>Admin marks a request as reviewed.</summary>
  Public Shared Sub Complete(notificationId As Integer)
    If Not SessionManager.IsAdmin Then Throw New InvalidOperationException("Only an admin can review notifications.")
    DatabaseHelper.ExecuteNonQuery(
      "UPDATE notification SET status = 'Completed', reviewed_by = @u, reviewed_at = NOW() " &
      "WHERE notification_id = @n AND status = 'Pending'",
      {DatabaseHelper.P("@u", SessionManager.CurrentUserId), DatabaseHelper.P("@n", notificationId)})
  End Sub
End Class
