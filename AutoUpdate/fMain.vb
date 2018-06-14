Imports ePanic.AutoUpdate.Common
Imports ePanic.CommonCore.Shared.Common
Imports System.Xml
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.IO

Public Class fMain
    Private Sub fMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' load the application default objects
        'MyCluster = New ClusterDatabase("FDB34611-E00B-4033-A427-57EE5E440DCA")
        'MySettings = New Settings(MyCluster)
        'MySettings.ClientLoad()

        'SaveImageToGallery("C:\Users\James\Google Drive\Pictures\pirate-icon.png", Enums.ImageType.Icon)
        'Dim img As GalleryImage = GetImageFromGallery(1)
        'PictureBox1.Image = Image.FromStream(img.Content)
        'PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
        'PictureBox1.Refresh()

        Me.ProcessUpdate(Now)
    End Sub

    Private Sub ProcessUpdate(ByVal effectiveDate As DateTime)
        'Dim m As New Machine(MyCluster.MachineKey)

        Dim clientAssemblyName As String = "ePanic.Client.exe"
        Dim updaterAssemblyName As String = "ePanic.AutoUpdate.exe"
        Dim schedulerAssemblyName As String = "ePanic.SchedulerService.exe"
        Dim commonCoreAssemblyName As String = "ePanic.CommonCore.Shared.dll"

        Dim lstLog As New List(Of String)
        lstLog.Add(Now.ToString & ": Update Started")

        Dim ftp As New Ftp()

        Dim schedulerServiceName As String = "ePanicScheduler"
        Dim clientAssembly As String = "ePanic.Client"

        Dim installPath As String = My.Application.Info.DirectoryPath
        Dim updatePath As String = My.Computer.FileSystem.CombinePath(My.Application.Info.DirectoryPath, "Updates\")
        If Not My.Computer.FileSystem.DirectoryExists(updatePath) Then My.Computer.FileSystem.CreateDirectory(updatePath)

        Dim remoteUpdateFolder As String = "/AutoUpdate" 'MySettings.GetSetting(Enums.ePanicSetting.RemoteUpdateFolder).value

        Dim schedulerExists As Boolean = Services.ServiceExists(schedulerServiceName)

        ' stop the scheduler service if it exists
        If schedulerExists Then
            Services.StopService(schedulerServiceName, 5000)
            lstLog.Add(Now.ToString & ": Scheduler Service Present, Service Stopped")
        Else lstLog.Add(Now.ToString & ": Scheduler Service Not Present")
        End If

        ' close the client application if it's running
        Dim clientRunning As String = "Client Not Running"
        Dim pProcess() As Process = System.Diagnostics.Process.GetProcessesByName(clientAssembly)
        For Each p As Process In pProcess
            p.Kill()
            clientRunning = "Client Running, Process Stopped"
        Next
        lstLog.Add(Now.ToString & ": " & clientRunning)

        Dim fileIsNew As Boolean = False

        ' get a list of updated files
        Dim updateFiles As List(Of String) = ftp.ListFiles(remoteUpdateFolder)
        lstLog.Add(Now.ToString & ": Processing " & updateFiles.Count & " Update Files")
        For Each f As String In updateFiles
            ' skip the autoupdate files, those are handled by the scheduler service
            If Not f.ToLower.StartsWith(updaterAssemblyName.Replace(".exe", "").ToLower) Then
                Dim updateFile As String = My.Computer.FileSystem.CombinePath(updatePath, f)
                Dim installedFile As String = My.Computer.FileSystem.CombinePath(installPath, f)

                fileIsNew = Not My.Computer.FileSystem.FileExists(installedFile)

                If My.Computer.FileSystem.FileExists(updateFile) Then My.Computer.FileSystem.DeleteFile(updateFile)

                ' download the file to the update folder
                ftp.DownloadFile(ftp.ftpUri(remoteUpdateFolder & "/" & f).ToString, updateFile)
                lstLog.Add(Now.ToString & ": " & f & " (Downloaded To Update Folder)")

                Dim processUpdate As Boolean = False
                If fileIsNew Then
                    processUpdate = True
                    lstLog.Add(Now.ToString & ": " & f & " (Created New File)")
                Else
                    If f.ToLower.EndsWith(".exe") Or f.ToLower.EndsWith(".dll") Then
                        ' we can only get the assembly info for executables or libraries
                        ' get the assembly info for both the new and old files
                        Dim assembly As System.Reflection.Assembly = System.Reflection.Assembly.LoadFile(updateFile)
                        Dim updateVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)
                        assembly = System.Reflection.Assembly.LoadFile(installedFile)
                        Dim installedVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)

                        ' make sure the file from the update is a newer version so we don't update files we don't need to
                        If CompareVersions(updateVersionInfo.FileVersion, installedVersionInfo.FileVersion) Then
                            lstLog.Add(Now.ToString & ": " & f & " (Updated From Version " & installedVersionInfo.FileVersion & " To Version " & updateVersionInfo.FileVersion & ")")
                            processUpdate = True
                        End If
                    Else
                        ' all other files we can use the date modified
                        Dim updateFileInfo As New FileInfo(updateFile)
                        Dim installedFileInfo As New FileInfo(installedFile)

                        If ftp.GetDateModified(remoteUpdateFolder & "/" & f) > installedFileInfo.LastWriteTime Then
                            lstLog.Add(Now.ToString & ": " & f & " (Updated Using Date Modified)")
                            processUpdate = True
                        End If
                    End If
                End If

                ' if we are good to update then we can proceed
                If processUpdate Then
                    Dim installedFileInfo As New FileInfo(installedFile)

                    ' rename the old file in case something goes wrong
                    My.Computer.FileSystem.RenameFile(installedFile, installedFileInfo.Name & ".AutoUpdateTemp")
                    lstLog.Add(Now.ToString & ": " & f & " (Backup Created At " & installedFileInfo.Name & ".AutoUpdateTemp)")

                    ' copy the update file to the application directory
                    Try
                        My.Computer.FileSystem.CopyFile(updateFile, installedFile)
                        lstLog.Add(Now.ToString & ": " & f & " (Copied To Install Folder)")
                    Catch ex As Exception
                        lstLog.Add(Now.ToString & ": Exception Copying File To Install Folder: " & ex.ToString)
                        My.Computer.FileSystem.RenameFile(installedFileInfo.FullName & ".AutoUpdateTemp", installedFileInfo.Name)
                        lstLog.Add(Now.ToString & ": " & installedFileInfo.Name & ".AutoUpdateTemp" & " (Backup Restored)")
                    End Try

                    ' register any libraries
                    If f.ToLower.EndsWith(".dll") And fileIsNew Then
                        Dim asm As Assembly = Assembly.LoadFile(f)
                        Dim regAsm As RegistrationServices = New RegistrationServices()
                        Dim bResult As Boolean = regAsm.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase)
                        lstLog.Add(Now.ToString & ": " & f & " (Library Registered)")
                    End If

                    ' delete the old version once the new one is copied
                    My.Computer.FileSystem.DeleteFile(installedFile & ".AutoUpdateTemp")
                    lstLog.Add(Now.ToString & ": " & installedFileInfo.Name & ".AutoUpdateTemp" & " (Backup Deleted)")

                    ' keep the installed versions in the machine record
                    'If f.ToLower.EndsWith(".exe") Or f.ToLower.EndsWith(".dll") Then
                    '    Dim asm As System.Reflection.Assembly = System.Reflection.Assembly.LoadFile(installedFile)
                    '    Dim installedVersionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(asm.Location)
                    '    Select Case True
                    '        Case f.ToLower = clientAssemblyName.ToLower
                    '            m.ClientVersion = New Version(installedVersionInfo.FileVersion)
                    '        Case f.ToLower = schedulerAssemblyName.ToLower
                    '            m.SchedulerVersion = New Version(installedVersionInfo.FileVersion)
                    '        Case f.ToLower = commonCoreAssemblyName.ToLower
                    '            m.CommonCoreVersion = New Version(installedVersionInfo.FileVersion)
                    '    End Select
                    'End If
                Else
                    lstLog.Add(Now.ToString & ": " & f & " (File Skipped)")
                End If
            End If
        Next

        If Not schedulerExists Then
            ' install the scheduler service if it did not exist prior to the update
            Services.InstallService(My.Computer.FileSystem.CombinePath(My.Application.Info.DirectoryPath, schedulerAssemblyName))
            lstLog.Add(Now.ToString & ": Scheduler Service Installed")
        End If

        ' restart the scheduler service
        Services.StartService(schedulerServiceName, 5000)
        lstLog.Add(Now.ToString & ": Scheduler Service Started")

        ' update the server with the update status
        lstLog.Add(Now.ToString & ": Update Complete.")

        'WriteUpdateLog(MyCluster, lstLog)
        'm.Save()
    End Sub
End Class
