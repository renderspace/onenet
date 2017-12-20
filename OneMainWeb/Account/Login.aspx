<%@ Page Title="One.NET Log in" Language="C#" MasterPageFile="~/Account/Account.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="OneMainWeb.Account.Login" Async="true" %>
<%@ Register Src="~/Account/OpenAuthProviders.ascx" TagPrefix="uc" TagName="OpenAuthProviders" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>

    <div class="row">
         <asp:MultiView runat="server" ID="MultiView1" ActiveViewIndex="0">
             <asp:View runat="server">
                 <div class="col-md-8">
                    <section id="loginForm">
                        <div class="form-horizontal">
                            <h4>Use a local account to log in.</h4>
                            <hr />
                              <asp:PlaceHolder runat="server" ID="ErrorMessage" Visible="false">
                                <p class="text-danger">
                                    <asp:Literal runat="server" ID="FailureText" />
                                </p>
                            </asp:PlaceHolder>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="UserName" CssClass="col-md-2 control-label">User name</asp:Label>
                                <div class="col-md-10">
                                    <asp:TextBox runat="server" ID="UserName" CssClass="form-control" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="UserName"
                                        CssClass="text-danger" ErrorMessage="The user name field is required." />
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="Password" CssClass="col-md-2 control-label">Password</asp:Label>
                                <div class="col-md-10">
                                    <asp:TextBox runat="server" ID="Password" TextMode="Password" CssClass="form-control" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="Password" CssClass="text-danger" ErrorMessage="The password field is required." />
                                </div>
                            </div>
                            <asp:Panel runat="server" ID="Panel2FA" CssClass="form-horizontal">
                                <div class="form-group">
                                    <asp:Label runat="server" AssociatedControlID="TextBoxCode" CssClass="col-md-2 control-label">Two factor authentication code</asp:Label>
                                    <div class="col-md-10">
                                        <asp:TextBox runat="server" ID="TextBoxCode" CssClass="form-control" />
                                    </div>
                                </div>
                            </asp:Panel>
                            <div class="form-group">
                                <div class="col-md-offset-2 col-md-10">
                                    <div class="checkbox">
                                        <asp:CheckBox runat="server" ID="RememberMe" />
                                        <asp:Label runat="server" AssociatedControlID="RememberMe">Remember me?</asp:Label>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-offset-2 col-md-10">
                                    <asp:Button runat="server" OnClick="LogIn" Text="Log in" CssClass="btn btn-success btn-lg" />
                                </div>
                            </div>
                        </div>
                        <p>
                            <asp:HyperLink runat="server" ID="RegisterHyperLink" ViewStateMode="Disabled">Register</asp:HyperLink>
                            if you don't have a local account.
                        </p>
                        <p>
                            <asp:HyperLink runat="server" ID="RecoverPasswordHyperLink" ViewStateMode="Disabled">Forgotten password?</asp:HyperLink>
                            recover your password if you have lost it.
                        </p>
                    </section>
                </div>

                <div class="col-md-4">
                    <section id="socialLoginForm">
                        <uc:OpenAuthProviders runat="server" ID="OpenAuthLogin" />
                    </section>
                </div>
             </asp:View>
         </asp:MultiView>
    </div>
</asp:Content>




