Imports ePanic.CommonCore.Shared.Common

Public Class ClusterDatabase
#Region "Properties"

    Public ConnectionString As String
    Public Customer As Customer
    Public User As SystemUser
    Public Machine As Machine
    Public CustomerID As Integer
    Public CustomerKey As String
    Public UserID As Integer
    Public UserKey As String
    Public MachineID As Integer
    Public MachineKey As String

#End Region

    Sub New(ByVal userKey As String)

    End Sub
    Sub New(ByVal customerKey As String, ByVal userKey As String)
        ' load the customer info
        Dim cn As New SqlClient.SqlConnection(MasterConnectionString)

        Try
            Dim cmd As New SqlClient.SqlCommand("SELECT [ID], [CustomerKey], [ClusterDB] FROM [Customers] WHERE [CustomerKey] LIKE @CustomerKey;", cn)
            cmd.Parameters.AddWithValue("@CustomerKey", customerKey)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            Dim rs As SqlClient.SqlDataReader = cmd.ExecuteReader
            If rs.Read Then
                Me.ConnectionString = My.Settings.DbConnection
                Me.ConnectionString = Me.ConnectionString.Replace("[DataBaseName]", rs("ClusterDB").ToString)
                Me.ConnectionString = Me.ConnectionString.Replace("[SQLServer]", My.Settings.SQLServer)
                Me.ConnectionString = Me.ConnectionString.Replace("[SQLUser]", My.Settings.SQLUser)
                Me.ConnectionString = Me.ConnectionString.Replace("[SQLPass]", My.Settings.SQLPass)

                Me.CustomerID = rs("ID").ToString.ToInteger
                Me.CustomerKey = rs("CustomerKey").ToString
            End If
            cmd.Cancel()
            rs.Close()

        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        ' load the machine and user info
        cn = New SqlClient.SqlConnection(Me.ConnectionString)

        Try
            Dim cmd As New SqlClient.SqlCommand("select u.[ID] AS [UserID], u.[UserKey], m.[ID] AS [MachineID], m.[MachineKey] FROM [Users] u INNER JOIN [Machines] m ON u.MachineID = m.ID WHERE [UserKey] LIKE @UserKey;", cn)
            cmd.Parameters.AddWithValue("@UserKey", userKey)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            Dim rs As SqlClient.SqlDataReader = cmd.ExecuteReader
            If rs.Read Then
                Me.UserID = rs("UserID").ToString.ToInteger
                Me.UserKey = rs("UserKey").ToString
                Me.MachineID = rs("MachineID").ToString.ToInteger
                Me.MachineKey = rs("MachineKey").ToString
            End If
            cmd.Cancel()
            rs.Close()

        Catch ex As Exception
        Finally
            cn.Close()
        End Try
    End Sub

End Class

Public Structure AuditLogEntry
    Public ActionType As String
    Public Description As String
    Public UserKey As String
    Public Project As Enums.ProjectName
End Structure

Public Structure ErrorLogEntry
    Sub New(ByVal userKey As String, ByVal project As Enums.ProjectName)
        Me.userKey = Me.userKey
        Me.project = project
    End Sub

    Public userKey As String
    Public project As Enums.ProjectName
End Structure

Public Structure Message
    Public ID As Integer
    Public Recipients As List(Of Integer)
    Public MessageText As String
    Public ThreadId As Integer

    Sub New(ByVal id As Integer)

    End Sub
End Structure



#Region "System Objects"

Public Class SystemUser
#Region "Properties"

    Public UserKey As String
    Public MachineID As Integer
    Public CustomerID As Integer
    Public ID As Integer

#End Region

    Sub New()

    End Sub
    Sub New(ByVal userKey As String)

    End Sub
End Class


Public Class Machine
#Region "Properties"

    Public MachineKey As String
    Public ID As Integer

#End Region

    Sub New()

    End Sub
    Sub New(ByVal machineKey As String)

    End Sub
End Class


Public Class Customer
#Region "Properties"

    Public CustomerKey As String
    Public ClusterDB As String
    Public ID As Integer

#End Region

    Sub New()

    End Sub
    Sub New(ByVal customerKey As String)
        Dim cn As New SqlClient.SqlConnection(MasterConnectionString)

        Try
            Dim cmd As New SqlClient.SqlCommand("SELECT [ID], [CustomerKey], [ClusterDB] FROM [Customers] WHERE [CustomerKey] LIKE @CustomerKey", cn)
            cmd.Parameters.AddWithValue("@CustomerKey", customerKey)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            Dim rs As SqlClient.SqlDataReader = cmd.ExecuteReader
            If rs.Read Then
                Me.ID = rs("ID").ToString.ToInteger
                Me.CustomerKey = rs("CustomerKey").ToString
                Me.ClusterDB = rs("ClusterDB").ToString
            End If
            cmd.Cancel()
            rs.Close()

        Catch ex As Exception
        Finally
            cn.Close()
        End Try
    End Sub

    Public Sub LoadFromUserKey(ByVal UserKey As String)
        Dim cn As New SqlClient.SqlConnection(MasterConnectionString)

        Try
            Dim cmd As New SqlClient.SqlCommand("GetCustomerFromLogin", cn)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@UserKey", UserKey)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            Dim rs As SqlClient.SqlDataReader = cmd.ExecuteReader
            If rs.Read Then
                Me.ID = rs("ID").ToString.ToInteger
                Me.CustomerKey = rs("CustomerKey").ToString
                Me.ClusterDB = rs("ClusterDB").ToString
            End If
            cmd.Cancel()
            rs.Close()

        Catch ex As Exception
        Finally
            cn.Close()
        End Try
    End Sub
End Class

#End Region
