using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Xml;

namespace LimeLM_API
{
    public partial class find_key : System.Web.UI.Page
    {
        public bool PkeyFound;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                //TODO: set your API KEY
                LimeLM.APIKey = "PASTE THE API KEY HERE";

                //TODO: set the version id to generate & find keys for
                // the version id is found in the url. For example http://wyday.com/limelm/version/100/
                // the version id is 100.
                const string versionID = "PASTE THE VERSION ID";

                string pkeyList = "";
                int totalPkeys;

                try
                {
                    // fetch the product key, and parse the XML response
                    using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(LimeLM.FindPKey(versionID, txtEmail.Text))))
                    {
                        reader.ReadToFollowing("rsp");

                        if (reader["stat"] != "ok")
                        {
                            reader.ReadToFollowing("err");
                            errorDiv.InnerHtml = "An error happened: " + reader["msg"];
                            return;
                        }

                        reader.ReadToFollowing("pkeys");

                        List<string> pkeys = new List<string>();
                        GetPKeys(reader, pkeys);

                        totalPkeys = pkeys.Count;

                        //TODO: instead of outputing the results to site, email the user's email (from txtEmail.Text)
                        foreach (var pkey in pkeys)
                            pkeyList += pkey + "\r\n";
                    }

                    PkeyFound = true;
                }
                catch (Exception ex)
                {
                    // failed to either contact LimeLM or parse the response from LimeLM
                    // TODO: handle the error somehow (log it, notify your support team, etc.)
                    //       and delete the following "throw" statement
                    throw;
                }

                //TODO: email the product key to your users
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
                MailMessage message = new MailMessage();

                message.To.Add(new MailAddress(txtEmail.Text));
                message.Subject = "Your product " + (totalPkeys > 1 ? "keys" : "key");
                message.Body += "Your product key" + (totalPkeys > 1 ? "s are" : " is") +  ":\r\n\r\n" + pkeyList;

                new SmtpClient().Send(message);
            }
        }

        static void GetPKeys(XmlReader reader, List<string> pkeys)
        {
            while (reader.Read())
            {
                // end of node </pkeys>
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName.Equals("pkeys"))
                    return;

                if (reader.NodeType == XmlNodeType.Element && reader.LocalName.Equals("pkey"))
                    pkeys.Add(reader["key"]);
            }
        }
    }
}