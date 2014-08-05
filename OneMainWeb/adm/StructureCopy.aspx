<%@ Page Title="One.NET copy" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="StructureCopy.aspx.cs" Inherits="OneMainWeb.adm.StructureCopy" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="notifier" />
    <div class="centerFull">
            <div class="centerStructure">
                <div id="treeHolder" runat="server" class="treeHolder">
                    <asp:TreeView OnUnload="TreeView1_Unload" ID="TreeView1" runat="server" BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" OnSelectedNodeChanged="TreeView1_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeView1_SelectedNodeChanged" Width="270" ExpandDepth="3">
                    </asp:TreeView>
                </div>
            </div>            
            <div class="mainEditor">
                <asp:Label ID="LabelEmptyWebSites" runat="server" AssociatedControlID="CheckBoxListEmptyWebSites" Text="$empty_websites" />
                <asp:CheckBoxList ID="CheckBoxListEmptyWebSites" runat="server"></asp:CheckBoxList>
            </div>
            <div class="form-group">
                <asp:Button ID="ButtonCopy" runat="server" text="$copy_pages_to_site" OnClick="ButtonCopy_Click" />
            </div>
        </div>

</asp:Content>
