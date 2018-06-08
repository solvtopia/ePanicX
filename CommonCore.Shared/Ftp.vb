Imports System.IO
Imports System.Net
Imports System.Text

Public Class Ftp

    Sub New()
        Me.ftpServer = My.Settings.FTPServer
        Me.ftpUser = My.Settings.FTPUser
        Me.ftpPass = My.Settings.FTPPass
    End Sub
    Sub New(ByVal server As String, ByVal user As String, ByVal pass As String)
        Me.ftpServer = server
        Me.ftpUser = user
        Me.ftpPass = pass
    End Sub

    Private ftpServer As String
    Private ftpUser As String
    Private ftpPass As String

    Public Function DirectoryExists(ByVal fldr As String) As Boolean
        Dim retVal As Boolean = True

        Try
            Dim request = DirectCast(WebRequest.Create(Me.ftpServer & fldr & "/"), FtpWebRequest)

            request.Credentials = New NetworkCredential(Me.ftpUser, Me.ftpPass)
            request.Method = WebRequestMethods.Ftp.ListDirectory

            Using response As FtpWebResponse =
            DirectCast(request.GetResponse(), FtpWebResponse)
                ' directory exists
            End Using

        Catch ex As WebException
            Dim response As FtpWebResponse = DirectCast(ex.Response, FtpWebResponse)
            If response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable Then
                retVal = False
            End If
        End Try

        Return retVal
    End Function

    Public Function CreateDirectory(ByVal fldr As String) As Boolean
        Dim retVal As Boolean = True

        Try
            Dim request As Net.FtpWebRequest = CType(FtpWebRequest.Create(Me.ftpServer & fldr), FtpWebRequest)
            request.Credentials = New NetworkCredential(Me.ftpUser, Me.ftpPass)
            request.Method = WebRequestMethods.Ftp.MakeDirectory

            Using response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)
                ' Folder created
            End Using

        Catch ex As WebException
            Dim response As FtpWebResponse = DirectCast(ex.Response, FtpWebResponse)
            If response.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable Then
                retVal = False
            End If
        End Try

        Return retVal
    End Function

    Public Function CopyFile(ByVal srcFile As String, ByVal destFile As String) As Boolean
        Dim retVal As Boolean = True

        Try
            Dim fi As New FileInfo(srcFile)

            ' Get the object used to communicate with the server.  
            Dim request As FtpWebRequest = DirectCast(WebRequest.Create(Me.ftpServer & destFile), FtpWebRequest)
            request.Method = WebRequestMethods.Ftp.UploadFile

            ' This example assumes the FTP site uses anonymous logon.  
            request.Credentials = New NetworkCredential(Me.ftpUser, Me.ftpPass)

            ' Copy the contents of the file to the request stream.  
            Dim sourceStream As New StreamReader(srcFile)
            Dim fileContents As Byte() = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd())
            sourceStream.Close()
            request.ContentLength = fileContents.Length

            Dim requestStream As Stream = request.GetRequestStream()
            requestStream.Write(fileContents, 0, fileContents.Length)
            requestStream.Close()

            Dim response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)

            response.Close()

        Catch ex As Exception
            retVal = False
        End Try

        Return retVal
    End Function

    Public Function FileExists(ByVal fileName As String) As Boolean
        Dim retVal As Boolean = True

        Try
            Dim request As FtpWebRequest = DirectCast(WebRequest.Create(Me.ftpServer & fileName), FtpWebRequest)
            request.Credentials = New NetworkCredential(Me.ftpUser, Me.ftpPass)
            request.Method = WebRequestMethods.Ftp.GetFileSize

            Dim response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)

        Catch ex As WebException
            Dim response As FtpWebResponse = DirectCast(ex.Response, FtpWebResponse)
            If FtpStatusCode.ActionNotTakenFileUnavailable = response.StatusCode Then
                retVal = False
            End If
        End Try

        Return retVal
    End Function

    Public Function DeleteFile(ByVal fileName As String) As Boolean
        Dim retVal As Boolean = True

        Try
            Dim request As FtpWebRequest = DirectCast(WebRequest.Create(Me.ftpServer & fileName), FtpWebRequest)
            request.Credentials = New NetworkCredential(Me.ftpUser, Me.ftpPass)
            request.Method = WebRequestMethods.Ftp.DeleteFile

            Dim response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)

        Catch ex As WebException
            Dim response As FtpWebResponse = DirectCast(ex.Response, FtpWebResponse)
            If FtpStatusCode.ActionNotTakenFileUnavailable = response.StatusCode Then
                retVal = False
            End If
        End Try

        Return retVal
    End Function

    Public Function RenameFile(ByVal srcFile As String, ByVal destFile As String) As Boolean
        Dim retVal As Boolean = True

        Try
            Dim request As FtpWebRequest = DirectCast(WebRequest.Create(Me.ftpServer & srcFile), FtpWebRequest)
            request.Credentials = New NetworkCredential(Me.ftpUser, Me.ftpPass)
            request.Method = WebRequestMethods.Ftp.Rename
            request.RenameTo = destFile

            Dim response As FtpWebResponse = DirectCast(request.GetResponse(), FtpWebResponse)

        Catch ex As WebException
            Dim response As FtpWebResponse = DirectCast(ex.Response, FtpWebResponse)
            If FtpStatusCode.ActionNotTakenFileUnavailable = response.StatusCode Then
                retVal = False
            End If
        End Try

        Return retVal
    End Function

    Public Function DownloadFile(ByVal srcFile As String, ByVal destFile As String) As Boolean
        Dim retVal As Boolean = True

        Try
            Dim src As Uri
            Try
                src = New Uri(srcFile)
            Catch ex As Exception
                srcFile = "http://" & Me.ftpServer & "/" & My.Settings.FTPFolder & "/" & srcFile
                src = New Uri(srcFile)
            End Try

            My.Computer.Network.DownloadFile(src, destFile, Me.ftpUser, Me.ftpPass)

        Catch ex As Exception
            retVal = False
        End Try

        Return retVal
    End Function
End Class