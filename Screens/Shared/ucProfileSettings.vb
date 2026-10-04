Public Class ucProfileSettings

    Private _role As String   ' the actual role code: "Teacher" or "Student"

    Public Sub SetData(fullName As String, roleCode As String, roleLabel As String, department As String, email As String, phone As String)
        _role = roleCode          ' "Teacher" / "Student" — used for routing
        lblFullName.Text = fullName
        lblRole.Text = roleLabel  ' "Professor" / "Student" — used for display
        lblDepartment.Text = department
        lblEmail.Text = email
        lblPhone.Text = phone
    End Sub

    Private Sub ucProfileSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If String.IsNullOrEmpty(_role) Then
            SetData("Last Name, Full Name M.I.", "Teacher", "Professor", "Department", "Email", "Phone Number")
        End If
        MakeAvatarCircular()
    End Sub

    Private Sub MakeAvatarCircular()
        Dim path As New Drawing2D.GraphicsPath()
        path.AddEllipse(0, 0, picAvatar.Width, picAvatar.Height)
        picAvatar.Region = New Region(path)
        picAvatar.BackColor = Color.FromArgb(220, 220, 220)
        picAvatar.SizeMode = PictureBoxSizeMode.Zoom
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click

        Dim mainForm As MainForm = TryCast(Me.FindForm(), MainForm)
        If mainForm Is Nothing Then Return

        Select Case _role
            Case "Teacher"
                mainForm.ShowPage(New ucTeacherDashboard())
            Case "Student"
                mainForm.ShowPage(New ucStudentDashboard())
            Case Else
                mainForm.ShowPage(New ucUserManagement())
        End Select

    End Sub

    Private Sub btnLogOut_Click(sender As Object, e As EventArgs) Handles btnLogOut.Click
        Dim mainForm As MainForm = TryCast(Me.FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            mainForm.pnlApp.Visible = False
            mainForm.pnlLogin.Visible = True
        End If
    End Sub

End Class