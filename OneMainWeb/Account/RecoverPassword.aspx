<%@ Page Title="" Language="C#" MasterPageFile="~/Account/Account.Master" AutoEventWireup="true" CodeBehind="RecoverPassword.aspx.cs" Inherits="OneMainWeb.Account.RecoverPassword" %>
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
                            <asp:PlaceHolder runat="server" ID="ErrorMessage" Visible="false">
                                <p class="text-danger">
                                    <asp:Literal runat="server" ID="FailureText" />
                                </p>
                            </asp:PlaceHolder>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="Email" CssClass="col-md-2 control-label">Email</asp:Label>
                                <div class="col-md-10">
                                    <asp:TextBox runat="server" ID="Email" CssClass="form-control email" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="Email" CssClass="text-danger" ErrorMessage="The email field is required." />
                                    <asp:RegularExpressionValidator runat="server" ControlToValidate = "Email" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ErrorMessage="Please enter valid email address."></asp:RegularExpressionValidator>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-offset-2 col-md-10">
                                    <asp:Button runat="server" OnClick="Recover_Click" Text="Recover" CssClass="btn btn-success btn-lg" />
                                </div>
                            </div>
                        </asp:View>
                        <asp:View runat="server" ID="View2">
                            <h4>Reset your local account password.</h4>
                            <hr />
                            <asp:PlaceHolder runat="server" ID="ErrorMessage2" Visible="false">
                                <p class="text-danger">
                                    <asp:Literal runat="server" ID="FailureText2" />
                                </p>
                            </asp:PlaceHolder>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="NewPassword" CssClass="col-md-2 control-label">New Password</asp:Label>
                                <div class="col-md-10">
                                    <asp:TextBox TextMode="Password" runat="server" ID="NewPassword" CssClass="form-control" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="NewPassword" CssClass="text-danger" ErrorMessage="The new password field is required." />
                                </div>
                            </div>
                            <div class="form-group">
                                <asp:Label runat="server" AssociatedControlID="ConfirmPassword" CssClass="col-md-2 control-label">Confirm Password</asp:Label>
                                <div class="col-md-10">
                                    <asp:TextBox TextMode="Password" runat="server" ID="ConfirmPassword" CssClass="form-control" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ConfirmPassword" CssClass="text-danger" ErrorMessage="The confirm password field is required." />
                                    <asp:CompareValidator runat="server" ControlToValidate="ConfirmPassword" ControlToCompare="NewPassword" ErrorMessage="Passwords do not match." CssClass="text-danger" />
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-md-offset-2 col-md-10">
                                    <asp:Button runat="server" OnClick="Reset_Click" Text="Change" CssClass="btn btn-success btn-lg" />
                                </div>
                            </div>
                        </asp:View>
                        <asp:View runat="server" ID="View3">
                            <h4>Reset success.</h4>
                            <p>Your password has been successfully reset.</p>
                        </asp:View>
                    </asp:MultiView>
                </div>
            </section>
        </div>
    </div>

</asp:Content>
