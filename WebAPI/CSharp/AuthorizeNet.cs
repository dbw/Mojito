using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Web;

namespace LimeLM_API
{
    public class AuthorizeNet
    {
        static string Login;
        static string TransKey;
        static bool UseTest;

        public static void SetLoginDetails(string login, string trans_key, bool test)
        {
            Login = login;
            TransKey = trans_key;
            UseTest = test;
        }

        public static string BankTransfer(string acct_num, string routing, string acct_type, string bank_name, string name_on_acct, string amount, string first_name, string last_name, string description, string email)
        {
            List<KeyValuePair<string, object>> postData = new List<KeyValuePair<string, object>>
                                          {
                                              new KeyValuePair<string, object>("x_login", Login),
                                              new KeyValuePair<string, object>("x_tran_key", TransKey),
                                              new KeyValuePair<string, object>("x_version", "3.1"),
                                              new KeyValuePair<string, object>("x_delim_data", "TRUE"),
                                              new KeyValuePair<string, object>("x_delim_char", "|"),
                                              new KeyValuePair<string, object>("x_relay_response", "FALSE"),
                                              new KeyValuePair<string, object>("x_type", "AUTH_CAPTURE"),
                                              new KeyValuePair<string, object>("x_method", "ECHECK"),
                                              new KeyValuePair<string, object>("x_recurring_billing", "FALSE"),

                                              new KeyValuePair<string, object>("x_bank_aba_code", routing),
                                              new KeyValuePair<string, object>("x_bank_acct_num", acct_num),
                                              new KeyValuePair<string, object>("x_bank_acct_type", acct_type),
                                              new KeyValuePair<string, object>("x_bank_name", bank_name),
                                              new KeyValuePair<string, object>("x_bank_acct_name", name_on_acct),
                                              new KeyValuePair<string, object>("x_echeck_type", "WEB"),

                                              new KeyValuePair<string, object>("x_amount", amount),
                                              new KeyValuePair<string, object>("x_description", description),

                                              new KeyValuePair<string, object>("x_first_name", first_name.Replace("|", "")),
                                              new KeyValuePair<string, object>("x_last_name", last_name.Replace("|", ""))
                                          };

            // get ip address
            string ip = HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"];

            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (!string.IsNullOrEmpty(ip))
                postData.Add(new KeyValuePair<string, object>("x_customer_ip", ip));

            // set the email
            if (!string.IsNullOrEmpty(email))
                postData.Add(new KeyValuePair<string, object>("x_email", email));

            string[] words = PostRequest(postData).Split('|');

            if (words[0] == "1")
                return null;

            // if the charge was not successful, get the error
            return words[3] ?? "Failed to make the bank transfer.";
        }

        public static string Charge(string cc, string exp_date, string amount, string first_name, string last_name, string zip, string street_address, string description, string email)
        {
            List<KeyValuePair<string, object>> postData = new List<KeyValuePair<string, object>>
                                          {
                                              new KeyValuePair<string, object>("x_login", Login),
                                              new KeyValuePair<string, object>("x_tran_key", TransKey),
                                              new KeyValuePair<string, object>("x_version", "3.1"),
                                              new KeyValuePair<string, object>("x_delim_data", "TRUE"),
                                              new KeyValuePair<string, object>("x_delim_char", "|"),
                                              new KeyValuePair<string, object>("x_relay_response", "FALSE"),
                                              new KeyValuePair<string, object>("x_type", "AUTH_CAPTURE"),
                                              new KeyValuePair<string, object>("x_method", "CC"),
                                              new KeyValuePair<string, object>("x_card_num", cc),
                                              new KeyValuePair<string, object>("x_exp_date", exp_date),
                                              new KeyValuePair<string, object>("x_amount", amount),
                                              new KeyValuePair<string, object>("x_description", description),
                                              new KeyValuePair<string, object>("x_first_name", first_name.Replace("|", "")),
                                              new KeyValuePair<string, object>("x_last_name", last_name.Replace("|", "")),
                                              new KeyValuePair<string, object>("x_address", street_address.Replace("|", "")),
                                              new KeyValuePair<string, object>("x_zip", zip.Replace("|", ""))
                                          };

            // get ip address
            string ip = HttpContext.Current.Request.ServerVariables["HTTP_CLIENT_IP"];

            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (!string.IsNullOrEmpty(ip))
                postData.Add(new KeyValuePair<string, object>("x_customer_ip", ip));

            // set the email
            if (!string.IsNullOrEmpty(email))
                postData.Add(new KeyValuePair<string, object>("x_email", email));

            string[] words = PostRequest(postData).Split('|');

            if (words[0] == "1")
                return null;

            // if the charge was not successful, get the error
            return words[3] ?? "Failed to charge the credit card.";
        }

        static string PostRequest(List<KeyValuePair<string, object>> postData)
        {
            // This section takes the input fields and converts them to the proper format
            // for an http post.
            string post_string = "";

            foreach (KeyValuePair<string, object> kv in postData)
            {
                if (kv.Value.GetType() == typeof(string[]))
                {
                    foreach (string val in (string[])kv.Value)
                    {
                        post_string += kv.Key + "[]=" + HttpUtility.UrlEncode(val, System.Text.Encoding.UTF8) + "&";
                    }
                }
                else
                    post_string += kv.Key + "=" + HttpUtility.UrlEncode((string)kv.Value, System.Text.Encoding.UTF8) + "&";
            }

            post_string = post_string.TrimEnd('&');

            // create an HttpWebRequest object to communicate with Authorize.Net
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(UseTest ? "https://test.authorize.net/gateway/transact.dll" : "https://secure.authorize.net/gateway/transact.dll");
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

            return post_response;
        }
    }
}