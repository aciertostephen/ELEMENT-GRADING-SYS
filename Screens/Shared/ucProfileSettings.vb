Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Windows.Forms

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

        ' Prevent click color change by setting ActiveLinkColor to normal link color
        linkProfilePicture.ActiveLinkColor = Color.FromArgb(32, 124, 64)
        linkEditInfo.ActiveLinkColor = Color.FromArgb(32, 124, 64)

        MakeAvatarCircular()
    End Sub

    Private Sub MakeAvatarCircular()
        Dim path As New GraphicsPath()
        path.AddEllipse(0, 0, picAvatar.Width, picAvatar.Height)
        picAvatar.Region = New Region(path)
        picAvatar.BackColor = Color.FromArgb(220, 220, 220)
        picAvatar.SizeMode = PictureBoxSizeMode.Zoom
    End Sub

    Private Sub linkProfilePicture_MouseEnter(sender As Object, e As EventArgs) Handles linkProfilePicture.MouseEnter
        linkProfilePicture.LinkColor = Color.FromArgb(23, 61, 50)
    End Sub

    Private Sub linkProfilePicture_MouseLeave(sender As Object, e As EventArgs) Handles linkProfilePicture.MouseLeave
        linkProfilePicture.LinkColor = Color.FromArgb(32, 124, 64)
    End Sub

    '========================================
    ' SET PROFILE PICTURE (CENTER CIRCLE CROP)
    '========================================

    Private Sub LinkLabelEdit1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linkProfilePicture.LinkClicked
        Using ofd As New OpenFileDialog()
            ofd.Title = "Select Profile Picture"
            ofd.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            ofd.Multiselect = False

            If ofd.ShowDialog(Me.FindForm()) = DialogResult.OK Then
                Try
                    ' Read image into MemoryStream to prevent file lock
                    Dim imgBytes As Byte() = File.ReadAllBytes(ofd.FileName)
                    Using ms As New MemoryStream(imgBytes)
                        Using originalImg As Image = Image.FromStream(ms)
                            ' Crop center square into circle
                            Dim circularImg As Bitmap = MakeCenterCircularBitmap(originalImg, picAvatar.Width, picAvatar.Height)

                            If picAvatar.Image IsNot Nothing Then
                                picAvatar.Image.Dispose()
                            End If

                            picAvatar.Image = circularImg
                        End Using
                    End Using

                    MessageBox.Show("Profile picture updated successfully!", "Success", MessageBoxButtons.OK)

                Catch ex As Exception
                    MessageBox.Show("Unable to load the selected image. Please try another file." & vbCrLf & ex.Message,
                                    "Error Loading Image", MessageBoxButtons.OK)
                End Try
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Extracts the largest possible center square from the image and crops it into a anti-aliased circle.
    ''' Prevents stretching or distortion.
    ''' </summary>
    Private Function MakeCenterCircularBitmap(srcImage As Image, targetWidth As Integer, targetHeight As Integer) As Bitmap
        ' 1. Determine maximum square dimensions from source center
        Dim squareSize As Integer = Math.Min(srcImage.Width, srcImage.Height)
        Dim srcX As Integer = (srcImage.Width - squareSize) \ 2
        Dim srcY As Integer = (srcImage.Height - squareSize) \ 2

        Dim srcRect As New Rectangle(srcX, srcY, squareSize, squareSize)
        Dim destRect As New Rectangle(0, 0, targetWidth, targetHeight)

        ' 2. Draw cropped center square into circular path
        Dim bmp As New Bitmap(targetWidth, targetHeight)

        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            g.Clear(Color.Transparent)

            Using path As New GraphicsPath()
                path.AddEllipse(0, 0, targetWidth - 1, targetHeight - 1)
                g.SetClip(path)
                ' Draws the center square without scaling non-proportionally
                g.DrawImage(srcImage, destRect, srcRect, GraphicsUnit.Pixel)
            End Using
        End Using

        Return bmp
    End Function

    '========================================
    ' EDIT INFO HOVER
    '========================================

    Private Sub linkEditInfo_MouseEnter_1(sender As Object, e As EventArgs) Handles linkEditInfo.MouseEnter
        linkEditInfo.LinkColor = Color.FromArgb(23, 61, 50)
    End Sub

    Private Sub linkEditInfo_MouseLeave(sender As Object, e As EventArgs) Handles linkEditInfo.MouseLeave
        linkEditInfo.LinkColor = Color.FromArgb(32, 124, 64)
    End Sub

    '========================================
    ' NAVIGATION & LOGOUT
    '========================================

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