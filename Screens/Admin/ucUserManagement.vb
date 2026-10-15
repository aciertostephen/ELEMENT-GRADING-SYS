Imports System.ComponentModel
Imports System.Drawing
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient
Imports MySqlConnector

Imports BCryptHasher = BCrypt.Net.BCrypt

Public Class ucUserManagement
    Inherits UserControl

    '========================================
    ' CONSTANTS / TYPES
    '========================================

    Private Enum Col
        UserId = 0
        FullName = 1
        Role = 2
        Email = 3
        Phone = 4
        Joined = 5
        Status = 6
    End Enum

    Private Const EXPECTED_COLUMNS As Integer = 7

    Private Const ROLE_STUDENT As String = "Student"
    Private Const ROLE_PROFESSOR As String = "Teacher"
    Private Const STATUS_ACTIVE As String = "Active"
    Private Const STATUS_INACTIVE As String = "Inactive"

    Private Const FILTER_ALL As String = "All"
    Private Const FILTER_STUDENT As String = "Student"
    Private Const FILTER_FACULTY As String = "Faculty"
    Private Const FILTER_ACTIVE As String = "Active"
    Private Const FILTER_INACTIVE As String = "Inactive"

    Private Const MYSQL_ERR_FK_PARENT As Integer = 1451

    Private Shared ReadOnly EmailRegex As New Regex("^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)
    Private Shared ReadOnly PhoneRegex As New Regex("^\d{7,15}$", RegexOptions.Compiled)

    '========================================
    ' STATE
    '========================================

    Public Property CurrentAdminUserId As String = ""

    Private currentFilter As String = FILTER_ALL

    Private ReadOnly cmsFilterMenu As New ContextMenuStrip()

    Private WithEvents tmrSearch As New Timer() With {.Interval = 250}

    Private ReadOnly statValueLabels As New Dictionary(Of String, Label)

    '========================================
    ' CONSTRUCTOR & LOAD / DISPOSE
    '========================================

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub ucUserManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Me.DesignMode OrElse LicenseManager.UsageMode = LicenseUsageMode.Designtime Then Return

        ConfigureGrid()
        BuildStatBoxes()
        InitializeFilterMenu()
        LoadUsersFromDatabase()
    End Sub

    Private Sub ucUserManagement_Disposed(sender As Object, e As EventArgs) Handles MyBase.Disposed
        tmrSearch.Dispose()
        cmsFilterMenu.Dispose()
    End Sub

    Private Sub ConfigureGrid()
        If dgvUsers Is Nothing Then Return

        dgvUsers.ReadOnly = True
        dgvUsers.AllowUserToAddRows = False
        dgvUsers.AllowUserToDeleteRows = False
        dgvUsers.MultiSelect = False
        dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect
    End Sub

    '========================================
    ' HELPERS
    '========================================

    Private Shared Function CellText(row As DataGridViewRow, c As Col) As String
        Return If(row.Cells(CInt(c)).Value?.ToString(), "")
    End Function

    Private Shared Function SameText(a As String, b As String) As Boolean
        Return String.Equals(a, b, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function ContainsText(source As String, query As String) As Boolean
        Return source.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Shared Function IsFaculty(role As String) As Boolean
        Return SameText(role, ROLE_PROFESSOR) OrElse
               SameText(role, "Faculty") OrElse
               SameText(role, "Teacher")
    End Function

    Private Function IsCurrentAdmin(userId As String) As Boolean
        Dim activeId As String = If(Not String.IsNullOrEmpty(CurrentAdminUserId), CurrentAdminUserId, Convert.ToString(SessionManager.CurrentUserId))
        Return Not String.IsNullOrEmpty(activeId) AndAlso SameText(userId, activeId)
    End Function

    Private Function GetSelectedUserRow(actionText As String) As DataGridViewRow
        If dgvUsers.SelectedRows.Count = 0 OrElse Not dgvUsers.SelectedRows(0).Visible Then
            MessageBox.Show($"Please select a user account from the table to {actionText}.",
                            "No User Selected", MessageBoxButtons.OK)
            Return Nothing
        End If
        Return dgvUsers.SelectedRows(0)
    End Function

    '========================================
    ' LIVE DATABASE SYNC
    '========================================

    Public Sub LoadUsersFromDatabase()
        If dgvUsers Is Nothing Then Return

        Dim previousUserId As String = Nothing
        If dgvUsers.CurrentRow IsNot Nothing AndAlso Not dgvUsers.CurrentRow.IsNewRow Then
            previousUserId = CellText(dgvUsers.CurrentRow, Col.UserId)
        End If

        Dim sql As String =
            "SELECT u.user_id, u.username, u.role, u.email, u.phone_no, u.status, " &
            "DATE_FORMAT(u.date_added, '%m/%d/%Y') AS formatted_date, " &
            "COALESCE(" &
            "  NULLIF(TRIM(CONCAT_WS(' ', tp.first_name, tp.last_name)), '')," &
            "  NULLIF(TRIM(CONCAT_WS(' ', sp.first_name, sp.last_name)), '')," &
            "  u.username) AS full_name " &
            "FROM users u " &
            "LEFT JOIN teacher_profile tp ON u.user_id = tp.user_id " &
            "LEFT JOIN student_profile sp ON u.user_id = sp.user_id " &
            "ORDER BY u.user_id DESC;"

        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteQuery(sql)

            dgvUsers.SuspendLayout()
            dgvUsers.CurrentCell = Nothing
            dgvUsers.Rows.Clear()

            If dt IsNot Nothing Then
                For Each row As DataRow In dt.Rows
                    Dim phoneVal As String = If(IsDBNull(row("phone_no")) OrElse
                                                String.IsNullOrWhiteSpace(row("phone_no").ToString()),
                                                "N/A", row("phone_no").ToString())
                    Dim dateVal As String = If(IsDBNull(row("formatted_date")), "N/A", row("formatted_date").ToString())
                    Dim emailVal As String = If(IsDBNull(row("email")), "", row("email").ToString())

                    Dim idx As Integer = dgvUsers.Rows.Add(
                        row("user_id").ToString(),
                        row("full_name").ToString(),
                        row("role").ToString(),
                        emailVal,
                        phoneVal,
                        dateVal,
                        row("status").ToString())

                    Dim statusCell As DataGridViewCell = dgvUsers.Rows(idx).Cells(CInt(Col.Status))
                    statusCell.Style.ForeColor = If(SameText(row("status").ToString(), STATUS_ACTIVE),
                                                    Color.SeaGreen, Color.Firebrick)
                Next
            End If

            ApplyFilter()
            RestoreSelection(previousUserId)

        Catch ex As Exception
            MessageBox.Show("Database Load Error: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        Finally
            dgvUsers.ResumeLayout()
        End Try
    End Sub

    Private Sub RestoreSelection(userId As String)
        If String.IsNullOrEmpty(userId) Then Return

        For Each row As DataGridViewRow In dgvUsers.Rows
            If row.IsNewRow OrElse Not row.Visible Then Continue For
            If SameText(CellText(row, Col.UserId), userId) Then
                row.Selected = True
                Return
            End If
        Next
    End Sub

    '========================================
    ' SUMMARY STATS CARDS
    '========================================

    Private Sub BuildStatBoxes()
        If flowStats Is Nothing Then Return
        flowStats.SuspendLayout()
        flowStats.Controls.Clear()
        statValueLabels.Clear()

        For Each caption As String In {"TOTAL USERS", "FACULTY", "STUDENTS", "INACTIVE"}
            AddStatBox(caption)
        Next

        flowStats.ResumeLayout()
    End Sub

    Private Sub AddStatBox(caption As String)
        Dim box As New RoundedPanel()
        box.Size = New Size(343, 70)
        box.BackColor = Color.White
        box.Margin = New Padding(15, 25, 0, 0)

        Dim lblCaption As New Label()
        lblCaption.Text = caption
        lblCaption.ForeColor = Color.Gray
        lblCaption.Font = New Font("Century Gothic", 8, FontStyle.Regular)
        lblCaption.Location = New Point(15, 12)
        lblCaption.AutoSize = True

        Dim lblValue As New Label()
        lblValue.Text = "0"
        lblValue.Font = New Font("Century Gothic", 15, FontStyle.Bold)
        lblValue.ForeColor = Color.Black
        lblValue.Location = New Point(13, 32)
        lblValue.AutoSize = True

        box.Controls.Add(lblCaption)
        box.Controls.Add(lblValue)
        flowStats.Controls.Add(box)

        statValueLabels(caption) = lblValue
    End Sub

    '========================================
    ' FILTERING & SEARCHING
    '========================================

    Private Sub ApplyFilter()
        If dgvUsers Is Nothing Then Return

        Dim query As String = txtSearch.Text.Trim().ToLower()
        Dim total, faculty, students, inactive As Integer

        dgvUsers.CurrentCell = Nothing
        dgvUsers.SuspendLayout()

        For Each row As DataGridViewRow In dgvUsers.Rows
            If row.IsNewRow Then Continue For

            Dim role As String = CellText(row, Col.Role)
            Dim status As String = CellText(row, Col.Status)

            total += 1
            If IsFaculty(role) Then
                faculty += 1
            ElseIf SameText(role, ROLE_STUDENT) Then
                students += 1
            End If
            If SameText(status, STATUS_INACTIVE) Then inactive += 1

            Dim matchesSearch As Boolean = String.IsNullOrEmpty(query) OrElse
                                           CellText(row, Col.UserId).ToLower().Contains(query) OrElse
                                           CellText(row, Col.FullName).ToLower().Contains(query) OrElse
                                           CellText(row, Col.Email).ToLower().Contains(query) OrElse
                                           CellText(row, Col.Role).ToLower().Contains(query)

            Dim matchesCat As Boolean = (currentFilter = FILTER_ALL) OrElse
                                        (currentFilter = FILTER_FACULTY AndAlso IsFaculty(role)) OrElse
                                        (currentFilter = FILTER_STUDENT AndAlso SameText(role, ROLE_STUDENT)) OrElse
                                        (currentFilter = FILTER_ACTIVE AndAlso SameText(status, STATUS_ACTIVE)) OrElse
                                        (currentFilter = FILTER_INACTIVE AndAlso SameText(status, STATUS_INACTIVE))

            row.Visible = matchesSearch AndAlso matchesCat
        Next

        dgvUsers.ResumeLayout()

        If statValueLabels.ContainsKey("TOTAL USERS") Then
            statValueLabels("TOTAL USERS").Text = total.ToString()
            statValueLabels("FACULTY").Text = faculty.ToString()
            statValueLabels("STUDENTS").Text = students.ToString()
            statValueLabels("INACTIVE").Text = inactive.ToString()
        End If
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        tmrSearch.Stop()
        tmrSearch.Start()
    End Sub

    Private Sub tmrSearch_Tick(sender As Object, e As EventArgs) Handles tmrSearch.Tick
        tmrSearch.Stop()
        ApplyFilter()
    End Sub

    '========================================
    ' FILTER DROPDOWN MENU
    '========================================

    Private Sub InitializeFilterMenu()
        cmsFilterMenu.Renderer = New TableBlueHoverRenderer()
        cmsFilterMenu.ShowImageMargin = False
        cmsFilterMenu.Font = New Font("Century Gothic", 9, FontStyle.Regular)
        cmsFilterMenu.Items.Clear()

        Dim options = {
            ("Show All Users", FILTER_ALL),
            ("Students", FILTER_STUDENT),
            ("Faculty", FILTER_FACULTY),
            ("Status: Active", FILTER_ACTIVE),
            ("Status: Inactive", FILTER_INACTIVE)
        }

        For Each opt In options
            Dim item As New ToolStripMenuItem(opt.Item1) With {.Tag = opt.Item2}
            AddHandler item.Click, AddressOf OnFilterMenuItemClicked
            cmsFilterMenu.Items.Add(item)
        Next
    End Sub

    Private Sub OnFilterMenuItemClicked(sender As Object, e As EventArgs)
        Dim item = TryCast(sender, ToolStripMenuItem)
        If item IsNot Nothing AndAlso item.Tag IsNot Nothing Then
            currentFilter = item.Tag.ToString()
            txtSearch.Clear()
            ApplyFilter()
        End If
    End Sub

    Private Sub btnFilter_Click(sender As Object, e As EventArgs) Handles btnFilter.Click
        ' Reset any global wait cursor state
        Me.UseWaitCursor = False
        btnFilter.UseWaitCursor = False
        Cursor.Current = Cursors.Hand

        If cmsFilterMenu IsNot Nothing Then
            cmsFilterMenu.Show(btnFilter, New Point(0, btnFilter.Height))
        End If
    End Sub

    '========================================
    ' EDIT USER INFORMATION WORKFLOW
    '========================================

    Private Sub linkEditInfo_Click(sender As Object, e As EventArgs) Handles linkEditInfo.Click
        Dim row As DataGridViewRow = GetSelectedUserRow("edit")
        If row Is Nothing Then Return

        Dim userId As String = CellText(row, Col.UserId)
        Dim role As String = CellText(row, Col.Role)

        Using dlg As New EditUserDialog(userId, CellText(row, Col.FullName), CellText(row, Col.Email), CellText(row, Col.Phone))
            If dlg.ShowDialog(Me.FindForm()) = DialogResult.OK Then
                Try
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE users SET email = @email, phone_no = @phone WHERE user_id = @uid;",
                        {
                            New MySqlParameter("@email", dlg.Email),
                            New MySqlParameter("@phone", dlg.Phone),
                            New MySqlParameter("@uid", userId)
                        })

                    Dim parts As String() = dlg.FullName.Trim().Split(New Char() {" "c}, 2, StringSplitOptions.RemoveEmptyEntries)
                    Dim fn As String = If(parts.Length > 0, parts(0), "")
                    Dim ln As String = If(parts.Length > 1, parts(1), "")

                    Dim profileTable As String = If(IsFaculty(role), "teacher_profile", If(SameText(role, ROLE_STUDENT), "student_profile", Nothing))
                    If profileTable IsNot Nothing Then
                        DatabaseHelper.ExecuteNonQuery(
                            $"UPDATE {profileTable} SET first_name = @fn, last_name = @ln WHERE user_id = @uid;",
                            {
                                New MySqlParameter("@fn", fn),
                                New MySqlParameter("@ln", ln),
                                New MySqlParameter("@uid", userId)
                            })
                    End If

                    LoadUsersFromDatabase()
                    MessageBox.Show($"Information for User [{userId}] was successfully updated in the database.", "Update Complete", MessageBoxButtons.OK)

                Catch ex As Exception
                    MessageBox.Show("Failed to update database: " & ex.Message, "Database Error", MessageBoxButtons.OK)
                End Try
            End If
        End Using
    End Sub

    Private Sub linkEditInfo_MouseEnter(sender As Object, e As EventArgs) Handles linkEditInfo.MouseEnter
        linkEditInfo.LinkColor = Color.Black
    End Sub

    Private Sub linkEditInfo_MouseLeave(sender As Object, e As EventArgs) Handles linkEditInfo.MouseLeave
        linkEditInfo.LinkColor = Color.DimGray
    End Sub

    '========================================
    ' DEACTIVATE / ACTIVATE USER WORKFLOW
    '========================================

    Private Sub linkUserStatus_Click(sender As Object, e As EventArgs) Handles linkUserStatus.Click
        Dim selectedRow As DataGridViewRow = GetSelectedUserRow("activate or deactivate")
        If selectedRow Is Nothing Then Return

        Dim userId As String = CellText(selectedRow, Col.UserId)
        Dim fullName As String = CellText(selectedRow, Col.FullName)
        Dim currentStatus As String = CellText(selectedRow, Col.Status)

        Dim deactivating As Boolean = SameText(currentStatus, STATUS_ACTIVE)

        If deactivating AndAlso IsCurrentAdmin(userId) Then
            MessageBox.Show("You cannot deactivate your own account.", "Action Not Allowed",
                            MessageBoxButtons.OK)
            Return
        End If

        Dim newStatus As String = If(deactivating, STATUS_INACTIVE, STATUS_ACTIVE)
        Dim verb As String = If(deactivating, "deactivate", "activate")
        Dim verbPast As String = If(deactivating, "deactivated", "activated")

        Dim promptMessage As String = $"Are you sure you want to {verb} this account?" & vbCrLf & vbCrLf &
                                      $"• User ID: {userId}" & vbCrLf &
                                      $"• Name: {fullName}" & vbCrLf &
                                      $"• Current Status: {currentStatus}" & vbCrLf &
                                      $"• New Status: {newStatus}"

        If Not ConfirmDialog.Ask(Me.FindForm(), promptMessage,
                                 $"Confirm Account {If(deactivating, "Deactivation", "Activation")}") Then Return

        Try
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE users SET status = @status WHERE user_id = @uid;",
                {
                    New MySqlParameter("@status", newStatus),
                    New MySqlParameter("@uid", userId)
                })

            LoadUsersFromDatabase()
            MessageBox.Show($"Account [{userId} - {fullName}] was {verbPast}.",
                            $"Account {If(deactivating, "Deactivated", "Activated")}",
                            MessageBoxButtons.OK)

        Catch ex As Exception
            MessageBox.Show("Failed to update status in database: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
    End Sub

    Private Sub linkUserStatus_MouseEnter(sender As Object, e As EventArgs) Handles linkUserStatus.MouseEnter
        linkUserStatus.LinkColor = Color.Black
    End Sub

    Private Sub linkUserStatus_MouseLeave(sender As Object, e As EventArgs) Handles linkUserStatus.MouseLeave
        linkUserStatus.LinkColor = Color.DimGray
    End Sub

    '========================================
    ' DELETE ACCOUNT WORKFLOW
    '========================================

    Private Sub btnDeleteBTN_Click(sender As Object, e As EventArgs) Handles btnDeleteBTN.Click
        Dim selectedRow As DataGridViewRow = GetSelectedUserRow("delete")
        If selectedRow Is Nothing Then Return

        Dim userId As String = CellText(selectedRow, Col.UserId)
        Dim fullName As String = CellText(selectedRow, Col.FullName)

        If IsCurrentAdmin(userId) Then
            MessageBox.Show("You cannot delete your own account.", "Action Not Allowed",
                            MessageBoxButtons.OK)
            Return
        End If

        Dim promptMessage As String = "Are you sure you want to permanently delete this user?" & vbCrLf & vbCrLf &
                                      $"• User ID: {userId}" & vbCrLf &
                                      $"• Name: {fullName}" & vbCrLf & vbCrLf &
                                      "This action cannot be undone."

        If Not ConfirmDialog.Ask(Me.FindForm(), promptMessage, "Confirm Account Deletion") Then Return

        Try
            Dim uidParam As MySqlParameter() = {New MySqlParameter("@uid", userId)}
            DatabaseHelper.ExecuteNonQuery("DELETE FROM teacher_profile WHERE user_id = @uid;", uidParam)
            DatabaseHelper.ExecuteNonQuery("DELETE FROM student_profile WHERE user_id = @uid;", {New MySqlParameter("@uid", userId)})
            DatabaseHelper.ExecuteNonQuery("DELETE FROM users WHERE user_id = @uid;", {New MySqlParameter("@uid", userId)})

            LoadUsersFromDatabase()
            MessageBox.Show($"Account [{userId} - {fullName}] was successfully deleted.",
                            "Account Deleted", MessageBoxButtons.OK)

        Catch ex As MySqlException When ex.Number = MYSQL_ERR_FK_PARENT
            MessageBox.Show("This account is referenced by other records (classes, grades or logs) and cannot be deleted." & vbCrLf & vbCrLf &
                            "Consider setting the account to Inactive instead.", "Cannot Delete", MessageBoxButtons.OK)
        Catch ex As Exception
            MessageBox.Show("Failed to delete user: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
    End Sub

    '========================================
    ' NESTED RENDERER FOR MATCHING BLUE HOVER
    '========================================

    Private Class TableBlueHoverRenderer
        Inherits ToolStripProfessionalRenderer

        Protected Overrides Sub OnRenderMenuItemBackground(e As ToolStripItemRenderEventArgs)
            If e.Item.Selected Then
                Dim fillRect As New Rectangle(Point.Empty, e.Item.Size)
                Using brush As New SolidBrush(Color.FromArgb(0, 120, 215))
                    e.Graphics.FillRectangle(brush, fillRect)
                End Using
            Else
                MyBase.OnRenderMenuItemBackground(e)
            End If
        End Sub

        Protected Overrides Sub OnRenderItemText(e As ToolStripItemTextRenderEventArgs)
            e.TextColor = If(e.Item.Selected, Color.White, Color.FromArgb(40, 40, 40))
            MyBase.OnRenderItemText(e)
        End Sub
    End Class

    '========================================
    ' NESTED CONFIRM DIALOG 
    '========================================

    Private NotInheritable Class ConfirmDialog
        Inherits Form

        Public Shared Function Ask(owner As IWin32Window, message As String, title As String) As Boolean
            Using dlg As New ConfirmDialog(message, title)
                Return dlg.ShowDialog(owner) = DialogResult.Yes
            End Using
        End Function

        Private Sub New(message As String, title As String)
            Text = title
            Font = New Font("Century Gothic", 9, FontStyle.Regular)
            FormBorderStyle = FormBorderStyle.FixedDialog
            StartPosition = FormStartPosition.CenterParent
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False

            Dim lbl As New Label() With {
                .Text = message,
                .AutoSize = True,
                .MaximumSize = New Size(380, 0),
                .Location = New Point(20, 20)
            }

            Controls.Add(lbl)

            Dim btnTop As Integer = lbl.Bottom + 25
            Dim clientWidth As Integer = Math.Max(lbl.Right + 20, 340)
            ClientSize = New Size(clientWidth, btnTop + 30 + 20)

            Dim btnConfirm As New Button() With {
                .Text = "Confirm",
                .Size = New Size(90, 30),
                .DialogResult = DialogResult.Yes
            }
            btnConfirm.Location = New Point(clientWidth - 20 - btnConfirm.Width, btnTop)

            Dim btnNo As New Button() With {
                .Text = "No",
                .Size = New Size(90, 30),
                .DialogResult = DialogResult.No
            }
            btnNo.Location = New Point(btnConfirm.Left - 10 - btnNo.Width, btnTop)

            Controls.Add(btnNo)
            Controls.Add(btnConfirm)

            AcceptButton = btnConfirm
            CancelButton = btnNo
        End Sub
    End Class

    '========================================
    ' NESTED EDIT DIALOG
    '========================================

    Private NotInheritable Class EditUserDialog
        Inherits Form

        Private ReadOnly txtName As New TextBox()
        Private ReadOnly txtEmail As New TextBox()
        Private ReadOnly txtPhone As New TextBox()
        Private ReadOnly btnOk As New Button()
        Private ReadOnly btnCancel As New Button()
        Private ReadOnly errors As New ErrorProvider()

        Public ReadOnly Property FullName As String
            Get
                Return txtName.Text.Trim()
            End Get
        End Property

        Public ReadOnly Property Email As String
            Get
                Return txtEmail.Text.Trim()
            End Get
        End Property

        Public ReadOnly Property Phone As String
            Get
                Return txtPhone.Text.Trim()
            End Get
        End Property

        Public Sub New(userId As String, fullName As String, email As String, phone As String)
            Text = $"Edit User Information - {userId}"
            Font = New Font("Century Gothic", 9, FontStyle.Regular)
            FormBorderStyle = FormBorderStyle.FixedDialog
            StartPosition = FormStartPosition.CenterParent
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False
            ClientSize = New Size(400, 230)

            errors.ContainerControl = Me
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink

            AddField("Full Name:", txtName, 20, fullName)
            AddField("Email Address:", txtEmail, 70, email)
            AddField("Phone Number:", txtPhone, 120, phone)

            btnOk.Text = "Save"
            btnOk.Size = New Size(90, 30)
            btnOk.Location = New Point(200, 180)
            AddHandler btnOk.Click, AddressOf OnSaveClicked

            btnCancel.Text = "Cancel"
            btnCancel.Size = New Size(90, 30)
            btnCancel.Location = New Point(300, 180)
            btnCancel.DialogResult = DialogResult.Cancel

            Controls.Add(btnOk)
            Controls.Add(btnCancel)

            AcceptButton = btnOk
            CancelButton = btnCancel
        End Sub

        Private Sub AddField(caption As String, box As TextBox, top As Integer, value As String)
            Dim lbl As New Label() With {
                .Text = caption,
                .AutoSize = True,
                .Location = New Point(20, top)
            }
            box.Location = New Point(20, top + 20)
            box.Width = 360
            box.Text = value
            Controls.Add(lbl)
            Controls.Add(box)
        End Sub

        Private Sub OnSaveClicked(sender As Object, e As EventArgs)
            errors.Clear()
            Dim firstInvalid As Control = Nothing

            If FullName.Length = 0 Then
                errors.SetError(txtName, "Full name is required.")
                If firstInvalid Is Nothing Then firstInvalid = txtName
            End If

            If Not EmailRegex.IsMatch(Email) Then
                errors.SetError(txtEmail, "Enter a valid email address.")
                If firstInvalid Is Nothing Then firstInvalid = txtEmail
            End If

            If Phone.Length > 0 AndAlso Not PhoneRegex.IsMatch(Phone) Then
                errors.SetError(txtPhone, "Phone must contain digits only (7 to 15 digits).")
                If firstInvalid Is Nothing Then firstInvalid = txtPhone
            End If

            If firstInvalid IsNot Nothing Then
                firstInvalid.Focus()
                Return
            End If

            DialogResult = DialogResult.OK
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then errors.Dispose()
            MyBase.Dispose(disposing)
        End Sub
    End Class

End Class