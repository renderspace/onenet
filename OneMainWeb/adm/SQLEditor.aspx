<%@ Page Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="SQLEditor.aspx.cs" Inherits="OneMainWeb.adm.SQLEditor" Title="One.NET SQL" %>

<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:Notifier ID="Notifier1" runat="server" />
    
    <div class="adminSection form-horizontal validationGroup">

        <div class="form-group">
            <div class="col-sm-offset-3 col-sm-9">
                <asp:DropDownList id="DropDownConnection" runat="server" />
            </div>
        </div>
	    <div class="form-group">
            <div class="col-sm-offset-3 col-sm-9">
                <asp:TextBox id="txtSql" runat="server" TextMode="MultiLine" Rows="20" CssClass="form-control required" />
            </div>
	    </div>

	    <div class="form-group">
            <div class="col-sm-offset-3 col-sm-9">
                <asp:LinkButton id="cmdExec" runat="server" OnClick="CmdExec_Click" Text="Execute" CssClass="btn btn-danger causesValidation" />
            </div>     
        </div>		
    </div>

    <asp:GridView id="sqlGrid" Runat="server" AutoGenerateColumns="true" CssClass="table table-hover" BorderWidth="0">
			</asp:GridView>
		
</asp:Content>
