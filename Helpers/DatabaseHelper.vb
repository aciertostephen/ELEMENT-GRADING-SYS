Imports System.Data
Imports MySqlConnector

Public Class DatabaseHelper

    ' Connection string must be Shared so Shared methods can access it
    Private Shared ReadOnly ConnectionString As String = "Server=127.0.0.1;Database=grading_sys;Uid=root;Pwd=;CharSet=utf8mb4;"

    Public Shared Function GetConnection() As MySqlConnection
        Return New MySqlConnection(ConnectionString)
    End Function

    Public Shared Function ExecuteQuery(query As String, Optional params As MySqlParameter() = Nothing) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As MySqlConnection = GetConnection()
                Using cmd As New MySqlCommand(query, conn)
                    If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Database Query Error: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
        Return dt
    End Function

    Public Shared Function ExecuteNonQuery(query As String, Optional params As MySqlParameter() = Nothing) As Integer
        Dim rowsAffected As Integer = 0
        Try
            Using conn As MySqlConnection = GetConnection()
                conn.Open()
                Using cmd As New MySqlCommand(query, conn)
                    If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                    rowsAffected = cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Database Action Error: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
        Return rowsAffected
    End Function

    Public Shared Function ExecuteScalar(query As String, Optional params As MySqlParameter() = Nothing) As Object
        Dim result As Object = Nothing
        Try
            Using conn As MySqlConnection = GetConnection()
                conn.Open()
                Using cmd As New MySqlCommand(query, conn)
                    If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                    result = cmd.ExecuteScalar()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Database Scalar Error: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
        Return result
    End Function

End Class