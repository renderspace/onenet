<%@ Page Language="C#" MasterPageFile="~/Account/Account.Master" AutoEventWireup="true" CodeBehind="RegisterExternalLogin.aspx.cs" Inherits="OneMainWeb.Account.RegisterExternalLogin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
        <h3>Register with your <%: ProviderName %> account</h3>

        <asp:PlaceHolder runat="server">
            <div class="form-horizontal">
                <h4>Association Form</h4>
                <hr />
                <asp:ValidationSummary runat="server" ShowModelStateErrors="true" CssClass="text-danger" />
                <p class="text-info">
                    You've authenticated with <strong><%: ProviderName %></strong>. Please enter a user name below for the current site
                    and click the Log in button.
                </p>

                <div class="form-group">
                    <asp:Label runat="server" AssociatedControlID="userName" CssClass="col-md-2 control-label">User name</asp:Label>
                    <div class="col-md-10">
                        <asp:TextBox runat="server" ID="userName" CssClass="form-control" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="userName"
                            Display="Dynamic" CssClass="text-danger" ErrorMessage="User name is required" />
                        <asp:ModelErrorMessage runat="server" ModelStateKey="UserName" CssClass="text-error" />
                    </div>
                </div>

                <div class="form-group">
                    <div class="col-md-offset-2 col-md-10">
                        <asp:Button runat="server" Text="Log in" CssClass="btn btn-default" OnClick="LogIn_Click" />
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>
</asp:Content>