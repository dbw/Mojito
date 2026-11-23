Imports System.IO
Imports System.Net
Imports System.IO.Compression

Public Class LimeLM

    ''' <summary>
    ''' Your API key to accessing LimeLM.
    ''' </summary>
    Public Shared Property APIKey As String
        Get
            Return m_APIKey
        End Get

        Set(ByVal value As String)
            m_APIKey = value
        End Set
    End Property

    Private Shared m_APIKey As String

    ' NOTE: If you're using the self-hosted version of LimeLM (that is,
    '       LimeLM running on your own servers), then replace the URL with your own.

    ' Almost all users should leave this line unchanged.
    Const post_url As String = "https://wyday.com/limelm/api/rest/"

    ''' <summary>
    ''' Add a new license feature for a particular version. Note that if you want to set the feature values for existing product keys, use the SetDetails() function.
    ''' </summary>
    ''' <param name="versionID">The version ID to search for the product keys.</param>
    ''' <param name="name">The name of the feature.</param>
    ''' <param name="required">Whether this feature will be required to be filled out when a new product key is created.</param>
    ''' <param name="type">The type of the feature. Possible values are "string", "date_time", "int"</param>
    ''' <returns>XML block with either an error or the returned feature id.</returns>
    Public Shared Function AddFeature(ByVal versionID As String, ByVal name As String, ByVal required As Boolean, ByVal type As String) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.feature.add"))
        postData.Add(New KeyValuePair(Of String, Object)("version_id", versionID))
        postData.Add(New KeyValuePair(Of String, Object)("name", name))
        postData.Add(New KeyValuePair(Of String, Object)("required", IIf(required, "true", "false")))

        If (Not type Is Nothing) Then
            postData.Add(New KeyValuePair(Of String, Object)("type", type))
        End If

        Return PostRequest(postData)
    End Function

    ''' <summary>
    ''' Delete an existing license feature. If you delete a feature it is removed from all the product keys.
    ''' </summary>
    ''' <param name="featureID">The id of the feature you'll be deleting.</param>
    ''' <returns>XML block with either an error or the returned success block.</returns>
    Public Shared Function DeleteFeature(ByVal featureID As String) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.feature.delete"))
        postData.Add(New KeyValuePair(Of String, Object)("feature_id", featureID))

        Return PostRequest(postData)
    End Function

    ''' <summary>
    ''' Edit an existing license feature. Note that if you want to set the feature values for existing product keys, use the SetDetails() function.
    ''' </summary>
    ''' <param name="featureID">The id of the feature you'll be editing.</param>
    ''' <param name="name">The new name of the feature.</param>
    ''' <param name="required">Whether this feature will be required to be filled out when a new product key is created.</param>
    ''' <returns>XML block with either an error or the returned success block.</returns>
    Public Shared Function EditFeature(ByVal featureID As String, ByVal name As String, ByVal required? As Boolean) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.feature.edit"))
        postData.Add(New KeyValuePair(Of String, Object)("feature_id", featureID))

        If (Not name Is Nothing) Then
            postData.Add(New KeyValuePair(Of String, Object)("name", name))
        End If

        If (Not required Is Nothing) Then
            postData.Add(New KeyValuePair(Of String, Object)("required", IIf(required, "true", "false")))
        End If

        Return PostRequest(postData)
    End Function

    ''' <summary>
    ''' Finds the product keys matching an email address.
    ''' </summary>
    ''' <param name="versionID">The version ID to search for the product keys.</param>
    ''' <param name="email">The email associated with the product keys.</param>
    ''' <returns>XML block with either an error or the returned pkeys.</returns>
    Public Shared Function FindPKey(ByVal versionID As String, ByVal email As String) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.pkey.find"))
        postData.Add(New KeyValuePair(Of String, Object)("version_id", versionID))
        postData.Add(New KeyValuePair(Of String, Object)("email", email))

        Return PostRequest(postData)
    End Function

    ''' <summary>
    ''' Generate product keys.
    ''' </summary>
    ''' <param name="version_id">The version ID to generate the product keys in.</param>
    ''' <param name="num_keys">The number of product keys to generate.</param>
    ''' <param name="num_acts">The number of activations per product key.</param>
    ''' <param name="email">The email to associate with the product keys. Set to NULL for no email.</param>
    ''' <param name="feature_names">A string array list of feature names to set. Set to null if you don't want to set any features.</param>
    ''' <param name="feature_values">A string array list of feature values to set. Set to null if you don't want to set any features.</param>
    ''' <returns>XML block with either an error or the returned pkeys.</returns>
    Public Shared Function GeneratePKeys(ByVal version_id As String, ByVal num_keys As Integer, ByVal num_acts As Integer, ByVal email As String, ByVal feature_names As String(), ByVal feature_values As String()) As String
        If (num_keys < 1) Then
            num_keys = 1
        End If
        If (num_acts < 1) Then
            num_acts = 1
        End If

        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.pkey.generate"))
        postData.Add(New KeyValuePair(Of String, Object)("version_id", version_id))
        postData.Add(New KeyValuePair(Of String, Object)("num_keys", num_keys.ToString))
        postData.Add(New KeyValuePair(Of String, Object)("num_acts", num_acts.ToString))

        If (Not email Is Nothing) Then
            postData.Add(New KeyValuePair(Of String, Object)("email", email))
        End If

        If (Not feature_names Is Nothing) Then
            postData.Add(New KeyValuePair(Of String, Object)("feature_name", feature_names))
            postData.Add(New KeyValuePair(Of String, Object)("feature_value", feature_values))
        End If

        Return PostRequest(postData)
    End Function


    ''' <summary>
    ''' Gets the details for a product key.
    ''' </summary>
    ''' <param name="pkey_id">The ID of the product key.</param>
    ''' <returns>XML block with either an error or the details of the product key.</returns>
    Public Shared Function GetDetails(ByVal pkey_id As String) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.pkey.getDetails"))
        postData.Add(New KeyValuePair(Of String, Object)("pkey_id", pkey_id))

        Return PostRequest(postData)
    End Function

    ''' <summary>
    ''' Get the ID for a product key.
    ''' </summary>
    ''' <param name="version_id">The version ID to look for.</param>
    ''' <param name="pkey">The product key.</param>
    ''' <returns>XML block with either an error or the product key ID.</returns>
    Public Shared Function GetPKeyID(ByVal version_id As String, ByVal pkey As String) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.pkey.getID"))
        postData.Add(New KeyValuePair(Of String, Object)("version_id", version_id))
        postData.Add(New KeyValuePair(Of String, Object)("pkey", pkey))

        Return PostRequest(postData)
    End Function

    ''' <summary>
    ''' Manually activates using the contents of an activation request file.
    ''' </summary>
    ''' <param name="xml_act_request">The contents of the XML activation request file.</param>
    ''' <returns>XML block with either an error or the XML Activation response block.</returns>
    Public Shared Function ManualActivation(ByVal xml_act_request As String) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.pkey.manualActivation"))
        postData.Add(New KeyValuePair(Of String, Object)("act_req_xml", xml_act_request))

        Return PostRequest(postData)
    End Function
    
    ''' <summary>
    ''' Sets the details for a product key. This include the number of activations, email, and any license features.
    ''' </summary>
    ''' <param name="pkey_id">The ID for the product key. Use GetPKeyID to get the pkey ID from a product key.</param>
    ''' <param name="num_acts">If non-zero this changes the number of activations allowed for the product key.</param>
    ''' <param name="email">If this is non-null this changes the email address associated with the product key.</param>
    ''' <param name="feature_names">A string array list of feature names to set. Set to null if you don't want to set any features.</param>
    ''' <param name="feature_values">A string array list of feature values to set. Set to null if you don't want to set any features.</param>
    ''' <returns>XML block with either an error or stating it was successful.</returns>
    Public Shared Function SetDetails(ByVal pkey_id As String, ByVal num_acts As Integer, ByVal email As String, ByVal feature_names As String(), ByVal feature_values As String()) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.pkey.setDetails"))
        postData.Add(New KeyValuePair(Of String, Object)("pkey_id", pkey_id))

        If (num_acts <> 0) Then
            postData.Add(New KeyValuePair(Of String, Object)("num_acts", num_acts.ToString))
        End If

        If (Not email Is Nothing) Then
            postData.Add(New KeyValuePair(Of String, Object)("email", email))
        End If

        If (Not feature_names Is Nothing) Then
            postData.Add(New KeyValuePair(Of String, Object)("feature_name", feature_names))
            postData.Add(New KeyValuePair(Of String, Object)("feature_value", feature_values))
        End If

        Return PostRequest(postData)
    End Function

    ''' <summary>
    ''' Generates a trial extension for a version ID.
    ''' </summary>
    ''' <param name="version_id">The version ID to generate the trial extension in.</param>
    ''' <param name="is_online">Is the trial extension a small online extension or the larger offline extension.</param>
    ''' <param name="length">The length of the trial extension in days.</param>
    ''' <param name="expires">When the trial expires in the form YYYY-MM-DD</param>
    ''' <param name="customer_id">An optional customer ID (e.g. email) to associate with the trial extension.</param>
    ''' <param name="max_uses">The maximum number of uses for the online trial extension.</param>
    ''' <returns>XML block with either an error or the returned trial extension.</returns>
    Public Shared Function GenerateTrialExtension(ByVal version_id As String, ByVal is_online As Boolean, ByVal length As UInt32, ByVal expires As String, ByVal customer_id As String, ByVal max_uses As UInt32) As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.trialExtension.generate"))
        postData.Add(New KeyValuePair(Of String, Object)("version_id", version_id))
        postData.Add(New KeyValuePair(Of String, Object)("is_online", IIf(is_online, "true", "false")))
        postData.Add(New KeyValuePair(Of String, Object)("length", length.ToString))
        postData.Add(New KeyValuePair(Of String, Object)("expires", expires))

        If is_online Then
            postData.Add(New KeyValuePair(Of String, Object)("max_uses", max_uses.ToString))
        End If

        If (Not customer_id Is Nothing) Then
            postData.Add(New KeyValuePair(Of String, Object)("customer_id", customer_id))
        End If

        Return PostRequest(postData)
    End Function

    ''' <summary>
    ''' A simple test to confirm your API key is correct.
    ''' </summary>
    ''' <returns>XML block with either an error or the echoed input.</returns>
    Public Shared Function TestEcho() As String
        Dim postData As New List(Of KeyValuePair(Of String, Object))
        postData.Add(New KeyValuePair(Of String, Object)("method", "limelm.test.echo"))

        Return PostRequest(postData)
    End Function


    Private Shared Function PostRequest(ByVal postData As List(Of KeyValuePair(Of String, Object))) As String
        If (APIKey Is Nothing) Then
            Throw New Exception("You must specify your LimeLM API key.")
        End If

        postData.Add(New KeyValuePair(Of String, Object)("api_key", APIKey))

        ' This section takes the input fields and converts them to the proper format
        ' for an http post.  For example: "method=limelm.pkey.find&version_id=100"
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

        ' create an HttpWebRequest object to communicate with LimeLM
        Dim objRequest As HttpWebRequest = DirectCast(WebRequest.Create(post_url), HttpWebRequest)
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
