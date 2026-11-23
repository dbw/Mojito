using System;
using System.Web.UI;

[assembly: WebResource("LimeLM_API.order.png", "img/png")]
[assembly: WebResource("LimeLM_API.sprites.png", "img/png")]

namespace LimeLM_API
{
    public partial class payment : Page
    {
        public int TotalPaymentMethods;

        public bool PayPalSelected;
        public bool MoneybookersSelected;
        public bool AuthNetCCSelected;
        public bool AuthNetBankSelected;

        public CCProcessor CCProc;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only force HTTPS when using Autorize.Net and *not* in test mode
            if ((PaymentSettings.UseAuthorizeNetCC || PaymentSettings.UseAuthorizeNetBank) && !PaymentSettings.AuthNetTest && !Request.IsSecureConnection)
            {
                string redirectUrl = Request.Url.ToString().Replace("http:", "https:");
                Response.Redirect(redirectUrl);
                return;
            }

            if (PaymentSettings.UseAuthorizeNetCC || PaymentSettings.UseAuthorizeNetBank)
                CCProc = new CCProcessor();

            if (Request.HttpMethod == "POST" && CCProc != null)
            {
                CCProc.Process();

                if (!CCProc.Errors)
                {
                    Response.Redirect(PaymentSettings.ThankYouPage);
                    return;
                }

                if (CCProc.IsCCCharge)
                    AuthNetCCSelected = true;
                else
                    AuthNetBankSelected = true;
            }

            // Count the total payment methods (to hide/show the payment selector)
            // Also select the default payment. Reorder the if-statements to select a different default.

            if (PaymentSettings.UseAuthorizeNetCC)
            {
                if (TotalPaymentMethods == 0 && !AuthNetBankSelected)
                    AuthNetCCSelected = true;

                TotalPaymentMethods++;
            }

            if (PaymentSettings.UseAuthorizeNetBank)
            {
                if (TotalPaymentMethods == 0 && !AuthNetCCSelected)
                    AuthNetBankSelected = true;

                TotalPaymentMethods++;
            }

            if (PaymentSettings.UsePayPal)
            {
                if (TotalPaymentMethods == 0)
                    PayPalSelected = true;

                TotalPaymentMethods++;
            }

            if (PaymentSettings.UseMoneybookers)
            {
                if (TotalPaymentMethods == 0)
                    MoneybookersSelected = true;

                TotalPaymentMethods++;
            }

            if (TotalPaymentMethods == 0)
                throw new Exception("You must select at least one payment method in PaymentSettings.cs (e.g. UsePayPal=true;)");
        }
    }
}