Public Class ucStudentGradebook

    Private _isPredictiveMode As Boolean = False
    Private _realGrades As New List(Of Object())   ' backup of the student's actual grades

    Private Sub ucStudentGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetupRubricLabels()
        LoadRealGrade()
        SetGridReadOnly(True)
    End Sub

    Private Sub SetupRubricLabels()
        AddRubricLabel("Class Paricipation(10%)")
        AddRubricLabel("Quizzes (20%)")
        AddRubricLabel("Projects (30%)")
        AddRubricLabel("Final Exam (40%)")
    End Sub

    Private Sub AddRubricLabel(text As String)
        Dim lbl As New Label()
        lbl.Text = text
        lbl.AutoSize = True
        lbl.Padding = New Padding(10, 6, 10, 6)
        lbl.BackColor = Color.White
        lbl.ForeColor = Color.Black
        lbl.Font = New Font("Century Gothic", 9, FontStyle.Bold)
        lbl.Margin = New Padding(0, 0, 10, 0)
        flowRubric.Controls.Add(lbl)
    End Sub

    Private Sub LoadRealGrade()
        dgvGradebook.Rows.Clear()
        ' Single row — this student's own real grade (from your mockup)
        dgvGradebook.Rows.Add(1, "Last Name, First Name M.I.", "", "5%", "5.00", "", "",
                              "", "50.00", "", "50.00", "10.00", "", "0.00", "", "50.00", "", "")
    End Sub

    Private Sub SetGridReadOnly(isReadOnly As Boolean)
        dgvGradebook.ReadOnly = isReadOnly
        dgvGradebook.BackgroundColor = If(isReadOnly, Color.White, Color.FromArgb(255, 252, 230))  ' faint yellow tint = editable
    End Sub


    '========================================
    ' PREDICTIVE MODE
    '========================================

    Private Sub btnPredictive_Click(sender As Object, e As EventArgs) Handles btnPredictive.Click

        _isPredictiveMode = Not _isPredictiveMode

        If _isPredictiveMode Then
            btnPredictive.Text = "Exit Predictive Mode"
            btnPredictive.BackColor = Color.FromArgb(220, 53, 69)   ' red = "you're simulating"
            btnPredictive.TextAlign = ContentAlignment.MiddleCenter
            SetGridReadOnly(False)
        Else
            btnPredictive.Text = "Predictive Mode"
            btnPredictive.BackColor = Color.FromArgb(241, 196, 15)  ' yellow = default
            SetGridReadOnly(True)
            LoadRealGrade()   ' discard whatever the student typed, restore real grade
        End If

    End Sub


    '========================================
    ' BACK
    '========================================

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Dim mainForm = TryCast(FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            mainForm.ShowPage(New ucStudentDashboard)
        End If
    End Sub

End Class