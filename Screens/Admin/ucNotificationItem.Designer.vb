<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ucNotificationItem
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        lblTimestamp = New Label()
        lblMessage = New Label()
        btnAction = New ReaLTaiizor.Controls.Button()
        SuspendLayout()
        ' 
        ' lblTimestamp
        ' 
        lblTimestamp.AutoSize = True
        lblTimestamp.BackColor = Color.Transparent
        lblTimestamp.Font = New Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblTimestamp.ForeColor = Color.Black
        lblTimestamp.Location = New Point(24, 10)
        lblTimestamp.Name = "lblTimestamp"
        lblTimestamp.Size = New Size(204, 20)
        lblTimestamp.TabIndex = 8
        lblTimestamp.Text = "HH:MM AM  -  DD-MM-YYYY"
        ' 
        ' lblMessage
        ' 
        lblMessage.AutoSize = True
        lblMessage.BackColor = Color.Transparent
        lblMessage.Font = New Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblMessage.ForeColor = Color.Black
        lblMessage.Location = New Point(24, 30)
        lblMessage.Name = "lblMessage"
        lblMessage.Size = New Size(475, 20)
        lblMessage.TabIndex = 9
        lblMessage.Text = "Official Midterm Grades for BSCS 1A - GE 001 are now available."
        ' 
        ' btnAction
        ' 
        btnAction.BackColor = Color.Transparent
        btnAction.BackgroundImageLayout = ImageLayout.Zoom
        btnAction.BorderColor = Color.FromArgb(CByte(32), CByte(34), CByte(37))
        btnAction.EnteredBorderColor = Color.Empty
        btnAction.EnteredColor = Color.FromArgb(CByte(44), CByte(110), CByte(90))
        btnAction.Font = New Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnAction.Image = Nothing
        btnAction.ImageAlign = ContentAlignment.MiddleLeft
        btnAction.InactiveColor = Color.FromArgb(CByte(34), CByte(90), CByte(74))
        btnAction.Location = New Point(707, 15)
        btnAction.Name = "btnAction"
        btnAction.PressedBorderColor = Color.Transparent
        btnAction.PressedColor = Color.FromArgb(CByte(25), CByte(70), CByte(57))
        btnAction.Size = New Size(179, 30)
        btnAction.TabIndex = 14
        btnAction.Text = "Review"
        btnAction.TextAlignment = StringAlignment.Center
        btnAction.UseWaitCursor = True
        ' 
        ' ucNotificationItem
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(btnAction)
        Controls.Add(lblMessage)
        Controls.Add(lblTimestamp)
        Name = "ucNotificationItem"
        Size = New Size(900, 61)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblTimestamp As Label
    Friend WithEvents lblMessage As Label
    Friend WithEvents btnAction As ReaLTaiizor.Controls.Button

End Class
