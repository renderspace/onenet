<%@ Page  Title="One.NET Recover Password" Language="C#" MasterPageFile="~/Account/Account.Master" AutoEventWireup="true" CodeBehind="RecoverPassword.aspx.cs" Inherits="OneMainWeb.Account.RecoverPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <h2><%: Title %>.</h2>

    <div class="row">
        <div class="col-md-8">
            <section id="loginForm">
                <div class="form-horizontal">
                    <asp:MultiView runat="server" ID="MultiView1">
                        <asp:View runat="server" ID="View1">
                            <h4>Recover your local account password.</h4>
                            <hr />
                            <asp:PlaceHolder runat="server" ID="PlaceHolderErrorMessage" Visible="false">
                                <p class="text-danger">
                                    <asp:Literal runat="server" ID="FailureLiteral" />
                                </p>
                            </asp:PlaceHolder>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="EmailTextBox" CssClass="col-md-2 control-label">Email</asp:Label>
                                <div class="col-md-10">
                                    <asp:TextBox runat="server" ID="EmailTextBox" CssClass="form-control email" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="EmailTextBox" CssClass="text-danger" ErrorMessage="The email field is required." />
                                    <asp:RegularExpressionValidator runat="server" ControlToValidate = "EmailTextBox" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ErrorMessage="Please enter valid email address."></asp:RegularExpressionValidator>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-offset-2 col-md-10">
                                    <asp:Button runat="server" OnClick="Recover_Click" Text="Recover" CssClass="btn btn-success btn-lg" />
                                </div>
                            </div>
                        </asp:View>
                        <asp:View runat="server" ID="View2">
                            <h4>Success.</h4>
                            <p>Reset link sent to your email. Please check your inbox and spam folder.</p>
                        </asp:View>
                        <asp:View runat="server" ID="View3">
                            <h4>Reset your local account password.</h4>
                            <hr />
                            <asp:PlaceHolder runat="server" ID="PlaceHolderErrorMessage2" Visible="false">
                                <p class="text-danger">
                                    <asp:Literal runat="server" ID="FailureLiteral2" />
                                </p>
                            </asp:PlaceHolder>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="NewPasswordTextBox" CssClass="col-md-2 control-label">New Password</asp:Label>
                                <div class="col-md-10">
                                    <asp:TextBox TextMode="Password" runat="server" ID="NewPasswordTextBox" CssClass="form-control" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="NewPasswordTextBox" CssClass="text-danger" ErrorMessage="The new password field is required." />
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="ConfirmPasswordTextBox" CssClass="col-md-2 control-label">Confirm Password</asp:Label>
                                <div class="col-md-10">
                                    <asp:TextBox TextMode="Password" runat="server" ID="ConfirmPasswordTextBox" CssClass="form-control" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ConfirmPasswordTextBox" CssClass="text-danger" ErrorMessage="The confirm password field is required." />
                                    <asp:CompareValidator runat="server" ControlToValidate="ConfirmPasswordTextBox" ControlToCompare="NewPasswordTextBox" ErrorMessage="Passwords do not match." CssClass="text-danger" />
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-offset-2 col-md-10">
                                    <asp:Button runat="server" OnClick="Reset_Click" Text="Change" CssClass="btn btn-success btn-lg" />
                                </div>
                            </div>
                        </asp:View>
                        <asp:View runat="server" ID="View4">
                            <h4>Reset success.</h4>
                            <p>Your password has been successfully reset.</p>
                            <asp:HyperLink runat="server" ID="LoginHyperLink" ViewStateMode="Disabled">Login</asp:HyperLink>
                        </asp:View>
                    </asp:MultiView>
                </div>
            </section>
        </div>
    </div>

</asp:Content>
