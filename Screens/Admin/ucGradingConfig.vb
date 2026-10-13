Imports System.Drawing
Imports System.Windows.Forms

Public Class ucGradingConfig

    ' State variables for active configuration
    Private CurrentSchoolYear As String = "2026-2027"
    Private CurrentTerm As String = "1"
    Private TermStartDate As DateTime = New DateTime(2026, 8, 18)
    Private TermEndDate As DateTime = New DateTime(2026, 12, 18)
    Private EncodingDeadline As DateTime = New DateTime(2026, 12, 15, 23, 59, 0)
    Private IsSystemLocked As Boolean = False

    Private Sub ucGradingConfig_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RefreshUI()
    End Sub

    '========================================
    ' UI REFRESH & GUARD CHECKS
    '========================================

    Private Sub RefreshUI()
        lblSchoolYearValue.Text = CurrentSchoolYear
        lblTermValue.Text = CurrentTerm
        lblStartDateValue.Text = If(TermStartDate = DateTime.MinValue, "Not Set", TermStartDate.ToString("MM/dd/yyyy"))
        lblEndDateValue.Text = If(TermEndDate = DateTime.MinValue, "Not Set", TermEndDate.ToString("MM/dd/yyyy"))

        Dim hasValidSemester As Boolean = TermStartDate <> DateTime.MinValue AndAlso TermEndDate <> DateTime.MinValue
        btnEditEncoding.Enabled = hasValidSemester

        lblDeadlineValue.Text = EncodingDeadline.ToString("MM/dd/yyyy hh:mm tt")
        lblLockStatusValue.Text = If(IsSystemLocked OrElse DateTime.Now > EncodingDeadline, "LOCKED", "OPEN")
        lblLockStatusValue.ForeColor = If(lblLockStatusValue.Text = "LOCKED", Color.Red, Color.Green)
    End Sub

    '========================================
    ' EDIT SEMESTER SCHEDULE
    '========================================

    Private Sub btnEditSemester_Click(sender As Object, e As EventArgs) Handles btnEditSemester.Click
        Using dlg As New SemesterConfigDialog(CurrentSchoolYear, CurrentTerm, TermStartDate, TermEndDate)
            If dlg.ShowDialog(Me.FindForm()) = DialogResult.OK Then
                If dlg.EndDate <= dlg.StartDate Then
                    MessageBox.Show("Term End Date must be after the Start Date.", "Validation Error", MessageBoxButtons.OK)
                    Return
                End If

                CurrentSchoolYear = dlg.SchoolYear
                CurrentTerm = dlg.Term
                TermStartDate = dlg.StartDate
                TermEndDate = dlg.EndDate

                ' Reset or adjust deadline if current deadline is out of range
                Dim maxAllowedDeadline As DateTime = TermEndDate.AddMonths(1)
                If EncodingDeadline < TermStartDate OrElse EncodingDeadline > maxAllowedDeadline Then
                    EncodingDeadline = TermEndDate.AddDays(14)
                End If

                RefreshUI()
                MessageBox.Show("Active semester configuration updated successfully.", "Success", MessageBoxButtons.OK)
            End If
        End Using
    End Sub

    '========================================
    ' EDIT ENCODING DEADLINE
    '========================================

    Private Sub btnEditEncoding_Click(sender As Object, e As EventArgs) Handles btnEditEncoding.Click
        If TermStartDate = DateTime.MinValue OrElse TermEndDate = DateTime.MinValue Then
            MessageBox.Show("Please configure the Active Semester Schedule before setting a grade encoding deadline.",
                            "Semester Required", MessageBoxButtons.OK)
            Return
        End If

        Using dlg As New DeadlineConfigDialog(EncodingDeadline, IsSystemLocked, TermStartDate, TermEndDate)
            If dlg.ShowDialog(Me.FindForm()) = DialogResult.OK Then
                EncodingDeadline = dlg.NewDeadline
                IsSystemLocked = dlg.IsLocked

                RefreshUI()
                MessageBox.Show("Grade encoding configuration updated successfully.", "Success", MessageBoxButtons.OK)
            End If
        End Using
    End Sub

    '========================================
    ' NESTED SEMESTER DIALOG
    '========================================

    Private NotInheritable Class SemesterConfigDialog
        Inherits Form

        Private ReadOnly txtSchoolYear As New TextBox() With {.Width = 310}
        Private ReadOnly cboTerm As New ComboBox() With {.Width = 310, .DropDownStyle = ComboBoxStyle.DropDownList}
        Private ReadOnly dtpStart As New DateTimePicker() With {.Width = 310, .Format = DateTimePickerFormat.Short}
        Private ReadOnly dtpEnd As New DateTimePicker() With {.Width = 310, .Format = DateTimePickerFormat.Short}

        Public ReadOnly Property SchoolYear As String
            Get
                Return txtSchoolYear.Text.Trim()
            End Get
        End Property

        Public ReadOnly Property Term As String
            Get
                Return cboTerm.SelectedItem?.ToString()
            End Get
        End Property

        Public ReadOnly Property StartDate As DateTime
            Get
                Return dtpStart.Value.Date
            End Get
        End Property

        Public ReadOnly Property EndDate As DateTime
            Get
                Return dtpEnd.Value.Date
            End Get
        End Property

        Public Sub New(sy As String, term As String, startDt As DateTime, endDt As DateTime)
            Text = "Edit Active Semester"
            Font = New Font("Century Gothic", 9, FontStyle.Regular)
            Size = New Size(370, 360)
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False

            cboTerm.Items.AddRange({"1", "2", "Summer / Midyear"})

            Dim pnl As New Panel() With {.Dock = DockStyle.Fill, .Padding = New Padding(20)}

            Dim y As Integer = 15
            AddLabeledControl(pnl, "School Year:", txtSchoolYear, sy, y)
            y += 60
            AddLabeledControlCombo(pnl, "Term:", cboTerm, term, y)
            y += 60
            AddLabeledControlDtp(pnl, "Term Start Date:", dtpStart, If(startDt = DateTime.MinValue, DateTime.Now, startDt), y)
            y += 60
            AddLabeledControlDtp(pnl, "Term End Date:", dtpEnd, If(endDt = DateTime.MinValue, DateTime.Now.AddMonths(4), endDt), y)
            y += 65

            Dim btnOk As New Button() With {
                .Text = "Save",
                .Size = New Size(90, 32),
                .Location = New Point(130, y),
                .BackColor = Color.FromArgb(30, 86, 62),
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .DialogResult = DialogResult.OK
            }
            btnOk.FlatAppearance.BorderSize = 0

            Dim btnCancel As New Button() With {
                .Text = "Cancel",
                .Size = New Size(90, 32),
                .Location = New Point(230, y),
                .BackColor = Color.Gray,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .DialogResult = DialogResult.Cancel
            }
            btnCancel.FlatAppearance.BorderSize = 0

            pnl.Controls.Add(btnOk)
            pnl.Controls.Add(btnCancel)

            Controls.Add(pnl)
            AcceptButton = btnOk
            CancelButton = btnCancel
        End Sub

        Private Sub AddLabeledControl(pnl As Panel, caption As String, box As TextBox, val As String, y As Integer)
            Dim lbl As New Label() With {.Text = caption, .AutoSize = True, .Location = New Point(20, y)}
            box.Location = New Point(20, y + 20)
            box.Text = val
            pnl.Controls.Add(lbl)
            pnl.Controls.Add(box)
        End Sub

        Private Sub AddLabeledControlCombo(pnl As Panel, caption As String, cbo As ComboBox, val As String, y As Integer)
            Dim lbl As New Label() With {.Text = caption, .AutoSize = True, .Location = New Point(20, y)}
            cbo.Location = New Point(20, y + 20)
            cbo.SelectedItem = val
            pnl.Controls.Add(lbl)
            pnl.Controls.Add(cbo)
        End Sub

        Private Sub AddLabeledControlDtp(pnl As Panel, caption As String, dtp As DateTimePicker, val As DateTime, y As Integer)
            Dim lbl As New Label() With {.Text = caption, .AutoSize = True, .Location = New Point(20, y)}
            dtp.Location = New Point(20, y + 20)
            dtp.Value = val
            pnl.Controls.Add(lbl)
            pnl.Controls.Add(dtp)
        End Sub
    End Class

    '========================================
    ' NESTED DEADLINE DIALOG
    '========================================

    Private NotInheritable Class DeadlineConfigDialog
        Inherits Form

        Private ReadOnly dtpDeadline As New DateTimePicker() With {.Width = 310, .Format = DateTimePickerFormat.Custom, .CustomFormat = "MM/dd/yyyy hh:mm tt"}
        Private ReadOnly chkLockSystem As New CheckBox() With {.Text = "Force Global Lock (Close Encoding Now)", .AutoSize = True}
        Private ReadOnly lblWarning As New Label() With {.Width = 310, .Height = 40, .ForeColor = Color.Red, .Font = New Font("Century Gothic", 8, FontStyle.Regular)}
        Private ReadOnly btnOk As New Button() With {.Text = "Save", .Size = New Size(90, 32), .BackColor = Color.FromArgb(30, 86, 62), .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .DialogResult = DialogResult.OK}
        Private ReadOnly btnCancel As New Button() With {.Text = "Cancel", .Size = New Size(90, 32), .BackColor = Color.Gray, .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat, .DialogResult = DialogResult.Cancel}

        Private ReadOnly termStart As DateTime
        Private ReadOnly termEnd As DateTime
        Private ReadOnly maxAllowedDeadline As DateTime

        Public ReadOnly Property NewDeadline As DateTime
            Get
                Return dtpDeadline.Value
            End Get
        End Property

        Public ReadOnly Property IsLocked As Boolean
            Get
                Return chkLockSystem.Checked
            End Get
        End Property

        Public Sub New(deadline As DateTime, isLocked As Boolean, termStartDt As DateTime, termEndDt As DateTime)
            Text = "Edit Grade Encoding Deadline"
            Font = New Font("Century Gothic", 9, FontStyle.Regular)
            Size = New Size(370, 310)
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False

            Me.termStart = termStartDt
            Me.termEnd = termEndDt
            Me.maxAllowedDeadline = termEndDt.AddMonths(1)

            Dim pnl As New Panel() With {.Dock = DockStyle.Fill, .Padding = New Padding(20)}

            Dim lblTitle As New Label() With {.Text = "Encoding Deadline:", .AutoSize = True, .Location = New Point(20, 15)}
            dtpDeadline.Location = New Point(20, 35)
            dtpDeadline.Value = If(deadline < termStartDt OrElse deadline > maxAllowedDeadline, termEndDt.AddDays(14), deadline)

            lblWarning.Location = New Point(20, 65)

            chkLockSystem.Checked = isLocked
            chkLockSystem.Location = New Point(20, 110)

            btnOk.Location = New Point(130, 160)
            btnOk.FlatAppearance.BorderSize = 0
            btnCancel.Location = New Point(230, 160)
            btnCancel.FlatAppearance.BorderSize = 0

            pnl.Controls.Add(lblTitle)
            pnl.Controls.Add(dtpDeadline)
            pnl.Controls.Add(lblWarning)
            pnl.Controls.Add(chkLockSystem)
            pnl.Controls.Add(btnOk)
            pnl.Controls.Add(btnCancel)

            Controls.Add(pnl)
            AcceptButton = btnOk
            CancelButton = btnCancel

            AddHandler dtpDeadline.ValueChanged, AddressOf OnDeadlineChanged
            ValidateSelectedDate(dtpDeadline.Value)
        End Sub

        Private Sub OnDeadlineChanged(sender As Object, e As EventArgs)
            ValidateSelectedDate(dtpDeadline.Value)
        End Sub

        Private Sub ValidateSelectedDate(selected As DateTime)
            If selected < termStart Then
                lblWarning.ForeColor = Color.Red
                lblWarning.Text = $"⚠️ Deadline cannot be earlier than Term Start Date ({termStart:MM/dd/yyyy})."
                btnOk.Enabled = False
            ElseIf selected > maxAllowedDeadline Then
                lblWarning.ForeColor = Color.Red
                lblWarning.Text = $"⚠️ Deadline exceeds 1-month post term-end margin ({maxAllowedDeadline:MM/dd/yyyy})."
                btnOk.Enabled = False
            Else
                If selected > termEnd Then
                    lblWarning.ForeColor = Color.DarkOrange
                    lblWarning.Text = $"ℹ️ Date falls in post-term grace period (Term Ends: {termEnd:MM/dd/yyyy})."
                Else
                    lblWarning.ForeColor = Color.Green
                    lblWarning.Text = "✓ Valid encoding window."
                End If
                btnOk.Enabled = True
            End If
        End Sub
    End Class

End Class