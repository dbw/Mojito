Imports System.Security.Cryptography
Imports System.Globalization
Imports System.IO.Compression
Imports System.IO
Imports System.Net

Public Class paychecker
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim quantity As Integer
        Dim email As String
        Dim first As String
        Dim last As String

        If (Not Request.QueryString.Item("paypal") Is Nothing) Then

            ' validate PayPal order
            If Not ValidatePP() Then
                Return
            End If

            quantity = Convert.ToInt32(Request.Form.Item("quantity"))
            email = Request.Form.Item("payer_email")
            first = Request.Form.Item("first_name")
            last = Request.Form.Item("last_name")
        ElseIf (Not Request.QueryString.Item("moneybookers") Is Nothing) Then

            ' validate Moneybookers order
            If Not ValidateMB() Then
                Return
            End If

            quantity = Convert.ToInt32(Request.Form.Item("quantity"))
            email = Request.Form.Item("pay_from_email")
            first = Request.Form.Item("first_name")
            last = Request.Form.Item("last_name")
        Else
            Return
        End If

        ' generate the product key and email them to the user
        PaymentSettings.SendPKeys(quantity, email, first, last)
    End Sub

    Private Shared Function StringToMD5(ByVal str As String) As String
        Dim ba As Byte() = New MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(str))

        Dim hex As New StringBuilder((ba.Length * 2))

        For Each b As Byte In ba
            hex.AppendFormat("{0:X2}", b)
        Next

        Return hex.ToString
    End Function

    ''' <summary>
    ''' Validate Moneybookers order.
    ''' </summary>
    ''' <returns>True if the order is valid.</returns>
    Private Function ValidateMB() As Boolean
        ' Status codes (see: 2.3.7 Detailed status description) :
        '-3 chargeback / -2 failed / 2 processed / 0 pending / -1 cancelled
        If (Request.Form.Item("status") <> "2") Then
            Return False
        End If

        ' make sure we're getting the money
        If (Request.Form.Item("pay_to_email") <> PaymentSettings.MBEmail) Then
            Return False
        End If

        Dim quantity As Integer = Convert.ToInt32(Request.Form.Item("quantity"))

        ' multiply Price * Quantity without using nasty lossy floats
        Dim expectedPrice As String = (((PaymentSettings.AppPrice(0) * quantity) + ((PaymentSettings.AppPrice(1) * quantity) \ 100)) & "." & ((PaymentSettings.AppPrice(1) * quantity) Mod 100).ToString("00", CultureInfo.InvariantCulture))

        ' validate the price
        If ((PaymentSettings.Currency <> Request.Form.Item("currency")) OrElse (expectedPrice <> Request.Form.Item("amount"))) Then
            Return False
        End If

        ' Validate the Moneybookers signature
        Dim concatFields As String = Request.Form.Item("merchant_id") &
            Request.Form.Item("transaction_id") &
            paychecker.StringToMD5(PaymentSettings.SecretWord) &
            Request.Form.Item("mb_amount") &
            Request.Form.Item("mb_currency") &
            Request.Form.Item("status")

        Return (Request.Form.Item("md5sig") = paychecker.StringToMD5(concatFields))
    End Function

    ''' <summary>
    ''' Validate the order with PayPal's servers
    ''' </summary>
    ''' <returns>True if the order is valid.</returns>
    Private Function ValidatePP() As Boolean
        ' Only send a product key if the payment has completed successfully
        If (Request.Form.Item("payment_status") <> "Completed") Then
            Return False
        End If

        ' make sure we're getting the money
        If (Request.Form.Item("receiver_email") <> PaymentSettings.PayPalEmail) Then
            Return False
        End If

        Dim quantity As Integer = Convert.ToInt32(Request.Form.Item("quantity"))

        ' multiply Price * Quantity without using nasty lossy floats
        Dim expectedPrice As String = (((PaymentSettings.AppPrice(0) * quantity) + ((PaymentSettings.AppPrice(1) * quantity) \ 100)) & "." & ((PaymentSettings.AppPrice(1) * quantity) Mod 100).ToString("00", CultureInfo.InvariantCulture))

        ' validate the price
        If ((PaymentSettings.Currency <> Request.Form.Item("mc_currency")) OrElse (expectedPrice <> Request.Form.Item("mc_gross"))) Then
            Return False
        End If

        ' form the POST string to validate with PayPal's servers
        Dim postString As New StringBuilder

        For i As Integer = 0 To Request.Form.AllKeys.Length - 1
            Dim key As String = Request.Form.AllKeys(i)

            ' exclude hidden ASP.NET hidden fields beginning in "__"
            If (key.Substring(0, 2) <> "__") Then
                postString.Append(HttpUtility.UrlEncode(key))
                postString.Append("=")
                postString.Append(HttpUtility.UrlEncode(Request.Form.Item(i)))
                postString.Append("&")
            End If
        Next i

        postString.Append("cmd=_notify-validate")

        ' create an HttpWebRequest object to communicate with PayPal
        Dim post_string As String = postString.ToString
        Dim objRequest As HttpWebRequest = DirectCast(WebRequest.Create(IIf(PaymentSettings.PayPalSandbox, "https://www.sandbox.paypal.com/cgi-bin/webscr", "https://www.paypal.com/cgi-bin/webscr")), HttpWebRequest)
        objRequest.Method = "POST"
        objRequest.ContentLength = post_string.Length
        objRequest.ContentType = "application/x-www-form-urlencoded"

        ' post data is sent as a stream
        Dim myWriter As New StreamWriter(objRequest.GetRequestStream)
        myWriter.Write(post_string)
        myWriter.Close()

        ' returned values are returned as a stream, then read into a string
        Dim post_response As String
        Dim objResponse As HttpWebResponse = DirectCast(objRequest.GetResponse, HttpWebResponse)

        Dim responseStream As Stream = objResponse.GetResponseStream
        If objResponse.ContentEncoding.ToLower.Contains("gzip") Then
            responseStream = New GZipStream(responseStream, CompressionMode.Decompress)
        ElseIf objResponse.ContentEncoding.ToLower.Contains("deflate") Then
            responseStream = New DeflateStream(responseStream, CompressionMode.Decompress)
        End If

        Using streamReader As StreamReader = New StreamReader(responseStream)
            post_response = streamReader.ReadToEnd
        End Using

        ' PayPal returns "VERIFIED" if it's a valid order
        Return post_response.Contains("VERIFIED")
    End Function
End Class