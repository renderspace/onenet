<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="SubscriptionManager.aspx.cs" Inherits="OneMainWeb.SubscriptionManager" Title="One.NET Newsletter" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Collections.Generic"%>
<%@ Import Namespace="One.Net.BLL"%>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />

		        <div class="adminSection">
                    <div class="select">
                        <asp:Label AssociatedControlID="ddlNewsletters" runat="server" ID="lblNewsletters" Text="Newsletter" />        
                        <asp:DropDownList ID="ddlNewsletters" runat="server" />
                    </div>
                    <div class="select">
                        <asp:Label AssociatedControlID="ddlSubscriptionFilter" runat="server" ID="lblSubscriptionFilter"
                            Text="Subscription filter" />        
                        <asp:DropDownList ID="ddlSubscriptionFilter" runat="server" />
                    </div>
                    
  	                <asp:LinkButton ID="cmdDisplaySubscriptions" runat="server" Text="Display subscriptions" OnClick="cmdDisplaySubscriptions_Click" CssClass="btn btn-success" />
  	                <asp:LinkButton ID="CmdExportSubscriptions" runat="server" Text="Export subscriptions" OnClick="CmdExportSubscriptions_Click"  CssClass="btn btn-default" />          
  	                <asp:LinkButton ID="cmdDisplayCSLSubscriptions" runat="server" Text="Display CSL subscriptions" OnClick="cmdDisplayCSLSubscriptions_Click"  CssClass="btn btn-default" />
                </div>        

                <asp:GridView ID="GridViewSubscriptions" runat="server" CssClass="table table-hover"
                    AllowSorting="True"
                    AutoGenerateColumns="False"
                    DataKeyNames="SubscriptionId"
                    OnRowCommand="GridViewSubscriptions_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Id" SortExpression="ns.id">
                            <ItemTemplate>
                                    <%# Eval("SubscriptionId")%>
                            </ItemTemplate>
                        </asp:TemplateField>                            
                        <asp:TemplateField HeaderText="Email" SortExpression="email">
                            <ItemTemplate>
                                    <%# Eval("Email")%>
                            </ItemTemplate>
                        </asp:TemplateField>                            
                        <asp:TemplateField HeaderText="Subscribed" SortExpression="date_subscribed">
                            <ItemTemplate>
                                    <%# ((bool)Eval("Subscribed")) ? ((DateTime?)Eval("DateSubscribed")).Value.ToString() : ""%>
                            </ItemTemplate>
                        </asp:TemplateField>                            
                        <asp:TemplateField HeaderText="Confirmed" SortExpression="date_confirmed">
                            <ItemTemplate>
                                    <%# ((bool)Eval("Confirmed")) ? ((DateTime?)Eval("DateConfirmed")).Value.ToString() : "" %>
                            </ItemTemplate>
                        </asp:TemplateField>  

                        <asp:TemplateField HeaderText="">
                            <ItemTemplate>
                                    <asp:Literal ID="litHash" runat="server" Text='<%# ((string)Eval("Hash")) %>' Visible="false" />
                                    <asp:Literal ID="litSubId" runat="server" Text='<%# (Eval("SubscriptionId")).ToString() %>' Visible="false" />
                                    <asp:LinkButton CommandName="UnSubscribe" Visible='<%# ((bool)Eval("Subscribed")) ? true : false %>' ID="cmdUnsubscribe" runat="server" Text="Unsubscribe" CommandArgument='<%# Container.DataItemIndex %>' />
                            </ItemTemplate>
                        </asp:TemplateField>                                                                                      
                    </Columns>
                </asp:GridView>  

                <two:PostbackPager id="TwoPostbackPager1" OnCommand="TwoPostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" CssClass="text-center" />	
                <asp:Panel runat="server" ID="PanelNoResults"  CssClass="col-md-12">
                     <div class="alert alert-info" role="alert">
                        No subscriptions to show.
                         </div>
                </asp:Panel> 
    
            <asp:PlaceHolder runat="server" ID="plhCSLSubs">
                <div class="adminSection">
                    <div class="row">
                        <asp:TextBox Rows="15" TextMode="multiLine" ID="txtCSV" runat="server" CssClass="col-sm-12" />
                    </div>
                </div>
            </asp:PlaceHolder>                    
</asp:Content>
