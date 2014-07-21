<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="ContentEdit.aspx.cs" Inherits="OneMainWeb.adm.ContentEdit" Title="$content_edit" ValidateRequest="false" %>

<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<%@ Register Src="~/AdminControls/History.ascx" TagName="History" TagPrefix="uc1" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Import Namespace="One.Net.BLL"%>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        <uc1:Notifier ID="Notifier1" runat="server" />
        
        <div class="centerStructure">
            <div id="treeHolder" runat="server" class="treeHolder">
                <asp:TreeView EnableViewState="false" OnUnload="TreeView1_Unload" ID="TreeView1" runat="server" BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" OnSelectedNodeChanged="TreeView1_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeView1_SelectedNodeChanged" Width="270" ExpandDepth="3">
                </asp:TreeView>
            </div>
        </div>
        <div class="mainEditor" id="rightSettings" runat="server">
            <div class="statusBars">
                <div class="pageButtons">
                    <div class="lastChanged">
                        <two:InfoLabel ID="InfoLabelLastChange" runat="server" Text="$last_changed_date" ContainerCssClass="lastChangedStatus"></two:InfoLabel>
                    </div>
                    
                    <div class="TextContentInstance">
                        <asp:DropDownList CssClass="selectTextContentInstance" ID="DropDownListModuleInstances" runat="server" ValidationGroup="MI"></asp:DropDownList>
                        <asp:Label ID="LabelModuleInstanceName" runat="server"></asp:Label>
                        <asp:Button CssClass="changeTextContentInstance" ID="ButtonChangeModuleInstance" runat="server" OnClick="ButtonChangeModuleInstance_Click" ValidationGroup="MI" Text="$label_change_module_instance" />
                    </div>
                </div>
            </div>
            <br style="clear:both;" />
            <div class="contentEntry">
                <one:TextContentControl ID="TextContentEditor" runat="server" />
                <div class="save">
                    <asp:Button ID="cmdRevertToPublished" runat="server" OnClick="cmdRevertToPublished_Click" ValidationGroup="RTP" Text="$revert_to_published" CssClass="left" />
                    <asp:Button ID="ButtonSave" runat="server" OnClick="ButtonSave_Click" Text="Save" CssClass="right save-btn" />
                </div>		
                <%-- 
                <two:InfoLabel runat="server" ID="InfoLabelVotes" Text="$votes" ></two:InfoLabel>                                
                <two:InfoLabel runat="server" ID="InfoLabelScore" Text="$score" ></two:InfoLabel>                                
                <two:InputWithButton ValidationGroup="vote" runat="server" ValidationType="integer" ID="InputWithButtonVote" Text="" ButtonText="Vote!"  OnClick="InputWithButtonVote_Click" />
                --%>   
                <uc1:History runat="server" OnRevertToAudit="HistoryControl_RevertToAudit" id="HistoryControl" />
            </div>		     
        </div>
</asp:Content>
