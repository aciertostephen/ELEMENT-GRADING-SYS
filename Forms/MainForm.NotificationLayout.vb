Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

''' <summary>Code-built bell menu shared by administrators, teachers, and students.</summary>
Partial Public Class MainForm

  Private pnlNotifications As Panel
  Private flowNotifList As FlowLayoutPanel
  Private _notifTimer As Timer
  Private _lblBadge As Label
  Private _notificationFilter As String = "All"
  Private _allNotificationsButton As Button
  Private _unreadNotificationsButton As Button
  Private _markAllReadButton As Button
  Private _notificationSubtitle As Label
  Private _requestsHovered As Boolean
  Private _requestsPressed As Boolean

  Friend Sub StartNotificationPolling()
    EnsureNotificationUi()
    EnsureBadge()
    If _notifTimer Is Nothing Then
      _notifTimer = New Timer() With {.Interval = 30000}
      AddHandler _notifTimer.Tick, Sub(s, ev) RefreshNotificationBadge()
    End If
    _notifTimer.Start()
    RefreshNotificationBadge()
  End Sub

  Friend Sub StopNotificationPolling()
    If _notifTimer IsNot Nothing Then _notifTimer.Stop()
    If _lblBadge IsNot Nothing Then _lblBadge.Visible = False
    If pnlNotifications IsNot Nothing Then pnlNotifications.Visible = False
  End Sub

  Private Sub EnsureNotificationUi()
    If pnlNotifications IsNot Nothing OrElse pnlApp Is Nothing Then Return

    pnlNotifications = New Panel() With {
      .Name = "notificationDropDown",
      .Size = New Size(450, 520),
      .BackColor = Color.White,
      .BorderStyle = BorderStyle.FixedSingle,
      .Visible = False,
      .TabIndex = 100
    }

    Dim accent As New Panel() With {.BackColor = Color.FromArgb(34, 90, 74), .Location = New Point(0, 0), .Height = 4}
    Dim title As New Label() With {
      .Text = "Notifications", .AutoSize = True, .Font = New Font("Segoe UI", 12, FontStyle.Bold),
      .ForeColor = Color.FromArgb(31, 41, 37), .Location = New Point(16, 12)
    }
    _notificationSubtitle = New Label() With {
      .AutoSize = False, .Font = New Font("Segoe UI", 8.5F), .ForeColor = Color.DimGray,
      .Location = New Point(16, 39), .Height = 20
    }
    _markAllReadButton = CreateNotificationButton("Mark all read", False)
    _markAllReadButton.Size = New Size(100, 28)
    _markAllReadButton.Visible = False
    AddHandler _markAllReadButton.Click, AddressOf MarkAllNotificationsRead

    Dim closeButton As Button = CreateNotificationButton("Close", False)
    closeButton.Size = New Size(56, 28)
    AddHandler closeButton.Click, Sub() pnlNotifications.Visible = False

    Dim filterPanel As New Panel() With {.Height = 46, .BackColor = Color.FromArgb(248, 250, 249)}
    _allNotificationsButton = CreateNotificationButton("All", True)
    _allNotificationsButton.SetBounds(14, 8, 64, 28)
    _unreadNotificationsButton = CreateNotificationButton("Unread", False)
    _unreadNotificationsButton.SetBounds(84, 8, 76, 28)
    AddHandler _allNotificationsButton.Click, Sub() SetNotificationFilter("All")
    AddHandler _unreadNotificationsButton.Click, Sub() SetNotificationFilter("Unread")
    filterPanel.Controls.AddRange({_allNotificationsButton, _unreadNotificationsButton})

    flowNotifList = New FlowLayoutPanel() With {
      .Name = "notificationItems",
      .AutoScroll = True,
      .BackColor = Color.White,
      .FlowDirection = FlowDirection.TopDown,
      .WrapContents = False,
      .Location = New Point(8, 116),
      .Size = New Size(432, 394),
      .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
    }

    pnlNotifications.Controls.Add(flowNotifList)
    pnlNotifications.Controls.Add(filterPanel)
    pnlNotifications.Controls.Add(accent)
    pnlNotifications.Controls.Add(title)
    pnlNotifications.Controls.Add(_notificationSubtitle)
    pnlNotifications.Controls.Add(_markAllReadButton)
    pnlNotifications.Controls.Add(closeButton)
    pnlApp.Controls.Add(pnlNotifications)
    AddHandler pnlNotifications.Resize, AddressOf LayoutNotificationDropDown
    AddHandler pnlApp.Resize, Sub() PositionNotificationPanel()
    LayoutNotificationDropDown(Nothing, EventArgs.Empty)
    PositionNotificationPanel()
  End Sub

  Private Sub LayoutNotificationDropDown(sender As Object, e As EventArgs)
    If pnlNotifications Is Nothing Then Return
    Dim width As Integer = pnlNotifications.ClientSize.Width
    Dim height As Integer = pnlNotifications.ClientSize.Height
    Dim markLeft As Integer = width - _markAllReadButton.Width - 78
    _markAllReadButton.Location = New Point(markLeft, 13)
    Dim closeButton As Button = TryCast(pnlNotifications.Controls.OfType(Of Button)().FirstOrDefault(Function(b) b.Text = "Close"), Button)
    If closeButton IsNot Nothing Then closeButton.Location = New Point(width - closeButton.Width - 12, 13)
    _notificationSubtitle.Width = Math.Max(120, markLeft - 26)
    Dim filterPanel As Control = pnlNotifications.Controls.OfType(Of Panel)().FirstOrDefault(Function(p) p.Height = 46)
    If filterPanel IsNot Nothing Then filterPanel.SetBounds(0, 66, width, 46)
    flowNotifList.SetBounds(8, 116, Math.Max(100, width - 16), Math.Max(100, height - 124))
    ResizeNotificationCards()
  End Sub

  Private Sub PositionNotificationPanel()
    If pnlNotifications Is Nothing OrElse pnlApp Is Nothing OrElse btnBell Is Nothing OrElse pnlApp.ClientSize.Width <= 0 Then Return
    Dim bellBottom As Point = pnlApp.PointToClient(btnBell.PointToScreen(New Point(btnBell.Width, btnBell.Height)))
    Dim x As Integer = Math.Max(8, Math.Min(pnlApp.ClientSize.Width - pnlNotifications.Width - 8, bellBottom.X - pnlNotifications.Width))
    Dim yMax As Integer = Math.Max(8, pnlApp.ClientSize.Height - pnlNotifications.Height - 8)
    Dim y As Integer = Math.Min(Math.Max(pnlHeader.Height - 4, bellBottom.Y + 4), yMax)
    pnlNotifications.Location = New Point(x, y)
  End Sub

  Private Sub EnsureBadge()
    If _lblBadge IsNot Nothing OrElse btnBell Is Nothing Then Return
    _lblBadge = New Label() With {
      .AutoSize = False, .Size = New Size(20, 20), .BackColor = Color.Firebrick, .ForeColor = Color.White,
      .TextAlign = ContentAlignment.MiddleCenter, .Font = New Font("Segoe UI", 8, FontStyle.Bold),
      .Cursor = Cursors.Hand, .Visible = False
    }
    Using path As New Drawing2D.GraphicsPath()
      path.AddEllipse(0, 0, 20, 20)
      _lblBadge.Region = New Region(path)
    End Using
    _lblBadge.Location = New Point(Math.Max(btnBell.Width - 20, 0), 0)
    AddHandler _lblBadge.Click, Sub(s, ev) ToggleNotificationPanel()
    btnBell.Controls.Add(_lblBadge)
    _lblBadge.BringToFront()
    btnBell.Cursor = Cursors.Hand
  End Sub

  Private Sub RefreshNotificationBadge()
    If _lblBadge Is Nothing Then Return
    Try
      If Not SessionManager.IsLoggedIn Then
        _lblBadge.Visible = False
        Return
      End If
      Dim count As Integer = NotificationService.UnreadCount()
      _lblBadge.Text = If(count > 9, "9+", count.ToString())
      _lblBadge.Visible = count > 0
    Catch
      _lblBadge.Visible = False
    End Try
  End Sub

  Private Sub btnBell_Click(sender As Object, e As EventArgs) Handles btnBell.Click
    ToggleNotificationPanel()
  End Sub

  Private Sub RequestsButtonPaint(sender As Object, e As PaintEventArgs)
    Dim isActive As Boolean = pnlNotifications IsNot Nothing AndAlso pnlNotifications.Visible
    If _requestsPressed OrElse isActive Then
      Using brush As New SolidBrush(Color.FromArgb(20, 70, 56))
        e.Graphics.FillRectangle(brush, btnBell.ClientRectangle)
      End Using
    ElseIf _requestsHovered Then
      Using brush As New SolidBrush(Color.FromArgb(45, 108, 89))
        e.Graphics.FillRectangle(brush, btnBell.ClientRectangle)
      End Using
    End If
    Using requestFont As New Font("Century Gothic", 11.0F, If(isActive, FontStyle.Bold, FontStyle.Regular))
      TextRenderer.DrawText(e.Graphics, "REQUESTS", requestFont, btnBell.ClientRectangle, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
    End Using
  End Sub

  Private Sub RequestsButtonMouseEnter(sender As Object, e As EventArgs)
    _requestsHovered = True
    btnBell.Invalidate()
  End Sub

  Private Sub RequestsButtonMouseLeave(sender As Object, e As EventArgs)
    _requestsHovered = False
    _requestsPressed = False
    btnBell.Invalidate()
  End Sub

  Private Sub RequestsButtonMouseDown(sender As Object, e As MouseEventArgs)
    If e.Button <> MouseButtons.Left Then Return
    _requestsPressed = True
    btnBell.Invalidate()
  End Sub

  Private Sub RequestsButtonMouseUp(sender As Object, e As MouseEventArgs)
    _requestsPressed = False
    btnBell.Invalidate()
  End Sub

  Private Sub ToggleNotificationPanel()
    EnsureNotificationUi()
    If pnlNotifications Is Nothing Then Return
    If pnlNotifications.Visible Then
      pnlNotifications.Visible = False
      btnBell.Invalidate()
      Return
    End If
    PositionNotificationPanel()
    pnlNotifications.Visible = True
    btnBell.Invalidate()
    pnlNotifications.BringToFront()
    LoadNotifications()
  End Sub

  Private Sub SetNotificationFilter(filter As String)
    _notificationFilter = If(filter = "Unread", "Unread", "All")
    If _allNotificationsButton IsNot Nothing Then StyleFilterButton(_allNotificationsButton, _notificationFilter = "All")
    If _unreadNotificationsButton IsNot Nothing Then StyleFilterButton(_unreadNotificationsButton, _notificationFilter = "Unread")
    LoadNotifications()
  End Sub

  Private Shared Sub StyleFilterButton(button As Button, active As Boolean)
    button.BackColor = If(active, Color.FromArgb(34, 90, 74), Color.White)
    button.ForeColor = If(active, Color.White, Color.FromArgb(45, 55, 50))
    button.FlatAppearance.BorderColor = If(active, Color.FromArgb(34, 90, 74), Color.FromArgb(210, 218, 214))
  End Sub

  Private Shared Function CreateNotificationButton(text As String, primary As Boolean) As Button
    Dim button As New Button() With {
      .Text = text, .FlatStyle = FlatStyle.Flat, .UseVisualStyleBackColor = False,
      .BackColor = If(primary, Color.FromArgb(34, 90, 74), Color.White),
      .ForeColor = If(primary, Color.White, Color.FromArgb(45, 55, 50)),
      .Font = New Font("Segoe UI", 8.5F), .Cursor = Cursors.Hand, .TabStop = True
    }
    button.FlatAppearance.BorderSize = 1
    button.FlatAppearance.BorderColor = If(primary, Color.FromArgb(34, 90, 74), Color.FromArgb(210, 218, 214))
    button.FlatAppearance.MouseOverBackColor = If(primary, Color.FromArgb(23, 61, 50), Color.FromArgb(242, 246, 244))
    Return button
  End Function

  Private Sub LoadNotifications()
    If flowNotifList Is Nothing Then Return
    DisposeNotificationCards()
    If _notificationSubtitle IsNot Nothing Then
      _notificationSubtitle.Text = If(SessionManager.IsAdmin, "Gradebook reviews and class updates",
        If(SessionManager.IsTeacher, "Class assignments and gradebook activity", "Class assignments and published grades"))
    End If

    Try
      Dim data As DataTable = NotificationService.ListForCurrentUser()
      Dim shown As Integer = 0
      If data IsNot Nothing Then
        For Each row As DataRow In data.Rows
          Dim card As Panel = BuildNotificationCard(row)
          If card IsNot Nothing Then
            flowNotifList.Controls.Add(card)
            shown += 1
          End If
        Next
      End If
      If shown = 0 Then
        flowNotifList.Controls.Add(New Label() With {
          .Text = If(_notificationFilter = "Unread", "You're all caught up.", "No notifications yet."),
          .AutoSize = False, .Height = 52, .Width = Math.Max(150, flowNotifList.ClientSize.Width - 24),
          .Padding = New Padding(12, 16, 8, 8), .ForeColor = Color.DimGray, .Font = New Font("Segoe UI", 9)
        })
      End If
      If _markAllReadButton IsNot Nothing Then
        _markAllReadButton.Visible = Not SessionManager.IsAdmin AndAlso NotificationService.UnreadCount() > 0
      End If
    Catch ex As Exception
      Msg.Fail("load notifications", ex)
    End Try
    RefreshNotificationBadge()
  End Sub

  Private Function BuildNotificationCard(row As DataRow) As Panel
    Dim id As Integer = Convert.ToInt32(row("notification_id"))
    Dim type As String = Convert.ToString(row("type"))
    Dim message As String = Convert.ToString(row("message"))
    Dim status As String = Convert.ToString(row("status"))
    Dim kind As String = Convert.ToString(row("kind"))
    Dim isRequest As Boolean = String.Equals(kind, "Request", StringComparison.OrdinalIgnoreCase)
    Dim isPending As Boolean = String.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)
    Dim isUnread As Boolean = If(isRequest, SessionManager.IsAdmin AndAlso isPending,
      Not IsDBNull(row("is_read")) AndAlso Convert.ToInt32(row("is_read")) = 0)
    If _notificationFilter = "Unread" AndAlso Not isUnread Then Return Nothing

    Dim classId As Integer? = Nothing
    If Not IsDBNull(row("related_class_id")) Then classId = Convert.ToInt32(row("related_class_id"))
    Dim width As Integer = Math.Max(300, flowNotifList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 16)
    Dim card As New Panel() With {
      .Size = New Size(width, 154), .BackColor = If(isUnread, Color.FromArgb(250, 253, 251), Color.White),
      .BorderStyle = BorderStyle.FixedSingle, .Margin = New Padding(3, 4, 3, 4), .Padding = New Padding(0)
    }
    Dim category As New Label() With {
      .Text = DisplayNotificationType(type, message, isRequest), .AutoSize = False,
      .Location = New Point(13, 10), .Size = New Size(width - 150, 20),
      .Font = New Font("Segoe UI", 9, FontStyle.Bold), .ForeColor = Color.FromArgb(34, 90, 74)
    }
    Dim stamp As New Label() With {
      .Text = Convert.ToDateTime(row("created_at")).ToString("h:mm tt · dd MMM yyyy"),
      .AutoSize = False, .Location = New Point(width - 148, 10), .Size = New Size(136, 20),
      .TextAlign = ContentAlignment.TopRight, .Font = New Font("Segoe UI", 7.5F), .ForeColor = Color.DimGray
    }
    Dim body As New Label() With {
      .Text = message, .AutoEllipsis = True, .AutoSize = False,
      .Location = New Point(13, 36), .Size = New Size(width - 26, 48),
      .Font = New Font("Segoe UI", 8.5F), .ForeColor = Color.FromArgb(45, 52, 49)
    }
    Dim stateText As String
    If isRequest Then
      stateText = If(isPending, If(SessionManager.IsAdmin, "Needs review", "Sent to admin"), "Reviewed")
    Else
      stateText = If(isUnread, "Unread", "Read")
    End If
    Dim stateLabel As New Label() With {
      .Text = stateText, .AutoSize = False, .Location = New Point(13, 88), .Size = New Size(130, 18),
      .Font = New Font("Segoe UI", 8), .ForeColor = Color.DimGray
    }

    card.Controls.AddRange({category, stamp, body, stateLabel})
    Dim right As Integer = width - 12
    If isRequest AndAlso SessionManager.IsAdmin AndAlso isPending Then
      Dim review As Button = CreateNotificationButton("Review", True)
      review.Size = New Size(78, 27)
      right -= review.Width
      review.Location = New Point(right, 116)
      AddHandler review.Click, Sub() ReviewNotification(id, type, message)
      card.Controls.Add(review)
      right -= 8
    End If
    If classId.HasValue Then
      Dim targetClass As Integer = classId.Value
      Dim openClass As Button = CreateNotificationButton("Open class", True)
      openClass.Size = New Size(96, 27)
      right -= openClass.Width
      openClass.Location = New Point(right, 116)
      AddHandler openClass.Click, Sub() OpenClassFromNotification(targetClass, id)
      card.Controls.Add(openClass)
      right -= 8
    End If
    If isUnread AndAlso Not SessionManager.IsAdmin Then
      Dim markRead As Button = CreateNotificationButton("Mark read", False)
      markRead.Size = New Size(84, 27)
      right -= markRead.Width
      markRead.Location = New Point(right, 116)
      AddHandler markRead.Click, Sub() MarkNotificationRead(id)
      card.Controls.Add(markRead)
    End If
    Return card
  End Function

  Private Shared Function DisplayNotificationType(type As String, message As String, isRequest As Boolean) As String
    If isRequest Then Return If(type = "Grade Release", "Gradebook saved", "Teacher request")
    If type = "Grade Release" Then
      If message.IndexOf("Midterm", StringComparison.OrdinalIgnoreCase) >= 0 Then Return "Midterm grades published"
      If message.IndexOf("Final", StringComparison.OrdinalIgnoreCase) >= 0 Then Return "Final grades published"
      Return "Gradebook update"
    End If
    If type = "Class Update" AndAlso
       (message.IndexOf("assigned", StringComparison.OrdinalIgnoreCase) >= 0 OrElse message.IndexOf("added to", StringComparison.OrdinalIgnoreCase) >= 0) Then
      Return "Class assignment"
    End If
    Return type
  End Function

  Private Sub ResizeNotificationCards()
    If flowNotifList Is Nothing Then Return
    Dim width As Integer = Math.Max(300, flowNotifList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 16)
    For Each card As Control In flowNotifList.Controls
      If TypeOf card Is Panel Then
        card.Width = width
        For Each label As Label In card.Controls.OfType(Of Label)()
          If label.TextAlign = ContentAlignment.TopRight Then
            label.Left = width - 148
          ElseIf label.Top = 10 Then
            label.Width = width - 150
          ElseIf label.Top = 36 Then
            label.Width = width - 26
          End If
        Next
        Dim actionRight As Integer = width - 12
        For Each button As Button In card.Controls.OfType(Of Button)().OrderByDescending(Function(b) b.Left)
          button.Left = actionRight - button.Width
          actionRight = button.Left - 8
        Next
      Else
        card.Width = width
      End If
    Next
  End Sub

  Private Sub DisposeNotificationCards()
    If flowNotifList Is Nothing Then Return
    Dim old As Control() = flowNotifList.Controls.Cast(Of Control)().ToArray()
    flowNotifList.Controls.Clear()
    For Each control As Control In old
      control.Dispose()
    Next
  End Sub

  Private Sub MarkAllNotificationsRead(sender As Object, e As EventArgs)
    Try
      NotificationService.MarkAllRead()
      LoadNotifications()
    Catch ex As Exception
      Msg.Fail("mark notifications as read", ex)
    End Try
  End Sub

  Private Sub MarkNotificationRead(notificationId As Integer)
    Try
      NotificationService.MarkRead(notificationId)
      LoadNotifications()
    Catch ex As Exception
      Msg.Fail("mark the notification as read", ex)
    End Try
  End Sub

  Private Sub ReviewNotification(id As Integer, type As String, message As String)
    Try
      If Msg.Confirm(message & vbCrLf & vbCrLf & "Mark this notification as reviewed?", type, "Mark reviewed") Then
        NotificationService.Complete(id)
        LoadNotifications()
      End If
    Catch ex As Exception
      Msg.Fail("review the notification", ex)
    End Try
  End Sub

  Private Sub OpenClassFromNotification(classId As Integer, notificationId As Integer)
    If pnlNotifications IsNot Nothing Then pnlNotifications.Visible = False
    Try
      If SessionManager.IsAdmin Then
        Dim page As New ucAcademicManagement()
        AddHandler page.Load, Sub(s, ev) page.OpenClassFromNotification(classId)
        If flowNav IsNot Nothing Then
          For Each control As Control In flowNav.Controls
            If control.Text = "ACADEMIC MANAGEMENT" Then SetActiveNavControl(control)
          Next
        End If
        ShowPage(page)
      ElseIf SessionManager.IsTeacher Then
        If Not ClassService.TeacherOwnsClass(classId, SessionManager.CurrentUserId) Then
          Msg.Warn("You are no longer assigned to this class.")
          Return
        End If
        If Not ClassService.IsClassInActiveTerm(classId) Then
          Msg.Warn("Teachers can only open classes for the active term.")
          Return
        End If
        ShowPage(New ucGradebook(classId))
      Else
        Dim enrollment As Object = DatabaseHelper.ExecuteScalar(
          "SELECT e.enrollment_id FROM enrollment e JOIN student_profile sp ON sp.student_id = e.student_id " &
          "JOIN classes c ON c.class_id = e.class_id JOIN academic_term t ON t.term_id = c.term_id " &
          "WHERE e.class_id = @c AND sp.user_id = @u AND e.status <> 'Dropped' AND t.is_active = 1",
          {DatabaseHelper.P("@c", classId), DatabaseHelper.P("@u", SessionManager.CurrentUserId)})
        If enrollment Is Nothing Then
          Msg.Warn("You are not enrolled in this class.")
          Return
        End If
        ShowPage(New ucStudentGradebook(classId, enrollment.ToString()))
      End If
      If Not SessionManager.IsAdmin Then
        NotificationService.MarkRead(notificationId)
        RefreshNotificationBadge()
      End If
    Catch ex As Exception
      Msg.Fail("open the class", ex)
    End Try
  End Sub
End Class
