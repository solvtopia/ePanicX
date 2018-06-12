Imports System.IO

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

    Public Shared Function GetImageFromGallery(ByVal id As Integer) As GalleryImage
        Dim retVal As New GalleryImage

        Dim cn As New SqlClient.SqlConnection(MasterConnectionString)

        Try
            Dim cmd As New SqlClient.SqlCommand("SELECT [ID], [Type], [FileName], [FileSize], [Description], [Content] FROM [ImageGallery] WHERE [ID] = " & id, cn)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            Dim rs As SqlClient.SqlDataReader = cmd.ExecuteReader
            If rs.Read Then
                retVal.ID = rs("ID").ToString.ToInteger
                retVal.Type = CType(rs("Type").ToString, Enums.ImageType)
                retVal.FileName = rs("FileName").ToString
                retVal.FileSize = rs("FileSize").ToString.ToInteger
                retVal.Description = rs("Description").ToString
                Dim buffer As Byte() = CType(rs("Content"), Byte())
                retVal.Content = New MemoryStream(buffer)
            End If
            cmd.Cancel()
            rs.Close()

        Catch ex As Exception
        Finally
            cn.Close()
        End Try

        Return retVal
    End Function

    Public Shared Sub SaveImageToGallery(ByVal imgPath As String, ByVal t As Enums.ImageType)
        Dim cn As New SqlClient.SqlConnection(MasterConnectionString)

        Try
            Dim FS As New FileStream(imgPath, FileMode.Open, FileAccess.Read)
            Dim img As Byte() = New Byte(FS.Length.ToInteger - 1) {}
            FS.Read(img, 0, Convert.ToInt32(FS.Length))

            Dim f As New IO.FileInfo(imgPath)

            Dim cmd As New SqlClient.SqlCommand("INSERT INTO [ImageGallery] ([Type], [FileName], [FileSize], [Description], [Content]) VALUES (@Type, @FileName, @FileSize, @Description, @Content);", cn)
            cmd.Parameters.AddWithValue("@Type", CStr(t))
            cmd.Parameters.AddWithValue("@FileName", f.Name)
            cmd.Parameters.AddWithValue("@FileSize", FS.Length)
            cmd.Parameters.AddWithValue("@Description", "")
            cmd.Parameters.AddWithValue("@Content", img)
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            cmd.ExecuteNonQuery()

        Catch ex As Exception
        Finally
            cn.Close()
        End Try
    End Sub

    Public Shared Function GetNextWeekday(ByVal start As DateTime, ByVal day As DayOfWeek) As DateTime
        Dim daysToAdd As Integer = (CInt(day) - CInt(start.DayOfWeek) + 7) Mod 7
        Return start.AddDays(daysToAdd)
    End Function

    Public Shared Function CompareVersions(ByVal versionA As String, ByVal versionB As String) As Boolean
        Dim vA As Version = New Version(versionA.Replace(",", "."))
        Dim vB As Version = New Version(versionB.Replace(",", "."))
        Return (vA.CompareTo(vB) > 0)
    End Function
End Class


