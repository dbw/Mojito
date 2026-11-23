<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="payment.aspx.cs" Inherits="LimeLM_API.payment" %>
<%@ Import Namespace="LimeLM_API" %>
<%@ Import Namespace="System.Globalization" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Buy <%= PaymentSettings.AppName %></title>
	<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
<% string SinglePrice = PaymentSettings.AppPrice[0] + "." + PaymentSettings.AppPrice[1].ToString("00", CultureInfo.InvariantCulture); %>
<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"></script>
<script type="text/javascript"><!--
function calcTot(dollars, cents, quantity)
{
	var n = Math.abs((cents * quantity) % 100);
	var zeroString = Math.pow(10, Math.max(0, 2 - Math.floor(n).toString().length)).toString().substr(1);
	return (dollars * quantity + Math.floor((cents * quantity) / 100)).toString()+'.'+zeroString+n;
}

$(document).ready(function(){

$("#quantity").keypress(function(evt){
	var charCode = (evt.which) ? evt.which : event.keyCode;
	return !(charCode > 31 && (charCode < 48 || charCode > 57));
});

var total;

$("#quantity").keyup(function(){
	total = calcTot(<%= PaymentSettings.AppPrice[0] + "," + PaymentSettings.AppPrice[1] %>,$("#quantity").val());
	$("#total_price").text(total);

<%
if (PaymentSettings.UseMoneybookers)
{
    if (PaymentSettings.UsePayPal)
        Response.Write("if ($(\"#Moneybookers\").prop(\"checked\")){$(\"#mb_amt\").attr(\"value\", total);}");
    else
        Response.Write("$(\"#mb_amt\").attr(\"value\", total);");
}
%>
});

total = calcTot(<%= PaymentSettings.AppPrice[0] + "," + PaymentSettings.AppPrice[1] %>,$("#quantity").val());
$("#total_price").text(total);


$("#signup_form").submit(function(){
	$("#submit").prop("disabled", true);
<%
if (TotalPaymentMethods == 1)
{
    if (PaymentSettings.UsePayPal)
        Response.Write("$(\"#progress\").html(\"Proceeding to PayPal for checkout...\");");
    else if (PaymentSettings.UseMoneybookers)
        Response.Write("$(\"#progress\").html(\"Proceeding to Skrill for checkout...\");");
    else
        Response.Write("$(\"#progress\").html(\"Placing your order - please wait...\");");
}
else
{
%>
	if ($("#PayPal").prop("checked"))
		$("#progress").html("Proceeding to PayPal for checkout...");
	else if ($("#Moneybookers").prop("checked"))
		$("#progress").html("Proceeding to Skrill for checkout...");
	else
		$("#progress").html("Placing your order - please wait...");
<%
}
%>
});

$("#Bank").click(function(){
	$("#signup_form").attr("action", "");
	$("#cc_info").slideUp("fast");
	$("#bank_info").slideDown("fast");
});
$("#Card").click(function(){
	$("#signup_form").attr("action", "");
	$("#bank_info").slideUp("fast");
	$("#cc_info").slideDown("fast");
});
$("#PayPal").click(function(){
	$("#signup_form").attr("action", "<%=PaymentSettings.PayPalSandbox ? "https://www.sandbox.paypal.com/cgi-bin/webscr":"https://www.paypal.com/cgi-bin/webscr" %>");
	$("#cc_info").slideUp("fast");
	$("#bank_info").slideUp("fast");
<% if (PaymentSettings.UseMoneybookers)
	  Response.Write("$(\"#mb_amt\").attr(\"value\", " + SinglePrice +");");
%>
});
$("#Moneybookers").click(function(){
	$("#signup_form").attr("action", "https://www.moneybookers.com/app/payment.pl");
	$("#cc_info").slideUp("fast");
	$("#bank_info").slideUp("fast");

	$("#mb_amt").attr("value", total);
});

if ($("#PayPal").prop("checked")){
	$("#signup_form").attr("action", "<%=PaymentSettings.PayPalSandbox ? "https://www.sandbox.paypal.com/cgi-bin/webscr":"https://www.paypal.com/cgi-bin/webscr" %>");
	$("#cc_info").hide();
	$("#bank_info").hide();
}
else if ($("#Bank").prop("checked")){
	$("#cc_info").hide();
	$("#bank_info").show();
}
else if ($("#Moneybookers").prop("checked")){
	$("#signup_form").attr("action", "https://www.moneybookers.com/app/payment.pl");
	$("#cc_info").hide();
	$("#bank_info").hide();
	$("#mb_amt").attr("value", total);
}

<% if (PaymentSettings.UseMoneybookers && TotalPaymentMethods == 1)
       Response.Write("$(\"#mb_amt\").attr(\"value\", total);");
%>
});
//--></script>
<style type="text/css">
<!--
.content{
width:705px;
margin:0 auto;
}

table{border-width:1px 0 0 0}
td{border:0px}
tr th{
text-align:left;
white-space:nowrap
}
#totalline{
background:#F6F3FF;
border-top:1px solid #9879FF;
margin:15px 0;
padding:5px
}
input,.large{font-size:110%}
div.error{color:red}
.error input{border:2px solid red}
.error label{
color:red;
font-weight:bold
}

#cc-card,#cc-paypal,#cc-bank,#cc-mb,#cc-gc,.cc-secure{
text-indent:-9999px;
display:block;
float:left;
background:url(<%= Page.ClientScript.GetWebResourceUrl(typeof(payment), "LimeLM_API.sprites.png") %>) no-repeat;
height:35px;
width:289px;
margin-right:15px
}
#cc-bank{
background-position:0 -38px;
width:56px
}
#cc-paypal{
background-position:0 -76px;
width:55px
}
#cc-mb{
background-position:0 -114px;
width:55px
}
#cc-gc{
background-position:0 -152px;
width:55px
}
.cc-secure{
background-position:0 -190px;
height:32px;
width:86px;
}
#cc-type{
height:35px;
margin-bottom:15px
}
#cc-type input{
float:left;
margin:8px 5px 0 10px;
width:1em
}
label.link{
cursor:pointer;
cursor:hand;
}

