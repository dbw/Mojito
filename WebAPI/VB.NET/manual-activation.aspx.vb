Imports System.IO
Imports System.Xml

Public Class manual_activation
    Inherits System.Web.UI.Page

    Public ActivationRequest As String
    Public ActivationResponse As String

    Public Success As Boolean


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If (Not Request.Form.Item("xmlresp") Is Nothing) Then

            Response.Clear()

            ' download the file and bail
            Response.AddHeader("Content-Description", "File Transfer")
            Response.ContentType = "application/xml"
            Response.AddHeader("Content-Disposition", "attachment; filename=ActivationResponse.xml")
            Response.AddHeader("Content-Transfer-Encoding", "binary")
            Response.AddHeader("Expires", "0")
            Response.AddHeader("Cache-Control", "must-revalidate, post-check=0, pre-check=0")
            Response.AddHeader("Pragma", "public")
            Response.AddHeader("Content-Length", Request.Form.Item("xmlresp").Length.ToString)

            ' output the file
            Response.Write(Request.Form.Item("xmlresp"))

            Response.End()
            Return
        End If

        If IsPostBack Then
            ' TODO: set your API KEY
            LimeLM.APIKey = "PASTE THE API KEY HERE"

            ' try to get the activation request
            ActivationRequest = Request.Form.Item("act_req")

            If String.IsNullOrEmpty(ActivationRequest) Then

                ' try to get it from file
                If (Not act_req_file.PostedFile Is Nothing) Then

                    ' stream in the entire string
                    Using sr As StreamReader = New StreamReader(act_req_file.PostedFile.InputStream)
                        ActivationRequest = sr.ReadToEnd
                    End Using
                Else
                    errorDiv.InnerHtml = "Oops. You must submit an activation request file."
                    Return
                End If
            End If

            Try
                ' process the manual activation
                Using reader As XmlReader = XmlReader.Create(New StringReader(LimeLM.ManualActivation(ActivationRequest)))

                    reader.ReadToFollowing("rsp")

                    If (reader.Item("stat") <> "ok") Then
                        reader.ReadToFollowing("err")

                        Select Case reader.Item("code")
                            Case "105"
                                errorDiv.InnerHtml = "This product key has been revoked. Contact us for more information."
                            Case "106"
                                errorDiv.InnerHtml = "This product key has been activated the maximum number of times. Buy more licenses."
                            Case Else
                                errorDiv.InnerHtml = "An error happened: " + reader.Item("msg")
                        End Select
                        Return
                    End If

                    reader.ReadToFollowing("act_resp_xml")
                    ActivationResponse = reader.Item("data")
                End Using

                Success = True
            Catch ex As Exception
                ' failed to either contact LimeLM or parse the response from LimeLM
                ' TODO: handle the error somehow (log it, notify your support team, etc.)
            End Try
        End If
    End Sub

End Class