Imports System.Data
Imports MySql.Data.MySqlClient
Imports MySqlConnector

Public Module DatabaseHelper

    Private ReadOnly ConnectionString As String = "Server=127.0.0.1;Database=grading_sys;Uid=root;Pwd=;Charset=utf8mb4;"

    Public Function GetConnection() As MySqlConnection
        Return New MySqlConnection(ConnectionString)
    End Function

    Public Function ExecuteQuery(sql As String, Optional params As MySqlParameter() = Nothing) As DataTable
        Dim dt As New DataTable()
        Try
            Using conn As MySqlConnection = GetConnection()
                Using cmd As New MySqlCommand(sql, conn)
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

    Public Function ExecuteNonQuery(sql As String, Optional params As MySqlParameter() = Nothing) As Integer
        Dim rowsAffected As Integer = 0
        Try
            Using conn As MySqlConnection = GetConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                    rowsAffected = cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Database Action Error: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
        Return rowsAffected
    End Function

    Public Function ExecuteScalar(sql As String, Optional params As MySqlParameter() = Nothing) As Object
        Dim result As Object = Nothing
        Try
            Using conn As MySqlConnection = GetConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                    result = cmd.ExecuteScalar()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Database Scalar Error: " & ex.Message, "Database Error", MessageBoxButtons.OK)
        End Try
        Return result
    End Function

End Module