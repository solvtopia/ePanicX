Imports ePanic.CommonCore.Shared.Common
Imports System.Runtime.CompilerServices

Public Module Logs

    ' writes a new entry to the audit log
    <Extension()> Public Sub WriteToAuditLog(ByVal entry As AuditLogEntry)
        Dim cluster As New ClusterDatabase(entry.UserKey)
        Dim cn As New SqlClient.SqlConnection(cluster.ConnectionString)

        Try
            Dim cmd As New SqlClient.SqlCommand("INSERT INTO [Sys_AuditLog] (ClientID, xmlData, dtInserted, dtUpdated, insertedBy, updatedBy) VALUES (@ClientID, @xmlData, '" & Now.ToString & "', '" & Now.ToString & "', '" & cluster.UserID & "', '" & cluster.UserID & "');", cn)
            cmd.Parameters.Clear()
            cmd.Parameters.AddWithValue("@ClientID", cluster.CustomerID)
            cmd.Parameters.AddWithValue("@xmlData", entry.SerializeToXml)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
            cmd.Cancel()

        Catch ex As Exception
            ex.WriteToErrorLog(New ErrorLogEntry(entry.UserKey, entry.Project))
        Finally
            cn.Close()
        End Try
    End Sub

    ' writes a new entry to the error log
    <Extension()> Public Sub WriteToErrorLog(ByVal exp As Exception, ByVal entry As ErrorLogEntry)
        Dim cluster As New ClusterDatabase(entry.userKey)
        Dim cn As New SqlClient.SqlConnection(cluster.ConnectionString)

        Try
            If exp IsNot Nothing Then
                Dim ex As String = exp.SerializeToXml
                If ex = "" Then ex = ObjectToXml(exp)
                If ex = "" Then ex = "<Exception>" & exp.ToString & "</Exception>"

                Dim cmd As New SqlClient.SqlCommand("INSERT INTO [Sys_ErrorLog] (UserID, ClientID, xmlData, Project, dtInserted, dtUpdated, insertedBy, updatedBy) VALUES (@UserID, @ClientID, @xmlData, @Project, '" & Now.ToString & "', '" & Now.ToString & "', '0', '0');", cn)
                cmd.Parameters.Clear()
                cmd.Parameters.AddWithValue("@UserID", cluster.UserID)
                cmd.Parameters.AddWithValue("@ClientID", cluster.CustomerID)
                cmd.Parameters.AddWithValue("@xmlData", ex)
                cmd.Parameters.AddWithValue("@Project", entry.project.ToString)
                If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
                cmd.CommandTimeout = 0
                cmd.ExecuteNonQuery()
                cmd.Cancel()
            End If

        Catch ex As Exception
            ex.WriteToEventLog
            exp.WriteToEventLog
        Finally
            cn.Close()
        End Try
    End Sub

    ' writes a new entry to the servers event log
    <Extension()> Public Function WriteToEventLog(ByVal exp As Exception) As Boolean
        Return exp.WriteToEventLog
    End Function
    <Extension()> Public Function WriteToEventLog(ByVal exp As Exception, ByVal userID As Integer) As Boolean
        Try
            ''stop

            'Dim logName As String = System.Configuration.ConfigurationManager.AppSettings("EventLog")

            '' Create the source, if it does not already exist.
            'If Not EventLog.SourceExists("NK5" & logName) Then
            '    EventLog.CreateEventSource("NK5" & logName, logName)
            'End If

            '' Create an EventLog instance and assign its source.
            'Dim myLog As New EventLog()
            'myLog.Source = "NK5" & logName

            '' Write an informational entry to the event log.
            'Dim usr As SystemUser = GetUser(userID)
            'Dim strMsg As String = "Error occurred at " & Date.Now.ToString & ControlChars.NewLine

            'If (usr.ID > 0) Then
            '    strMsg &= "Current User: " & usr.Email & ControlChars.NewLine
            'End If
            'strMsg &= "Exception: " & exp.ToString

            'myLog.WriteEntry(strMsg, EventLogEntryType.Error)

            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    ' writes a new entry to the update log
    Public Sub WriteUpdateLog(ByVal cluster As ClusterDatabase, ByVal log As List(Of String))
        Dim cn As New SqlClient.SqlConnection(cluster.ConnectionString)

        Try
            Dim cmd As New SqlClient.SqlCommand("INSERT INTO [UpdateLog] ([MachineID], [Status], [Log], [dtInserted], [InsertedBy], [dtUpdated], [UpdatedBy]) VALUES (@MachineID, @Status, @Log, '" & Now.ToString & "', '" & cluster.UserID & "', '" & Now.ToString & "', '" & cluster.UserID & "')")
            cmd.Parameters.AddWithValue("@MachineID", cluster.MachineID)
            cmd.Parameters.AddWithValue("@Status", "1")
            cmd.Parameters.AddWithValue("@Log", log.SerializeToXml)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            cmd.ExecuteNonQuery()

        Catch ex As Exception
            ex.WriteToErrorLog(New ErrorLogEntry(cluster.UserKey, Enums.ProjectName.AutoUpdate))
        Finally
            cn.Close()
        End Try
    End Sub

End Module
