Imports ePanic.CommonCore.Shared.Common
Imports System.IO

Public Class Login
    Inherits MyPage

    Protected Sub Login_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim img As GalleryImage = GetImageFromGallery(1)

        img2.ImageUrl = "data:image/png;base64," & Convert.ToBase64String(img.Content.ToArray(), 0, img.Content.ToArray().Length)


    End Sub
End Class