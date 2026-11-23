using System;
using System.IO;
using System.Xml;

namespace LimeLM_API
{
    public partial class limit_updates : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles("C:\\updates");

            string requestedFile = Request.QueryString["update"];
            string updateFile = null;

            // get the update file
            foreach (string file in files)
            {
                if (string.Equals(Path.GetFileName(file), requestedFile, StringComparison.InvariantCultureIgnoreCase))
                    updateFile = file;
            }

            // if updateFile is null, throw a 404 error
            if (updateFile == null)
            {
                Response.Clear();

                // throw a 404 error
                Response.StatusCode = 404;
                Response.End();
                return;
            }


            bool validLicense = false;

            //TODO: set your API KEY
            LimeLM.APIKey = "PASTE THE API KEY HERE";

            //TODO: set the version id to limit updates for
            // the version id is found in the url. For example http://wyday.com/limelm/version/100/
            // the version id is 100.
            const string versionID = "PASTE THE VERSION ID";

            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(LimeLM.GetPKeyID(versionID, Request.QueryString["pkey"]))))
                {
                    reader.ReadToFollowing("rsp");

                    if (reader["stat"] == "ok")
                    {
                        reader.ReadToFollowing("pkey");
                        string pkeyID = reader["id"];

                        using (XmlReader detReader = XmlReader.Create(new StringReader(LimeLM.GetDetails(pkeyID))))
                        {
                            detReader.ReadToFollowing("rsp");

                            if (detReader["stat"] == "ok")
                            {
                                detReader.ReadToFollowing("pkey");

                                // if the pkey isn't revoked (and there's a features block)
                                if (detReader["revoked"] != "true" && detReader.ReadToFollowing("features"))
                                {
                                    bool keepLooking = true;

                                    while (keepLooking && detReader.Read())
                                    {
                                        switch (detReader.NodeType)
                                        {
                                            case XmlNodeType.Element: // feature node

                                                if (detReader["name"] == "update_expires")
                                                {
                                                    // see if "update_expires" is later than right now
                                                    if (DateTime.Parse(detReader["value"]).CompareTo(DateTime.Now) > 0)
                                                        validLicense = true;

                                                    keepLooking = false;
                                                }

                                                break;
                                            case XmlNodeType.EndElement:
                                                if (detReader.Name == "features")
                                                    keepLooking = false;
                                                break;
                                        }
                                    }
                                }
                            }
                            else // failure
                            {
                                detReader.ReadToFollowing("err");

                                //TODO: use the error code & message
                                //detReader["code"]
                                //detReader["msg"]
                            }
                        }
                    }
                    else // failure
                    {
                        reader.ReadToFollowing("err");

                        //TODO: use the error code & message
                        //reader["code"]
                        //reader["msg"]
                    }
                }
            }
            catch (Exception)
            {
                // failed to either contact LimeLM or parse the response from LimeLM
                // TODO: handle the error somehow (log it, notify your support team, etc.)
            }

            // forbid invalid licenses
            if (!validLicense)
            {
                Response.Clear();

                // header('HTTP/1.1 403 Forbidden');
                Response.StatusCode = 403;
                Response.End();
                return;
            }


            Response.Clear();

            // download the file and bail
            Response.AddHeader("Content-Description", "File Transfer");
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(updateFile));
            Response.AddHeader("Content-Transfer-Encoding", "binary");
            Response.AddHeader("Expires", "0");
            Response.AddHeader("Cache-Control", "must-revalidate, post-check=0, pre-check=0");
            Response.AddHeader("Pragma", "public");
            Response.AddHeader("Content-Length", new FileInfo(updateFile).Length.ToString());

            // output the file
            Response.WriteFile(updateFile);

            Response.End();
        }
    }
}