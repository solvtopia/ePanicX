Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports ePanic.CommonCore.Shared.Common

Public Class Service
    Dim MyCluster As ClusterDatabase
    Dim MySettings As Settings

    ' setup a timer to check for auto updates
    ' 5 minutes
    Dim UpdateTimer As New System.Timers.Timer() With {
            .Enabled = True,
            .Interval = 30000
        }

    ' setup a timer to pull the latest settings and update the last online date
    ' 1 minute
    Dim SettingsTimer As New System.Timers.Timer() With {
            .Enabled = True,
            .Interval = 60000
        }

    Protected Overrides Sub OnStart(ByVal args() As String)
        MyCluster = New ClusterDatabase
        MySettings = New Settings

        ' set defaults for now until the clusters are online
        MySettings.AllSettings.Add(New Settings.Setting(Enums.ePanicSettingType.Global, Enums.ePanicSetting.UpdateDay, "everyday"))
        MySettings.AllSettings.Add(New Settings.Setting(Enums.ePanicSettingType.Global, Enums.ePanicSetting.UpdateTime, "12:00:00 AM"))
        MySettings.AllSettings.Add(New Settings.Setting(Enums.ePanicSettingType.Global, Enums.ePanicSetting.RemoteUpdateFolder, "/AutoUpdate"))

        '' load the application default objects
        'MyCluster = New ClusterDatabase("FDB34611-E00B-4033-A427-57EE5E440DCA")
        'MySettings = New Settings(MyCluster)
        'MySettings.ClientLoad()

        AddHandler UpdateTimer.Elapsed, AddressOf Me.OnUpdateTimer
        UpdateTimer.Start()

        AddHandler SettingsTimer.Elapsed, AddressOf Me.OnSettingsTimer
        SettingsTimer.Start()
    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
    End Sub

    Private Sub OnUpdateTimer(sender As Object, e As Timers.ElapsedEventArgs)
        UpdateTimer.Stop()

        Dim updateDay As String = MySettings.GetSetting(Enums.ePanicSetting.UpdateDay).value
        Dim updateTime As String = MySettings.GetSetting(Enums.ePanicSetting.UpdateTime).value
        Dim updateDateTime As DateTime

        Dim updateDate As Date
        Select Case updateDay.ToLower
            Case "everyday"
                updateDate = Now.Date
            Case "monday"
                If Now.DayOfWeek = DayOfWeek.Monday Then
                    updateDate = Now.Date
                Else updateDate = GetNextWeekday(Now, DayOfWeek.Monday)
                End If
            Case "tuesday"
                If Now.DayOfWeek = DayOfWeek.Tuesday Then
                    updateDate = Now.Date
                Else updateDate = GetNextWeekday(Now, DayOfWeek.Tuesday)
                End If
            Case "wednesday"
                If Now.DayOfWeek = DayOfWeek.Wednesday Then
                    updateDate = Now.Date
                Else updateDate = GetNextWeekday(Now, DayOfWeek.Wednesday)
                End If
            Case "thursday"
                If Now.DayOfWeek = DayOfWeek.Thursday Then
                    updateDate = Now.Date
                Else updateDate = GetNextWeekday(Now, DayOfWeek.Thursday)
                End If
            Case "friday"
                If Now.DayOfWeek = DayOfWeek.Friday Then
                    updateDate = Now.Date
                Else updateDate = GetNextWeekday(Now, DayOfWeek.Friday)
                End If
            Case "saturday"
                If Now.DayOfWeek = DayOfWeek.Saturday Then
                    updateDate = Now.Date
                Else updateDate = GetNextWeekday(Now, DayOfWeek.Saturday)
                End If
            Case "sunday"
                If Now.DayOfWeek = DayOfWeek.Sunday Then
                    updateDate = Now.Date
                Else updateDate = GetNextWeekday(Now, DayOfWeek.Sunday)
                End If
        End Select

        updateDateTime = CDate(FormatDateTime(updateDate, DateFormat.ShortDate) & " " & updateTime)

        ' check to see if it's time to run the autoupdate
        If Now >= updateDateTime And Now <= updateDateTime.AddMilliseconds(UpdateTimer.Interval) Then
            Dim m As New Machine(MyCluster.MachineKey)

            ' see if there is a new version of the updater
            Dim ftp As New Ftp()

            Dim updaterAssemblyName As String = "ePanic.AutoUpdate.exe"
            Dim installPath As String = My.Application.Info.DirectoryPath
            Dim updatePath As String = My.Computer.FileSystem.CombinePath(My.Application.Info.DirectoryPath, "Updates\")
            If Not My.Computer.FileSystem.DirectoryExists(updatePath) Then My.Computer.FileSystem.CreateDirectory(updatePath)

            Dim remoteUpdateFolder As String = MySettings.GetSetting(Enums.ePanicSetting.RemoteUpdateFolder).value

            ' close the updater application if it's running
            Dim pProcess() As Process = System.Diagnostics.Process.GetProcessesByName(updaterAssemblyName)
            For Each p As Process In pProcess
                p.Kill()
            Next

            Dim fileIsNew As Boolean = False

            ' get a list of updated files
            Dim updateFiles As List(Of String) = ftp.ListFiles(remoteUpdateFolder)
            For Each f As String In updateFiles
                ' only process the autoupdate files, everything else is handled by the update utility
                If f.ToLower.StartsWith(updaterAssemblyName.Replace(".exe", "").ToLower) Then
                    Dim updateFile As String = My.Computer.FileSystem.CombinePath(updatePath, f)
                    Dim installedFile As String = My.Computer.FileSystem.CombinePath(installPath, f)

                    fileIsNew = Not My.Computer.FileSystem.FileExists(installedFile)

                    If My.Computer.FileSystem.FileExists(updateFile) Then My.Computer.FileSystem.DeleteFile(updateFile)

                    ' download the file to the update folder
                    ftp.DownloadFile(ftp.ftpUri(remoteUpdateFolder & "/" & f).ToString, updateFile)

                    Dim processUpdate As Boolean = False
                    If f.ToLower.EndsWith(".exe") Or f.ToLower.EndsWith(".dll") Then
                        ' we can only get the assembly info for executables or libraries
                        ' get the assembly info for both the new and old files
                        Dim assembly As System.Reflection.Assembly = System.Reflection.Assembly.LoadFile(updateFile)
                        Dim updateVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)
                        assembly = System.Reflection.Assembly.LoadFile(installedFile)
                        Dim installVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)

                        ' make sure the file from the update is a newer version so we don't update files we don't need to
                        If CompareVersions(updateVersionInfo.FileVersion, installVersionInfo.FileVersion) Then
                            processUpdate = True
                        End If
                    Else
                        ' all other files we can use the date modified
                        Dim updateFileInfo As New FileInfo(updateFile)
                        Dim installedFileInfo As New FileInfo(installedFile)

                        If ftp.GetDateModified(remoteUpdateFolder & "/" & f) > installedFileInfo.LastWriteTime Then
                            processUpdate = True
                        End If
                    End If

                    ' if we are good to update then we can proceed
                    If processUpdate Then
                        Dim installedFileInfo As New FileInfo(installedFile)

                        ' rename the old file in case something goes wrong
                        My.Computer.FileSystem.RenameFile(installedFile, installedFileInfo.Name & ".AutoUpdateTemp")

                        ' copy the update file to the application directory
                        Try
                            My.Computer.FileSystem.CopyFile(updateFile, installedFile)
                        Catch ex As Exception
                            My.Computer.FileSystem.RenameFile(installedFileInfo.FullName & ".AutoUpdateTemp", installedFileInfo.Name)
                        End Try

                        ' register any libraries
                        If f.ToLower.EndsWith(".dll") And fileIsNew Then
                            Dim asm As Assembly = Assembly.LoadFile(f)
                            Dim regAsm As RegistrationServices = New RegistrationServices()
                            Dim bResult As Boolean = regAsm.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase)
                        End If

                        ' delete the old version once the new one is copied
                        My.Computer.FileSystem.DeleteFile(installedFile & ".AutoUpdateTemp")

                        ' keep the installed versions in the machine record
                        If f.ToLower.EndsWith(".exe") Or f.ToLower.EndsWith(".dll") Then
                            Dim asm As System.Reflection.Assembly = System.Reflection.Assembly.LoadFile(installedFile)
                            Dim installedVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(asm.Location)
                            If f.ToLower = updaterAssemblyName.ToLower Then
                                m.AutoUpdateVersion = New Version(installedVersionInfo.FileVersion)
                            End If
                        End If
                    End If
                End If
            Next

            m.Save()

            ' run the updater
            Process.Start(My.Computer.FileSystem.CombinePath(installPath, updaterAssemblyName))
        End If

        UpdateTimer.Start()
    End Sub

    Private Sub OnSettingsTimer(sender As Object, e As Timers.ElapsedEventArgs)
        SettingsTimer.Stop()

        'MySettings.UpdateLocal()

        SettingsTimer.Start()
    End Sub

End Class
