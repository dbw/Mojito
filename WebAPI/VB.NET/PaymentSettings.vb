Imports System.Xml
Imports System.IO
Imports System.Net.Mail

Public Class PaymentSettings
    'TODO: Set the following constants. All URLs must be publicly accessible.
    ' See http://wyday.com/limelm/help/how-to-generate-product-keys-after-order/
    ' to learn how to configure the payment form and set everything up.

    '==== General Config ====

    ' Set any of the following 4 boolean values to either
    ' true or false based on whether you're using that
    ' particular payment method. "payment.aspx" automatically
    ' shows or hides the payment method based on these boolean
    ' values.

    ' If you set a payment setting to true then you'll also
    ' have to configure the payment settings for that
    ' particular payment method below.
    Public Const UseAuthorizeNetCC As Boolean = True
    Public Const UseAuthorizeNetBank As Boolean = True
    Public Const UseMoneybookers As Boolean = True
    Public Const UsePayPal As Boolean = True


    ' dollars, cents
    Public Shared AppPrice As Integer() = New Integer() {69, 99}
    Public Const Currency As String = "USD"

    ' The currency symbol that the end-user will see.
    ' If you're not using "$" then you should use
    ' the HTML-entity equivalent of the symbol.
    ' Your customers will see the symbol.

    ' € = &euro;
    ' £ = &pound;
    ' ¥ = &yen;
    ' etc.
    Public Const CurrencySign As String = "$"

    Public Const CompanyName As String = "YourCompany"
    Public Const AppName As String = "YourApp"

    ' api key found in http://wyday.com/limelm/settings/
    Public Const LimeLM_ApiKey As String = "PASTE THE API KEY HERE"

    ' the version id is found in the url.
    ' For example http://wyday.com/limelm/version/100/   the version id is "100".
    Public Const LimeLM_VersionID As String = "PASTE THE VERSION ID"



    ' URL of the "paychecker.aspx" script.
    ' This is where Moneybookers and PayPal orders are confirmed and processed.
    ' If you're not using Moneybookers or PayPal then you don't have to set this.
    Public Const CheckScript As String = "http://example.com/buy/paychecker.aspx"

    ' Where the user can buy your products
    Public Const BuyPage As String = "http://example.com/buy/payment.aspx"

    ' Thank you page (once payment is made, user is sent to this page)
    Public Const ThankYouPage As String = "http://example.com/buy/pay-thankyou.aspx"


    ' The logo to display on the PayPal checkout
    ' this site must be HTTPS or it won't display.
    ' Maximum size: 150px (width) by 50px (height)
    Public Const YourLogo As String = Nothing ' = "https://example.com/buy/toplogo.png"




    '==== Authorize.Net Config ====

    ' the API Login ID and Transaction Key must be replaced with valid values
    Public Const AuthNetLogin As String = "Paste API Login ID here"
    Public Const AuthNetTansKey As String = "Paste API Transaction Key here"
    Public Const AuthNetTest As Boolean = True




    '==== PayPal Config ====

    ' Use PayPal Sandbox (set to false on your live server)
    Public Const PayPalSandbox As Boolean = True

    ' Your Paypal email (where the money will be transferred to)
    Public Const PayPalEmail As String = "your@email.com"




    '==== Moneybookers Config ====

    ' Your Moneybookers email (where the money will be transferred to)
    Public Const MBEmail As String = "your@email.com"

    ' Set the "Secret Word" in your Moneybookers account
    ' on the "Merchant tools" page. Then set it here.
    Public Const SecretWord As String = "PASTE YOUR SECRET WORD HERE"




    ''' <summary>
    ''' Generate and send the product keys to your users.
    ''' </summary>
    ''' <param name="quantity">The number of licenses ordered.</param>
    ''' <param name="email">The email of the new customer.</param>
    ''' <param name="first">The first name of the customer.</param>
    ''' <param name="last">The last name of the customer.</param>
    Public Shared Sub SendPKeys(ByVal quantity As Integer, ByVal email As String, ByVal first As String, ByVal last As String)

        Dim pkeyList As String = String.Empty

        Try
            LimeLM.APIKey = PaymentSettings.LimeLM_ApiKey

            ' generate the product key (1 key with N activations)
            Dim resp As String = LimeLM.GeneratePKeys(PaymentSettings.LimeLM_VersionID, 1, quantity, email, Nothing, Nothing)

            ' parse the XML response
            Using reader As XmlReader = XmlReader.Create(New StringReader(resp))
                reader.ReadToFollowing("rsp")

                If (reader.Item("stat") <> "ok") Then
                    'TODO: handle errors how ever you need to (logging, throwing exceptions, etc.)
                    reader.ReadToFollowing("err")
                    Throw New Exception(("Failed to generate the product keys: " & reader.Item("msg")))
                    Return
                End If

                reader.ReadToFollowing("pkeys")

                Dim pkeys As New List(Of String)
                GetPKeys(reader, pkeys)

                For Each pkey As String In pkeys
                    pkeyList += pkey & vbNewLine
                Next
            End Using
        Catch ex As Exception
            ' failed to generate product keys
            ' TODO: handle the error somehow (log it, notify your support team, etc.)
            '       and delete the following "throw" statement
            Throw
        End Try




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

        ' email the product key to your users
        ' (only if we succeeded in getting product keys)
        If Not pkeyList Is String.Empty Then
            Dim message As New MailMessage

            message.To.Add(New MailAddress(email))
            message.Subject = ("Your " & AppName & " product key")

            Dim customerName As String = first

            If Not String.IsNullOrEmpty(last) Then
                customerName = (customerName & " " & last)
            End If

            message.Body = customerName & "," & vbNewLine + vbNewLine + "    Thank you for your order. Your product key is:" & vbNewLine + vbNewLine & pkeyList & vbNewLine + vbNewLine
            message.Body += "This product key is licensed for " & quantity
            message.Body += IIf((quantity > 1), " users", " user")
            message.Body += " of " + AppName + "." + vbNewLine + vbNewLine + "Thank you," + vbNewLine + CompanyName

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
