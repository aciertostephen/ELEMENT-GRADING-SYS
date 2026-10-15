Imports MySqlConnector
Imports ClosedXML.Excel

Public Class ucGradebook

    ' Custom Categories & Weights (Exam is permanently locked at 40%)
    Private customCat1Name As String = "Class Participation"
    Private customCat1Weight As Decimal = 10

    Private customCat2Name As String = "Quizzes"
    Private customCat2Weight As Decimal = 20

    Private customCat3Name As String = "Projects"
    Private customCat3Weight As Decimal = 30

    Private weightExam As Decimal = 40

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        Dim mainForm As MainForm = TryCast(Me.FindForm(), MainForm)
        If mainForm IsNot Nothing Then
            mainForm.ShowPage(New ucTeacherDashboard())
        End If
    End Sub

    Private Sub ucGradebook_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadDummyStudents()
        ConfigureGridEditableColumns()
    End Sub

    Private Sub LoadDummyStudents()
        dgvGradebook.Rows.Clear()
        For i As Integer = 1 To 14
            dgvGradebook.Rows.Add(i, "Last Name, First Name M.I.", "", "5%", "5.00", "", "", "", "50.00", "", "50.00", "10.00", "", "0.00", "", "50.00", "", "")
        Next
    End Sub

    ' ==========================================
    ' 1. EXPORT METHOD
    ' ==========================================
    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExportExcel.Click
        Using sfd As New SaveFileDialog()
            sfd.Filter = "CSV File|*.csv"
            sfd.Title = "Export Class Grades"
            sfd.FileName = "Class_Grades_" & DateTime.Now.ToString("yyyyMMdd") & ".csv"

            If sfd.ShowDialog() = DialogResult.OK Then
                Try
                    Dim sb As New System.Text.StringBuilder()

                    ' Export Column Headers
                    For Each col As DataGridViewColumn In dgvGradebook.Columns
                        sb.Append("""" & col.HeaderText.Replace("""", """""") & """,")
                    Next
                    sb.AppendLine()

                    ' Export Data Rows
                    For Each row As DataGridViewRow In dgvGradebook.Rows
                        If Not row.IsNewRow Then
                            For Each cell As DataGridViewCell In row.Cells
                                Dim val As String = If(cell.Value IsNot Nothing, cell.Value.ToString(), "")
                                sb.Append("""" & val.Replace("""", """""") & """,")
                            Next
                            sb.AppendLine()
                        End If
                    Next

                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8)
                    MessageBox.Show("Class grades have been exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

                Catch ex As Exception
                    MessageBox.Show("An error occurred while exporting: " & ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    ' ==========================================
    ' 2. GRADING INPUT & VALIDATION METHODS
    ' ==========================================
    Private Sub ConfigureGridEditableColumns()
        If dgvGradebook.Columns.Count > 1 Then
            dgvGradebook.Columns(0).ReadOnly = True
            dgvGradebook.Columns(1).ReadOnly = True
        End If
    End Sub

    ' Click-to-Toggle for DRP Column with Confirmation Prompt
    Private Sub dgvGradebook_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGradebook.CellClick
        If e.RowIndex >= 0 AndAlso dgvGradebook.Columns(e.ColumnIndex).HeaderText.Equals("DRP", StringComparison.OrdinalIgnoreCase) Then
            Dim currentVal As String = If(dgvGradebook.Rows(e.RowIndex).Cells(e.ColumnIndex).Value IsNot Nothing,
                                          dgvGradebook.Rows(e.RowIndex).Cells(e.ColumnIndex).Value.ToString(), "")

            If String.IsNullOrEmpty(currentVal) Then
                Dim result As DialogResult = MessageBox.Show(
                    "Are you sure you want to mark this student as Dropped?",
                    "Confirm Student Drop",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                )

                If result = DialogResult.Yes Then
                    dgvGradebook.Rows(e.RowIndex).Cells(e.ColumnIndex).Value = "Dropped"
                    dgvGradebook.InvalidateRow(e.RowIndex)
                End If
            Else
                dgvGradebook.Rows(e.RowIndex).Cells(e.ColumnIndex).Value = ""
                dgvGradebook.InvalidateRow(e.RowIndex)
            End If
        End If
    End Sub

    Private Sub dgvGradebook_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvGradebook.CellEndEdit
        If e.RowIndex < 0 Then Return

        Dim colHeader As String = dgvGradebook.Columns(e.ColumnIndex).HeaderText.Trim()
        If colHeader.Equals("NO.", StringComparison.OrdinalIgnoreCase) OrElse
           colHeader.Equals("STUDENT NAME", StringComparison.OrdinalIgnoreCase) OrElse
           colHeader.Equals("DRP", StringComparison.OrdinalIgnoreCase) Then
            Return
        End If

        Dim editedRow As DataGridViewRow = dgvGradebook.Rows(e.RowIndex)
        Dim editedCell As DataGridViewCell = editedRow.Cells(e.ColumnIndex)

        If editedCell.Value IsNot Nothing Then
            Dim inputString As String = editedCell.Value.ToString()
            If String.IsNullOrWhiteSpace(inputString) Then Return

            Dim numericValue As Decimal
            If Decimal.TryParse(inputString, numericValue) Then
                If numericValue < 0 OrElse numericValue > 100 Then
                    MessageBox.Show("Please enter a valid grade score between 0 and 100.", "Invalid Score", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    editedCell.Value = "0.00"
                Else
                    RecalculateCopernicanGrade(editedRow)
                End If
            Else
                MessageBox.Show("Please enter a numeric value.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                editedCell.Value = "0.00"
            End If
        End If
    End Sub

    Private Sub RecalculateCopernicanGrade(row As DataGridViewRow)
        Try
            ' Placeholder for weighted grade computations
        Catch ex As Exception
        End Try
    End Sub

    ' Highlight the DRP cell in red when marked as "Dropped"
    Private Sub dgvGradebook_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvGradebook.CellFormatting
        If e.RowIndex >= 0 AndAlso dgvGradebook.Columns(e.ColumnIndex).HeaderText.Equals("DRP", StringComparison.OrdinalIgnoreCase) Then
            If e.Value IsNot Nothing AndAlso e.Value.ToString().Equals("Dropped", StringComparison.OrdinalIgnoreCase) Then
                e.CellStyle.BackColor = Color.LightCoral
                e.CellStyle.ForeColor = Color.DarkRed
                e.CellStyle.SelectionBackColor = Color.IndianRed
                e.CellStyle.SelectionForeColor = Color.White
            Else
                e.CellStyle.BackColor = Color.White
                e.CellStyle.ForeColor = Color.Black
            End If
        End If
    End Sub

    ' ==========================================
    ' 3. CUSTOM CATEGORY & WEIGHT CONFIGURATION WORKFLOW
    ' ==========================================
    Private Sub btnConfigureWeights_Click(sender As Object, e As EventArgs) Handles btnConfigureWeights.Click
        Try
            ' 1. Custom Category 1 (Name & Weight)
            Dim name1 As String = Microsoft.VisualBasic.InputBox(
                "Enter the custom name for Category 1:",
                "Configure Custom Categories",
                customCat1Name)
            If String.IsNullOrWhiteSpace(name1) Then Return

            Dim weight1Input As String = Microsoft.VisualBasic.InputBox(
                "Enter the weight percentage for " & name1.Trim() & ":",
                "Configure Category Weight",
                customCat1Weight.ToString())
            If String.IsNullOrEmpty(weight1Input) Then Return

            Dim w1 As Decimal
            If Not Decimal.TryParse(weight1Input.Trim(), w1) OrElse w1 < 0 Then
                MessageBox.Show("Please enter a valid non-negative number for " & name1 & ".", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 2. Custom Category 2 (Name & Weight)
            Dim name2 As String = Microsoft.VisualBasic.InputBox(
                "Enter the custom name for Category 2:",
                "Configure Custom Categories",
                customCat2Name)
            If String.IsNullOrWhiteSpace(name2) Then Return

            Dim weight2Input As String = Microsoft.VisualBasic.InputBox(
                "Enter the weight percentage for " & name2.Trim() & ":",
                "Configure Category Weight",
                customCat2Weight.ToString())
            If String.IsNullOrEmpty(weight2Input) Then Return

            Dim w2 As Decimal
            If Not Decimal.TryParse(weight2Input.Trim(), w2) OrElse w2 < 0 Then
                MessageBox.Show("Please enter a valid non-negative number for " & name2 & ".", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 3. Custom Category 3 (Name & Weight)
            Dim name3 As String = Microsoft.VisualBasic.InputBox(
                "Enter the custom name for Category 3:",
                "Configure Custom Categories",
                customCat3Name)
            If String.IsNullOrWhiteSpace(name3) Then Return

            Dim weight3Input As String = Microsoft.VisualBasic.InputBox(
                "Enter the weight percentage for " & name3.Trim() & ":",
                "Configure Category Weight",
                customCat3Weight.ToString())
            If String.IsNullOrEmpty(weight3Input) Then Return

            Dim w3 As Decimal
            If Not Decimal.TryParse(weight3Input.Trim(), w3) OrElse w3 < 0 Then
                MessageBox.Show("Please enter a valid non-negative number for " & name3 & ".", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 4. Major Exam is permanently locked at 40%
            Dim examVal As Decimal = 40D

            ' 5. Strict 100% Total Validation Check
            Dim totalSum As Decimal = w1 + w2 + w3 + examVal

            If totalSum <> 100D Then
                MessageBox.Show($"The total distribution must equal exactly 100%. Your current total is {totalSum}%." & vbCrLf & vbCrLf &
                                 "Note: Major Exam is fixed at 40%, meaning your custom categories must combine to equal 60%.",
                                 "Weight Distribution Mismatch",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Warning)
                Return
            End If

            ' 6. Commit Custom State
            customCat1Name = name1.Trim()
            customCat1Weight = w1
            customCat2Name = name2.Trim()
            customCat2Weight = w2
            customCat3Name = name3.Trim()
            customCat3Weight = w3
            weightExam = examVal

            SaveActiveWeightsToDatabase(customCat1Name, customCat1Weight, customCat2Name, customCat2Weight, customCat3Name, customCat3Weight, weightExam)

            ' 7. Reflect the set weights and custom categories directly in the success message box
            MessageBox.Show("Gradebook Configuration Saved Successfully!" & vbCrLf & vbCrLf &
                             $"• {customCat1Name}: {customCat1Weight}%" & vbCrLf &
                             $"• {customCat2Name}: {customCat2Weight}%" & vbCrLf &
                             $"• {customCat3Name}: {customCat3Weight}%" & vbCrLf &
                             $"• Major Exam: {weightExam}% (Locked)" & vbCrLf &
                             $"Total Gradebook Weight: {totalSum}%",
                             "Custom Gradebook Configured",
                             MessageBoxButtons.OK,
                             MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show("An unexpected error occurred while configuring weights: " & ex.Message, "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SaveActiveWeightsToDatabase(cat1Name As String, cat1Val As Decimal, cat2Name As String, cat2Val As Decimal, cat3Name As String, cat3Val As Decimal, examVal As Decimal)
        ' Database persistence logic via MySqlConnector goes here if needed
    End Sub

    ' ==========================================
    ' 4. CATEGORY-AWARE DYNAMIC COLUMN ADDITION
    ' ==========================================
    Private Sub btnAddColumn_Click(sender As Object, e As EventArgs) Handles btnAddColumn.Click
        Dim categoryChoice As String = Microsoft.VisualBasic.InputBox(
            "Select the category for this column:" & vbCrLf &
            "1. " & customCat1Name & vbCrLf &
            "2. " & customCat2Name & vbCrLf &
            "3. " & customCat3Name & vbCrLf &
            "4. Final Exam",
            "Select Category",
            customCat2Name)

        If String.IsNullOrWhiteSpace(categoryChoice) Then Return

        Dim targetCategoryHeader As String = ""
        Select Case categoryChoice.Trim().ToLower()
            Case "1", customCat1Name.ToLower(), "1."
                targetCategoryHeader = customCat1Name.ToUpper()
            Case "2", customCat2Name.ToLower(), "2."
                targetCategoryHeader = customCat2Name.ToUpper()
            Case "3", customCat3Name.ToLower(), "3."
                targetCategoryHeader = customCat3Name.ToUpper()
            Case "4", "final exam", "exam", "4."
                targetCategoryHeader = "EXAM"
            Case Else
                targetCategoryHeader = categoryChoice.ToUpper()
        End Select

        Dim columnName As String = Microsoft.VisualBasic.InputBox(
            "Enter the name of the new assessment column (e.g., Quiz 3, Activity 4):",
            "Add Assessment Column",
            "New Assessment")

        If String.IsNullOrWhiteSpace(columnName) Then Return

        Dim targetIndex As Integer = -1
        For i As Integer = 0 To dgvGradebook.Columns.Count - 1
            If dgvGradebook.Columns(i).HeaderText.Equals(targetCategoryHeader, StringComparison.OrdinalIgnoreCase) Then
                targetIndex = i + 1
                While targetIndex < dgvGradebook.Columns.Count
                    Dim nextHeader As String = dgvGradebook.Columns(targetIndex).HeaderText.ToUpper()
                    If nextHeader.Contains("GRA") OrElse nextHeader.Equals(customCat1Name.ToUpper()) OrElse nextHeader.Equals(customCat2Name.ToUpper()) OrElse nextHeader.Equals(customCat3Name.ToUpper()) OrElse nextHeader.Equals("EXAM") Then
                        targetIndex += 1
                    Else
                        Exit While
                    End If
                End While
                Exit For
            End If
        Next

        If targetIndex <= 0 Then
            targetIndex = dgvGradebook.Columns.Count - 1
            If targetIndex < 0 Then targetIndex = 0
        End If

        Dim newCol As New DataGridViewTextBoxColumn()
        newCol.Name = "col_" & columnName.Replace(" ", "_")
        newCol.HeaderText = columnName.ToUpper()
        newCol.Width = 70
        newCol.ReadOnly = False

        dgvGradebook.Columns.Insert(targetIndex, newCol)

        For Each row As DataGridViewRow In dgvGradebook.Rows
            If Not row.IsNewRow Then
                row.Cells(targetIndex).Value = ""
            End If
        Next

        MessageBox.Show("Column '" & columnName.ToUpper() & "' added successfully under " & targetCategoryHeader & "!", "Column Added", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ' ==========================================
    ' 5. DYNAMIC COLUMN DELETION
    ' ==========================================
    Private clickedColumnIndex As Integer = -1

    Private Sub dgvGradebook_ColumnHeaderMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvGradebook.ColumnHeaderMouseClick
        If e.Button = MouseButtons.Right AndAlso e.RowIndex = -1 Then
            clickedColumnIndex = e.ColumnIndex

            Dim colHeader As String = dgvGradebook.Columns(e.ColumnIndex).HeaderText.Trim()
            If colHeader.Equals("MAJOR EXAM", StringComparison.OrdinalIgnoreCase) OrElse
               colHeader.Equals("GRADE", StringComparison.OrdinalIgnoreCase) OrElse
               colHeader.Equals("NO.", StringComparison.OrdinalIgnoreCase) OrElse
               colHeader.Equals("STUDENT NAME", StringComparison.OrdinalIgnoreCase) Then
                Return
            End If

            Dim menu As New ContextMenuStrip()
            Dim deleteItem As ToolStripMenuItem = menu.Items.Add("Delete Column '" & colHeader & "'")

            AddHandler deleteItem.Click, AddressOf DeleteSelectedColumn_Click
            menu.Show(Cursor.Position)
        End If
    End Sub

    Private Sub DeleteSelectedColumn_Click(sender As Object, e As EventArgs)
        If clickedColumnIndex >= 0 AndAlso clickedColumnIndex < dgvGradebook.Columns.Count Then
            Dim colName As String = dgvGradebook.Columns(clickedColumnIndex).HeaderText

            Dim result As DialogResult = MessageBox.Show(
                "Are you sure you want to delete the column '" & colName & "'? All entered grades in this column will be lost.",
                "Confirm Column Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            )

            If result = DialogResult.Yes Then
                dgvGradebook.Columns.RemoveAt(clickedColumnIndex)
                MessageBox.Show("Column deleted successfully.", "Column Removed", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End If
    End Sub

End Class