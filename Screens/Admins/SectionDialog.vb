Imports System.Windows.Forms

''' <summary>Edit a section's code and batch (program, school year and year level are fixed).</summary>
Public Class SectionDialog
  Inherits DialogBase

  Private ReadOnly _sectionId As Integer
  Private ReadOnly txtCode As TextBox = MakeText(15)
  Private ReadOnly txtBatch As TextBox = MakeText(9)

  Public Sub New(sectionId As Integer)
    MyBase.New("Edit Section", 400)
    _sectionId = sectionId
    Dim info = SectionService.GetInfo(sectionId)
    txtCode.Text = info.Code
    txtBatch.Text = info.Batch

    AddNote(info.ProgramCode & "  |  " & AcademicContext.OrdinalYear(info.YearLevel) & "  |  " & info.SchoolYear &
            "  (program, year level and school year cannot be changed)")
    AddField("Section code:", txtCode)
    AddField("Batch (e.g. 2025-2026, optional):", txtBatch)
    FinishLayout("Save")
  End Sub

  Protected Overrides Function OnSubmit() As Boolean
    Try
      SectionService.Update(_sectionId, txtCode.Text, txtBatch.Text)
      Return True
    Catch ex As InvalidOperationException
      Msg.Warn(ex.Message)
      Return False
    End Try
  End Function
End Class
