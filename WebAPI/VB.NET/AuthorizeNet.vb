Imports System.IO.Compression
Imports System.IO
Imports System.Net

Public Class AuthorizeNet

    Private Shared Login As String
    Private Shared TransKey As String
    Private Shared UseTest As Boolean

    Public Shared Sub SetLoginDetails(ByVal login As String, ByVal trans_key As String, ByVal test As Boolean)
        AuthorizeNet.Login = login
        AuthorizeNet.TransKey = trans_key
        AuthorizeNet.UseTest = test
    End Sub

    Public Shared Function BankTransfer(ByVal acct_num As String, ByVal routing As String, ByVal acct_type As String, ByVal bank_name As String, ByVal name_on_acct As String, ByVal amount As String, ByVal first_name As String, ByVal last_name As String, ByVal description As String, ByVal email As String) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("x_login", AuthorizeNet.Login))
        postData.Add(New KeyValuePair(Of String, Object)("x_tran_key", AuthorizeNet.TransKey))
        postData.Add(New KeyValuePair(Of String, Object)("x_version", "3.1"))
        postData.Add(New KeyValuePair(Of String, Object)("x_delim_data", "TRUE"))
        postData.Add(New KeyValuePair(Of String, Object)("x_delim_char", "|"))
        postData.Add(New KeyValuePair(Of String, Object)("x_relay_response", "FALSE"))
        postData.Add(New KeyValuePair(Of String, Object)("x_type", "AUTH_CAPTURE"))
        postData.Add(New KeyValuePair(Of String, Object)("x_method", "ECHECK"))
        postData.Add(New KeyValuePair(Of String, Object)("x_recurring_billing", "FALSE"))

        postData.Add(New KeyValuePair(Of String, Object)("x_bank_aba_code", routing))
        postData.Add(New KeyValuePair(Of String, Object)("x_bank_acct_num", acct_num))
        postData.Add(New KeyValuePair(Of String, Object)("x_bank_acct_type", acct_type))
        postData.Add(New KeyValuePair(Of String, Object)("x_bank_name", bank_name))
        postData.Add(New KeyValuePair(Of String, Object)("x_bank_acct_name", name_on_acct))
        postData.Add(New KeyValuePair(Of String, Object)("x_echeck_type", "WEB"))

        postData.Add(New KeyValuePair(Of String, Object)("x_amount", amount))
        postData.Add(New KeyValuePair(Of String, Object)("x_description", description))

        postData.Add(New KeyValuePair(Of String, Object)("x_first_name", first_name.Replace("|", "")))
        postData.Add(New KeyValuePair(Of String, Object)("x_last_name", last_name.Replace("|", "")))

        ' get ip address
        Dim ip As String = HttpContext.Current.Request.ServerVariables.Item("HTTP_CLIENT_IP")

        If String.IsNullOrEmpty(ip) Then
            ip = HttpContext.Current.Request.ServerVariables.Item("HTTP_X_FORWARDED_FOR")
        End If

        If String.IsNullOrEmpty(ip) Then
            ip = HttpContext.Current.Request.ServerVariables.Item("REMOTE_ADDR")
        End If

        If Not String.IsNullOrEmpty(ip) Then
            postData.Add(New KeyValuePair(Of String, Object)("x_customer_ip", ip))
        End If

        ' set the email
        If Not String.IsNullOrEmpty(email) Then
            postData.Add(New KeyValuePair(Of String, Object)("x_email", email))
        End If

        Dim words As String() = AuthorizeNet.PostRequest(postData).Split(New Char() {"|"c})

        ' if the charge was not successful, get the error
        If (words(0) = "1") Then
            Return Nothing
        End If

        Return IIf(words(3) <> Nothing, words(3), "Failed to make the bank transfer.")
    End Function

    Public Shared Function Charge(ByVal cc As String, ByVal exp_date As String, ByVal amount As String, ByVal first_name As String, ByVal last_name As String, ByVal zip As String, ByVal street_address As String, ByVal description As String, ByVal email As String) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("x_login", AuthorizeNet.Login))
        postData.Add(New KeyValuePair(Of String, Object)("x_tran_key", AuthorizeNet.TransKey))
        postData.Add(New KeyValuePair(Of String, Object)("x_version", "3.1"))
        postData.Add(New KeyValuePair(Of String, Object)("x_delim_data", "TRUE"))
        postData.Add(New KeyValuePair(Of String, Object)("x_delim_char", "|"))
        postData.Add(New KeyValuePair(Of String, Object)("x_relay_response", "FALSE"))
        postData.Add(New KeyValuePair(Of String, Object)("x_type", "AUTH_CAPTURE"))
        postData.Add(New KeyValuePair(Of String, Object)("x_method", "CC"))
        postData.Add(New KeyValuePair(Of String, Object)("x_card_num", cc))
        postData.Add(New KeyValuePair(Of String, Object)("x_exp_date", exp_date))
        postData.Add(New KeyValuePair(Of String, Object)("x_amount", amount))
        postData.Add(New KeyValuePair(Of String, Object)("x_description", description))
        postData.Add(New KeyValuePair(Of String, Object)("x_first_name", first_name.Replace("|", "")))
        postData.Add(New KeyValuePair(Of String, Object)("x_last_name", last_name.Replace("|", "")))
        postData.Add(New KeyValuePair(Of String, Object)("x_address", street_address.Replace("|", "")))
        postData.Add(New KeyValuePair(Of String, Object)("x_zip", zip.Replace("|", "")))

        ' get ip address
        Dim ip As String = HttpContext.Current.Request.ServerVariables.Item("HTTP_CLIENT_IP")

        If String.IsNullOrEmpty(ip) Then
            ip = HttpContext.Current.Request.ServerVariables.Item("HTTP_X_FORWARDED_FOR")
        End If

        If String.IsNullOrEmpty(ip) Then
            ip = HttpContext.Current.Request.ServerVariables.Item("REMOTE_ADDR")
        End If

        If Not String.IsNullOrEmpty(ip) Then
            postData.Add(New KeyValuePair(Of String, Object)("x_customer_ip", ip))
        End If

        ' set the email
        If Not String.IsNullOrEmpty(email) Then
            postData.Add(New KeyValuePair(Of String, Object)("x_email", email))
        End If

        Dim words As String() = AuthorizeNet.PostRequest(postData).Split(New Char() {"|"c})

        ' if the charge was not successful, get the error
        If (words(0) = "1") Then
            Return Nothing
        End If

        Return IIf(words(3) <> Nothing, words(3), "Failed to charge the credit card.")
    End Function

    Private Shared Function PostRequest(ByVal postData As List(Of KeyValuePair(Of String, Object))) As String
        ' This section takes the input fields and converts them to the proper format
        ' for an http post.
        Dim post_string As String = ""

        Dim kv As KeyValuePair(Of String, Object)
        For Each kv In postData

            If (kv.Value.GetType Is GetType(String())) Then
                Dim val As String

                For Each val In DirectCast(kv.Value, String())
                    post_string += kv.Key + "[]=" + HttpUtility.UrlEncode(val, Encoding.UTF8) + "&"
                Next
            Else
                post_string += kv.Key + "=" + HttpUtility.UrlEncode(CStr(kv.Value), Encoding.UTF8) + "&"
            End If
        Next

        ' trim the trailing ampersand
        post_string = Left(post_string, Len(post_string) - 1)

        ' create an HttpWebRequest object to communicate with Authorize.Net
        Dim objRequest As HttpWebRequest = DirectCast(WebRequest.Create(IIf(AuthorizeNet.UseTest, "https://test.authorize.net/gateway/transact.dll", "https://secure.authorize.net/gateway/transact.dll")), HttpWebRequest)
        objRequest.Method = "POST"
        objRequest.ContentLength = post_string.Length
        objRequest.ContentType = "application/x-www-form-urlencoded"
        objRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate")

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

        Return post_response
    End Function
End Class
