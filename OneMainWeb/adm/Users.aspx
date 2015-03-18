<%@ Page Title="One.NET Users" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="OneMainWeb.adm.Users" %>
<%@ Register Src="~/AdminControls/Notifier.ascx" TagName="Notifier" TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:Notifier runat="server" ID="Notifier1" />

    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="MultiView1_ActiveViewChanged" ActiveViewIndex="0">
        <asp:View runat="server">
            <div class="adminSection">
			    <h4>To add new user, the user must register on main login page. Then it will appear on the list below, where you can add roles.</h4>
                 <asp:Button ID="Button1" runat="server" Text="Update roles" OnClick="ButtonUpdateRoles_Click" />
            </div>
            <asp:GridView	ID="GridViewUsers"
					runat="server"
					CssClass="table table-hover"
					AutoGenerateColumns="false"
					AllowPaging="false"
					AllowSorting="false"
					DataKeyNames="UserName" OnSelectedIndexChanged="GridViewUsers_SelectedIndexChanged">
		        <Columns>
                    <asp:BoundField HeaderText="UserName" DataField="UserName" />
                    <asp:BoundField HeaderText="Email" DataField="Email" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton Text='<span class="glyphicon glyphicon-pencil"></span> Edit' CommandName="Select" CommandArgument='<%# Eval("UserName") %>' ID="cmdEdit" runat="server" CssClass="btn btn-info btn-xs  " />
					    </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

        </asp:View>
        <asp:View runat="server">
             <div class="adminSection form-horizontal">
                
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
                        <asp:LinkButton  ValidationGroup="user" id="ButtonSave" runat="server" OnClick="ButtonSave_Click"  Text="Save" CssClass="btn btn-success" />
                    </div>
               </div>
            </div>
        </asp:View>
        
    </asp:MultiView>
     
</asp:Content>
