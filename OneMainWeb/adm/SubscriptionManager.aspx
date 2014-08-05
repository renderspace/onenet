<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="SubscriptionManager.aspx.cs" Inherits="OneMainWeb.SubscriptionManager" Title="One.NET Newsletter" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Collections.Generic"%>
<%@ Import Namespace="One.Net.BLL"%>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />

    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="tabMultiview_OnViewIndexChanged" ActiveViewIndex="0">
        <asp:View ID="View1" runat="server">
                <asp:PlaceHolder ID="plhNewsletterList" runat="server">
		        <div class="adminSection">
                    <div class="select">
                        <asp:Label AssociatedControlID="ddlNewsletters" runat="server" ID="lblNewsletters" Text="$newsletter" />        
                        <asp:DropDownList ID="ddlNewsletters" runat="server" />
                    </div>
                    <div class="select">
                        <asp:Label AssociatedControlID="ddlSubscriptionFilter" runat="server" ID="lblSubscriptionFilter"
                            Text="$subscription_filter" />        
                        <asp:DropDownList ID="ddlSubscriptionFilter" runat="server" />
                    </div>
                    
  	                <asp:Button ID="cmdDisplaySubscriptions" runat="server" Text="Display subscriptions" OnClick="cmdDisplaySubscriptions_Click" />
  	                <asp:Button ID="CmdExportSubscriptions" runat="server" Text="Export subscriptions" OnClick="CmdExportSubscriptions_Click" />  	                
  	                <asp:Button ID="cmdDisplayCSLSubscriptions" runat="server" Text="Display CSL subscriptions" OnClick="cmdDisplayCSLSubscriptions_Click" />
                </div>        
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="plhCSLSubs">
                <asp:TextBox Required="false" Rows="15" Text="$csv" TextMode="multiLine" ID="txtCSV" ValidationGroup="MG" runat="server" />
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="plhSubscriptions" runat="server">
                <asp:GridView ID="subscriptionGridView" runat="server"  CssClass="table table-hover"
                    AllowSorting="True"
                    AutoGenerateColumns="False"
                    DataKeyNames="SubscriptionId"
                    OnRowCommand="subscriptionGridView_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="$subscription_id" SortExpression="ns.id">
                            <ItemTemplate>
                                    <%# Eval("SubscriptionId")%>
                            </ItemTemplate>
                        </asp:TemplateField>                            
                        <asp:TemplateField HeaderText="$email" SortExpression="email">
                            <ItemTemplate>
                                    <%# Eval("Email")%>
                            </ItemTemplate>
                        </asp:TemplateField>                            
                        <asp:TemplateField HeaderText="$subscribed" SortExpression="date_subscribed">
                            <ItemTemplate>
                                    <%# ((bool)Eval("Subscribed")) ? ((DateTime?)Eval("DateSubscribed")).Value.ToString() : ""%>
                            </ItemTemplate>
                        </asp:TemplateField>                            
                        <asp:TemplateField HeaderText="$confirmed" SortExpression="date_confirmed">
                            <ItemTemplate>
                                    <%# ((bool)Eval("Confirmed")) ? ((DateTime?)Eval("DateConfirmed")).Value.ToString() : "" %>
                            </ItemTemplate>
                        </asp:TemplateField>  

                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                    <asp:Literal ID="litHash" runat="server" Text='<%# ((string)Eval("Hash")) %>' Visible="false" />
                                    <asp:Literal ID="litSubId" runat="server" Text='<%# (Eval("SubscriptionId")).ToString() %>' Visible="false" />
                                    <asp:LinkButton CommandName="UnSubscribe" Visible='<%# ((bool)Eval("Subscribed")) ? true : false %>' ID="cmdUnsubscribe" runat="server" Text="$unsubscribe" CommandArgument='<%# Container.DataItemIndex %>' />
                            </ItemTemplate>
                        </asp:TemplateField>  
                                                                                                                  
                    </Columns>
                </asp:GridView>  
                <div class="text-center">
                    <two:PostbackPager id="TwoPostbackPager1" OnCommand="TwoPostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
                </div>                             
            </asp:PlaceHolder>
        </asp:View>
        <asp:View ID="View2" runat="server">
            <div class="adminSection">
                <div class="select">
                    <asp:Label AssociatedControlID="ddlNewsletterFilter" runat="server" ID="Label1"
                        Text="$newsletter" />        
                    <asp:DropDownList ID="ddlNewsletterFilter" runat="server" />
                </div>
                <div class="form-group">
                    <div class="col-sm-3">
                        <label>$start_from_days_ago</label>
                    </div>
                    <div class="col-sm-9">
                        <asp:TextBox ID="txtBackFromDays" runat="server" Text="" ValidationGroup="DS" type="number" />
                    </div>

                </div>
                <asp:Button ValidationGroup="DS" ID="cmdDeleteUnconfirmedSubscriptions" runat="server" Text="$delete_unconfirmed_subscriptions" OnClick="cmdDeleteUnconfirmedSubscriptions_Click" /><br />
                <asp:CheckBox ID="chkDeleteAll" runat="server" Text="$delete_all_subscriptions_precaution" />
                <asp:Button ID="cmdDeleteSubscriptions" runat="server" Text="$delete_all_subscriptions" OnClick="cmdDeleteSubscriptions_Click" />
            </div>  
        </asp:View>
        <asp:View ID="View3" runat="server">
                <div class="adminSection">
                    <div class="select">
                        <asp:Label AssociatedControlID="DropDownListNewsletters" runat="server" ID="LabelNewsletters" Text="$newsletters"></asp:Label>
                        <asp:DropDownList ID="DropDownListNewsletters" runat="server" />
                    </div>
                    <asp:TextBox ValidationGroup="EI" runat="server" ID="InputEmails" TextMode="multiLine" Rows="20" Text="$emails" />
                    <asp:Button OnClick="CmdImport_Click" ValidationGroup="EI" runat="server" ID="CmdImport" Text="$import" />
	            </div>
        </asp:View>
        <asp:View ID="View4" runat="server"></asp:View>
    </asp:MultiView>
</asp:Content>
