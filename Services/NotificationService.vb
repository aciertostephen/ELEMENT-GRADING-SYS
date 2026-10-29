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

  ''' <summary>Called inside the grade-release transaction: a pending admin review plus notices to graded students.</summary>
  Friend Shared Sub NotifyGradesPublished(tx As DbTx, classId As Integer, teacherUserId As Integer, period As String)
    If period <> "Midterm" AndAlso period <> "Final" Then Throw New InvalidOperationException("Choose Midterm or Final.")
    Dim gradeColumn As String = If(period = "Midterm", "midterm_grade", "finals_grade")
    Dim msg As String = Convert.ToString(tx.Scalar(
      "SELECT CONCAT(tp.first_name, ' ', tp.last_name, ' published the official ', @p, ' period grades for ', co.course_code, ' - ', s.section_code, " &
      "' (', t.school_year, ' Sem ', t.semester, ').') " &
      "FROM classes c JOIN teacher_profile tp ON tp.teacher_id = c.teacher_id " &
      "JOIN course co ON co.course_id = c.course_id JOIN section s ON s.section_id = c.section_id " &
      "JOIN academic_term t ON t.term_id = c.term_id WHERE c.class_id = @c",
      DatabaseHelper.P("@c", classId), DatabaseHelper.P("@p", period)))

    Dim pending As Object = tx.Scalar(
      "SELECT notification_id FROM notification WHERE type = 'Grade Release' AND status = 'Pending' " &
      "AND recipient_user_id IS NULL AND related_class_id = @c " &
      "AND (message LIKE CONCAT('% published the official ', @p, ' period grades for %') " &
      "     OR message LIKE CONCAT('% saved the ', @p, ' grades of %')) LIMIT 1",
      DatabaseHelper.P("@c", classId), DatabaseHelper.P("@p", period))

    If pending IsNot Nothing Then
      tx.Exec("UPDATE notification SET message = @m, created_at = NOW() WHERE notification_id = @n",
              DatabaseHelper.P("@m", msg), DatabaseHelper.P("@n", pending))
    Else
      tx.Exec("INSERT INTO notification (sender_id, type, message, related_class_id) VALUES (@u, 'Grade Release', @m, @c)",
              DatabaseHelper.P("@u", teacherUserId), DatabaseHelper.P("@m", msg), DatabaseHelper.P("@c", classId))
    End If

    ' One unread notice per student and grading period; Midterm and Final releases stay distinct.
    tx.Exec(
      "INSERT INTO notification (sender_id, recipient_user_id, type, message, related_class_id, status) " &
      "SELECT @t, sp.user_id, 'Grade Release', " &
      "       CONCAT('Your official ', @p, ' period grade for ', co.course_code, ' - ', co.course_name, ' has been published.', " &
      "              IF(@p = 'Final', ' The semester average appears after both Midterm and Final grades are published.', '')), " &
      "       c.class_id, 'Completed' " &
      "FROM enrollment e JOIN student_profile sp ON sp.student_id = e.student_id " &
      "JOIN classes c ON c.class_id = e.class_id JOIN course co ON co.course_id = c.course_id " &
      "WHERE e.class_id = @c AND e.status <> 'Dropped' AND e." & gradeColumn & " IS NOT NULL " &
      "AND NOT EXISTS (SELECT 1 FROM notification n WHERE n.recipient_user_id = sp.user_id " &
      "                AND n.related_class_id = c.class_id AND n.type = 'Grade Release' AND n.is_read = 0 " &
      "                AND (n.message LIKE CONCAT('Your official ', @p, ' period grade for %') " &
      "                     OR n.message LIKE CONCAT('Your ', @p, ' grades for %')))",
      DatabaseHelper.P("@t", teacherUserId), DatabaseHelper.P("@c", classId), DatabaseHelper.P("@p", period))
  End Sub

  ''' <summary>Notifies a student when a teacher adds or re-enrolls them in a class roster.</summary>
  Friend Shared Sub NotifyStudentAssigned(tx As DbTx, classId As Integer, studentId As String)
    Dim recipient As Object = tx.Scalar(
      "SELECT sp.user_id FROM student_profile sp WHERE sp.student_id = @s", DatabaseHelper.P("@s", studentId))
    If recipient Is Nothing OrElse Convert.IsDBNull(recipient) Then Return

    Dim label As String = Convert.ToString(tx.Scalar(
      "SELECT CONCAT(co.course_code, ' - ', co.course_name, ' (', s.section_code, ', ', t.school_year, ')') " &
      "FROM classes c JOIN course co ON co.course_id = c.course_id " &
      "JOIN section s ON s.section_id = c.section_id JOIN academic_term t ON t.term_id = c.term_id " &
      "WHERE c.class_id = @c", DatabaseHelper.P("@c", classId)))
    tx.Exec(InsertNoticeSql,
      DatabaseHelper.P("@s", SessionManager.CurrentUserId), DatabaseHelper.P("@r", Convert.ToInt32(recipient)),
      DatabaseHelper.P("@t", "Class Update"), DatabaseHelper.P("@m", "You were added to " & label & ". Open the class to view its gradebook."),
      DatabaseHelper.P("@c", classId))
  End Sub

  ''' <summary>Teacher sends a Class Update or Add-Drop-Transfer request to the admin.</summary>
  Public Shared Sub SendRequest(type As String, message As String, Optional classId As Integer? = Nothing)
    If Not SessionManager.IsTeacher Then Throw New InvalidOperationException("Only teachers can send requests.")
    If classId.HasValue AndAlso Not ClassService.TeacherOwnsClass(classId.Value, SessionManager.CurrentUserId) Then
      Throw New InvalidOperationException("You can only send a request about one of your classes.")
    End If
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

  Public Shared Sub MarkRead(notificationId As Integer)
    If SessionManager.IsAdmin OrElse Not SessionManager.IsLoggedIn Then Return
    DatabaseHelper.ExecuteNonQuery(
      "UPDATE notification SET is_read = 1 WHERE notification_id = @n AND recipient_user_id = @u AND is_read = 0",
      {DatabaseHelper.P("@n", notificationId), DatabaseHelper.P("@u", SessionManager.CurrentUserId)})
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
