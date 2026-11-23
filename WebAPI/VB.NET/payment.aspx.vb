
<Assembly: WebResource("LimeLM_API.order.png", "img/png")> 
<Assembly: WebResource("LimeLM_API.sprites.png", "img/png")> 

Public Class payment
    Inherits System.Web.UI.Page

    Public TotalPaymentMethods As Integer

    Public PayPalSelected As Boolean
    Public MoneybookersSelected As Boolean
    Public AuthNetCCSelected As Boolean
    Public AuthNetBankSelected As Boolean

    Public CCProc As CCProcessor

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Only force HTTPS when using Autorize.Net and *not* in test mode
        If ((PaymentSettings.UseAuthorizeNetCC Or PaymentSettings.UseAuthorizeNetBank) And Not PaymentSettings.AuthNetTest And Not Request.IsSecureConnection) Then
            Dim redirectUrl As String = MyBase.Request.Url.ToString.Replace("http:", "https:")
            MyBase.Response.Redirect(redirectUrl)
            Return
        End If

        If (PaymentSettings.UseAuthorizeNetCC OrElse PaymentSettings.UseAuthorizeNetBank) Then
            Me.CCProc = New CCProcessor
        End If


        If ((MyBase.Request.HttpMethod = "POST") AndAlso (Not Me.CCProc Is Nothing)) Then
            CCProc.Process()

            If Not CCProc.Errors Then
                Response.Redirect(PaymentSettings.ThankYouPage)
                Return
            End If

            If CCProc.IsCCCharge Then
                AuthNetCCSelected = True
            Else
                AuthNetBankSelected = True
            End If
        End If

        ' Count the total payment methods (to hide/show the payment selector)
        ' Also select the default payment. Reorder the if-statements to select a different default.

        If PaymentSettings.UseAuthorizeNetCC Then
            If Not ((TotalPaymentMethods <> 0) OrElse AuthNetBankSelected) Then
                AuthNetCCSelected = True
            End If

            TotalPaymentMethods += 1
        End If

        If PaymentSettings.UseAuthorizeNetBank Then
            If Not ((TotalPaymentMethods <> 0) OrElse AuthNetCCSelected) Then
                AuthNetBankSelected = True
            End If

            TotalPaymentMethods += 1
        End If

        If PaymentSettings.UsePayPal Then
            If (TotalPaymentMethods = 0) Then
                PayPalSelected = True
            End If

            TotalPaymentMethods += 1
        End If

        If PaymentSettings.UseMoneybookers Then
            If (TotalPaymentMethods = 0) Then
                MoneybookersSelected = True
            End If

            TotalPaymentMethods += 1
        End If

        If (TotalPaymentMethods = 0) Then
            Throw New Exception("You must select at least one payment method in PaymentSettings.cs (e.g. UsePayPal = True)")
        End If
    End Sub

End Class