Public Class Common

    Public Shared Function MasterConnectionString() As String
        Dim retVal As String = ""

        retVal = My.Settings.DbConnection
        retVal = retVal.Replace("[DataBaseName]", "ePanic")
        retVal = retVal.Replace("[SQLServer]", My.Settings.SQLServer)
        retVal = retVal.Replace("[SQLUser]", My.Settings.SQLUser)
        retVal = retVal.Replace("[SQLPass]", My.Settings.SQLPass)

        Return retVal
    End Function

    Public Shared Function NewGUID() As String
        Dim retVal As String = System.Guid.NewGuid.ToString()
        Return retVal
    End Function

    Public Shared Function InitialClientSetup(ByVal cluster As ClusterDatabase) As Boolean
        Return InitialClientSetup(cluster, "", "")
    End Function
    Public Shared Function InitialClientSetup(ByVal cluster As ClusterDatabase, ByVal machineKey As String, ByVal userKey As String) As Boolean
        Dim retVal As Boolean = True

        Dim cn As New SqlClient.SqlConnection(cluster.ConnectionString)

        Try
            Dim cmd As New SqlClient.SqlCommand("InitialClientSetup")
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@MachineKey", If(machineKey = "", NewGUID(), machineKey))
            cmd.Parameters.AddWithValue("@UserKey", If(userKey = "", NewGUID(), userKey))
            cmd.Parameters.AddWithValue("@CustomerID", cluster.CustomerID)
            cmd.Parameters.AddWithValue("@AuditUserID", 0)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            Dim rs As SqlClient.SqlDataReader = cmd.ExecuteReader
            If rs.Read Then
                ' MachineID, MachineKey, UserID, UserKey are returned
            End If
            cmd.Cancel()
            rs.Close()

        Catch ex As Exception
            retVal = False
        End Try

        Return retVal
    End Function
End Class


