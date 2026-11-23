<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="payment.aspx.vb" Inherits="LimeLM_API.payment" %>
<%@ Import Namespace="LimeLM_API" %>
<%@ Import Namespace="System.Globalization" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Buy <%= PaymentSettings.AppName%></title>
	<meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
<% Dim SinglePrice As String = PaymentSettings.AppPrice(0).toString() + "." + PaymentSettings.AppPrice(1).ToString("00", CultureInfo.InvariantCulture) %>
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
	total = calcTot(<%= PaymentSettings.AppPrice(0).toString() + "," + PaymentSettings.AppPrice(1).toString() %>,$("#quantity").val());
	$("#total_price").text(total);
<%
If (PaymentSettings.UseMoneybookers) Then
    If (PaymentSettings.UsePayPal) Then
	    Response.Write("if ($(""#Moneybookers"").prop(""checked"")){$(""#mb_amt"").attr(""value"", total);}")
    Else
        Response.Write("$(""#mb_amt"").attr(""value"", total);")
    End If
End If
%>
});

total = calcTot(<%= PaymentSettings.AppPrice(0).toString() + "," + PaymentSettings.AppPrice(1).toString() %>,$("#quantity").val());
$("#total_price").text(total);


$("#signup_form").submit(function(){
	$("#submit").prop("disabled", true);
<%
If (TotalPaymentMethods = 1) Then
    If (PaymentSettings.UsePayPal) Then
	    Response.Write("$(""#progress"").html(""Proceeding to PayPal for checkout..."");")
    ElseIf (PaymentSettings.UseMoneybookers) Then
        Response.Write("$(""#progress"").html(""Proceeding to Skrill for checkout..."");")
    Else
        Response.Write("$(""#progress"").html(""Placing your order - please wait..."");")
    End If
Else %>
	if ($("#PayPal").prop("checked"))
		$("#progress").html("Proceeding to PayPal for checkout...");
	else if ($("#Moneybookers").prop("checked"))
		$("#progress").html("Proceeding to Skrill for checkout...");
	else
		$("#progress").html("Placing your order - please wait...");
<%
End If
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
	$("#signup_form").attr("action", "<%= If(PaymentSettings.PayPalSandbox, "https://www.sandbox.paypal.com/cgi-bin/webscr", "https://www.paypal.com/cgi-bin/webscr") %>");
	$("#cc_info").slideUp("fast");
	$("#bank_info").slideUp("fast");
<%
If (PaymentSettings.UseMoneybookers) Then
	Response.Write("$(""#mb_amt"").attr(""value"", " + SinglePrice + ");")
End If
%>
});
$("#Moneybookers").click(function(){
	$("#signup_form").attr("action", "https://www.moneybookers.com/app/payment.pl");
	$("#cc_info").slideUp("fast");
	$("#bank_info").slideUp("fast");

	$("#mb_amt").attr("value", total);
});

if ($("#PayPal").prop("checked")){
	$("#signup_form").attr("action", "<%= If(PaymentSettings.PayPalSandbox, "https://www.sandbox.paypal.com/cgi-bin/webscr", "https://www.paypal.com/cgi-bin/webscr") %>");
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

<%
If (PaymentSettings.UseMoneybookers And TotalPaymentMethods = 1) Then
	Response.Write("$(""#mb_amt"").attr(""value"", total);")
End If
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
background:url(<%= Page.ClientScript.GetWebResourceUrl(GetType(payment), "LimeLM_API.sprites.png") %>) no-repeat;
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
If Not AuthNetCCSelected Then
	Response.Write("#cc_info{display:none}")
End If

If Not AuthNetBankSelected Then
	Response.Write("#bank_info{display:none}")
End If
%>

h3#su1,h3#su2,h3#su3{
background:url(<%= Page.ClientScript.GetWebResourceUrl(GetType(payment), "LimeLM_API.sprites.png") %>) no-repeat -91px -38px;
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
	<h1>Buy <%= PaymentSettings.AppName%></h1>
	<p><%= PaymentSettings.AppName %> costs <strong><%=PaymentSettings.CurrencySign%><%= SinglePrice %></strong> per license. Your licenses will be e-mailed to you immediately after your order is processed.</p>

<%
    Dim DefAction As String

    If PayPalSelected Then
        DefAction = If(PaymentSettings.PayPalSandbox, "https://www.sandbox.paypal.com/cgi-bin/webscr", "https://www.paypal.com/cgi-bin/webscr")
    ElseIf MoneybookersSelected Then
        DefAction = "https://www.moneybookers.com/app/payment.pl"
    Else
        DefAction = ""
    End If
%>
<form action="<%=DefAction%>" method="post" id="signup_form">
<%
    Dim HeaderShown As Integer = 1

    If PaymentSettings.UseAuthorizeNetCC Or PaymentSettings.UseAuthorizeNetBank Then%>
		<h3 id="su<%= HeaderShown %><% HeaderShown+=1 %>">Your info</h3>
		<table>
			<tr<%= If(CCProc.FirstNameErr, " class=""error""", "") %>>
				<th><label for="form_first_name">First name</label></th>
				<td><input type="text" id="form_first_name" name="first_name" value="<%= Server.HtmlEncode(CCProc.FirstName) %>" size="24"/></td>
			</tr>
			<tr<%= If(CCProc.LastNameErr, " class=""error""", "") %>>
				<th><label for="form_last_name">Last name</label></th>
				<td><input type="text" id="form_last_name" name="last_name" value="<%= Server.HtmlEncode(CCProc.LastName) %>" size="24"/></td>
			</tr>
			<tr<%= If(CCProc.EmailErr, " class=""error""", "") %>>
				<th><label for="form_email">Email</label></th>
				<td><input type="text" id="form_email" name="email" value="<%= Server.HtmlEncode(CCProc.Email) %>" size="24"/></td>
			</tr>
		</table>
<%  End If%>

	<h3 id="su<%= HeaderShown %><% HeaderShown+=1 %>">Number of licenses</h3>
	<table> 
		<tr<%= If(Not(CCProc is Nothing) AndAlso CCProc.QuantityErr, " class=""error""", "") %>> 
			<th><label for="quantity">Number of licenses </label></th> 
			<td><input type="text" name="quantity" id="quantity" value="<%= If(CCProc is Nothing, 1, CCProc.Quantity) %>" size="2"/></td>
		</tr>
        <% If (Not (CCProc Is Nothing) AndAlso CCProc.QuantityErr) Then%><tr class="error"><th> </th><td><div class="error">You must enter a quantity greater than 0</div></td></tr><% End If%>
		<tr>
			<th>Price per license </th>
			<td class="large"><%=PaymentSettings.CurrencySign%><%= SinglePrice %></td>
		</tr>
	</table>

<%
    If (TotalPaymentMethods = 1 And (PaymentSettings.UseAuthorizeNetCC Or PaymentSettings.UseAuthorizeNetBank)) Then
        Response.Write("<h3 id=""su" + HeaderShown + """>Enter your billing information &mdash; SECURE</h3>")
    ElseIf TotalPaymentMethods > 1 Then

        ' form the payment selection header
        Dim PaymentHead As String = "Payment method &mdash; "

        Dim Used = 0

        If PaymentSettings.UseAuthorizeNetCC Then
            PaymentHead += "Credit card"
            Used += 1
        End If

        If PaymentSettings.UseAuthorizeNetBank Then
            If Used > 0 Then
                PaymentHead += If(TotalPaymentMethods = (Used + 1), If(TotalPaymentMethods > 2, ", or ", " or "), ", ")
            End If

            PaymentHead += "Bank transfer"
            Used += 1
        End If

        If PaymentSettings.UsePayPal Then
            If Used > 0 Then
                PaymentHead += If(TotalPaymentMethods = (Used + 1), If(TotalPaymentMethods > 2, ", or ", " or "), ", ")
            End If

            PaymentHead += "PayPal"
            Used += 1
        End If
        
        If PaymentSettings.UseMoneybookers Then
            If Used > 0 Then
                PaymentHead += If(TotalPaymentMethods = (Used + 1), If(TotalPaymentMethods > 2, ", or ", " or "), ", ")
            End If

            PaymentHead += "Skrill"
        End If
%>
	<h3 id="su<%= HeaderShown %>"><%= PaymentHead %></h3>
	<div id="cc-type">
	<% If (PaymentSettings.UseAuthorizeNetCC) Then%>
		<input type="radio" id="Card" value="Card" name="mf_cardtype" title="Card"<%= If(AuthNetCCSelected, " checked=""checked""", "") %>/>
		<label for="Card" title="Pay with a credit card" id="cc-card" class="link">Visa / American Express / MasterCard / Discover / JCB</label>
	<%End If%>
	<%If (PaymentSettings.UseAuthorizeNetBank) Then%>
		<input type="radio" id="Bank" value="Bank" name="mf_cardtype" title="Bank"<%= If(AuthNetBankSelected, " checked=""checked""", "") %>/>
		<label for="Bank" title="Pay with an eCheck (Bank transfer)" id="cc-bank" class="link">Pay with an eCheck (Bank transfer)</label>
	<%End If%>
	<%If (PaymentSettings.UsePayPal) Then%>
		<input type="radio" id="PayPal" value="PayPal" name="mf_cardtype" title="PayPal"<%= If(PayPalSelected, " checked=""checked""", "") %>/>
		<label for="PayPal" title="Pay with PayPal" id="cc-paypal" class="link">PayPal</label>
	<%End If%>
	<%If (PaymentSettings.UseMoneybookers) Then%>
		<input type="radio" id="Moneybookers" value="Moneybookers" name="mf_cardtype" title="Skrill"<%= If(MoneybookersSelected, " checked=""checked""", "") %>/>
		<label for="Moneybookers" title="Pay with Skrill" id="cc-mb" class="link">Skrill</label>
	<%End If%>
	</div>
<%
    End If
%>


<%
    If (PaymentSettings.UseAuthorizeNetCC) Then
%>
	<div id="cc_info">
		<table>
<%
    If TotalPaymentMethods = 1 Then
        Response.Write("<tr><th>We accept</th><td colspan=""2""><div id=""cc-card"">Visa / American Express / MasterCard / Discover / JCB</div><input type=""hidden"" name=""mf_cardtype"" value=""Card""/></td></tr>")
    End If
%>
			<tr<%= If(CCProc.CCNumErr, " class=""error""", "") %>>
				<th><label for="form_ccnum">Card number</label></th>
				<td><input id="form_ccnum" name="cc_num" size="22" type="text" value="<%= Server.HtmlEncode(CCProc.CCNumber) %>"/></td>
				<td valign="top"><div class="cc-secure">Secure</div></td>
			</tr>
			<% If (CCProc.CCNumErr) Then%><tr class="error"><th> </th><td><div class="error"><%= CCProc.CCNumErrMsg %></div></td></tr><% End If%>
			<tr<%= If(CCProc.CCExpErr, " class=""error""", "") %>>
				<th><label for="form_cc_expires">Expires on</label></th>
				<td>
			<select id="form_cc_expires" name="cc_expires_m">
				<option <%= If(CCProc.CCExpiresM = 1 Or CCProc.CCExpiresM = 0, "selected=""selected"" ", "") %>value="1">1 - January</option>
				<option <%= If(CCProc.CCExpiresM = 2, "selected=""selected"" ", "") %>value="2">2 - February</option>
				<option <%= If(CCProc.CCExpiresM = 3, "selected=""selected"" ", "") %>value="3">3 - March</option>
				<option <%= If(CCProc.CCExpiresM = 4, "selected=""selected"" ", "") %>value="4">4 - April</option>
				<option <%= If(CCProc.CCExpiresM = 5, "selected=""selected"" ", "") %>value="5">5 - May</option>
				<option <%= If(CCProc.CCExpiresM = 6, "selected=""selected"" ", "") %>value="6">6 - June</option>
				<option <%= If(CCProc.CCExpiresM = 7, "selected=""selected"" ", "") %>value="7">7 - July</option>
				<option <%= If(CCProc.CCExpiresM = 8, "selected=""selected"" ", "") %>value="8">8 - August</option>
				<option <%= If(CCProc.CCExpiresM = 9, "selected=""selected"" ", "") %>value="9">9 - September</option>
				<option <%= If(CCProc.CCExpiresM = 10, "selected=""selected"" ", "") %>value="10">10 - October</option>
				<option <%= If(CCProc.CCExpiresM = 11, "selected=""selected"" ", "") %>value="11">11 - November</option>
				<option <%= If(CCProc.CCExpiresM = 12, "selected=""selected"" ", "") %>value="12">12 - December</option>
			</select>

			<select id="form_cc_expires_y" name="cc_expires_y">
			<%
			    ' form the years dropdown list from the current year to 20 years from now
			    Dim currYear As Integer = DateTime.Now.Year
			    Dim endYear As Integer = currYear + 20
			    Dim expYear As Integer = If(CCProc.CCExpiresY = 0, currYear, CCProc.CCExpiresY)

			    For i As Integer = currYear To endYear
			        Response.Write("<option")

			        If expYear = i Then
			            Response.Write(" selected=""selected""")
			        End If

			        Response.Write(" value=""" + i.ToString + """>" + i.ToString + "</option>")
			    Next i
			%>
			</select>

				</td>
			</tr>
			<% If (CCProc.CCExpErr) Then%><tr class="error"><th> </th><td><div class="error">The date you've entered has expired. Either correct the date or use a card that hasn't expired.</div></td></tr><% End If%>
			<tr<%= If(CCProc.CCAddrErr, " class=""error""", "") %>>
				<th><label for="form_cc_adr">Street address</label></th>
				<td><input id="form_cc_adr" name="cc_adr" size="22" type="text" value="<%= Server.HtmlEncode(CCProc.CCAddress) %>"/></td>
			</tr>
			<tr<%= If(CCProc.CCZipErr, " class=""error""", "") %>>
				<th><label for="form_cc_zip">Billing ZIP code</label></th>
				<td><input id="form_cc_zip" name="cc_zip" size="10" type="text" value="<%= Server.HtmlEncode(CCProc.CCZip) %>"/></td>
			</tr>
			<tr>
				<th> </th>
				<td><small>(or Postal Code if not in the USA)</small></td>
			</tr>
		</table>
	</div>
<%
End If

If PaymentSettings.UseAuthorizeNetBank Then
    If TotalPaymentMethods = 1 Then
        Response.Write("<input type=""hidden"" name=""mf_cardtype"" value=""Bank""/>")
    End If
%>
	<div id="bank_info">
		<table>
			<% If Not (CCProc.BankErrMsg Is Nothing) Then%><tr class="error"><th> </th><td><div class="error"><%= CCProc.BankErrMsg %></div></td></tr><% End If%>
			<tr<%= If(CCProc.BankAcctNameErr, " class=""error""", "") %>>
				<th><label for="form_bank_acct_name">Name on account</label></th>
				<td><input id="form_bank_acct_name" name="bank_acct_name" size="27" type="text" value="<%= Server.HtmlEncode(CCProc.BankAcctName) %>"/></td>
				<td valign="top" align="center"><div class="cc-secure">Secure</div></td>
			</tr>

			<tr<%= If(CCProc.BankAcctTypeErr, " class=""error""", "") %>>
				<th><label for="form_acct_type">Type of account</label></th>
				<td>
					<select id="form_acct_type" name="bank_acct_type">
						<option <%= If(string.IsNullOrEmpty(CCProc.BankAcctType), "selected=""selected"" ", "") %>value="">&mdash; Select one &mdash;</option>
						<option <%= If(CCProc.BankAcctType = "C", "selected=""selected"" ", "") %>value="C">Checking</option>
						<option <%= If(CCProc.BankAcctType = "S", "selected=""selected"" ", "") %>value="S">Savings</option>
					</select>
				</td>
			</tr>
			<% If (CCProc.BankAcctTypeErr) Then%><tr class="error"><th> </th><td><div class="error">Select the type of bank account from the dropdown.</div></td></tr><%End If%>

			<tr<%= If(CCProc.BankNameErr, " class=""error""", "") %>>
				<th><label for="form_bank_name">Bank Name</label></th>
				<td><input id="form_bank_name" name="bank_name" size="27" type="text" value="<%= Server.HtmlEncode(CCProc.BankName) %>"/></td>
			</tr>
			<tr<%= If(CCProc.BankRouteNumErr, " class=""error""", "") %>>
				<th><label for="form_bank_route_num">ABA Routing Number</label></th>
				<td><input id="form_bank_route_num" name="bank_route_num" size="11" type="text" value="<%= Server.HtmlEncode(CCProc.BankRouteNum) %>"/></td>
			</tr>
			<% If (CCProc.BankRouteNumErr) Then%><tr class="error"><th> </th><td><div class="error">The bank routing number is not valid &mdash; enter the correct 9 digits. You can find this number on your checks.</div></td></tr><% End If%>

			<tr<%= If(CCProc.BankAcctNumErr, " class=""error""", "") %>>
				<th><label for="form_bank_acct_num">Account Number</label></th>
				<td><input id="form_bank_acct_num" name="bank_acct_num" size="18" type="text" value="<%= Server.HtmlEncode(CCProc.BankAcctNum) %>"/></td>
			</tr>
		</table>
	</div>
<%
End If

If (PaymentSettings.UsePayPal Or PaymentSettings.UseMoneybookers) Then %>
	<input type="hidden" name="amount" value="<%= SinglePrice %>" id="mb_amt"/>
<%
End If

If PaymentSettings.UsePayPal Then
%>
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
    If Not (String.IsNullOrEmpty(PaymentSettings.YourLogo)) Then
        Response.Write("<input type=""hidden"" name=""image_url"" value=""" + PaymentSettings.YourLogo + """/>")
    End If
End If

If PaymentSettings.UseMoneybookers Then%>
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
    If Not (String.IsNullOrEmpty(PaymentSettings.YourLogo)) Then
        Response.Write("<input type=""hidden"" name=""logo_url"" value=""" + PaymentSettings.YourLogo + """/>")
    End If
End If
%>

<div id="totalline" class="large">Your total: <strong><%=PaymentSettings.CurrencySign%><span id="total_price"><%= SinglePrice %> per license</span></strong></div>
<div><input type="image" src="<%= Page.ClientScript.GetWebResourceUrl(GetType(payment), "LimeLM_API.order.png") %>" name="submit" id="submit" alt="Place my order" style="width:180px;height:38px;vertical-align:middle;"/>&nbsp;&nbsp;<strong id="progress"></strong></div>
</form>

<%
    ' Show warnings and errors in the config.

    ' Don't delete these lines. You won't see them on your
    ' live site if you have everything configured correctly.

    If (PaymentSettings.UsePayPal And PaymentSettings.PayPalSandbox) Then
        Response.Write("<p><strong><font color=""#FF0000"">PayPal sandbox is on</font> &mdash; payments will not be sent to your live account. To disable sandbox mode set <code>PayPalSandbox = False</code> in <code>PaymentSettings.vb</code>.</strong></p>")
    End If

    If ((PaymentSettings.UseAuthorizeNetCC Or PaymentSettings.UseAuthorizeNetBank) And PaymentSettings.AuthNetTest) Then
        Response.Write("<p><strong><font color=""#FF0000"">Authorize.Net payments is in Test Mode</font> &mdash; payments will not be sent to your live account. To turn off test mode set <code>AuthNetTest = False</code> in <code>PaymentSettings.vb</code>.</strong></p>")

        If Not Request.IsSecureConnection Then
            Response.Write("<p><strong><font color=""#FF0000"">Authorize.Net payments <em>requires</em> a secure connection (https://).</font> &mdash; when you set <code>AuthNetTest = False</code> in <code>PaymentSettings.vb</code> this page will automatically redirect you to the secure version of this page (https://) if you visit it from the non-secure version (http://). However, in test mode you can use the non-secure version to remove the hassle of creating self-signed SSL certificates.</strong></p>")
        End If
    End If
%>

</div></body></html>