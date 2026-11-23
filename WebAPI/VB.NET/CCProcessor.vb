Imports System.Globalization

Public Class CCProcessor
    Public Errors As Boolean

    Public FirstName As String
    Public FirstNameErr As Boolean

    Public LastName As String
    Public LastNameErr As Boolean

    Public Email As String
    Public EmailErr As Boolean

    Public Quantity As Integer = 1
    Public QuantityErr As Boolean

    Public IsCCCharge As Boolean


    ' Credit card fields / errors
    Public CCNumber As String
    Public CCNumErr As Boolean
    Public CCNumErrMsg As String

    Public CCAddress As String
    Public CCAddrErr As Boolean

    Public CCZip As String
    Public CCZipErr As Boolean

    Public CCExpiresM As Integer
    Public CCExpiresY As Integer
    Public CCExpErr As Boolean


    ' Bank transfer fields / errors
    Public BankErrMsg As String

    Public BankAcctName As String
    Public BankAcctNameErr As Boolean

    Public BankAcctNum As String
    Public BankAcctNumErr As Boolean

    Public BankAcctType As String
    Public BankAcctTypeErr As Boolean

    Public BankName As String
    Public BankNameErr As Boolean

    Public BankRouteNum As String
    Public BankRouteNumErr As Boolean

    Public Function Process() As Boolean
        Dim req As HttpRequest = HttpContext.Current.Request

        FirstName = If(req.Form.Item("first_name") Is Nothing, String.Empty, req.Form.Item("first_name").Trim)
        LastName = If(req.Form.Item("last_name") Is Nothing, String.Empty, req.Form.Item("last_name").Trim)
        Email = If(req.Form.Item("email") Is Nothing, String.Empty, req.Form.Item("email").Trim)

        Quantity = Convert.ToInt32(req.Form.Item("quantity"))

        Dim LongAccountType As String = Nothing

        If (FirstName = String.Empty) Then
            FirstNameErr = True
            Errors = True
        End If

        If (LastName = String.Empty) Then
            LastNameErr = True
            Errors = True
        End If

        If (Email = String.Empty) Then
            EmailErr = True
            Errors = True
        End If

        IsCCCharge = req.Form.Item("mf_cardtype") = "Card"

        If (IsCCCharge) Then
            CCNumber = Regex.Replace(IIf(req.Form.Item("cc_num") <> Nothing, req.Form.Item("cc_num"), String.Empty), "[\D]", "")
            CCAddress = If(req.Form.Item("cc_adr") Is Nothing, String.Empty, req.Form.Item("cc_adr").Trim)
            CCZip = If(req.Form.Item("cc_zip") Is Nothing, String.Empty, req.Form.Item("cc_zip").Trim)
            CCExpiresM = Convert.ToInt32(req.Form.Item("cc_expires_m"))
            CCExpiresY = Convert.ToInt32(req.Form.Item("cc_expires_y"))

            If (CCZip = String.Empty) Then
                CCZipErr = True
                Errors = True
            End If

            If (CCAddress = String.Empty) Then
                CCAddrErr = True
                Errors = True
            End If

            If ((CCExpiresM < 1) OrElse (CCExpiresM > 12)) Then
                CCExpErr = True
                Errors = True
            End If

            Dim dat As DateTime = DateTime.Now

            ' check for expired ccs
            If ((CCExpiresY < dat.Year) OrElse ((CCExpiresY = dat.Year) AndAlso (CCExpiresM < dat.Month))) Then
                CCExpErr = True
                Errors = True
            End If

            ' validate credit card for typos
            ' (this doesn't guarantee the CC is real - just that it passes the Luhn algorithm)

            If Not ((CCNumber.Length <> 0) AndAlso CCProcessor.IsLuhnValid(CCNumber)) Then
                CCNumErrMsg = "Enter a valid credit card number"
                CCNumErr = True
                Errors = True
            ElseIf Not CCProcessor.IsAcceptedCC(CCNumber) Then
                CCNumErrMsg = "We only accept " & AcceptedCCs
                CCNumErr = True
                Errors = True
            End If
        Else ' Bank transfer

            BankAcctName = If(req.Form.Item("bank_acct_name") Is Nothing, String.Empty, req.Form.Item("bank_acct_name").Trim)
            BankAcctType = If(req.Form.Item("bank_acct_type") Is Nothing, String.Empty, req.Form.Item("bank_acct_type").Trim)
            BankName = If(req.Form.Item("bank_name") Is Nothing, String.Empty, req.Form.Item("bank_name").Trim)
            BankAcctNum = Regex.Replace(IIf(req.Form.Item("bank_acct_num") <> Nothing, req.Form.Item("bank_acct_num"), String.Empty), "[\D]", "")
            BankRouteNum = Regex.Replace(IIf(req.Form.Item("bank_route_num") <> Nothing, req.Form.Item("bank_route_num"), String.Empty), "[\D]", "")

            If (BankAcctName = String.Empty) Then
                BankAcctNameErr = True
                Errors = True
            End If

            Select Case BankAcctType
                Case "C"
                    LongAccountType = "CHECKING"
                Case "S"
                    LongAccountType = "SAVINGS"
                Case Else
                    BankAcctTypeErr = True
                    Errors = True
                    BankAcctType = Nothing
            End Select

            If (BankName = String.Empty) Then
                BankNameErr = True
                Errors = True
            End If

            If Not ((BankRouteNum.Length = 9) AndAlso CCProcessor.IsABARouteValid(BankRouteNum)) Then
                BankRouteNumErr = True
                Errors = True
            End If

            If (BankAcctNum.Length = 0) Then
                BankAcctNumErr = True
                Errors = True
            End If
        End If

        If Quantity < 1 Then
            Errors = True
            QuantityErr = True
            Quantity = 1
        End If

        If Errors Then
            Return False
        End If

        ' format in the XXX.YY format
        Dim amount As String = (((PaymentSettings.AppPrice(0) * Quantity) + ((PaymentSettings.AppPrice(1) * Quantity) \ 100)) & "." & ((PaymentSettings.AppPrice(1) * Quantity) Mod 100).ToString("00", CultureInfo.InvariantCulture))

        ' set the Auth.Net details
        AuthorizeNet.SetLoginDetails(PaymentSettings.AuthNetLogin, PaymentSettings.AuthNetTansKey,
                                     PaymentSettings.AuthNetTest)

        ' Charge or transfer
        If IsCCCharge Then
            Dim result As String = AuthorizeNet.Charge(CCNumber, (CCExpiresM.ToString("00", CultureInfo.InvariantCulture) & "/" & CCExpiresY),
                                                       amount, FirstName, LastName, CCZip, CCAddress, PaymentSettings.AppName, Email)

            If Not (result Is Nothing) Then
                CCNumErrMsg = result
                CCNumErr = True
                Errors = True
                Return False
            End If
        Else
            Dim result As String = AuthorizeNet.BankTransfer(BankAcctNum, BankRouteNum, LongAccountType, BankName, BankAcctName,
                                                             amount, FirstName, LastName,
                                                             PaymentSettings.AppName, Email)

            If Not (result Is Nothing) Then
                BankErrMsg = result
                Errors = True
                Return False
            End If
        End If

        ' generate the product key and email them to the user
        PaymentSettings.SendPKeys(Quantity, Email, FirstName, LastName)

        ' return true if no errors have occurred
        Return True
    End Function

    Private Shared Function IsLuhnValid(ByVal number As String) As Boolean
        Dim DELTAS As Integer() = New Integer() {0, 1, 2, 3, 4, -4, -3, -2, -1, 0}
        Dim checksum As Integer = 0
        Dim chars As Char() = number.ToCharArray
        Dim i As Integer = (chars.Length - 1)
        Do While (i > -1)
            Dim j As Integer = Val(chars(i))
            checksum = (checksum + j)
            If (((i - chars.Length) Mod 2) = 0) Then
                checksum = (checksum + DELTAS(j))
            End If
            i -= 1
        Loop

        Return ((checksum Mod 10) = 0)
    End Function

    Private Shared Function IsAcceptedCC(ByVal cc As String) As Boolean
        Select Case cc.Chars(0)
            Case "1"c, "2"c ' JCB starting with 1 or 2
                Return Regex.IsMatch(cc, "^(?:2131|1800)\d{11}$")

            Case "3"c ' American Express or JCB
                Return (Regex.IsMatch(cc, "^3[47][0-9]{13}$") OrElse Regex.IsMatch(cc, "^(?:35)\d{14}$"))

            Case "4"c ' Visa
                Return Regex.IsMatch(cc, "^4[0-9]{12}(?:[0-9]{3})?$")

            Case "5"c ' MasterCard
                Return Regex.IsMatch(cc, "^5[1-5][0-9]{14}$")

            Case "6"c ' Discover
                Return Regex.IsMatch(cc, "^6(?:011|5[0-9]{2})[0-9]{12}$")
        End Select

        Return False
    End Function

    Private Shared Function IsABARouteValid(ByVal routeNum As String) As Boolean
        Dim r As Char() = routeNum.ToCharArray
        Return (Val(r(8)) = ((((7 * ((Val(r(0)) + Val(r(3))) + Val(r(6)))) + (3 * ((Val(r(1)) + Val(r(4))) + Val(r(7))))) + (9 * (Val(r(2)) + Val(r(5))))) Mod 10))
    End Function

    Public Const AcceptedCCs As String = "Visa, American Express, MasterCard, Discover, and JCB"
End Class
