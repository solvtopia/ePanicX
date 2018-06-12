Imports System.IO
Imports System.Xml
Imports ePanic.CommonCore.Shared.Common

Public Class Settings
    Public Structure Setting
        Sub New(ByVal t As Enums.ePanicSettingType, ByVal s As Enums.ePanicSetting, ByVal v As String)
            Me.setting = s
            Me.type = t
            Me.value = v
        End Sub

        Public setting As Enums.ePanicSetting
        Public type As Enums.ePanicSettingType
        Public value As String
    End Structure

#Region "Properties"

    Public cluster As ClusterDatabase
    Public AllSettings As List(Of Setting)
    Public ReadOnly Property LocalFile As String
        Get
            Return My.Computer.FileSystem.CombinePath(My.Application.Info.DirectoryPath, Me.cluster.UserKey & ".xml")
        End Get
    End Property

#End Region

    Sub New(ByVal cluster As ClusterDatabase)
        Me.cluster = cluster
        Me.AllSettings = New List(Of Setting)
    End Sub

    Private Sub Refresh()
        ' loop through all the settings in the system to see if we have a value
        ' if we don't set a default
        Dim cnGlobal As New SqlClient.SqlConnection(MasterConnectionString)
        Dim cn As New SqlClient.SqlConnection(Me.cluster.ConnectionString)

        Try
            Me.AllSettings = New List(Of Setting)

            Dim cmd As SqlClient.SqlCommand
            Dim rs As SqlClient.SqlDataReader

            ' settings roll up so we start at the user, then machine, then customer, then global for each
            Dim enumValues As Array = System.[Enum].GetValues(GetType(Enums.ePanicSetting))
            For Each resource As Enums.ePanicSetting In enumValues
                Dim t As Enums.ePanicSettingType = Enums.ePanicSettingType.Default
                Dim v As String = ""

                Try
                    ' user specific
                    cmd = New SqlClient.SqlCommand("SELECT [Setting], [Value] FROM [Settings] WHERE ISNULL([UserID],0) = @UserID AND ISNULL([MachineID],0) = 0 AND @Today BETWEEN [EffectiveFrom] AND [EffectiveTo] AND [Setting] = @Setting;", cn)
                    cmd.Parameters.AddWithValue("@UserID", Me.cluster.UserID)
                    cmd.Parameters.AddWithValue("@Today", Now.ToString)
                    cmd.Parameters.AddWithValue("@Setting", CStr(resource))
                    If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
                    rs = cmd.ExecuteReader
                    If rs.Read Then
                        v = rs("Value").ToString : t = Enums.ePanicSettingType.User
                    Else
                        ' machine specific
                        cmd = New SqlClient.SqlCommand("SELECT [Setting], [Value] FROM [Settings] WHERE ISNULL([UserID],0) = 0 AND ISNULL([MachineID],0) = @MachineID AND @Today BETWEEN [EffectiveFrom] AND [EffectiveTo] AND [Setting] = @Setting;", cn)
                        cmd.Parameters.AddWithValue("@MachineID", Me.cluster.MachineID)
                        cmd.Parameters.AddWithValue("@Today", Now.ToString)
                        cmd.Parameters.AddWithValue("@Setting", CStr(resource))
                        If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
                        rs = cmd.ExecuteReader
                        If rs.Read Then
                            v = rs("Value").ToString : t = Enums.ePanicSettingType.Machine
                        Else
                            ' customer specific
                            cmd = New SqlClient.SqlCommand("SELECT [Setting], [Value] FROM [Settings] WHERE ISNULL([CustomerID],0) = @CustomerID AND @Today BETWEEN [EffectiveFrom] AND [EffectiveTo] AND [Setting] = @Setting;", cnGlobal)
                            cmd.Parameters.AddWithValue("@CustomerID", Me.cluster.CustomerID)
                            cmd.Parameters.AddWithValue("@Today", Now.ToString)
                            cmd.Parameters.AddWithValue("@Setting", CStr(resource))
                            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
                            rs = cmd.ExecuteReader
                            If rs.Read Then
                                v = rs("Value").ToString : t = Enums.ePanicSettingType.Cluster
                            Else
                                ' global
                                cmd = New SqlClient.SqlCommand("SELECT [Setting], [Value] FROM [Settings] WHERE ISNULL([CustomerID],0) = 0 AND @Today BETWEEN [EffectiveFrom] AND [EffectiveTo] AND [Setting] = @Setting;", cnGlobal)
                                cmd.Parameters.AddWithValue("@CustomerID", Me.cluster.CustomerID)
                                cmd.Parameters.AddWithValue("@Today", Now.ToString)
                                cmd.Parameters.AddWithValue("@Setting", CStr(resource))
                                If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
                                rs = cmd.ExecuteReader
                                If rs.Read Then
                                    v = rs("Value").ToString : t = Enums.ePanicSettingType.Global
                                Else
                                    t = Enums.ePanicSettingType.Default
                                    v = DefaultSetting(resource)
                                End If
                                cmd.Cancel()
                                rs.Close()
                            End If
                            cmd.Cancel()
                            rs.Close()
                        End If
                        cmd.Cancel()
                        rs.Close()
                    End If
                    cmd.Cancel()
                    rs.Close()
                Catch ex As Exception
                    t = Enums.ePanicSettingType.Default
                    v = DefaultSetting(resource)
                End Try

                ' add the setting to the list
                Me.AllSettings.Add(New Setting(t, resource, v))
            Next

        Catch ex As Exception
        Finally
            cn.Close()
        End Try
    End Sub

    Private Function DefaultSetting(ByVal s As Enums.ePanicSetting) As String
        Dim retVal As String = ""

        ' return a default setting if needed
        Select Case s
            Case Enums.ePanicSetting.UpdateDay
                retVal = "everyday"
            Case Enums.ePanicSetting.UpdateTime
                retVal = "12:00:00 AM"
            Case Enums.ePanicSetting.RemoteUpdateFolder
                retVal = "/AutoUpdate"
        End Select

        Return retVal
    End Function

    Public Sub UpdateLocal()
        ' refresh the settings from the server
        Me.Refresh()

        If My.Computer.FileSystem.FileExists(Me.LocalFile) Then My.Computer.FileSystem.DeleteFile(Me.LocalFile)

        ' save the settings collection to a local xml document for the client to use
        Dim xDoc As New XmlDocument
        xDoc.LoadXml(Me.AllSettings.SerializeToXml)
        xDoc.Save(Me.LocalFile)
    End Sub

    Public Sub ClientLoad()
        If My.Computer.FileSystem.FileExists(Me.LocalFile) Then
            ' load the xml document into the settings object
            Dim fileText As String = File.ReadAllText(Me.LocalFile)
            Me.AllSettings = CType(Me.AllSettings.DeserializeFromXml(fileText), List(Of Setting))

            ' make sure we have an entry for all the settings
            Dim enumValues As Array = System.[Enum].GetValues(GetType(Enums.ePanicSetting))
            For Each resource As Enums.ePanicSetting In enumValues
                If Not Me.AllSettings.HasSetting(resource) Then
                    Me.AllSettings.Add(New Setting(Enums.ePanicSettingType.Default, resource, DefaultSetting(resource)))
                End If
            Next
        Else
            ' no local file so load all the defaults
            Dim enumValues As Array = System.[Enum].GetValues(GetType(Enums.ePanicSetting))
            For Each resource As Enums.ePanicSetting In enumValues
                Me.AllSettings.Add(New Setting(Enums.ePanicSettingType.Default, resource, DefaultSetting(resource)))
            Next
        End If
    End Sub

    Public Function GetSetting(ByVal s As Enums.ePanicSetting) As Setting
        Dim retVal As New Setting

        For Each i As Setting In Me.AllSettings
            If i.setting = s Then
                retVal = i
                Exit For
            End If
        Next

        Return retVal
    End Function
End Class
