Imports System.Windows.Forms

''' <summary>
''' Create a class (course x teacher x active term x section) or edit an existing one.
''' New(0)       -> create in the ACTIVE term
''' New(classId) -> edit course / section / teacher of that class (term never changes)
''' </summary>
Public Class ClassDialog
    Inherits DialogBase

    Private ReadOnly _classId As Integer
    Private ReadOnly _termId As Integer
    Private ReadOnly cboCourse As ComboBox
    Private ReadOnly cboSection As ComboBox
    Private ReadOnly cboTeacher As ComboBox

    Public Sub New(Optional classId As Integer = 0)
        MyBase.New(If(classId = 0, "Create Class", "Edit Class"), 440)
        _classId = classId

        Dim schoolYear As String
        Dim info As ClassService.ClassInfo = Nothing
        If classId = 0 Then
            _termId = AcademicContext.ActiveTermId()
            schoolYear = AcademicContext.ActiveSchoolYear()
        Else
            info = ClassService.GetInfo(classId)
            _termId = info.TermId
            schoolYear = info.SchoolYear
        End If

        cboCourse = MakeCombo(CourseService.ListForCombo(), "display", "course_id")
        cboSection = MakeCombo(ClassService.SectionsForCombo(schoolYear), "section_label", "section_id")
        cboTeacher = MakeCombo(ClassService.TeachersForCombo(), "teacher_name", "teacher_id")

        AddField("Course:", cboCourse)
        AddField("Section (label only - the teacher imports the real class list):", cboSection)
        AddField("Teacher:", cboTeacher)
        AddNote("School year: " & schoolYear & If(classId = 0, "  (active term)", "  (term cannot be changed)"))
        If info IsNot Nothing AndAlso info.ScoreCount > 0 Then
            AddNote("This class already has " & info.ScoreCount & " recorded score(s). Changing the teacher moves the whole gradebook to the new teacher.", True)
        End If
        FinishLayout(If(classId = 0, "Create", "Save"))

        If info IsNot Nothing Then
            cboCourse.SelectedValue = info.CourseId
            cboSection.SelectedValue = info.SectionId
            cboTeacher.SelectedValue = info.TeacherId
        End If
    End Sub

    Protected Overrides Function OnSubmit() As Boolean
        If cboCourse.SelectedValue Is Nothing OrElse cboSection.SelectedValue Is Nothing OrElse cboTeacher.SelectedValue Is Nothing Then
            Msg.Warn("Please select a course, a section and a teacher.")
            Return False
        End If
        Try
            Dim courseId As Integer = Convert.ToInt32(cboCourse.SelectedValue)
            Dim sectionId As Integer = Convert.ToInt32(cboSection.SelectedValue)
            Dim teacherId As String = cboTeacher.SelectedValue.ToString()
            If _classId = 0 Then
                ClassService.Create(courseId, sectionId, teacherId, _termId)
            Else
                ClassService.Update(_classId, courseId, sectionId, teacherId)
            End If
            Return True
        Catch ex As InvalidOperationException
            Msg.Warn(ex.Message)
            Return False
        End Try
    End Function
End Class