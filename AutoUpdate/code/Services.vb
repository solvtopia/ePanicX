Imports System.ServiceProcess
Imports System.Runtime.InteropServices

Public Class Services

    Public Shared Sub StartService(ByVal serviceName As String, ByVal timeoutMilliseconds As Integer)
        Dim service As New ServiceController(serviceName)

        Try
            Dim timeout As TimeSpan = TimeSpan.FromMilliseconds(timeoutMilliseconds)
            service.Start()
            service.WaitForStatus(ServiceControllerStatus.Running, timeout)

        Catch ex As Exception
        End Try
    End Sub

    Public Shared Sub StopService(ByVal serviceName As String, ByVal timeoutMilliseconds As Integer)
        Dim service As New ServiceController(serviceName)

        Try
            Dim timeout As TimeSpan = TimeSpan.FromMilliseconds(timeoutMilliseconds)
            service.[Stop]()
            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout)

        Catch ex As Exception
        End Try
    End Sub

    Public Shared Sub RestartService(ByVal serviceName As String, ByVal timeoutMilliseconds As Integer)
        Dim service As New ServiceController(serviceName)

        Try
            Dim millisec1 As Integer = Environment.TickCount
            Dim timeout As TimeSpan = TimeSpan.FromMilliseconds(timeoutMilliseconds)
            service.[Stop]()
            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout)
            Dim millisec2 As Integer = Environment.TickCount
            timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1))
            service.Start()
            service.WaitForStatus(ServiceControllerStatus.Running, timeout)

        Catch ex As Exception
        End Try
    End Sub

    Public Shared Sub InstallService(ByVal ExeFilename As String)
        Dim mySavedState = New Hashtable()

        Try
            Dim Installer As New System.Configuration.Install.AssemblyInstaller() With {
                .Path = ExeFilename,
                .UseNewContext = True
            }
            Installer.Install(mySavedState)
            Installer.Commit(mySavedState)

        Catch ex As Exception
        End Try
    End Sub

    Public Shared Sub UninstallService(ByVal ExeFilename As String)
        Dim mySavedState = New Hashtable()

        Try
            Dim Installer As New System.Configuration.Install.AssemblyInstaller() With {
                .Path = ExeFilename,
                .UseNewContext = True
            }
            Installer.Uninstall(mySavedState)

        Catch ex As Exception
        End Try
    End Sub

    Public Shared Function ServiceExists(ByVal Name As String) As Boolean
        Dim retVal As Boolean = False

        Try
            Dim servicesButNotDevices As ServiceController() = ServiceController.GetServices()

            For Each service As ServiceController In servicesButNotDevices
                If service.ServiceName.ToLower = Name.ToLower Or service.DisplayName.ToLower = Name.ToLower Then
                    retVal = True
                    Exit For
                End If
            Next

        Catch ex As Exception
        End Try

        Return retVal
    End Function
End Class
