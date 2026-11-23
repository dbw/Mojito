<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="manual-activation.aspx.vb" Inherits="LimeLM_API.manual_activation" validateRequest="false" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Manual product activation</title>
</head>
<body>
    <% If (Success) Then%>

	<h2>Activated successfully!</h2>

    <form name="download" accept-charset="utf-8" action="" method="post">
		<input type="hidden" value="<%= HttpContext.Current.Server.HtmlEncode(ActivationResponse) %>" name="xmlresp"/>
		<p><strong><a href="javascript: submitform()">Download the Activation response XML file</a></strong></p>
	</form>
	<script type="text/javascript">function submitform() { document.download.submit(); }</script>

	<p>Or, copy and paste the activation response:</p>
	<textarea rows="12" cols="80" readonly="readonly"><%= HttpContext.Current.Server.HtmlEncode(ActivationResponse) %></textarea>

    <% Else%>

	<h1>Manual product activation</h1>
	<p>A simple example showing how to do manual activation.</p>

    <h2>Instructions</h2>
	<ol>
		<li>Set your LimeLM API key in find-key.aspx.cs</li>
	</ol>

    <hr/>

    <p><big><strong runat="server" id="errorDiv"></strong></big></p>

   	<form id="Form1" enctype="multipart/form-data" accept-charset="utf-8" method="post" action="" runat="server">
		<h2>Upload the activation request file</h2>
		<input id="act_req_file" name="act_req_file" type="file" runat="server"/><br/>

		<div>
			<h2><label for="form_act_req">Or, paste the activation request:</label></h2>
			<textarea id="form_act_req" name="act_req" rows="9" cols="75"><%= HttpContext.Current.Server.HtmlEncode(ActivationRequest) %></textarea>
		</div>

		<div class="submit"><asp:Button ID="btnSubmit" runat="server" Text="Activate"/></div>
	</form>

    <% End If%>
</body>
</html>