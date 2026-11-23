<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="find-key.aspx.cs" Inherits="LimeLM_API.find_key" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Find product key</title>
</head>
<body>
    <% if (PkeyFound) { %>

	<h2>Product keys have been sent</h2>
	<p>We've found your product keys and sent them to your email address.</p>

    <% } else { %>
    
    <h1>Find product key</h1>
    <p>A simple example to find the product keys using an email address.</p>

    <h2>Instructions</h2>
	<ol>
		<li>Set your LimeLM API key &amp; version ID in find-key.aspx.cs</li>
        <li>Set the SMTP email settings in your application's web.config file (see the comment in find-key.aspx.cs for details)</li>
		<li>Customize your email message</li>
		<li>Set your email address or logging details for errors</li>
	</ol>

    <hr/>

    <p><big><strong runat="server" id="errorDiv"></strong></big></p>

    <form id="form1" runat="server">
    <div>
    	<table>
			<tr>
				<th><label for="txtEmail">Enter your email:</label></th>
				<td><asp:TextBox ID="txtEmail" runat="server" Width="209px"></asp:TextBox></td>
				<td><asp:Button ID="btnFindSerials" runat="server" Text="Find Serials"/></td>
			</tr>
		</table>
    </div>
    </form>

    <% } %>
</body>
</html>