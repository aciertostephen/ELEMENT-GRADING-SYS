''' <summary>
''' The teacher's gradebook Save button = compute final grades with the ONE formula, make them
''' visible to students (classes.grades_released_at), and notify the admin. Atomic.
''' </summary>
Public Class GradeReleaseService

  ''' <summary>Returns how many students received a final grade.</summary>
  Public Shared Function SaveAndRelease(classId As Integer) As Integer
    If Not SessionManager.IsTeacher Then Throw New InvalidOperationException("Only the class teacher can save grades.")
    If Not ClassService.TeacherOwnsClass(classId, SessionManager.CurrentUserId) Then
      Throw New InvalidOperationException("You are not the teacher of this class.")
    End If
    Dim info = ClassService.GetInfo(classId)
    ClassService.EnsureEditable(info)

    DatabaseHelper.RunInTransaction(
      Sub(tx)
        tx.Exec("CALL sp_release_class_grades(@c)", DatabaseHelper.P("@c", classId))
        NotificationService.NotifyGradesSaved(tx, classId, SessionManager.CurrentUserId)
      End Sub)

    Return DatabaseHelper.ScalarInt(
      "SELECT COUNT(*) FROM enrollment WHERE class_id = @c AND final_grade IS NOT NULL",
      {DatabaseHelper.P("@c", classId)})
  End Function
End Class
