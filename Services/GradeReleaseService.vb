''' <summary>
''' Publishes the selected grading period and notifies the admin after Final is released.
''' </summary>
Public Class GradeReleaseService

  ''' <summary>Returns how many students received a grade for the selected period.</summary>
  Public Shared Function SaveAndRelease(classId As Integer, Optional period As String = "Final") As Integer
    If period <> "Midterm" AndAlso period <> "Final" Then Throw New InvalidOperationException("Choose Midterm or Final.")
    If Not SessionManager.IsTeacher Then Throw New InvalidOperationException("Only the class teacher can save grades.")
    If Not ClassService.TeacherOwnsClass(classId, SessionManager.CurrentUserId) Then
      Throw New InvalidOperationException("You are not the teacher of this class.")
    End If
    Dim info = ClassService.GetInfo(classId)
    If info Is Nothing Then Throw New InvalidOperationException("Class not found.")
    ClassService.EnsureEditable(info)
    Dim blockedReason As String = ClassService.CannotChangeGradebookReason(classId)
    If blockedReason IsNot Nothing Then Throw New InvalidOperationException(blockedReason)

    DatabaseHelper.RunInTransaction(
      Sub(tx)
        tx.Exec("CALL sp_release_class_grades(@c, @p)", DatabaseHelper.P("@c", classId), DatabaseHelper.P("@p", period))
        NotificationService.NotifyGradesSaved(tx, classId, SessionManager.CurrentUserId, period)
      End Sub)

    Return DatabaseHelper.ScalarInt(
      "SELECT COUNT(*) FROM enrollment WHERE class_id = @c AND " & If(period = "Midterm", "midterm_grade", "finals_grade") & " IS NOT NULL",
      {DatabaseHelper.P("@c", classId)})
  End Function
End Class
