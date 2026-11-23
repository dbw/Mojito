using System;
using System.IO;
using System.Xml;

namespace LimeLM_API
{
    public partial class manual_activation : System.Web.UI.Page
    {
        public string ActivationRequest;
        public string ActivationResponse;

        public bool Success;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Form["xmlresp"] != null)
            {
                Response.Clear();

                // download the file and bail
                Response.AddHeader("Content-Description", "File Transfer");
                Response.ContentType = "application/xml";
                Response.AddHeader("Content-Disposition", "attachment; filename=ActivationResponse.xml");
                Response.AddHeader("Content-Transfer-Encoding", "binary");
                Response.AddHeader("Expires", "0");
                Response.AddHeader("Cache-Control", "must-revalidate, post-check=0, pre-check=0");
                Response.AddHeader("Pragma", "public");
                Response.AddHeader("Content-Length", Request.Form["xmlresp"].Length.ToString());

                // output the file
                Response.Write(Request.Form["xmlresp"]);

                Response.End();
                return;
            }

            if (IsPostBack)
            {
                //TODO: set your API KEY
                LimeLM.APIKey = "PASTE THE API KEY HERE";

                // try to get the activation request
                ActivationRequest = Request.Form["act_req"];

                if (string.IsNullOrEmpty(ActivationRequest))
                {
                    // try to get it from file
                    if (act_req_file.PostedFile != null)
                    {
                        // stream in the entire string
                        using (StreamReader sr = new StreamReader(act_req_file.PostedFile.InputStream))
                        {
                            ActivationRequest = sr.ReadToEnd();
                        }
                    }
                    else
                    {
                        errorDiv.InnerHtml = "Oops. You must submit an activation request file.";
                        return;
                    }
                }

                try
                {
                    // process the manual activation
                    using (XmlReader reader = XmlReader.Create(new StringReader(LimeLM.ManualActivation(ActivationRequest))))
                    {
                        reader.ReadToFollowing("rsp");

                        if (reader["stat"] != "ok")
                        {
                            reader.ReadToFollowing("err");

                            switch (reader["code"])
                            {
                                case "105":
                                    errorDiv.InnerHtml = "This product key has been revoked. Contact us for more information.";
                                    break;
                                case "106":
                                    errorDiv.InnerHtml = "This product key has been activated the maximum number of times. Buy more licenses.";
                                    break;
                                default:
                                    errorDiv.InnerHtml = "An error happened: " + reader["msg"];
                                    break;
                            }
                        
                            return;
                        }

                        reader.ReadToFollowing("act_resp_xml");
                        ActivationResponse = reader["data"];
                    }

                    Success = true;
                }
                catch (Exception)
                {
                    // failed to either contact LimeLM or parse the response from LimeLM
                    // TODO: handle the error somehow (log it, notify your support team, etc.)
                }
            }
        }
    }
}