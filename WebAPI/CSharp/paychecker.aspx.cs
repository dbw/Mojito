using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace LimeLM_API
{
    public partial class paychecker : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int quantity;
            string email, first, last;

            if (Request.QueryString["paypal"] != null)
            {
                // validate PayPal order
                if (!ValidatePP())
                    return;

                quantity = Convert.ToInt32(Request.Form["quantity"]);
                email = Request.Form["payer_email"];
                first = Request.Form["first_name"];
                last = Request.Form["last_name"];
            }
            else if (Request.QueryString["moneybookers"] != null)
            {
                // validate Moneybookers order
                if (!ValidateMB())
                    return;

                quantity = Convert.ToInt32(Request.Form["quantity"]);
                email = Request.Form["pay_from_email"];
                first = Request.Form["first_name"];
                last = Request.Form["last_name"];
            }
            else
                return;

            // generate the product key and email them to the user
            PaymentSettings.SendPKeys(quantity, email, first, last);
        }


        static string StringToMD5(string str)
        {
            MD5CryptoServiceProvider cryptHandler = new MD5CryptoServiceProvider();
            byte[] ba = cryptHandler.ComputeHash(Encoding.UTF8.GetBytes(str));

            StringBuilder hex = new StringBuilder(ba.Length * 2);

            foreach (byte b in ba)
                hex.AppendFormat("{0:X2}", b);

            return hex.ToString();
        }

        /// <summary>
        /// Validate Moneybookers order.
        /// </summary>
        /// <returns>True if the order is valid.</returns>
        bool ValidateMB()
        {
            // Status codes (see: 2.3.7 Detailed status description) :
            //-3 chargeback / -2 failed / 2 processed / 0 pending / -1 cancelled
            if (Request.Form["status"] != "2")
                return false;

            // make sure we're getting the money
            if (Request.Form["pay_to_email"] != PaymentSettings.MBEmail)
                return false;

            int quantity = Convert.ToInt32(Request.Form["quantity"]);

            // multiply Price * Quantity without using nasty lossy floats
            string expectedPrice = ((PaymentSettings.AppPrice[0] * quantity + ((PaymentSettings.AppPrice[1] * quantity) / 100)) + "." +
                                ((PaymentSettings.AppPrice[1] * quantity) % 100).ToString("00", CultureInfo.InvariantCulture));

            // validate the price
            if (PaymentSettings.Currency != Request.Form["currency"] || expectedPrice != Request.Form["amount"])
                return false;

            // Validate the Moneybookers signature
            string concatFields = Request.Form["merchant_id"]
                + Request.Form["transaction_id"]
                + StringToMD5(PaymentSettings.SecretWord)
                + Request.Form["mb_amount"]
                + Request.Form["mb_currency"]
                + Request.Form["status"];

            return Request.Form["md5sig"] == StringToMD5(concatFields);
        }

        /// <summary>
        /// Validate the order with PayPal's servers
        /// </summary>
        /// <returns>True if the order is valid.</returns>
        bool ValidatePP()
        {
            // Only send a product key if the payment has completed successfully
            if (Request.Form["payment_status"] != "Completed")
                return false;

            // make sure we're getting the money
            if (Request.Form["receiver_email"] != PaymentSettings.PayPalEmail)
                return false;

            int quantity = Convert.ToInt32(Request.Form["quantity"]);

            // multiply Price * Quantity without using nasty lossy floats
            string expectedPrice = ((PaymentSettings.AppPrice[0] * quantity + ((PaymentSettings.AppPrice[1] * quantity) / 100)) + "." +
                                ((PaymentSettings.AppPrice[1] * quantity) % 100).ToString("00", CultureInfo.InvariantCulture));

            // validate the price
            if (PaymentSettings.Currency != Request.Form["mc_currency"] || expectedPrice != Request.Form["mc_gross"])
                return false;


            // form the POST string to validate with PayPal's servers
            StringBuilder postString = new StringBuilder();

            for (int i = 0; i < Request.Form.AllKeys.Length; i++)
            {
                String key = Request.Form.AllKeys[i];

                // exclude hidden ASP.NET hidden fields beginning in "__"
                if (key.Substring(0, 2) != "__")
                {
                    postString.Append(HttpUtility.UrlEncode(key));
                    postString.Append("=");
                    postString.Append(HttpUtility.UrlEncode(Request.Form[i]));
                    postString.Append("&");
                }
            }

            postString.Append("cmd=_notify-validate");


            string post_string = postString.ToString();

            // create an HttpWebRequest object to communicate with PayPal
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(PaymentSettings.PayPalSandbox ? "https://www.sandbox.paypal.com/cgi-bin/webscr" : "https://www.paypal.com/cgi-bin/webscr");
            objRequest.Method = "POST";
            objRequest.ContentLength = post_string.Length;
            objRequest.ContentType = "application/x-www-form-urlencoded";
            objRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            // post data is sent as a stream
            StreamWriter myWriter = new StreamWriter(objRequest.GetRequestStream());
            myWriter.Write(post_string);
            myWriter.Close();

            // returned values are returned as a stream, then read into a string
            string post_response;
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

            Stream responseStream = objResponse.GetResponseStream();

            if (objResponse.ContentEncoding.ToLower().Contains("gzip"))
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            else if (objResponse.ContentEncoding.ToLower().Contains("deflate"))
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);

            using (StreamReader streamReader = new StreamReader(responseStream))
            {
                post_response = streamReader.ReadToEnd();
            }

            // PayPal returns "VERIFIED" if it's a valid order
            return post_response.Contains("VERIFIED");
        }
    }
}