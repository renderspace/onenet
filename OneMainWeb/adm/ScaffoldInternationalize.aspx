<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="ScaffoldInternationalize.aspx.cs" Inherits="OneMainWeb.adm.ScaffoldInternationalize" %>
<%@ Register TagPrefix="bll" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="cc1" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <cc1:Notifier runat="server" ID="Notifier1" />

    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="MultiView1_ActiveViewChanged">
        <asp:View runat="server" ID="View1">
             <div class="adminSection">
			    <div class="col-md-5" >
                    <asp:DropDownList runat="server" DataTextField="FriendlyName" DataValueField="Id" ID="DropDownListVirtual"></asp:DropDownList>
                    <asp:LinkButton ID="ButtonSelect" runat="server" onclick="ButtonSelect_Click"  text="<span class='glyphicon glyphicon-plus'></span> Select table" CssClass="btn btn-success" />
                </div>
            </div>

            <asp:GridView ID="GridViewItems" runat="server" CssClass="table table-hover" AutoGenerateColumns="false" OnRowCommand="GridViewItems_RowCommand" OnRowDataBound="GridViewItems_RowDataBound">
                <Columns>
                    <asp:BoundField HeaderText="Id" DataField="Id" />
                    <asp:BoundField HeaderText="Column type" DataField="ColumnType" /> 
                    <asp:BoundField HeaderText="FriendlyName" DataField="FriendlyName" />
                    <asp:BoundField HeaderText="Description" DataField="Description" />
                    <asp:TemplateField HeaderText="Wysiwyg">
                        <ItemTemplate>
                            <asp:CheckBox Enabled="false" ID="CheckBoxWysiwyg" runat="server" Checked='<%# bool.Parse((string)Eval("IsWysiwyg")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>       
                    <asp:TemplateField HeaderText="MultiLanguageContent">
                        <ItemTemplate>
                            <asp:CheckBox Enabled="false" ID="CheckBoxIsMultiLanguageContent" runat="server" Checked='<%# bool.Parse((string)Eval("IsMultiLanguageContent")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>     
                    <asp:TemplateField HeaderText="Show on list">
                        <ItemTemplate>
                            <asp:CheckBox Enabled="false"  ID="CheckBoxShowOnList" runat="server" Checked='<%# bool.Parse((string)Eval("ShowOnList")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton ID="CmdInternationalize" runat="server" Text="Internationalize" CommandArgument='<%# Eval("Id") %>' CommandName="internationalize" CssClass="btn btn-danger btn-xs" />
                        </ItemTemplate>
                    </asp:TemplateField>                    
                </Columns>
            </asp:GridView>

        </asp:View>

        <asp:View runat="server" ID="View3">
            <asp:Label ID="Label1" runat="server" Text="No configuration tables found."></asp:Label>
        </asp:View>
    </asp:MultiView>

</asp:Content>
