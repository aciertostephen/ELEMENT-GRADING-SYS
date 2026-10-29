Imports System.Data
Imports MySqlConnector

''' <summary>
''' Single door to MySQL. Errors are THROWN (never swallowed), so callers' Try/Catch
''' blocks finally work and "success" messages can no longer show after a failed insert.
''' The application startup in MainForm catches uncaught UI-thread errors and
''' shows one friendly dialog instead of crashing.
''' </summary>
Public Class DatabaseHelper

  Private Shared ReadOnly _connectionString As New Lazy(Of String)(AddressOf ResolveConnectionString)

  ''' <summary>
  ''' Production builds must be configured outside the source tree. Debug builds retain
  ''' the original local XAMPP default for developer machines.
  ''' </summary>
  Public Shared ReadOnly Property ConnectionString As String
    Get
      Return _connectionString.Value
    End Get
  End Property

  Private Shared Function ResolveConnectionString() As String
    Dim configured As String = Environment.GetEnvironmentVariable("ELEMENT_GRADING_DB_CONNECTION_STRING")
    If Not String.IsNullOrWhiteSpace(configured) Then Return configured.Trim()

#If DEBUG Then
    Return "Server=127.0.0.1;Database=gradebook_db;Uid=root;Pwd=;CharSet=utf8mb4;"
#Else
    Throw New InvalidOperationException(
      "Database configuration is missing. Set the ELEMENT_GRADING_DB_CONNECTION_STRING environment variable before starting a Release build.")
#End If
  End Function

  Public Shared Function GetConnection() As MySqlConnection
    Return New MySqlConnection(ConnectionString)
  End Function

  Public Shared Function ExecuteQuery(query As String, Optional params As MySqlParameter() = Nothing) As DataTable
    Dim dt As New DataTable()
    Using conn As MySqlConnection = GetConnection()
      Using cmd As New MySqlCommand(query, conn)
        If params IsNot Nothing Then cmd.Parameters.AddRange(params)
        Using adapter As New MySqlDataAdapter(cmd)
          adapter.Fill(dt)
        End Using
      End Using
    End Using
    Return dt
  End Function

  Public Shared Function ExecuteNonQuery(query As String, Optional params As MySqlParameter() = Nothing) As Integer
    Using conn As MySqlConnection = GetConnection()
      conn.Open()
      Using cmd As New MySqlCommand(query, conn)
        If params IsNot Nothing Then cmd.Parameters.AddRange(params)
        Return cmd.ExecuteNonQuery()
      End Using
    End Using
  End Function

  Public Shared Function ExecuteScalar(query As String, Optional params As MySqlParameter() = Nothing) As Object
    Using conn As MySqlConnection = GetConnection()
      conn.Open()
      Using cmd As New MySqlCommand(query, conn)
        If params IsNot Nothing Then cmd.Parameters.AddRange(params)
        Dim r As Object = cmd.ExecuteScalar()
        If r Is DBNull.Value Then Return Nothing
        Return r
      End Using
    End Using
  End Function

  ''' <summary>Scalar converted to Integer (0 when NULL).</summary>
  Public Shared Function ScalarInt(query As String, Optional params As MySqlParameter() = Nothing) As Integer
    Dim r As Object = ExecuteScalar(query, params)
    If r Is Nothing Then Return 0
    Return Convert.ToInt32(r)
  End Function

  ''' <summary>
  ''' Runs several statements atomically. Replaces the ExecTx copies.
  ''' Inside the work delegate call tx.Exec(sql, params) as many times as needed.
  ''' Commits when work finishes, rolls back and rethrows on any error.
  ''' </summary>
  Public Shared Sub RunInTransaction(work As Action(Of DbTx))
    Using conn As MySqlConnection = GetConnection()
      conn.Open()
      Using trans As MySqlTransaction = conn.BeginTransaction()
        Try
          work(New DbTx(conn, trans))
          trans.Commit()
        Catch
          Try
            trans.Rollback()
          Catch
          End Try
          Throw
        End Try
      End Using
    End Using
  End Sub

  Public Shared Function P(name As String, value As Object) As MySqlParameter
    Return New MySqlParameter(name, If(value, DBNull.Value))
  End Function

End Class

''' <summary>Transaction handle given to RunInTransaction.</summary>
Public Class DbTx
  Private ReadOnly _conn As MySqlConnection
  Private ReadOnly _tx As MySqlTransaction

  Friend Sub New(conn As MySqlConnection, tx As MySqlTransaction)
    _conn = conn
    _tx = tx
  End Sub

  Public Function Exec(sql As String, ParamArray params As MySqlParameter()) As Integer
    Using cmd As New MySqlCommand(sql, _conn, _tx)
      If params IsNot Nothing Then cmd.Parameters.AddRange(params)
      Return cmd.ExecuteNonQuery()
    End Using
  End Function

  Public Function Scalar(sql As String, ParamArray params As MySqlParameter()) As Object
    Using cmd As New MySqlCommand(sql, _conn, _tx)
      If params IsNot Nothing Then cmd.Parameters.AddRange(params)
      Dim r As Object = cmd.ExecuteScalar()
      If r Is DBNull.Value Then Return Nothing
      Return r
    End Using
  End Function
End Class
