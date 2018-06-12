Public Class Enums
    ' dbname used for the connection string
    Public Enum DBName
        ePanic
        ePanicModel
        Custom
    End Enum

    ' settings
    Public Enum ePanicSettingType
        User
        Machine
        Cluster
        [Global]
        [Default]
    End Enum
    Public Enum ePanicSetting
        UpdateDay = 1
        UpdateTime = 2
        RemoteUpdateFolder = 3
    End Enum

    ' image gallery
    Public Enum ImageType
        Icon
        [Global]
    End Enum

    ' logs
    Public Enum ProjectName
        AutoUpdate
        Client
        SchedulerService
        CommonCoreShared
    End Enum
End Class
