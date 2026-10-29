Imports System.Diagnostics
Imports System.IO
Imports System.Threading.Tasks
Imports MySqlConnector

''' <summary>Creates one consistent MySQL logical backup per day and retains two weeks.</summary>
Public NotInheritable Class DatabaseBackupService
    Private Sub New()
    End Sub

    Public Shared Function CreateDailyBackup() As String
        Dim settings As New MySqlConnectionStringBuilder(DatabaseHelper.ConnectionString)
        Dim backupFolder As String = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ELEMENT Grading System", "Backups")
        Directory.CreateDirectory(backupFolder)

        Dim targetPath As String = Path.Combine(backupFolder, "gradebook_db_" & Date.Now.ToString("yyyy-MM-dd") & ".sql")
        If File.Exists(targetPath) AndAlso New FileInfo(targetPath).Length > 0 Then Return Nothing

        Dim dumpPath As String = FindMySqlDump()
        If dumpPath Is Nothing Then
            Return "Automatic backup was not created because mysqldump.exe was not found. Install MySQL command-line tools or add mysqldump.exe to PATH."
        End If

        Dim tempDefaults As String = Nothing
        Dim partialPath As String = targetPath & ".partial"
        Try
            Dim start As New ProcessStartInfo With {
                .FileName = dumpPath, .UseShellExecute = False, .CreateNoWindow = True,
                .RedirectStandardOutput = True, .RedirectStandardError = True
            }
            If Not String.IsNullOrWhiteSpace(settings.Password) Then
                tempDefaults = Path.Combine(backupFolder, "mysql-backup-" & Guid.NewGuid().ToString("N") & ".cnf")
                File.WriteAllText(tempDefaults, "[client]" & Environment.NewLine &
                    "password=""" & settings.Password.Replace("\", "\\").Replace("""", "\""") & """" & Environment.NewLine)
                start.ArgumentList.Add("--defaults-extra-file=" & tempDefaults)
            End If
            start.ArgumentList.Add("--host=" & settings.Server)
            start.ArgumentList.Add("--port=" & settings.Port.ToString())
            start.ArgumentList.Add("--user=" & settings.UserID)
            start.ArgumentList.Add("--single-transaction")
            start.ArgumentList.Add("--routines")
            start.ArgumentList.Add("--triggers")
            start.ArgumentList.Add("--events")
            start.ArgumentList.Add("--databases")
            start.ArgumentList.Add(settings.Database)

            Using fs As New FileStream(partialPath, FileMode.Create, FileAccess.Write, FileShare.None)
                Using proc As New Process With {.StartInfo = start}
                    If Not proc.Start() Then Throw New InvalidOperationException("Could not start mysqldump.exe.")
                    Dim stderrTask As Task(Of String) = proc.StandardError.ReadToEndAsync()
                    Dim outputTask As Task = proc.StandardOutput.BaseStream.CopyToAsync(fs)
                    proc.WaitForExit(300000)
                    If Not proc.HasExited Then
                        proc.Kill(True)
                        Throw New TimeoutException("The database backup timed out after five minutes.")
                    End If
                    outputTask.GetAwaiter().GetResult()
                    Dim errorText As String = stderrTask.GetAwaiter().GetResult()
                    If proc.ExitCode <> 0 Then Throw New InvalidOperationException("mysqldump failed: " & errorText.Trim())
                End Using
            End Using

            If New FileInfo(partialPath).Length = 0 Then Throw New InvalidOperationException("mysqldump produced an empty backup.")
            If File.Exists(targetPath) Then File.Delete(targetPath)
            File.Move(partialPath, targetPath)
            PruneOldBackups(backupFolder)
            Return Nothing
        Catch ex As Exception
            If File.Exists(partialPath) Then File.Delete(partialPath)
            Return "Automatic database backup failed: " & ex.Message
        Finally
            If tempDefaults IsNot Nothing AndAlso File.Exists(tempDefaults) Then File.Delete(tempDefaults)
        End Try
    End Function

    Private Shared Function FindMySqlDump() As String
        Dim explicitPath As String = Environment.GetEnvironmentVariable("MYSQLDUMP_PATH")
        If Not String.IsNullOrWhiteSpace(explicitPath) AndAlso File.Exists(explicitPath) Then Return explicitPath
        For Each folder As String In (If(Environment.GetEnvironmentVariable("PATH"), "")).Split(Path.PathSeparator)
            Dim candidate As String = Path.Combine(folder.Trim(""""c), "mysqldump.exe")
            If File.Exists(candidate) Then Return candidate
        Next
        Dim candidates As New List(Of String) From {
            "C:\xampp\mysql\bin\mysqldump.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MySQL", "MySQL Server 8.0", "bin", "mysqldump.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "MySQL", "MySQL Server 8.4", "bin", "mysqldump.exe")
        }
        For Each baseDir As String In {"C:\wamp64\bin\mysql", "C:\Program Files\MariaDB"}
            If Directory.Exists(baseDir) Then
                For Each dir As String In Directory.GetDirectories(baseDir)
                    candidates.Add(Path.Combine(dir, "bin", "mysqldump.exe"))
                    candidates.Add(Path.Combine(dir, "mysqldump.exe"))
                Next
            End If
        Next
        Return candidates.FirstOrDefault(Function(p) File.Exists(p))
    End Function

    Private Shared Sub PruneOldBackups(folder As String)
        For Each path As String In Directory.GetFiles(folder, "gradebook_db_*.sql")
            Dim info As New FileInfo(path)
            If info.LastWriteTime < Date.Now.AddDays(-14) Then info.Delete()
        Next
    End Sub
End Class
