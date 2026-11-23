<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LimeLM_API._Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>LimeLM Web API for ASP.NET</title>
</head>
<body>
    <h1>Using the web API</h1>
    <p>Included in this project is the LimeLM class that allows easy integration with your website. Also included are the following examples:</p>
    <ul>
        <li><a href="find-key.aspx">Find product key</a></li>
        <li><a href="payment.aspx">Payment example showing how to use PayPal, Moneybookers, and Authorize.Net</a></li>
        <li><a href="manual-activation.aspx">Manual activation example</a></li>
        <li><a href="http://wyday.com/limelm/help/saas-and-time-limited-licensing/">Limit updates example</a>, see limit-updates.aspx.cs.</li>
    </ul>

    <h2>Using the LimeLM class</h2>
    <p>Below is an example that generates a product key. It involves 3 steps:</p>
    <ol>
        <li>Setting the API key</li>
        <li>Calling the LimeLM.GeneratePkey() function</li>
        <li>Parsing the <a href="http://wyday.com/limelm/help/api/response.rest/">XML response</a> from the server.</li>
    </ol>

    <pre>//TODO: set your API key found in http://wyday.com/limelm/settings/
LimeLM.APIKey = "PASTE THE API KEY HERE";

//TODO: set the version id to generate & find keys for
// the version id is found in the url. For example http://wyday.com/limelm/version/100/
// the version id is 100.
const string versionID = "PASTE THE VERSION ID";

string ret = LimeLM.GeneratePKeys(versionID, 1, 2, "an.email@example.com", new[] { "update_expires" }, new[] { "2011-06-25 23:19:52" });

//TODO: parse the XML response</pre>
</body></html>