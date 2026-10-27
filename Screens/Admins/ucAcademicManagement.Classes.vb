Imports System.Windows.Forms

' =====================================================================
' ucAcademicManagement - Courses / Classes part (new partial class).
' Needs a button named btnCreateCourse in the designer (you said you add it).
' See PATCHES_AND_TODO.md for the 3 small edits in the existing ucAcademicManagement file.
' =====================================================================
Partial Public Class ucAcademicManagement

    ' ---------- Create Course ----------
    Private Sub btnCreateCourse_Click(sender As Object, e As EventArgs) Handles btnCreateCourse.Click
        Using dlg As New CreateCourseDialog()
            If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                Msg.Info("Course created. It can now be used when creating a class.")
            End If
        End Using
    End Sub

    ' ---------- Create Class (call this from btnAddCourse_Click's class branch) ----------
    Friend Sub ShowAddClassDialog()
        Dim sy As String = AcademicContext.ActiveSchoolYear()
        If String.IsNullOrEmpty(sy) Then
            Msg.Warn("No active semester. Activate one in Semester/Grading Settings first.")
            Return
        End If
        If ClassService.SectionsForCombo(sy).Rows.Count = 0 Then
            Msg.Warn("There are no sections for " & sy & " yet. Create a section first.")
            Return
        End If
        If CourseService.ListForCombo().Rows.Count = 0 Then
            Msg.Warn("There are no courses yet. Use Create Course first.")
            Return
        End If
        If ClassService.TeachersForCombo().Rows.Count = 0 Then
            Msg.Warn("There are no teachers yet. Create a teacher account first.")
            Return
        End If

        Using dlg As New ClassDialog()
            If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                Msg.Info("Class created with the default grading rubric. The teacher can now import the class list.")
                LoadCoursesFromDatabase()
                LoadAcademicStats()
            End If
        End Using
    End Sub

    ' ---------- Right-click on a class card: Edit / Delete ----------
    Friend Sub AttachClassMenu(card As Control, classId As Integer)
        Dim menu As New ContextMenuStrip()
        Dim miEdit As ToolStripItem = menu.Items.Add("Edit class...")
        Dim miDelete As ToolStripItem = menu.Items.Add("Delete class")
        AddHandler miEdit.Click, Sub() EditClass(classId)
        AddHandler miDelete.Click, Sub() DeleteClass(classId)
        AddHandler card.Disposed, Sub() menu.Dispose()
        card.AttachContextMenu(menu)
    End Sub

    Private Sub EditClass(classId As Integer)
        Try
            Dim info = ClassService.GetInfo(classId)
            If info Is Nothing Then Return
            ClassService.EnsureEditable(info)
            Using dlg As New ClassDialog(classId)
                If dlg.ShowDialog(FindForm()) = DialogResult.OK Then
                    LoadCoursesFromDatabase()
                End If
            End Using
        Catch ex As InvalidOperationException
            Msg.Warn(ex.Message)
        Catch ex As Exception
            Msg.Fail("edit the class", ex)
        End Try
    End Sub

    Private Sub DeleteClass(classId As Integer)
        Try
            Dim info = ClassService.GetInfo(classId)
            If info Is Nothing Then Return
            ClassService.EnsureEditable(info)
            If info.IsReleased Then
                Msg.Warn("Grades for " & info.Title & " were already released to students, so this class can no longer be deleted.")
                Return
            End If
            Dim text As String = "Delete " & info.Title & "?" & vbCrLf & vbCrLf &
                info.StudentCount & " enrolled student(s) and " & info.ScoreCount & " recorded score(s) will be permanently removed."
            If Not Msg.Confirm(text, "Delete class", "Delete", True) Then Return
            ClassService.Delete(classId)
            LoadCoursesFromDatabase()
            LoadAcademicStats()
        Catch ex As InvalidOperationException
            Msg.Warn(ex.Message)
        Catch ex As Exception
            Msg.Fail("delete the class", ex)
        End Try
    End Sub

    ' ---------- History view: open on the ACTIVE school year, other years stay browsable ----------
    Friend Sub SelectActiveYearInFilter()
        Dim sy As String = AcademicContext.ActiveSchoolYear()
        If String.IsNullOrEmpty(sy) Then Return
        Dim idx As Integer = cboAY.Items.IndexOf(sy)
        If idx > 0 Then cboAY.SelectedIndex = idx
    End Sub

    ' ---------- Opened from a notification's "Open class" link ----------
    Friend Sub OpenClassFromNotification(classId As Integer)
        Try
            Dim dt = DatabaseHelper.ExecuteQuery(
                "SELECT co.course_code, co.course_name, s.section_code, CONCAT(tp.first_name, ' ', tp.last_name) AS teacher " &
                "FROM classes c JOIN course co ON co.course_id = c.course_id JOIN section s ON s.section_id = c.section_id " &
                "JOIN teacher_profile tp ON tp.teacher_id = c.teacher_id WHERE c.class_id = @c", {DatabaseHelper.P("@c", classId)})
            If dt.Rows.Count = 0 Then
                Msg.Warn("This class no longer exists.")
                Return
            End If
            Dim r = dt.Rows(0)
            ShowGradebookPOV(classId, r("course_code").ToString() & " - " & r("course_name").ToString(), r("section_code").ToString(), r("teacher").ToString())
        Catch ex As Exception
            Msg.Fail("open the class", ex)
        End Try
    End Sub

End Class