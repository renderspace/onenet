<%@ Page Title="One.NET delete" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="StructureDelete.aspx.cs" Inherits="OneMainWeb.adm.StructureDelete" EnableEventValidation="false" ValidateRequest="false" %>
<%@ OutputCache Location="None" VaryByParam="None" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="notifier" />
    <div class="tabl">
	    <div class="tabltr">
            <div class="s1a">
                <section class="module tall msideb">
				    <h2>Page structure</h2>
				    <div class="msideb-inn">
                        <div id="treeHolder2" runat="server" class="treeView">
                            <asp:TreeView OnUnload="TreeView2_Unload" ID="TreeView2" runat="server" OnSelectedNodeChanged="TreeView2_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeView2_SelectedNodeChanged" ExpandDepth="3">
                            </asp:TreeView>
                        </div>
                    </div>
                </section>
            </div>
            <div class="s2a">
                <div class="page-and-module-settings">
                    <section class="module page-settings msideb">
                        <div class="msideb-inn">
			                <div class="pageproperties form-horizontal validationGroup">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">Selected page</label>
                                    <div class="col-sm-8">
                                        <asp:Label runat="server" ID="LabelSelectedPageName"></asp:Label>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">Selected page ID</label>
                                    <div class="col-sm-8">
                                        <asp:Label runat="server" ID="LabelSelectedPageId"></asp:Label>
                                    </div>
                                </div>
                                <div class="form-group">

                                <div class="col-sm-12">
                                    <span class="pull-right pbtns">
                                            <asp:Button OnPreRender="ButtonDelete_PreRender" ID="ButtonDelete" runat="server" text="Delete node and everything below" OnClick="ButtonDelete_Click" CssClass="left btn btn-danger" />
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>
            </div>
        </div>
    </div>
</asp:Content>
