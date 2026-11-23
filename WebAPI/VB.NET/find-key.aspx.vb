Imports System.IO
Imports System.Xml
Imports System.Net.Mail

Public Class find_key
    Inherits System.Web.UI.Page

    Public PkeyFound As Boolean


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If IsPostBack Then
            'TODO: set your API KEY
            LimeLM.APIKey = "PASTE THE API KEY HERE"

            'TODO: set the version id to generate & find keys for
            ' the version id is found in the url. For example http://wyday.com/limelm/version/100/
            ' the version id is 100.
            Const versionID As String = "PASTE THE VERSION ID"

            Dim pkeyList As String = ""
            Dim totalPkeys As Integer

            Try
                ' fetch the product key, and parse the XML response
                Using reader As XmlReader = XmlReader.Create(New StringReader(LimeLM.FindPKey(versionID, txtEmail.Text)))
                    reader.ReadToFollowing("rsp")

                    If (reader.Item("stat") <> "ok") Then
                        reader.ReadToFollowing("err")
                        errorDiv.InnerHtml = "An error happened: " + reader.Item("msg")
                        Return
                    End If

                    reader.ReadToFollowing("pkeys")

                    Dim pkeys As New List(Of String)
                    GetPKeys(reader, pkeys)

                    totalPkeys = pkeys.Count

                    ' form the pkey list
                    For Each pkey As String In pkeys
                        pkeyList += pkey + vbNewLine
                    Next
                End Using

                PkeyFound = True
            Catch ex As Exception
                ' failed to either contact LimeLM or parse the response from LimeLM
                ' TODO: handle the error somehow (log it, notify your support team, etc.)
                '       and delete the following "throw" statement
                Throw
            End Try



            'TODO: email the product key to your users

            '  System.Net.Mail reads SMTP configuration data out of the standard .NET configuration system
            '  (so for ASP.NET applications you’d configure this in your application’s web.config file).

            '  Here is an example of how to configure it:

            '  <system.net>
            '    <mailSettings>
            '      <smtp from="test@foo.com">
            '        <network host="smtpserver1" port="25"
            '                 userName="username" password="secret" defaultCredentials="true" />
            '      </smtp>
            '    </mailSettings>
            '  </system.net>

            Dim message As New MailMessage

            message.To.Add(New MailAddress(txtEmail.Text))
            message.Subject = "Your product " + IIf((totalPkeys > 1), "keys", "key")
            message.Body = "Your product key" & IIf((totalPkeys > 1), "s are", " is") & ":" & vbNewLine + vbNewLine + pkeyList

            Dim smtp As New SmtpClient()
            smtp.Send(message)
        End If
    End Sub

    Private Shared Sub GetPKeys(ByVal reader As XmlReader, ByVal pkeys As List(Of String))
        Do While reader.Read

            ' end of node </pkeys>
            If ((reader.NodeType = XmlNodeType.EndElement) AndAlso reader.LocalName.Equals("pkeys")) Then
                Exit Do
            End If

            If ((reader.NodeType = XmlNodeType.Element) AndAlso reader.LocalName.Equals("pkey")) Then
                pkeys.Add(reader.Item("key"))
            End If
        Loop
    End Sub
End Class