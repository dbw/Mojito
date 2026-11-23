Imports System.IO
Imports System.Xml

Public Class limit_updates
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim files As String() = Directory.GetFiles("C:\updates")

        Dim requestedFile As String = Request.QueryString.Item("update")
        Dim updateFile As String = Nothing

        ' get the update file
        Dim file As String
        For Each file In files
            If String.Equals(Path.GetFileName(file), requestedFile, StringComparison.InvariantCultureIgnoreCase) Then
                updateFile = file
            End If
        Next

        ' if updateFile is null, throw a 404 error
        If (updateFile Is Nothing) Then
            Response.Clear()

            ' throw a 404 error
            Response.StatusCode = 404
            Response.End()
            Return
        End If


        Dim validLicense As Boolean = False

        'TODO: set your API KEY
        LimeLM.APIKey = "PASTE THE API KEY HERE"

        'TODO: set the version id to limit updates for
        ' the version id is found in the url. For example http://wyday.com/limelm/version/100/
        ' the version id is 100.
        Const versionID As String = "PASTE THE VERSION ID"

        Try
            Using reader As XmlReader = XmlReader.Create(New StringReader(LimeLM.GetPKeyID(versionID, Request.QueryString.Item("pkey"))))
                reader.ReadToFollowing("rsp")

                If (reader.Item("stat") = "ok") Then
                    reader.ReadToFollowing("pkey")
                    Dim pkeyID As String = reader.Item("id")

                    Using detReader As XmlReader = XmlReader.Create(New StringReader(LimeLM.GetDetails(pkeyID)))
                        detReader.ReadToFollowing("rsp")

                        If (detReader.Item("stat") = "ok") Then
                            detReader.ReadToFollowing("pkey")

                            ' if the pkey isn't revoked (and there's a features block)
                            If ((detReader.Item("revoked") <> "true") AndAlso detReader.ReadToFollowing("features")) Then
                                Dim keepLooking As Boolean = True

                                Do While (keepLooking AndAlso detReader.Read)
                                    Select Case detReader.NodeType

                                        Case XmlNodeType.Element ' feature node
                                            If (detReader.Item("name") = "update_expires") Then

                                                ' see if "update_expires" is later than right now
                                                If (DateTime.Parse(detReader.Item("value")).CompareTo(DateTime.Now) > 0) Then
                                                    validLicense = True
                                                End If

                                                keepLooking = False
                                            End If
                                            Exit Select

                                        Case XmlNodeType.EndElement
                                            If (detReader.Name = "features") Then
                                                keepLooking = False
                                            End If
                                            Exit Select
                                    End Select
                                Loop
                            End If
                        Else ' failure
                            detReader.ReadToFollowing("err")

                            'TODO: use the error code & message
                            'detReader["code"]
                            'detReader["msg"]
                        End If
                    End Using
                Else ' failure
                    reader.ReadToFollowing("err")

                    'TODO: use the error code & message
                    'reader["code"]
                    'reader["msg"]
                End If
            End Using
        Catch ex As Exception
            ' failed to either contact LimeLM or parse the response from LimeLM
            ' TODO: handle the error somehow (log it, notify your support team, etc.)
        End Try

        ' forbid invalid licenses
        If Not validLicense Then
            Response.Clear()

            ' header('HTTP/1.1 403 Forbidden');
            Response.StatusCode = 403
            Response.End()
            Return
        End If


        Response.Clear()

        ' download the file and bail
        Response.AddHeader("Content-Description", "File Transfer")
        Response.ContentType = "application/octet-stream"
        Response.AddHeader("Content-Disposition", ("attachment; filename=" & Path.GetFileName(updateFile)))
        Response.AddHeader("Content-Transfer-Encoding", "binary")
        Response.AddHeader("Expires", "0")
        Response.AddHeader("Cache-Control", "must-revalidate, post-check=0, pre-check=0")
        Response.AddHeader("Pragma", "public")
        Response.AddHeader("Content-Length", New FileInfo(updateFile).Length.ToString)

        ' output the file
        Response.WriteFile(updateFile)

        Response.End()
    End Sub

End Class