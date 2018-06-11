Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml
Imports System.Xml.Serialization

Module Extensions

    <Extension()> Public Function ToBoolean(ByVal value As Object) As Boolean
        Dim retVal As Boolean = False

        Try
            Select Case value.ToString.ToLower
                Case "a", "1", "y", "yes", "true", "t"
                    retVal = True
                Case "b", "0", "n", "no", "false", "f"
                    retVal = False
                Case Else
                    retVal = CBool(value)
            End Select
        Catch ex As Exception
            retVal = False
        End Try

        Return retVal
    End Function

    <Extension()> Public Function IsNullOrNothing(ByVal obj As Object, valueIfNull As Integer) As Integer
        Dim retVal As Integer = valueIfNull
        If IsDBNull(obj) OrElse obj Is Nothing Then retVal = valueIfNull Else retVal = obj.ToString.ToInteger
        Return retVal
    End Function
    <Extension()> Public Function IsNullOrNothing(ByVal obj As Object, valueIfNull As String) As String
        Dim retVal As String = valueIfNull
        If IsDBNull(obj) OrElse obj Is Nothing Then retVal = valueIfNull Else retVal = obj.ToString
        Return retVal
    End Function

    <Extension()> Public Function CheckString(ByVal obj As Object, ByVal ValueIfNothing As String) As String
        Return ToString(obj, ValueIfNothing)
    End Function

    <Extension()> Public Function ToString(ByVal obj As Object, ByVal ValueIfNothing As String) As String
        If obj Is Nothing Then
            obj = ValueIfNothing
        End If

        Return obj.ToString
    End Function

    <Extension()> Public Function ToDate(ByVal str As String, ByVal DefaultReturnValue As Date) As Date
        Try
            If IsDate(str) Then
                Return CDate(str)
            Else
                Return DefaultReturnValue
            End If

        Catch ex As Exception
            Return DefaultReturnValue
        End Try
    End Function

    <Extension()> Public Function ToDecimal(ByVal obj As Object) As Decimal
        Return obj.ToString.ToDecimal
    End Function
    <Extension()> Public Function ToDecimal(ByVal obj As String) As Decimal
        Return obj.ToDecimal(0)
    End Function
    <Extension()> Public Function ToDecimal(ByVal obj As Object, ByVal DefaultReturnValue As Decimal) As Decimal
        Return obj.ToString.ToDecimal(DefaultReturnValue)
    End Function
    <Extension()> Public Function ToDecimal(ByVal str As String, ByVal DefaultReturnValue As Decimal) As Decimal
        Try
            'remove trailing "%"
            If str.EndsWith("%") Then
                str = str.TrimEnd(CChar("%"))
                str = CStr(str.ToDecimal / 100)
            End If

            Return CDec(str)

        Catch ex As Exception
            Return DefaultReturnValue
        End Try
    End Function

    <Extension()> Public Function ToDouble(ByVal obj As Object) As Decimal
        Return obj.ToString.ToDouble
    End Function
    <Extension()> Public Function ToDouble(ByVal str As String) As Decimal
        Return str.ToDouble(0)
    End Function
    <Extension()> Public Function ToDouble(ByVal obj As Object, ByVal DefaultReturnValue As Decimal) As Decimal
        Return obj.ToString.ToDouble(DefaultReturnValue)
    End Function
    <Extension()> Public Function ToDouble(ByVal str As String, ByVal DefaultReturnValue As Decimal) As Decimal
        Try
            'remove trailing "%"
            If str.EndsWith("%") Then
                str = str.TrimEnd(CChar("%"))
                str = CStr(str.ToDecimal / 100)
            End If

            Return CDec(str)

        Catch ex As Exception
            Return DefaultReturnValue
        End Try
    End Function

    <Extension()> Public Function ToInteger(ByVal obj As Object) As Integer
        Return obj.ToString.ToInteger
    End Function
    <Extension()> Public Function ToInteger(ByVal str As String) As Integer
        Return str.ToInteger(0)
    End Function
    <Extension()> Public Function ToInteger(ByVal obj As Object, ByVal DefaultReturnValue As Integer) As Integer
        Return obj.ToString.ToInteger(DefaultReturnValue)
    End Function
    <Extension()> Public Function ToInteger(ByVal str As String, ByVal DefaultReturnValue As Integer) As Integer
        Try
            Return CInt(str)
        Catch ex As Exception
            Return DefaultReturnValue
        End Try
    End Function

    <Extension()> Public Function ToLongInteger(ByVal obj As Object) As Long
        Return obj.ToString.ToLongInteger
    End Function
    <Extension()> Public Function ToLongInteger(ByVal str As String) As Long
        Return str.ToLongInteger(0)
    End Function
    <Extension()> Public Function ToLongInteger(ByVal obj As Object, ByVal DefaultReturnValue As Long) As Long
        Return obj.ToString.ToLongInteger(DefaultReturnValue)
    End Function
    <Extension()> Public Function ToLongInteger(ByVal str As String, ByVal DefaultReturnValue As Long) As Long
        Try
            Return CLng(str)
        Catch ex As Exception
            Return DefaultReturnValue
        End Try
    End Function

    <Extension()> Public Function HasSetting(ByVal lst As List(Of Settings.Setting), ByVal s As Enums.ePanicSetting) As Boolean
        Dim retVal As Boolean = False

        For Each itm As Settings.Setting In lst
            If itm.setting = s Then
                Return True
                Exit For
            End If
        Next

        Return retVal
    End Function

#Region "Object Extensions"

    <Extension()> Public Function SerializeToXml(ByVal o As Object) As String
        Dim sw As New StringWriter()
        Dim tw As XmlTextWriter = Nothing

        Try
            Dim serializer As New XmlSerializer(o.GetType)
            tw = New XmlTextWriter(sw)
            serializer.Serialize(tw, o)

        Catch ex As Exception
        Finally
            sw.Close()
            If tw IsNot Nothing Then
                tw.Close()
            End If
        End Try

        Return sw.ToString()
    End Function

    'Public Function DeserializeFromXML(ByVal xml As String, objectType As Type) As Object
    <Extension()> Public Function DeserializeFromXml(ByVal o As Object, ByVal xml As String) As Object
        Dim strReader As StringReader = Nothing
        Dim serializer As XmlSerializer = Nothing
        Dim xmlReader As XmlTextReader = Nothing
        Dim obj As Object = Nothing

        Try
            strReader = New StringReader(xml)
            serializer = New XmlSerializer(o.GetType)
            xmlReader = New XmlTextReader(strReader)
            obj = serializer.Deserialize(xmlReader)

        Catch exp As Exception
        Finally
            If xmlReader IsNot Nothing Then
                xmlReader.Close()
            End If
            If strReader IsNot Nothing Then
                strReader.Close()
            End If
        End Try

        Return obj
    End Function

#End Region

End Module
