<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Website.aspx.cs" Inherits="OneMainWeb.adm.Website" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="notifier" />

    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="MultiView1_ActiveViewChanged" ActiveViewIndex="0">
        <asp:View runat="server">
            <div class="searchFull">
			    <div class="col-md-2">
                    <asp:LinkButton ID="ButtonStartWizard" runat="server" onclick="ButtonStartWizard_Click"  text="<span class='glyphicon glyphicon-plus'></span> New website" CssClass="btn btn-success" />
			    </div>
            </div>
            <asp:GridView	ID="GridViewWebsites"
					runat="server"
					CssClass="gv"
					AutoGenerateColumns="false"
					AllowPaging="false"
					AllowSorting="false"
					DataKeyNames="Id">
		        <Columns>
                    <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true" />
                    <asp:BoundField HeaderText="Title" DataField="DisplayName" />
                    <asp:BoundField HeaderText="Last Changed" DataField="DisplayLastChanged" />
                </Columns>
            </asp:GridView>

        </asp:View>
        <asp:View runat="server">

            <div class="form-group">
                <label class="col-sm-3 control-label">Website title</label>
                <div class="col-sm-9">
                    <asp:TextBox ValidationGroup="website" Text="" ID="InputTitle" runat="server" CssClass="form-control" MaxLength="255" placeholder="website title is important for SEO" />
                </div>
            </div>
            <div class="form-group">
                <label class="col-sm-3 control-label">Language</label>
                <div class="col-sm-9">
                    <asp:DropDownList ID="DropDownList1" ValidationGroup="website" runat="server" />
                </div>
            </div>
            <div class="form-group">
                <label class="col-sm-3 control-label">Preview website address</label>
                <div class="col-sm-9">
                    <asp:TextBox ValidationGroup="website" Text="" ID="TextBoxPreviewUrl" runat="server" CssClass="form-control" MaxLength="255" placeholder="typically http://sitename.w.renderspace.net (in general: http://preview.example.com)" />
                </div>
            </div>

            <div class="form-group">
                <label class="col-sm-3 control-label">Production website address</label>
                <div class="col-sm-9">
                    <asp:TextBox ValidationGroup="website" Text="" ID="TextBoxProductionUrl" runat="server" CssClass="form-control" MaxLength="255" placeholder="http://www.example.com" />
                </div>
            </div>

            <div class="form-group">
                <div class="col-sm-offset-3 col-sm-9">
                    <div class="checkbox">
                        <label>
                            <asp:CheckBox runat="server" ID="CheckboxNewDatabase" CssClass="j_control_new_database" /> Create new database (you'll need admin password for database).
                        </label>
                    </div>
                </div>
            </div>

            

            <div class="form-group">

                <asp:LinkButton  ValidationGroup="website" id="ButtonAdd" runat="server" OnClick="ButtonAdd_Click" text="<span class='glyphicon glyphicon-plus'></span> Create website" CssClass="btn btn-success" />
           </div>

        </asp:View>
    </asp:MultiView>

	
</asp:Content>