<%
if (!AuthNetCCSelected)
	Response.Write("#cc_info{display:none}");

if (!AuthNetBankSelected)
	Response.Write("#bank_info{display:none}");
%>

h3#su1,h3#su2,h3#su3{
background:url(<%= Page.ClientScript.GetWebResourceUrl(typeof(payment), "LimeLM_API.sprites.png") %>) no-repeat -91px -38px;
color:#002C6D;
margin:30px 0 20px;
padding:2px 0 0 36px;
min-height:26px;
height:auto!important;
height:26px
}
h3#su2{background-position:-91px -114px}
h3#su3{background-position:-91px -190px}
-->
</style></head>
<body><div class="content">
	<h1>Buy <%= PaymentSettings.AppName %></h1>
	<p><%= PaymentSettings.AppName %> costs <strong><%=PaymentSettings.CurrencySign%><%= SinglePrice %></strong> per license. Your licenses will be e-mailed to you immediately after your order is processed.</p>

<%
	string defAction;

	if (PayPalSelected)
		defAction = PaymentSettings.PayPalSandbox ? "https://www.sandbox.paypal.com/cgi-bin/webscr" : "https://www.paypal.com/cgi-bin/webscr";
	else if (MoneybookersSelected)
		defAction = "https://www.moneybookers.com/app/payment.pl";
	else
		defAction = "";
%>
<form action="<%=defAction%>" method="post" id="signup_form">
<%
int headerShown = 1;

