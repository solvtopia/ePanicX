Imports ePanic.AutoUpdate.Common
Imports ePanic.CommonCore.Shared.Common
Imports System.Xml
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.IO

Public Class fMain
    Private Sub fMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' load the application default objects
        MyCluster = New ClusterDatabase("FDB34611-E00B-4033-A427-57EE5E440DCA")
        MySettings = New Settings(MyCluster)
        MySettings.ClientLoad()

        'SaveImageToGallery("C:\Users\James\Google Drive\Pictures\pirate-icon.png", Enums.ImageType.Icon)
        'Dim img As GalleryImage = GetImageFromGallery(1)
        'PictureBox1.Image = Image.FromStream(img.Content)
        'PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage
        'PictureBox1.Refresh()

        Me.ProcessUpdate(Now)
    End Sub

    Private Sub ProcessUpdate(ByVal effectiveDate As DateTime)
        Dim ftp As New Ftp()

        Dim schedulerServiceName As String = "ePanicScheduler"
        Dim schedulerServiceAssembly As String = "ePanic.SchedulerService.exe"
        Dim clientAssembly As String = "ePanic.Client"

        Dim installPath As String = My.Application.Info.DirectoryPath
        Dim updatePath As String = My.Computer.FileSystem.CombinePath(My.Application.Info.DirectoryPath, "Updates\")
        If Not My.Computer.FileSystem.DirectoryExists(updatePath) Then My.Computer.FileSystem.CreateDirectory(updatePath)

        Dim remoteUpdateFolder As String = MySettings.GetSetting(Enums.ePanicSetting.RemoteUpdateFolder).value

        Dim lstLog As New List(Of String)

        Dim schedulerExists As Boolean = Services.ServiceExists(schedulerServiceName)

        ' stop the scheduler service if it exists
        If schedulerExists Then Services.StopService(schedulerServiceName, 5000)

        ' close the client application if it's running
        Dim pProcess() As Process = System.Diagnostics.Process.GetProcessesByName(clientAssembly)
        For Each p As Process In pProcess
            p.Kill()
        Next

        Dim fileIsNew As Boolean = False

        ' get a list of updated files
        Dim updateFiles As List(Of String) = ftp.ListFiles(remoteUpdateFolder)
        For Each f As String In updateFiles
            ' skip the autoupdate files, those are handled by the scheduler service
            If Not f.ToLower.StartsWith("epanic.autoupdate") Then
                Dim updateFile As String = My.Computer.FileSystem.CombinePath(updatePath, f)
                Dim installFile As String = My.Computer.FileSystem.CombinePath(installPath, f)

                fileIsNew = Not My.Computer.FileSystem.FileExists(installFile)

                If My.Computer.FileSystem.FileExists(updateFile) Then My.Computer.FileSystem.DeleteFile(updateFile)

                ' download the file to the update folder
                ftp.DownloadFile(ftp.ftpUri(remoteUpdateFolder & "/" & f).ToString, updateFile)

                Dim processUpdate As Boolean = False
                If f.ToLower.EndsWith(".exe") Or f.ToLower.EndsWith(".dll") Then
                    ' we can only get the assembly info for executables or libraries
                    ' get the assembly info for both the new and old files
                    Dim assembly As System.Reflection.Assembly = System.Reflection.Assembly.LoadFile(updateFile)
                    Dim updateFileInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)
                    assembly = System.Reflection.Assembly.LoadFile(installFile)
                    Dim installFileInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location)

                    ' make sure the file from the update is a newer version so we don't update files we don't need to
                    If CompareVersions(updateFileInfo.FileVersion, installFileInfo.FileVersion) Then
                        processUpdate = True
                    End If
                Else
                    ' all other files we can use the date modified
                    Dim updateFileInfo As New FileInfo(updateFile)
                    Dim installFileInfo As New FileInfo(installFile)

                    If ftp.GetDateModified(remoteUpdateFolder & "/" & f) > installFileInfo.LastWriteTime Then
                        processUpdate = True
                    End If
                End If

                ' if we are good to update then we can proceed
                If processUpdate Then
                    Dim installFileInfo As New FileInfo(installFile)

                    ' rename the old file in case something goes wrong
                    My.Computer.FileSystem.RenameFile(installFile, installFileInfo.Name & ".AutoUpdateTemp")

                    ' copy the update file to the application directory
                    My.Computer.FileSystem.CopyFile(updateFile, installFile)

                    ' register any libraries
                    If f.ToLower.EndsWith(".dll") And fileIsNew Then
                        Dim asm As Assembly = Assembly.LoadFile(f)
                        Dim regAsm As RegistrationServices = New RegistrationServices()
                        Dim bResult As Boolean = regAsm.RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase)
                    End If

                    ' delete the old version once the new one is copied
                    My.Computer.FileSystem.DeleteFile(installFile & ".AutoUpdateTemp")
                End If
            End If
        Next

        If Not schedulerExists Then
            ' install the scheduler service if it did not exist prior to the update
            Services.InstallService(My.Computer.FileSystem.CombinePath(My.Application.Info.DirectoryPath, schedulerServiceAssembly))
        End If

        ' restart the scheduler service
        Services.StartService(schedulerServiceName, 5000)

        ' update the server with the update status

    End Sub
End Class
