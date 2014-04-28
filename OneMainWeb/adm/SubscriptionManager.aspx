<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="SubscriptionManager.aspx.cs" Inherits="OneMainWeb.SubscriptionManager" Title="$sub_manager" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Collections.Generic"%>
<%@ Import Namespace="One.Net.BLL"%>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />
    
    <two:TabularMultiView ID="tabMultiview" runat="server" OnViewIndexChanged="tabMultiview_OnViewIndexChanged">
        <two:TabularView ID="tabListSubscriptions" runat="server" TabName="$subscription_list">
            <asp:PlaceHolder ID="plhNewsletterList" runat="server">
		        <div class="searchFull">
                    <div class="select">
                        <asp:Label AssociatedControlID="ddlNewsletters" runat="server" ID="lblNewsletters" Text="$newsletter" />        
                        <asp:DropDownList ID="ddlNewsletters" runat="server" />
                    </div>
                    <div class="select">
                        <asp:Label AssociatedControlID="ddlSubscriptionFilter" runat="server" ID="lblSubscriptionFilter"
                            Text="$subscription_filter" />        
                        <asp:DropDownList ID="ddlSubscriptionFilter" runat="server" />
                    </div>
                    
  	                <asp:Button ID="cmdDisplaySubscriptions" runat="server" Text="$display_subscriptions" OnClick="cmdDisplaySubscriptions_Click" />
  	                <asp:Button ID="CmdExportSubscriptions" runat="server" Text="$export_subscriptions" OnClick="CmdExportSubscriptions_Click" />  	                
  	                <asp:Button ID="cmdDisplayCSLSubscriptions" runat="server" Text="$display_csl_subscriptions" OnClick="cmdDisplayCSLSubscriptions_Click" />
                </div>        
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="plhCSLSubs">
                <two:Input Required="false" Rows="15" Text="$csv" TextMode="multiLine" ID="txtCSV" ValidationGroup="MG" runat="server" />
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="plhSubscriptions" runat="server">
                  <div class="centerFull">
                    <div class="biggv">  
                        <asp:GridView ID="subscriptionGridView" runat="server" PageSize="10" PageIndex="0"
                            PagerSettings-Mode="NumericFirstLast"
                            PagerSettings-LastPageText="$last"
                            PagerSettings-FirstPageText="$first"
                            PagerSettings-PageButtonCount="7" 
                            AllowPaging="True" 
                            AllowSorting="True"
                            AutoGenerateColumns="False"
                            DataSourceID="SubscriptionSource" 
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
                       <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
                            EnablePaging="True" ID="SubscriptionSource" runat="server" SelectMethod="ListSubscriptions"
                            TypeName="OneMainWeb.SubscriptionHelper" SelectCountMethod="GetSubscriptionCount" OnSelecting="SubscriptionSource_Selecting" SortParameterName="sortBy">
                            <SelectParameters>
                                <asp:ControlParameter Type="Int32" ControlID="ddlNewsletters" Name="newsletterId" PropertyName="SelectedValue" />
                                <asp:ControlParameter Type="Int32" ControlID="ddlSubscriptionFilter" Name="subscriptionType" PropertyName="SelectedValue" />
                            </SelectParameters> 
                       </asp:ObjectDataSource>    
                </div>
            </div>                                  
            </asp:PlaceHolder>
        </two:TabularView>
        <two:TabularView ID="tabMaintenance" runat="server" TabName="$maintenance">
	        <div class="searchFull">
                <div class="select">
                    <asp:Label AssociatedControlID="ddlNewsletterFilter" runat="server" ID="Label1"
                        Text="$newsletter" />        
                    <asp:DropDownList ID="ddlNewsletterFilter" runat="server" />
                </div>
                <two:ValidInput ID="txtBackFromDays" runat="server" Text="$start_from_days_ago" ValidationGroup="DS" ValidationType="integer" />
                <asp:Button ValidationGroup="DS" ID="cmdDeleteUnconfirmedSubscriptions" runat="server" Text="$delete_unconfirmed_subscriptions" OnClick="cmdDeleteUnconfirmedSubscriptions_Click" /><br />
                <asp:CheckBox ID="chkDeleteAll" runat="server" Text="$delete_all_subscriptions_precaution" />
                <asp:Button ID="cmdDeleteSubscriptions" runat="server" Text="$delete_all_subscriptions" OnClick="cmdDeleteSubscriptions_Click" />
            </div>        
        </two:TabularView>
        <two:TabularView ID="tabImport" runat="server" TabName="$import">
	        <div class="searchFull">
                <div class="select">
                    <asp:Label AssociatedControlID="DropDownListNewsletters" runat="server" ID="LabelNewsletters" Text="$newsletters"></asp:Label>
                    <asp:DropDownList ID="DropDownListNewsletters" runat="server" />
                </div>
                <two:Input ValidationGroup="EI" runat="server" ID="InputEmails" TextMode="multiLine" Rows="20" Text="$emails" />
                <asp:Button OnClick="CmdImport_Click" ValidationGroup="EI" runat="server" ID="CmdImport" Text="$import" />
	        </div>
        </two:TabularView>
    </two:TabularMultiView>
</asp:Content>