if (PaymentSettings.UseAuthorizeNetCC || PaymentSettings.UseAuthorizeNetBank) {%>
	<h3 id="su<%= headerShown++ %>">Your info</h3>
	<table>
		<tr<% if (CCProc.FirstNameErr) Response.Write(" class=\"error\""); %>>
			<th><label for="form_first_name">First name</label></th>
			<td><input type="text" id="form_first_name" name="first_name" value="<%= Server.HtmlEncode(CCProc.FirstName) %>" size="24"/></td>
		</tr>
		<tr<% if (CCProc.LastNameErr) Response.Write(" class=\"error\""); %>>
			<th><label for="form_last_name">Last name</label></th>
			<td><input type="text" id="form_last_name" name="last_name" value="<%= Server.HtmlEncode(CCProc.LastName) %>" size="24"/></td>
		</tr>
		<tr<% if (CCProc.EmailErr) Response.Write(" class=\"error\""); %>>
			<th><label for="form_email">Email</label></th>
			<td><input type="text" id="form_email" name="email" value="<%= Server.HtmlEncode(CCProc.Email) %>" size="24"/></td>
		</tr>
	</table>
<%}%>

	<h3 id="su<%= headerShown++ %>">Number of licenses</h3>
	<table> 
		<tr<% if (CCProc != null && CCProc.QuantityErr) Response.Write(" class=\"error\""); %>> 
			<th><label for="quantity">Number of licenses </label></th> 
			<td><input type="text" name="quantity" id="quantity" value="<%= CCProc != null ? CCProc.Quantity : 1 %>" size="2"/></td>
		</tr>
        <% if (CCProc != null && CCProc.QuantityErr) {%><tr class="error"><th> </th><td><div class="error">You must enter a quantity greater than 0</div></td></tr><%}%>
		<tr>
			<th>Price per license </th>
			<td class="large"><%=PaymentSettings.CurrencySign%><%= SinglePrice %></td>
		</tr>
	</table>

<%if (TotalPaymentMethods == 1 && (PaymentSettings.UseAuthorizeNetCC || PaymentSettings.UseAuthorizeNetBank))
  {
	  Response.Write("<h3 id=\"su" + headerShown + "\">Enter your billing information &mdash; SECURE</h3>");
  }
  else if (TotalPaymentMethods > 1) {

	  // form the payment selection header
	  string paymentHead = "Payment method &mdash; ";

	  int used = 0;

	  if (PaymentSettings.UseAuthorizeNetCC)
	  {
		  paymentHead += "Credit card";
		  used++;
	  }

	  if (PaymentSettings.UseAuthorizeNetBank)
	  {
		  if (used > 0)
			  paymentHead += TotalPaymentMethods == used + 1 ? (TotalPaymentMethods > 2 ? ", or " : " or ") : ", ";
		  paymentHead += "Bank transfer";
		  used++;
	  }

	  if (PaymentSettings.UsePayPal)
	  {
		  if (used > 0)
			  paymentHead += TotalPaymentMethods == used + 1 ? (TotalPaymentMethods > 2 ? ", or " : " or ") : ", ";
		  paymentHead += "PayPal";
		  used++;
	  }

	  if (PaymentSettings.UseMoneybookers)
	  {
		  if (used > 0)
			  paymentHead += TotalPaymentMethods == used + 1 ? (TotalPaymentMethods > 2 ? ", or " : " or ") : ", ";
		  paymentHead += "Skrill";
	  }
%>
	<h3 id="su<%= headerShown %>"><%= paymentHead %></h3>
	<div id="cc-type">
	<%if (PaymentSettings.UseAuthorizeNetCC) {%>
		<input type="radio" id="Card" value="Card" name="mf_cardtype" title="Card"<%= AuthNetCCSelected ? " checked=\"checked\"" : "" %>/>
		<label for="Card" title="Pay with a credit card" id="cc-card" class="link">Visa / American Express / MasterCard / Discover / JCB</label>
	<%}%>
	<%if (PaymentSettings.UseAuthorizeNetBank) {%>
		<input type="radio" id="Bank" value="Bank" name="mf_cardtype" title="Bank"<%= AuthNetBankSelected ? " checked=\"checked\"" : "" %>/>
		<label for="Bank" title="Pay with an eCheck (Bank transfer)" id="cc-bank" class="link">Pay with an eCheck (Bank transfer)</label>
	<%}%>
	<%if (PaymentSettings.UsePayPal) {%>
		<input type="radio" id="PayPal" value="PayPal" name="mf_cardtype" title="PayPal"<%= PayPalSelected ? " checked=\"checked\"" : "" %>/>
		<label for="PayPal" title="Pay with PayPal" id="cc-paypal" class="link">PayPal</label>
	<%}%>
	<%if (PaymentSettings.UseMoneybookers) {%>
		<input type="radio" id="Moneybookers" value="Moneybookers" name="mf_cardtype" title="Skrill"<%= MoneybookersSelected ? " checked=\"checked\"" : "" %>/>
		<label for="Moneybookers" title="Pay with Skrill" id="cc-mb" class="link">Skrill</label>
	<%}%>
	</div>
<%}

