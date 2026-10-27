Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

' =====================================================================
' Notifications: list, red unread badge on the bell (refreshed every 30 s), and "Open class".
' Start/stop polling is called from MainForm.Navigation (SetupNavForRole / ShowLoginScreen).
' =====================================================================
Partial Public Class MainForm

  Private _notifTimer As System.Windows.Forms.Timer
  Private _lblBadge As Label

  ' ---------------- badge + polling ----------------
  Friend Sub StartNotificationPolling()
    EnsureBadge()
    If _notifTimer Is Nothing Then
      _notifTimer = New System.Windows.Forms.Timer() With {.Interval = 30000}
      AddHandler _notifTimer.Tick, Sub(s, ev) RefreshNotificationBadge()
    End If
    _notifTimer.Start()
    RefreshNotificationBadge()
  End Sub

  Friend Sub StopNotificationPolling()
    If _notifTimer IsNot Nothing Then _notifTimer.Stop()
    If _lblBadge IsNot Nothing Then _lblBadge.Visible = False
  End Sub

  Private Sub EnsureBadge()
    If _lblBadge IsNot Nothing OrElse btnNotif Is Nothing Then Return
    _lblBadge = New Label() With {
      .AutoSize = False, .Size = New Size(20, 20), .BackColor = Color.Firebrick, .ForeColor = Color.White,
      .TextAlign = ContentAlignment.MiddleCenter, .Font = New Font("Segoe UI", 8, FontStyle.Bold),
      .Cursor = Cursors.Hand, .Visible = False}
    Using gp As New Drawing2D.GraphicsPath()
      gp.AddEllipse(0, 0, 20, 20)
      _lblBadge.Region = New Region(gp)
    End Using
    _lblBadge.Location = New Point(Math.Max(btnNotif.Width - 24, 0), 2)
    AddHandler _lblBadge.Click, Sub(s, ev) btnNotif.PerformClick()
    btnNotif.Controls.Add(_lblBadge)
    _lblBadge.BringToFront()
  End Sub

  Private Sub RefreshNotificationBadge()
    If _lblBadge Is Nothing Then Return
    Try
      If Not SessionManager.IsLoggedIn Then
        _lblBadge.Visible = False
        Return
      End If
      Dim n As Integer = NotificationService.UnreadCount()
      _lblBadge.Text = If(n > 9, "9+", n.ToString())
      _lblBadge.Visible = n > 0
    Catch
      _lblBadge.Visible = False     ' a polling error must never pop up a dialog
    End Try
  End Sub

  ' ---------------- list ----------------
  Private Sub LoadNotifications()
    If flowNotifList Is Nothing Then Return
    flowNotifList.DisposeAndClear()
    Try
      Dim dt As DataTable = NotificationService.ListForCurrentUser()
      If dt.Rows.Count = 0 Then
        flowNotifList.Controls.Add(New Label() With {
          .Text = "No notifications.", .AutoSize = True, .Padding = New Padding(12),
          .ForeColor = UiTheme.Muted, .Font = UiFont(10)})
      Else
        For Each row As DataRow In dt.Rows
          AddNotification(row)
        Next
      End If
      ' everything shown now counts as read (admin requests stay until reviewed)
      NotificationService.MarkAllRead()
    Catch ex As Exception
      Msg.Fail("load notifications", ex)
    End Try
    RefreshNotificationBadge()
  End Sub

  Private Sub AddNotification(row As DataRow)
    Dim id As Integer = Convert.ToInt32(row("notification_id"))
    Dim type As String = row("type").ToString()
    Dim message As String = row("message").ToString()
    Dim kind As String = row("kind").ToString()
    Dim isDone As Boolean = row("status").ToString() = "Completed"
    Dim isUnread As Boolean = If(kind = "Notice", Convert.ToInt32(row("is_read")) = 0, SessionManager.IsAdmin AndAlso Not isDone)
    Dim stamp As String = Convert.ToDateTime(row("created_at")).ToString("hh:mm tt - dd-MM-yyyy")
    Dim classId As Integer? = Nothing
    If Not IsDBNull(row("related_class_id")) Then classId = Convert.ToInt32(row("related_class_id"))

    Dim item As New ucNotificationItem()
    item.Width = Math.Max(flowNotifList.Width - 25, 100)
    item.SetData(stamp, If(isUnread, "NEW  ", "") & "[" & type & "] " & message, isDone)

    If kind = "Notice" Then
      item.SetStatusOnly(If(type = "Grade Release", "Released", "Notice"))
    ElseIf SessionManager.IsAdmin Then
      If Not isDone Then AddHandler item.ActionClicked, Sub() ReviewNotification(id, type, message)
    Else
      item.SetStatusOnly(If(isDone, "Reviewed", "Pending"))
    End If

    If classId.HasValue Then
      Dim cid As Integer = classId.Value
      item.SetOpenLink(Sub() OpenClassFromNotification(cid))
    End If
    flowNotifList.Controls.Add(item)
  End Sub

  Private Sub ReviewNotification(id As Integer, type As String, message As String)
    Try
      If Msg.Confirm(message & vbCrLf & vbCrLf & "Mark this notification as reviewed?", type, "Mark done") Then
        NotificationService.Complete(id)
        LoadNotifications()
      End If
    Catch ex As Exception
      Msg.Fail("update the notification", ex)
    End Try
  End Sub

  ' ---------------- "Open class" ----------------
  Private Sub OpenClassFromNotification(classId As Integer)
    If pnlNotifications IsNot Nothing Then pnlNotifications.Visible = False
    Try
      If SessionManager.IsAdmin Then
        Dim page As New ucAcademicManagement()
        AddHandler page.Load, Sub(s, ev) page.OpenClassFromNotification(classId)
        If flowNav IsNot Nothing Then
          For Each c As Control In flowNav.Controls
            If c.Text = "ACADEMIC MANAGEMENT" Then SetActiveNavControl(c)
          Next
        End If
        ShowPage(page)

      ElseIf SessionManager.IsTeacher Then
        If Not ClassService.TeacherOwnsClass(classId, SessionManager.CurrentUserId) Then
          Msg.Warn("You are no longer the teacher of this class.")
          Return
        End If
        ShowPage(New ucGradebook(classId))

      Else
        Dim enr As Object = DatabaseHelper.ExecuteScalar(
          "SELECT e.enrollment_id FROM enrollment e JOIN student_profile sp ON sp.student_id = e.student_id " &
          "WHERE e.class_id = @c AND sp.user_id = @u",
          {DatabaseHelper.P("@c", classId), DatabaseHelper.P("@u", SessionManager.CurrentUserId)})
        If enr Is Nothing Then
          Msg.Warn("You are not enrolled in this class.")
          Return
        End If
        ShowPage(New ucStudentGradebook(classId, enr.ToString()))
      End If
    Catch ex As Exception
      Msg.Fail("open the class", ex)
    End Try
  End Sub
End Class
