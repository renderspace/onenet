<%@ Page Title="One.NET Users" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="OneMainWeb.adm.Users" %>

<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:Notifier runat="server" ID="Notifier1" />
    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="MultiView1_ActiveViewChanged" ActiveViewIndex="0">
        <asp:View runat="server">
            <div class="adminSection">
                <asp:Button ID="ButtonAddUser" runat="server" Text="Add user" OnClick="ButtonAddUser_Click" />
                <asp:Button ID="ButtonUpdateRoles" runat="server" Text="Update roles" OnClick="ButtonUpdateRoles_Click" />
            </div>
            <asp:GridView ID="GridViewUsers"
                runat="server"
                CssClass="table table-hover table-clickable-row"
                AutoGenerateColumns="false"
                AllowPaging="false"
                AllowSorting="false"
                DataKeyNames="UserName"
                OnSelectedIndexChanged="GridViewUsers_SelectedIndexChanged"
                OnRowDataBound="GridViewUsers_RowDataBound">
                <Columns>
                    <asp:TemplateField>
						<ItemTemplate>
                            <asp:Literal ID="LiteralUserId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							<asp:CheckBox ID="CheckBoxDelete" runat="server" Text="" />
						</ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="UserName" DataField="UserName" />
                    <asp:BoundField HeaderText="Email" DataField="Email" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton Text='<span class="glyphicon glyphicon-pencil"></span> Edit' CommandName="Select" CommandArgument='<%# Eval("UserName") %>' ID="cmdEdit" runat="server" CssClass="btn btn-info btn-xs  " />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:Panel runat="server" ID="PanelGridButtons" CssClass="form-group">
                <div class="col-sm-12">
                    <asp:LinkButton CssClass="btn btn-danger" ID="ButtonDelete" OnClick="ButtonDelete_Click" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" />
                </div>
            </asp:Panel>
        </asp:View>
        <asp:View runat="server">
            <div class="adminSection form-horizontal edit-users">
                <div class="form-group">
                    <label class="col-sm-3 control-label">Username</label>
                    <div class="col-sm-9">
                        <asp:Label runat="server" ID="LabelUsername"></asp:Label>
                    </div>
                </div>
                <div class="form-group">
                    <asp:Repeater runat="server" ID="RepeaterRoles" OnItemDataBound="RepeaterRoles_ItemDataBound">
                        <ItemTemplate>
                            <div class="col-sm-offset-3 col-sm-9">
                                <div class="checkbox">
                                    <asp:CheckBox runat="server" ID="Checkbox1" Text='<%# Eval("Name") %>' />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
                        <asp:LinkButton ValidationGroup="user" ID="ButtonSave" runat="server" OnClick="ButtonSave_Click" Text="Save" CssClass="btn btn-success" />
                    </div>
                </div>
            </div>
        </asp:View>
        <asp:View runat="server">
            <div class="adminSection form-horizontal edit-users">
                <div class="form-group">
                    <label class="col-sm-3 control-label">Username</label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ID="TextBoxUsername"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator1" ControlToValidate="TextBoxUsername" Text="" ErrorMessage="*" ValidationGroup="add_user"></asp:RequiredFieldValidator>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Password</label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ID="TextBoxPassword"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator2" ControlToValidate="TextBoxPassword" Text="" ErrorMessage="*" ValidationGroup="add_user"></asp:RequiredFieldValidator>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Confirm Passowrd</label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ID="TextBoxConfirmPassword"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator3" ControlToValidate="TextBoxConfirmPassword" Text="" ErrorMessage="*" ValidationGroup="add_user"></asp:RequiredFieldValidator>
                        <asp:CompareValidator runat="server" ControlToValidate="TextBoxPassword" ControlToCompare="TextBoxConfirmPassword" Text="" ValidationGroup="add_user" ErrorMessage="*"></asp:CompareValidator>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Email</label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ID="TextBoxEmail"></asp:TextBox>
                        <asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator4" ControlToValidate="TextBoxEmail" Text="" ErrorMessage="*" ValidationGroup="add_user"></asp:RequiredFieldValidator>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
                        <asp:LinkButton ValidationGroup="add_user" ID="ButtonSaveUser" runat="server" OnClick="ButtonSaveUser_Click" Text="Save" CssClass="btn btn-success" />
                    </div>
                </div>
            </div>
        </asp:View>
    </asp:MultiView>
</asp:Content>