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

#End Region

    Sub New(ByVal cluster As ClusterDatabase)
        Me.cluster = cluster
        Me.AllSettings = New List(Of Setting)

        Me.Refresh()
    End Sub

    Public Sub Refresh()
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
                                ' some settings need a default value, others are fine with a blank string
                                Select Case resource
                                    Case Enums.ePanicSetting.UpdateDay
                                        v = "everyday"
                                    Case Enums.ePanicSetting.UpdateTime
                                        v = "12:00:00 AM"
                                End Select
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

                ' add the setting to the list
                Me.AllSettings.Add(New Setting(t, resource, v))
            Next

        Catch ex As Exception
        Finally
            cn.Close()
        End Try
    End Sub

    Public Sub SaveToLocal()
        If Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ePanic") Is Nothing Then
            My.Computer.Registry.CurrentUser.CreateSubKey("ePanic")
        End If

        For Each s As Setting In Me.AllSettings
            If Microsoft.Win32.Registry.CurrentUser.OpenSubKey("ePanic") Is Nothing Then
                My.Computer.Registry.CurrentUser.CreateSubKey("ePanic")
            End If

            My.Computer.Registry.SetValue("HKEY_CURRENT_USER\ePanic", s.ToString, s.value)
        Next
    End Sub
End Class
