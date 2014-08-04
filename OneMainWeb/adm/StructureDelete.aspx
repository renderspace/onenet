<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="StructureDelete.aspx.cs" Inherits="OneMainWeb.adm.StructureDelete" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="notifier" />


    <div class="centerFull">
                    <div class="centerStructure">
                        <div id="treeHolder2" runat="server" class="treeHolder">
                            <asp:TreeView OnUnload="TreeView2_Unload" ID="TreeView2" runat="server" BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" OnSelectedNodeChanged="TreeView2_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeView2_SelectedNodeChanged" Width="270" ExpandDepth="3">
                            </asp:TreeView>
                        </div>
                    </div>   
                    <div class="mainEditor">
                        Selected page: <asp:Label runat="server" ID="LabelSelectedPageName"></asp:Label><br />
                        Selected page ID: <asp:Label runat="server" ID="LabelSelectedPageId"></asp:Label>                                
                    </div>                         
                    <div class="form-group">
                        <asp:Button OnPreRender="ButtonDelete_PreRender" ID="ButtonDelete" runat="server" text="Delete node and everything below" OnClick="ButtonDelete_Click" CssClass="delete-btn" />
                    </div>
                </div>
</asp:Content>
