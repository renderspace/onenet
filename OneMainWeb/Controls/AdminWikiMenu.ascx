<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdminWikiMenu.ascx.cs" Inherits="OneMainWeb.Controls.AdminWikiMenu" %>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>

<link rel="stylesheet" href="/adm/css/adminWikiMenu.css" />


<div id="oneNetPreview">
    <asp:Literal runat="server" ID="LiteralInfo" EnableViewState="false"></asp:Literal>
    <uc1:Notifier ID="Notifier1" runat="server" />

    <asp:LoginView ID="LoginView1" runat="server">
        <AnonymousTemplate>
            <div id="loginwrapper"><asp:LoginStatus ID="LoginStatus1" runat="server" /></div>
        </AnonymousTemplate>
        <LoggedInTemplate>
            <h3>No rights for editing</h3>
			<div id="loginwrapper"><asp:LoginStatus ID="LoginStatus1" runat="server" /></div>
        </LoggedInTemplate>
        <RoleGroups>
                <asp:RoleGroup Roles="admin">
                        <ContentTemplate>
                            <div id="menubarwrapper">
	                            <ul class="first">
		                            <li class="l1"><a id="newPage" class="button">New page</a></li>
                                    <li class="l2"><asp:LinkButton ID="LinkButtonDelete" runat="server" onclick="LinkButtonDelete_Click" OnClientClick="return confirm('Are you sure?')">
                                        Delete page</asp:LinkButton></li>
                                    <li class="l3"><a id="editPage" class="button">Edit page</a></li>
								</ul>
								<ul class="mid">
                                    <li><asp:LinkButton ID="LinkButtonClearCache" runat="server" onclick="LinkButtonClearCache_Click" CssClass="button">Clear cache</asp:LinkButton></li>
								</ul>
								<ul class="last">
                                    <li><asp:LinkButton ID="LinkButtonLogout" runat="server" onclick="LinkButtonLogout_Click">Logout</asp:LinkButton></li>
	                            </ul>
                            </div>

                            <div id="modulesListWrapper">
                                <p>Drag and drop module to page:</p>
                                <asp:Repeater ID="RepeaterModules" runat="server">
                                    <HeaderTemplate><ul></HeaderTemplate>
                                    <FooterTemplate></ul></FooterTemplate>
                                    <ItemTemplate><li id='moduleId_<%# Eval("Id") %>' class='ddModule'><%# Eval("Name")%></li></ItemTemplate>
                                </asp:Repeater>
                            </div>

                            <div id="newPageModal">
	                            <h1>New page</h1>
                                <asp:Label ID="LabelPageTitle" runat="server" Text="Name of the new page" AssociatedControlID="TextBoxPageTitle"></asp:Label>
                                <asp:TextBox ID="TextBoxPageTitle" runat="server"></asp:TextBox>
                                <asp:LinkButton ID="LinkButtonNewPage" runat="server" onclick="LinkButtonNewPage_Click" CssClass="button">Create</asp:LinkButton>
                            </div>
                            <script src="/ckeditor4/ckeditor.js" type="text/javascript" ></script>
                            <script src="/JavaScript/AdminWikiMenu.js"></script>
                        </ContentTemplate>
                </asp:RoleGroup>
        </RoleGroups>
    </asp:LoginView>    
</div>