if (PaymentSettings.UseAuthorizeNetCC) {%>
	<div id="cc_info">
		<table>
<% if (TotalPaymentMethods == 1)
{
	Response.Write("<tr><th>We accept</th><td colspan=\"2\"><div id=\"cc-card\">Visa / American Express / MasterCard / Discover / JCB</div><input type=\"hidden\" name=\"mf_cardtype\" value=\"Card\"/></td></tr>");
} %>
			<tr<% if (CCProc.CCNumErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_ccnum">Card number</label></th>
				<td><input id="form_ccnum" name="cc_num" size="22" type="text" value="<%= Server.HtmlEncode(CCProc.CCNumber) %>"/></td>
				<td valign="top"><div class="cc-secure">Secure</div></td>
			</tr>
			<% if (CCProc.CCNumErr){%><tr class="error"><th> </th><td><div class="error"><%= CCProc.CCNumErrMsg %></div></td></tr><%}%>
			<tr<% if (CCProc.CCExpErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_cc_expires">Expires on</label></th>
				<td>
			<select id="form_cc_expires" name="cc_expires_m">
				<option <% if (CCProc.CCExpiresM == 1 || CCProc.CCExpiresM == 0) Response.Write("selected=\"selected\" "); %>value="1">1 - January</option>
				<option <% if (CCProc.CCExpiresM == 2) Response.Write("selected=\"selected\" "); %>value="2">2 - February</option>
				<option <% if (CCProc.CCExpiresM == 3) Response.Write("selected=\"selected\" "); %>value="3">3 - March</option>
				<option <% if (CCProc.CCExpiresM == 4) Response.Write("selected=\"selected\" "); %>value="4">4 - April</option>
				<option <% if (CCProc.CCExpiresM == 5) Response.Write("selected=\"selected\" "); %>value="5">5 - May</option>
				<option <% if (CCProc.CCExpiresM == 6) Response.Write("selected=\"selected\" "); %>value="6">6 - June</option>
				<option <% if (CCProc.CCExpiresM == 7) Response.Write("selected=\"selected\" "); %>value="7">7 - July</option>
				<option <% if (CCProc.CCExpiresM == 8) Response.Write("selected=\"selected\" "); %>value="8">8 - August</option>
				<option <% if (CCProc.CCExpiresM == 9) Response.Write("selected=\"selected\" "); %>value="9">9 - September</option>
				<option <% if (CCProc.CCExpiresM == 10) Response.Write("selected=\"selected\" "); %>value="10">10 - October</option>
				<option <% if (CCProc.CCExpiresM == 11) Response.Write("selected=\"selected\" "); %>value="11">11 - November</option>
				<option <% if (CCProc.CCExpiresM == 12) Response.Write("selected=\"selected\" "); %>value="12">12 - December</option>
			</select>

			<select id="form_cc_expires_y" name="cc_expires_y">
			<%
			// form the years dropdown list from the current year to 20 years from now
			int currYear = DateTime.Now.Year;
			int endYear = currYear + 21;
			int expYear = CCProc.CCExpiresY == 0 ? currYear : CCProc.CCExpiresY;

			for (; currYear < endYear; currYear++)
			{
				Response.Write("<option");

				if (expYear == currYear)
					Response.Write(" selected=\"selected\"");

				Response.Write(" value=\"" + currYear + "\">" + currYear + "</option>");
			}
			%>
			</select>

				</td>
			</tr>
			<% if (CCProc.CCExpErr){%><tr class="error"><th> </th><td><div class="error">The date you've entered has expired. Either correct the date or use a card that hasn't expired.</div></td></tr><%}%>
			<tr<% if (CCProc.CCAddrErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_cc_adr">Street address</label></th>
				<td><input id="form_cc_adr" name="cc_adr" size="22" type="text" value="<%= Server.HtmlEncode(CCProc.CCAddress) %>"/></td>
			</tr>
			<tr<% if (CCProc.CCZipErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_cc_zip">Billing ZIP code</label></th>
				<td><input id="form_cc_zip" name="cc_zip" size="10" type="text" value="<%= Server.HtmlEncode(CCProc.CCZip) %>"/></td>
			</tr>
			<tr>
				<th> </th>
				<td><small>(or Postal Code if not in the USA)</small></td>
			</tr>
		</table>
	</div>
<%}

if (PaymentSettings.UseAuthorizeNetBank) {

	if (TotalPaymentMethods == 1)
		Response.Write("<input type=\"hidden\" name=\"mf_cardtype\" value=\"Bank\"/>");
%>
	<div id="bank_info">
		<table>
			<% if (CCProc.BankErrMsg != null){%><tr class="error"><th> </th><td><div class="error"><%= CCProc.BankErrMsg %></div></td></tr><%}%>
			<tr<% if (CCProc.BankAcctNameErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_bank_acct_name">Name on account</label></th>
				<td><input id="form_bank_acct_name" name="bank_acct_name" size="27" type="text" value="<%= Server.HtmlEncode(CCProc.BankAcctName) %>"/></td>
				<td valign="top" align="center"><div class="cc-secure">Secure</div></td>
			</tr>

			<tr<% if (CCProc.BankAcctTypeErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_acct_type">Type of account</label></th>
				<td>
					<select id="form_acct_type" name="bank_acct_type">
						<option <% if (string.IsNullOrEmpty(CCProc.BankAcctType)) Response.Write("selected=\"selected\" "); %>value="">&mdash; Select one &mdash;</option>
						<option <% if (CCProc.BankAcctType == "C") Response.Write("selected=\"selected\" "); %>value="C">Checking</option>
						<option <% if (CCProc.BankAcctType == "S") Response.Write("selected=\"selected\" "); %>value="S">Savings</option>
					</select>
				</td>
			</tr>
			<% if (CCProc.BankAcctTypeErr){%><tr class="error"><th> </th><td><div class="error">Select the type of bank account from the dropdown.</div></td></tr><%}%>

			<tr<% if (CCProc.BankNameErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_bank_name">Bank Name</label></th>
				<td><input id="form_bank_name" name="bank_name" size="27" type="text" value="<%= Server.HtmlEncode(CCProc.BankName) %>"/></td>
			</tr>
			<tr<% if (CCProc.BankRouteNumErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_bank_route_num">ABA Routing Number</label></th>
				<td><input id="form_bank_route_num" name="bank_route_num" size="11" type="text" value="<%= Server.HtmlEncode(CCProc.BankRouteNum) %>"/></td>
			</tr>
			<% if (CCProc.BankRouteNumErr){%><tr class="error"><th> </th><td><div class="error">The bank routing number is not valid &mdash; enter the correct 9 digits. You can find this number on your checks.</div></td></tr><%}%>

			<tr<% if (CCProc.BankAcctNumErr) Response.Write(" class=\"error\""); %>>
				<th><label for="form_bank_acct_num">Account Number</label></th>
				<td><input id="form_bank_acct_num" name="bank_acct_num" size="18" type="text" value="<%= Server.HtmlEncode(CCProc.BankAcctNum) %>"/></td>
			</tr>
		</table>
	</div>
<%}
  
if (PaymentSettings.UsePayPal || PaymentSettings.UseMoneybookers) {%>
	<input type="hidden" name="amount" value="<%= SinglePrice %>" id="mb_amt"/>
<%}

if (PaymentSettings.UsePayPal) {%>
	<input type="hidden" name="cmd" value="_xclick"/>
	<input type="hidden" name="business" value="<%= PaymentSettings.PayPalEmail %>"/>
	<input type="hidden" name="undefined_quantity" value="1"/>
	<input type="hidden" name="item_name" value="<%= PaymentSettings.AppName %> license"/>
	<input type="hidden" name="item_number" value="1"/>
	<input type="hidden" name="no_shipping" value="1"/>
	<input type="hidden" name="no_note" value="1"/>
	<input type="hidden" name="currency_code" value="<%= PaymentSettings.Currency %>"/>
	<input type="hidden" name="lc" value="US"/>
	<input type="hidden" name="rm" value="2"/>
	<input type="hidden" name="return" value="<%= PaymentSettings.ThankYouPage %>"/>
	<input type="hidden" name="cancel_return" value="<%= PaymentSettings.BuyPage %>"/>
	<input type="hidden" name="notify_url" value="<%= PaymentSettings.CheckScript + "?paypal=1" %>"/>
<%
	if (!string.IsNullOrEmpty(PaymentSettings.YourLogo))
		Response.Write("<input type=\"hidden\" name=\"image_url\" value=\"" + PaymentSettings.YourLogo + "\"/>");
}

if (PaymentSettings.UseMoneybookers) {%>
	<input type="hidden" name="language" value="EN"/>
	<input type="hidden" name="currency" value="<%= PaymentSettings.Currency %>"/>
	<input type="hidden" name="pay_to_email" value="<%= PaymentSettings.MBEmail %>"/>
	<input type="hidden" name="return_url" value="<%= PaymentSettings.ThankYouPage %>"/>
	<input type="hidden" name="cancel_url" value="<%= PaymentSettings.BuyPage %>"/>
	<input type="hidden" name="status_url" value="<%= PaymentSettings.CheckScript + "?moneybookers=1" %>"/>
	<input type="hidden" name="detail1_description" value="<%= PaymentSettings.AppName %> licenses"/>
	<input type="hidden" name="detail1_text" value="<%= PaymentSettings.AppName %> licenses"/>
	<input type="hidden" name="merchant_fields" value="quantity"/>
<%
	if (!string.IsNullOrEmpty(PaymentSettings.YourLogo))
		Response.Write("<input type=\"hidden\" name=\"logo_url\" value=\"" + PaymentSettings.YourLogo + "\"/>");
}%>

<div id="totalline" class="large">Your total: <strong><%=PaymentSettings.CurrencySign%><span id="total_price"><%= SinglePrice %> per license</span></strong></div>
<div><input type="image" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(payment), "LimeLM_API.order.png") %>" name="submit" id="submit" alt="Place my order" style="width:180px;height:38px;vertical-align:middle;"/>&nbsp;&nbsp;<strong id="progress"></strong></div>
</form>

<%
// Show warnings and errors in the config.

// Don't delete these lines. You won't see them on your
// live site if you have everything configured correctly.

if (PaymentSettings.UsePayPal && PaymentSettings.PayPalSandbox)
	Response.Write("<p><strong><font color=\"#FF0000\">PayPal sandbox is on</font> &mdash; payments will not be sent to your live account. To disable sandbox mode set <code>PayPalSandbox = false;</code> in <code>PaymentSettings.cs</code>.</strong></p>");

if ((PaymentSettings.UseAuthorizeNetCC || PaymentSettings.UseAuthorizeNetBank) && PaymentSettings.AuthNetTest)
{
	Response.Write("<p><strong><font color=\"#FF0000\">Authorize.Net payments is in Test Mode</font> &mdash; payments will not be sent to your live account. To turn off test mode set <code>AuthNetTest = false;</code> in <code>PaymentSettings.cs</code>.</strong></p>");

	if (!Request.IsSecureConnection)
		Response.Write("<p><strong><font color=\"#FF0000\">Authorize.Net payments <em>requires</em> a secure connection (https://).</font> &mdash; when you set <code>AuthNetTest = false;</code> in <code>PaymentSettings.cs</code> this page will automatically redirect you to the secure version of this page (https://) if you visit it from the non-secure version (http://). However, in test mode you can use the non-secure version to remove the hassle of creating self-signed SSL certificates.</strong></p>");
}
%>

</div></body></html>