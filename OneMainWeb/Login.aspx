<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OneMainWeb.Login" %>
<%@ Import namespace="One.Net.BLL" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title></title>
	<link href="../Utils/bootstrap.min.css" rel="stylesheet" />
	<style type="text/css">
	@import url(http://fonts.googleapis.com/css?family=Roboto:400,700&subset=latin,latin-ext);
	body {font-family: 'Roboto', sans-serif!important; font-size: 13px; margin: 0; padding: 0;}
	h1 {font-size:11px;margin:0;padding: 0 0 5px 110px;}
	.innerLogin {float:left;width:400px;position:relative;}
	label {display:block;float:left; width:100px;text-align:right;padding-right: 10px;padding-top:3px;}
	label em {font-style:normal;}
	input {float:left;}
	input[type='text'],input[type='password'] {border: 1px #ccc solid;padding: 3px 5px;}
	.innerLogin div {width:300px;float:left;padding-bottom:5px;}
	.rememberMe {width:auto;padding-left: 106px;}
	.rememberMe label {width:150px;}
	.subPanel {width:auto;padding-left: 100px;}
	.subPanel input {font-size: 11px;font-weight:bold;padding: 1px 0 2px 0;width:70px;cursor:pointer;}
	.innerLogin .failure {position:absolute;top:18px;right:0;width:130px;color: #a40101;font-weight:bold;}
	.browser-link { display: block;padding: 4px;padding-top: 110px;background-repeat: no-repeat;background-position: center top;text-decoration: none;text-align: center; }
	.browser-logos { padding-top: 20px; }
	</style>
</head>

<body>
	<form id="form1" runat="server">
	    <% if (!HasBrowserError)
	       { %>
		<div>
			<table style="margin-top: 100px; border-top: 3px solid #000000; border-bottom: 3px solid #000000; background-color: #EBEBE5; border-collapse: collapse; color: #000;">
				<tr>
					<td colspan="6" style="height: 15px;">&nbsp;</td>
				</tr>
				<tr>
					<td style="width: 240px;">
					    <span style="width: 100%; text-align: right; display:block;"><%= AppVersion %></span>
					    <img runat="server" src="~/Res/one-net-logo-vs.png" alt="One.NET" style="margin-left: 20px;" />
					</td>
					<td class="td-splitter-blue"></td>
					<td style="width: 500px;" class="loginscreen">
						 <asp:Login ID="Login1" runat="server" Width="483px" Font-Names="Verdana" Font-Size="11px" ForeColor="Black"  RememberMeSet="false" DisplayRememberMe="false"
						 VisibleWhenLoggedIn="False" DestinationPageUrl="~/adm/ContentEdit.aspx" OnLoggedIn="Login1_LoggedIn" />
					</td>
					<td class="td-splitter-blue"></td>
					<td style="width: 240px;"></td>
					<td style="width: 100%"></td>
				</tr>
				<tr>
					<td colspan="6" style="height: 15px;">&nbsp;</td>
				</tr>
			</table>
		</div>
        <% }
	       else
	       { %>
		<div>
			<table style="margin-top: 100px;width:100%; border-top: 3px solid #000000; border-bottom: 3px solid #000000; background-color: #EBEBE5; border-collapse: collapse; color: #000;">
				<tr>
					<td colspan="5" style="height: 15px;width:100%">&nbsp;</td>
				</tr>
                <tr>
                    <td style="width:202px;">
					    <span style="width: 100%; text-align: right; display:block;"><%= AppVersion %></span>
					    <img id="Img1" runat="server" src="~/Res/one-net-logo-vs.png" alt="One.NET" style="margin-left: 20px;" />
					</td>
					<td class="td-splitter-blue"></td>
                    <td style="padding-left:20px; padding-right:20px;width:500px;white-space:nowrap;">
                        <%=ResourceManager.GetString("$browser_error") %>
                        <table class="browser-logos">
                            <tbody><tr>
                            <td>
                                <a class="browser-link" style="background-image: url('/Res/browsers/ff.png');" href="http://www.mozilla.com/firefox/" target="_blank"><span class="bro">Firefox</span>   
                                <span class="vendor">Mozilla Foundation</span></a>
                            </td>
                            <td>
                                <a class="browser-link" style="background-image: url('/Res/browsers/ch.png');" href="http://www.google.com/chrome?hl=en" target="_blank"><span class="bro">Chrome</span>   
                                <span class="vendor">Google</span></a>
                            </td>
                            <td>
                                <a class="browser-link" style="background-image: url('/Res/browsers/ie.png');" href="http://windows.microsoft.com/en-GB/internet-explorer/downloads/ie" target="_blank"><span class="bro">Internet Explorer</span>   
                                <span class="vendor">Microsoft</span></a>
                            </td>
    
                            </tr>
                        </tbody></table>
                    </td>
					<td class="td-splitter-blue"></td>
					<td style="width: 100%;"></td>
                </tr>
				<tr>
					<td colspan="5" style="height: 15px;">&nbsp;</td>
				</tr>
            </table>
        </div>
        <% } %>
	</form>
</body>
</html>
