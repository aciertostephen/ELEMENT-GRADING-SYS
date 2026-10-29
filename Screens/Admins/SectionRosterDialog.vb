Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Read-only list of every student who was placed in a section (from the section history).
''' Old sections keep their batch here even after the students moved up. The CLASS lists of that
''' batch are the enrollments of its classes and are never touched by placement.
''' </summary>
Public Class SectionRosterDialog
  Inherits Form

  Public Sub New(sectionId As Integer)
    Dim info = SectionService.GetInfo(sectionId)
    Text = "Students of " & info.Code & " (" & info.SchoolYear & ")"
    FormBorderStyle = FormBorderStyle.Sizable
    StartPosition = FormStartPosition.CenterParent
    MinimizeBox = False
    ShowInTaskbar = False
    ShowIcon = False
    ClientSize = New Size(640, 440)

    Dim lbl As New Label() With {
      .Dock = DockStyle.Top, .Height = 44, .Padding = New Padding(10, 8, 10, 0), .ForeColor = SystemColors.ControlText,
      .Text = "Everyone who was placed in this section. Students who moved on are kept here as history. " &
              "Use ""View this section's classes"" to see the class lists of this batch."}

    Dim grid As New DataGridView() With {
      .Dock = DockStyle.Fill, .ReadOnly = True, .AllowUserToAddRows = False, .AllowUserToDeleteRows = False,
      .RowHeadersVisible = False, .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
      .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, .BackgroundColor = SystemColors.Window}
    grid.DataSource = SectionService.RosterHistory(sectionId)
    If grid.Columns.Contains("student_id") Then grid.Columns("student_id").HeaderText = "Student No."
    If grid.Columns.Contains("full_name") Then grid.Columns("full_name").HeaderText = "Name"
    If grid.Columns.Contains("effective_from") Then grid.Columns("effective_from").HeaderText = "In section from"
    If grid.Columns.Contains("effective_to") Then grid.Columns("effective_to").HeaderText = "Until"
    If grid.Columns.Contains("status") Then grid.Columns("status").HeaderText = "Status"
    For Each c As String In {"effective_from", "effective_to"}
      If grid.Columns.Contains(c) Then grid.Columns(c).DefaultCellStyle.Format = "MM/dd/yyyy"
    Next

    Dim bottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 44}
    Dim btnClose As New Button() With {.Text = "Close", .DialogResult = DialogResult.Cancel, .Size = New Size(80, 26)}
    btnClose.Location = New Point(bottom.Width - 96, 9)
    btnClose.Anchor = AnchorStyles.Top Or AnchorStyles.Right
    bottom.Controls.Add(btnClose)

    Controls.Add(grid)
    Controls.Add(bottom)
    Controls.Add(lbl)
    CancelButton = btnClose
  End Sub
End Class
