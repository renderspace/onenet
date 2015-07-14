<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewsSubscription.ascx.cs" Inherits="OneMainWeb.CommonModules.NewsSubscription" %>

<%@ Import Namespace="One.Net.BLL"  %>

<h1><%=Translate("newsletter_module_title") %></h1>

<asp:PlaceHolder id="PlaceHolderSubscribe" runat="server">

	<p id="PSubscribeText" runat="server" class="subscribeText"><%= Translate("newsletter_info_about") %></p>
	
    <%--<% these messages apply to both single newsletter and multiple newsletters %>--%>        
    <p id="PNoNewsletterChecked" class="notchecked" runat="server"><%=Translate("no_newsletter_checked") %></p>
    <p id="PEmailRequired" class="required" runat="server"><%=Translate("email_required") %></p>
    <p id="PEmailInvalid" class="required" runat="server"><%=Translate("email_invalid") %></p>

    <%--<% repeater apply to multiple newsletters %>--%>
    <asp:Repeater runat="server" ID="RepeaterResults" OnItemDataBound="RepeaterResults_ItemDataBound">
        <HeaderTemplate><ul></HeaderTemplate>
        <ItemTemplate>
            <li class="exists" id="liExists" runat="server"><strong><%# Eval("NewsletterName") %></strong><em><%=Translate("error_already_subscribed")%></em></li>
            <li class="failed" id="liFailed" runat="server"><strong><%# Eval("NewsletterName") %></strong><em><%=Translate("error_sending_failed")%></em></li>
            <li class="sent" id="liSent" runat="server"><strong><%# Eval("NewsletterName") %></strong><em><%=Translate("email_confirmation_sent")%></em></li>                                                
        </ItemTemplate>
        <FooterTemplate></ul></FooterTemplate>
    </asp:Repeater>
    
    <%--<% these messages apply to single newsletter %>--%>        
    <p id="PEmailExists" runat="server" class="exists"><%=Translate("error_already_subscribed")%></p>
    <p id="PSendingFailed" runat="server" class="failed"><%=Translate("error_sending_failed")%></p>
    <p id="PConfirmationEmailSent" runat="server" class="sent"><%=Translate("email_confirmation_sent")%></p>                        
    
    <asp:Panel ID="PanelEmail" runat="server" CssClass="emailPannel">
        <asp:Label AssociatedControlID="InputEmail" ID="LabelEmail" runat="server" EnableViewState="false" />
        <asp:TextBox runat="server" ID="InputEmail" CssClass="text" ValidationGroup="NLSUB" />
        <asp:CheckBoxList ID="CheckBoxListNewsletters" CssClass="multiNlItems" runat="server" />
        <span class="button"><asp:LinkButton Text="submit" ID="CmdSubscribe" CausesValidation="true" OnClick="CmdSubscribe_Click" runat="server" ValidationGroup="NLSUB" /></span>
    </asp:Panel>
	
</asp:PlaceHolder>

<asp:PlaceHolder id="PlaceHolderStatus" runat="server">
	<p id="PConfirmText" runat="server" class="confirmText"><%=Translate("newsletter_confirm_text") %></p>
	<p id="PUnsubscribeText" runat="server" class="unsubscribeText"><%=Translate("newsletter_unsubscribe_text") %></p>	
	<div class="messages">
		<p id="PEmailConfirmed" runat="server" class="emailConfirmed"><%=Translate("email_confirmed")%></p>
		<p id="PEmailConfirmationFailed" runat="server" class="confirmationFailed"><%=Translate("email_confirmation_failed")%></p>
		<p id="PUserUnsubscribed" runat="server" class="succes"><%=Translate("user_unsubscribed")%></p>
		<p id="PErrorSubscriptionAlreadyCancelled" runat="server" class="succes"><%=Translate("error_subscription_already_canceled")%></p>
    </div>	
	
</asp:PlaceHolder>
