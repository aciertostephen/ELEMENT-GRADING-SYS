Imports System.Windows.Forms

''' <summary>Opened by btnCreateCourse in Academic Management.</summary>
Public Class CreateCourseDialog
    Inherits DialogBase

    Private ReadOnly txtCode As TextBox = MakeText(15)
    Private ReadOnly txtName As TextBox = MakeText(100)
    Private ReadOnly nudUnits As New NumericUpDown() With {.Minimum = 1, .Maximum = 9, .Value = 3}

    Public Sub New()
        MyBase.New("Create Course", 420)
        AddField("Course code (e.g. CS 104):", txtCode)
        AddField("Course name:", txtName)
        AddField("Units:", nudUnits)
        FinishLayout("Create")
    End Sub

    Protected Overrides Function OnSubmit() As Boolean
        Try
            CourseService.Create(txtCode.Text, txtName.Text, CInt(nudUnits.Value))
            Return True
        Catch ex As InvalidOperationException
            Msg.Warn(ex.Message)
            Return False
        End Try
    End Function
End Class