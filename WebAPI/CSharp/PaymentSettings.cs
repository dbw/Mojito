using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Xml;

namespace LimeLM_API
{
    public static class PaymentSettings
    {
        //TODO: Set the following constants. All URLs must be publicly accessible.
        // See http://wyday.com/limelm/help/how-to-generate-product-keys-after-order/
        // to learn how to configure the payment form and set everything up.

        //==== General Config ====

        // Set any of the following 4 boolean values to either
        // true or false based on whether you're using that
        // particular payment method. "payment.aspx" automatically
        // shows or hides the payment method based on these boolean
        // values.

        // If you set a payment setting to true then you'll also
        // have to configure the payment settings for that
        // particular payment method below.
        public const bool UseAuthorizeNetCC = true;
        public const bool UseAuthorizeNetBank = true;
        public const bool UseMoneybookers = true;
        public const bool UsePayPal = true;


        // dollars, cents
        public static int[] AppPrice = new[] { 69, 99 };
        public const string Currency = "USD";

        // The currency symbol that the end-user will see.
        // If you're not using "$" then you should use
        // the HTML-entity equivalent of the symbol.
        // Your customers will see the symbol.

        // € = &euro;
        // £ = &pound;
        // ¥ = &yen;
        // etc.
        public const string CurrencySign = "$";

        public const string CompanyName = "YourCompany";
        public const string AppName = "YourApp";

        // api key found in http://wyday.com/limelm/settings/
        public const string LimeLM_ApiKey = "PASTE THE API KEY HERE";

        // the version id is found in the url.
        // For example http://wyday.com/limelm/version/100/   the version id is "100".
        public const string LimeLM_VersionID = "PASTE THE VERSION ID";



        // URL of the "paychecker.aspx" script.
        // This is where Moneybookers and PayPal orders are confirmed and processed.
        // If you're not using Moneybookers or PayPal then you don't have to set this.
        public const string CheckScript = "http://example.com/buy/paychecker.aspx";

        // Where the user can buy your products
        public const string BuyPage = "http://example.com/buy/payment.aspx";

        // Thank you page (once payment is made, user is sent to this page)
        public const string ThankYouPage = "http://example.com/buy/pay-thankyou.aspx";

        // The logo to display on the PayPal / Moneybookers checkout
        // this site must be HTTPS or it won't display.
        // Maximum size (PayPal): 150px (width) by 50px (height)
        // Maximum size (Moneybookers): 200px (width) by 50px (height)
        public const string YourLogo = null; // = "https://example.com/buy/toplogo.png";




        //==== Authorize.Net Config ====

        // the API Login ID and Transaction Key must be replaced with valid values
        public const string AuthNetLogin = "Paste API Login ID here";
        public const string AuthNetTansKey = "Paste API Transaction Key here";
        public const bool AuthNetTest = true;




        //==== PayPal Config ====

        // Use PayPal Sandbox (set to false on your live server)
        public const bool PayPalSandbox = true;

        // Your Paypal email (where the money will be transferred to)
        public const string PayPalEmail = "your@email.com";




        //==== Moneybookers Config ====

        // Your Moneybookers email (where the money will be transferred to)
        public const string MBEmail = "your@email.com";

        // Set the "Secret Word" in your Moneybookers account
        // on the "Merchant tools" page. Then set it here.
        public const string SecretWord = "PASTE YOUR SECRET WORD HERE";




        /// <summary>
        /// Generate and send the product keys to your users.
        /// </summary>
        /// <param name="quantity">The number of licenses ordered.</param>
        /// <param name="email">The email of the new customer.</param>
        /// <param name="first">The first name of the customer.</param>
        /// <param name="last">The last name of the customer.</param>
        public static void SendPKeys(int quantity, string email, string first, string last)
        {
            string pkeyList = string.Empty;

            try
            {
                LimeLM.APIKey = LimeLM_ApiKey;

                // generate the product key (1 key with N activations)
                string resp = LimeLM.GeneratePKeys(LimeLM_VersionID, 1, quantity, email, null, null);

                // parse the XML response
                using (XmlReader reader = XmlReader.Create(new StringReader(resp)))
                {
                    reader.ReadToFollowing("rsp");

                    if (reader["stat"] != "ok")
                    {
                        //TODO: handle errors how ever you need to (logging, throwing exceptions, etc.)
                        reader.ReadToFollowing("err");
                        throw new Exception("Failed to generate the product keys: " + reader["msg"]);
                        return;
                    }

                    reader.ReadToFollowing("pkeys");

                    List<string> pkeys = new List<string>();
                    GetPKeys(reader, pkeys);

                    foreach (var pkey in pkeys)
                        pkeyList += pkey + "\r\n";
                }
            }
            catch (Exception ex)
            {
                // failed to generate product keys
                // TODO: handle the error somehow (log it, notify your support team, etc.)
                //       and delete the following "throw" statement
                throw;
            }



            /*
              System.Net.Mail reads SMTP configuration data out of the standard .NET configuration system
              (so for ASP.NET applications you’d configure this in your application’s web.config file).

              Here is an example of how to configure it:
 
              <system.net>
                <mailSettings>
                  <smtp from="test@foo.com">
                    <network host="smtpserver1" port="25"
                             userName="username" password="secret" defaultCredentials="true" />
                  </smtp>
                </mailSettings>
              </system.net>
            */

            // email the product key to your users
            // (only if we succeeded in getting product keys)
            if (pkeyList != string.Empty)
            {
                MailMessage message = new MailMessage();

                message.To.Add(new MailAddress(email));
                message.Subject = "Your " + AppName + " product key";

                string customerName = first;

                if (!string.IsNullOrEmpty(last))
                    customerName += " " + last;

                message.Body = customerName + ",\r\n\r\n    Thank you for your order. Your product key is:\r\n\r\n" + pkeyList + "\r\n\r\n";
                message.Body += "This product key is licensed for " + quantity;
                message.Body += quantity > 1 ? " users" : " user";
                message.Body += " of " + AppName + ".\r\n\r\nThank you,\r\n" + CompanyName;

                new SmtpClient().Send(message);
            }
        }

        static void GetPKeys(XmlReader reader, List<string> pkeys)
        {
            while (reader.Read())
            {
                //end of node </pkeys>
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName.Equals("pkeys"))
                    return;

                if (reader.NodeType == XmlNodeType.Element && reader.LocalName.Equals("pkey"))
                    pkeys.Add(reader["key"]);
            }
        }
    }
}