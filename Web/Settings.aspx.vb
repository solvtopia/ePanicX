Public Class Settings
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Me.LoadLists()
        End If
    End Sub

    Private Sub LoadLists()
        Dim enumValues As Array = System.[Enum].GetValues(GetType(Enums.ePanicSettingType))
        For Each resource As Enums.ePanicSettingType In enumValues
            Me.ddlType.Items.Add(New ListItem(resource.ToString, CStr(resource)))
        Next

        enumValues = System.[Enum].GetValues(GetType(Enums.ePanicSetting))
        For Each resource As Enums.ePanicSetting In enumValues
            Me.ddlSetting.Items.Add(New ListItem(resource.ToString, CStr(resource)))
        Next
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click

    End Sub
End Class