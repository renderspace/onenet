<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OpenAuthProviders.ascx.cs" Inherits="OneMainWeb.Account.OpenAuthProviders" %>
<div id="socialLoginList">
    <h4>Use another service to log in.</h4>
    <hr />
    <asp:ListView runat="server" ID="providerDetails" ItemType="System.String" SelectMethod="GetProviderNames" ViewStateMode="Disabled">
        <ItemTemplate>
            <p>
                <button type="submit" class="btn btn-primary" name="provider" value="<%#: Item %>"
                    title="Log in using your <%#: Item %> account.">
                    <span class="glyphicon glyphicon-user">
                    <%#: Item %></span>
                </button>
            </p>
        </ItemTemplate>
        <EmptyDataTemplate>
            <div>
                <p>There are no external authentication services configured.</p>
            </div>
        </EmptyDataTemplate>
    </asp:ListView>
</div>