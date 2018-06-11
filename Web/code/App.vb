Public Class App
    Public Shared ReadOnly Property MyCluster As ClusterDatabase
        Get
            If HttpContext.Current.Session("MyCluster") Is Nothing Then HttpContext.Current.Session("MyCluster") = New ClusterDatabase(Me.CurrentUser.UserKey)
            Return CType(HttpContext.Current.Session("MyCluster"), ClusterDatabase)
        End Get
    End Property

    Public Shared Property CurrentUser As SystemUser
        Get
            If HttpContext.Current.Session("CurrentUser") Is Nothing Then HttpContext.Current.Session("CurrentUser") = New SystemUser()
            Return CType(HttpContext.Current.Session("CurrentUser"), SystemUser)
        End Get
        Set(value As SystemUser)
            HttpContext.Current.Session("CurrentUser") = value
        End Set
    End Property
End Class
