Imports ePanic.CommonCore.Shared.Common
Imports System.Xml
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.IO

Public Module Xml
    ' loops through all properties and fields of an object and returns a new XmlDocument with values
    Public Function ObjectToXml(ByVal obj As Object) As String
        Dim retVal As String = ""

        Dim myValue As String = ""
        Dim myName As String = ""
        Dim xDoc As New XmlDocument

        Dim dec As XmlDeclaration = xDoc.CreateXmlDeclaration("1.0", "utf-16", "")
        xDoc.PrependChild(dec)

        ' get the type of the object we are working with
        Dim objectType As Type = CType(obj.GetType, Type)

        'add root element
        Dim quoteElement As XmlElement = xDoc.CreateElement(objectType.ToString)

        ' get a list of properties and fields to loop through
        Dim properties As PropertyInfo() = objectType.GetProperties()
        Dim fields As FieldInfo() = objectType.GetFields()

        ' loop through properties first (this works on compiled objects)
        For Each prop As PropertyInfo In properties
            Dim value As String = ""
            Try
                If Not prop.GetValue(obj, Nothing) Is Nothing Then
                    value = CStr(prop.GetValue(obj, Nothing))
                End If

            Catch ex As Exception
                ' skip complex types
            End Try

            quoteElement.AppendChild(NewElement(xDoc, prop.Name.XmlEncode, value.XmlEncode))
        Next

        ' now loop through the fields (this works on local classes)
        For Each fld As FieldInfo In fields
            Dim value As String = ""
            Try
                If Not fld.GetValue(obj) Is Nothing Then
                    value = CStr(fld.GetValue(obj))
                End If

            Catch ex As Exception
                ' skip complex types
            End Try

            quoteElement.AppendChild(NewElement(xDoc, fld.Name.XmlEncode, value.XmlEncode))
        Next

        xDoc.AppendChild(quoteElement)

        'return just the string
        Dim sw As StringWriter = New StringWriter
        xDoc.Save(sw)

        xDoc = Nothing

        retVal = sw.ToString()

        Return retVal
    End Function


    <Extension()> Public Function NewElement(ByVal xDoc As XmlDocument, ByVal name As String) As XmlElement
        Return NewElement(xDoc, name, "")
    End Function
    <Extension()> Public Function NewElement(ByVal xDoc As XmlDocument, ByVal name As String, ByVal value As String) As XmlElement
        Return NewElement(xDoc, name, value, Nothing)
    End Function
    <Extension()> Public Function NewElement(ByVal xDoc As XmlDocument, ByVal name As String, ByVal value As String, ByVal attribute As XmlAttribute) As XmlElement
        Dim xElem As XmlElement = xDoc.CreateElement(name.XmlEncode(True))
        If value IsNot Nothing AndAlso value <> "" Then
            If value.Trim.Length > 0 Then xElem.InnerText = value.XmlEncode
        End If
        If attribute IsNot Nothing Then xElem.Attributes.Append(attribute)
        Return xElem
    End Function
    <Extension()> Public Function NewElement(ByVal xDoc As XmlDocument, ByVal name As String, ByVal value As String, ByVal type As String, ByVal selected As String, ByVal index As String, ByVal attribute As XmlAttribute) As XmlElement
        Dim xElem As XmlElement = NewElement(xDoc, name.XmlEncode(True), value.XmlEncode)
        If type <> "" Then
            Dim xAttr As XmlAttribute = xDoc.CreateAttribute("Type")
            xAttr.Value = type.XmlEncode
            xElem.Attributes.Append(xAttr)
        End If
        If selected <> "" Then
            Dim xAttr As XmlAttribute = xDoc.CreateAttribute("Selected")
            xAttr.Value = selected.XmlEncode
            xElem.Attributes.Append(xAttr)
        End If
        If index <> "" Then
            Dim xAttr As XmlAttribute = xDoc.CreateAttribute("Index")
            xAttr.Value = index.XmlEncode
            xElem.Attributes.Append(xAttr)
        End If
        If attribute IsNot Nothing Then xElem.Attributes.Append(attribute)

        Return xElem
    End Function

    <Extension()> Public Function NewAttribute(ByRef xDoc As XmlDocument, ByVal name As String, ByVal value As String) As XmlAttribute
        Dim retVal As XmlAttribute = xDoc.CreateAttribute(name)
        retVal.Value = value

        Return retVal
    End Function

    <Extension()> Public Function XmlEncode(ByVal str As String, Optional ByVal deep As Boolean = False) As String
        Dim sNew As String = str

        sNew = Replace(sNew, "<", "&lt;")
        sNew = Replace(sNew, ">", "&gt;")
        sNew = Replace(sNew, "&", "&amp;")
        sNew = Replace(sNew, """", "&quot;")
        sNew = Replace(sNew, "'", "&apos;")
        If deep Then
            sNew = Replace(sNew, " ", "_nbsp;")
            sNew = Replace(sNew, "$", "_cur;")
            sNew = Replace(sNew, "/", "_fsl;")
            sNew = Replace(sNew, "\", "_bksl;")
            sNew = Replace(sNew, ":", "_colon;")
            sNew = Replace(sNew, "!", "_bang;")
            sNew = Replace(sNew, "=", "_eq;")
            sNew = Replace(sNew, "?", "_quest;")
            sNew = Replace(sNew, ";", "_scolon")
        End If

        Return sNew
    End Function

    <Extension()> Public Function XmlDecode(ByVal str As String, Optional ByVal deep As Boolean = False) As String
        Dim sNew As String = str

        ' we call this so many times to make sure we have all the ampersands
        sNew = Replace(sNew, "&amp;", "&")
        sNew = Replace(sNew, "&amp;", "&")
        sNew = Replace(sNew, "&amp;", "&")
        sNew = Replace(sNew, "&amp;", "&")

        sNew = Replace(sNew, "&lt;", "<")
        sNew = Replace(sNew, "&gt;", ">")
        sNew = Replace(sNew, "&quot;", """")
        sNew = Replace(sNew, "&apos;", "'")
        If deep Then
            sNew = Replace(sNew, "_nbsp;", " ")
            sNew = Replace(sNew, "_cur;", "$")
            sNew = Replace(sNew, "_fsl;", "/")
            sNew = Replace(sNew, "_bksl;", "\")
            sNew = Replace(sNew, "_colon;", ":")
            sNew = Replace(sNew, "_bang;", "!")
            sNew = Replace(sNew, "_eq;", "=")
            sNew = Replace(sNew, "_quest;", "?")
            sNew = Replace(sNew, "_scolon", ";")
        End If

        Return sNew
    End Function

    Public Function NewXmlDocument() As XmlDocument
        Return NewXmlDocument("Data")
    End Function
    Public Function NewXmlDocument(ByVal rootElement As String) As XmlDocument
        Dim retVal As New XmlDocument

        Dim dec As XmlDeclaration = retVal.CreateXmlDeclaration("1.0", "utf-16", "")
        retVal.PrependChild(dec)

        Dim root As XmlElement = retVal.CreateElement(rootElement)
        retVal.AppendChild(root)

        Return retVal
    End Function

End Module
