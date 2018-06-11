Public Class MyPage
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            App.CurrentUser = New SystemUser()
        End If
    End Sub
End Class