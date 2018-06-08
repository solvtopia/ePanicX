Imports System.Xml

Public Class fMain
    Private Sub fMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.ProcessUpdate(Now)
    End Sub

    Private Sub ProcessUpdate(ByVal effectiveDate As DateTime)
        Dim cluster As New ClusterDatabase("JamesDev", "FDB34611-E00B-4033-A427-57EE5E440DCA")
        Dim settings As New Settings(cluster)

        Dim lstLog As New List(Of String)

        ' close the applications
        ' get a list of updated files
        ' rename each file in the list
        ' download the new files
        ' register any libraries
        ' restart the applications
        ' update the server with the update status

    End Sub
End Class
