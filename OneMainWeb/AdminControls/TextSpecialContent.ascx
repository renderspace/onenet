﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TextSpecialContent.ascx.cs" Inherits="OneMainWeb.AdminControls.TextSpecialContent" %>
<%@ Register TagPrefix="one" TagName="TextContentControl" Src="~/AdminControls/TextContentControl.ascx" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ Register src="LastChangeAndHistory.ascx" tagname="LastChangeAndHistory" tagprefix="uc2" %>

        <uc1:Notifier ID="Notifier1" runat="server" />
    <div class="row">
    
        <div class="col-md-3">
            <section class="module tall">
                <header><h3 class="tabs_involved">Tree structure</h3></header>
                <div id="treeHolder" runat="server" class="treeview">
                    <asp:TreeView EnableViewState="false" OnUnload="TreeView1_Unload" ID="TreeView1" runat="server" BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" OnSelectedNodeChanged="TreeView1_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeView1_SelectedNodeChanged" Width="270" ExpandDepth="3">
                    </asp:TreeView>
                </div>
            </section>
        </div>
         <div class="col-md-9">
                <section class="module">
                     <div>
                        <asp:DropDownList CssClass="selectTextContentInstance" ID="DropDownListModuleInstances" runat="server" ValidationGroup="MI"></asp:DropDownList>
                        <asp:Label ID="LabelModuleInstanceName" runat="server"></asp:Label>
                        <asp:Button CssClass="changeTextContentInstance" ID="ButtonChangeModuleInstance" runat="server" OnClick="ButtonChangeModuleInstance_Click" ValidationGroup="MI" Text="Change module instance" />
                     </div>
                </section>
                <section class="module">
                    <asp:Panel runat="server" ID="PanelEditor">
                        <one:TextContentControl ID="TextContentEditor" runat="server" />
                        <uc2:LastChangeAndHistory ID="LastChangeAndHistory1" runat="server" />
				        <div class="submit-links">
					        <asp:Button ID="cmdRevertToPublished" runat="server" OnClick="cmdRevertToPublished_Click" ValidationGroup="RTP" Text="Revert to published" CssClass="left" />
                            <asp:Button ID="ButtonSave" runat="server" OnClick="ButtonSave_Click" Text="Save" CssClass="right save-btn" />
				        </div>
                    </asp:Panel>
                </section>
             </div>
       </div> 
