<%@ Page Title="One.NET config" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="ScaffoldConfig.aspx.cs" Inherits="OneMainWeb.adm.ScaffoldConfig" %>
    <%@ Register TagPrefix="bll" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="cc1" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">


     <cc1:Notifier runat="server" ID="Notifier1" />

    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="MultiView1_ActiveViewChanged">
        <asp:View runat="server" ID="View1">
             <div class="adminSection">
			    <div class="col-md-5" >
                    <asp:DropDownList runat="server" DataTextField="FriendlyName" DataValueField="StartingPhysicalTable" ID="DropDownListPhysical"></asp:DropDownList>
                    <asp:LinkButton ID="ButtonInsert" runat="server" onclick="ButtonAdd_Click"  text="<span class='glyphicon glyphicon-plus'></span> Add table" CssClass="btn btn-success" />
                </div>
            </div>

            <asp:GridView ID="GridViewVirtualTables" runat="server" AllowSorting="false" AutoGenerateColumns="false"  CssClass="table table-hover"
                onselectedindexchanged="GridViewVirtualTables_SelectedIndexChanged" 
                OnRowDeleted="GridViewVirtualTables_RowDeleted"
                OnRowCommand="GridViewVirtualTables_RowCommand"
                OnRowDeleting="GridViewVirtualTables_Deleting"
                OnRowDataBound="GridViewVirtualTables_RowDataBound">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton CommandArgument='<%# Eval("Id") %>' CommandName="Delete" ID="CmdDelete" runat="server" Text="Delete" CssClass="btn btn-danger btn-xs" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="StartingPhysicalTable" HeaderText="Physical table" />
                    <asp:TemplateField HeaderText="Show">
                        <ItemTemplate>
                            <asp:CheckBox ID="CheckBoxShowOnMenu" runat="server" Checked='<%# Eval("ShowOnMenu") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Friendly name">
                        <ItemTemplate>
                            <asp:TextBox ID="TextBoxFriendlyName" runat="server" Text='<%# Eval("FriendlyName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Where condition" >
                        <ItemTemplate>
                            <asp:TextBox ID="TextBoxWhereCondition" runat="server" Text='<%# Eval("Condition") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>  
                    <asp:TemplateField HeaderText="Order by">
                        <ItemTemplate>
                            <asp:DropDownList runat="server" ID="DropDownListOrder" />
                        </ItemTemplate>
                    </asp:TemplateField>              
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton ID="CmdSelect" runat="server" Text="Edit columns" CommandArgument='<%# Eval("Id") %>' CommandName="select" CssClass="btn btn-info btn-xs" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <div class="form-horizontal">
                <div class="form-group">
                        <div class="col-sm-12">
                        <asp:LinkButton ID="CmdCancel" runat="server" Text="Cancel" OnClick="CmdCancel_Click" CssClass="btn btn-default" />

                        <asp:LinkButton ID="CmdSave" runat="server" Text="Save" OnClick="CmdSave_Click" CssClass="btn btn-success" />
                    </div>
                </div>     
            </div>

        </asp:View>
        <asp:View runat="server" ID="View2">
                <div class="row">
                    <div class="col-md-3">
                        <asp:ListBox ID="ListBoxPhysicalColumns" runat="server" DataTextField="Description" DataValueField="Name" Rows="15" CssClass="form-control">
                        </asp:ListBox>
                        
                    </div>
                    <div class="col-md-1">
                        <p>
                       <asp:LinkButton ID="CmdAddAllColumns" runat="server" Text="Add all columns" OnClick="CmdAddAllColumns_Click" CssClass="btn btn-info btn-xs" />
                            </p>
                        <p>
                       <asp:LinkButton ID="CmdAddColumn" runat="server" Text="Add column" OnClick="CmdAddColumn_Click" CssClass="btn btn-success" />                  
                        </p>
                    </div>                
                
                
                    <div class="col-md-3 col-md-offset-2">
                        <asp:ListBox ID="ListBoxOneToManyVirtualTables" AutoPostBack="true" OnSelectedIndexChanged="ListBoxOneToManyVirtualTables_SelectedIndexChanged" runat="server" 
                            CssClass="form-control"
                            DataTextField="Description" DataValueField="Id" Rows="8" />
                      
            
                    
                        <asp:ListBox ID="ListBoxRelationshipDisplayColumn" runat="server" DataTextField="Description" DataValueField="Name" CssClass="form-control" />
                    </div>
                    <div class="col-md-1">
                        <asp:LinkButton ID="CmdAddRelationship" runat="server" Text="Add relationship" OnClick="CmdAddRelationship_Click" CssClass="btn btn-success" />    
                    </div>
            
                    
                </div>

                <asp:GridView ID="GridViewItems" runat="server" AllowSorting="True"  CssClass="table table-hover"
                    AutoGenerateColumns="false" OnRowDeleting="GridViewItems_RowDeleting" OnRowDeleted="GridViewItems_RowDeleted" OnRowCommand="GridViewItems_RowCommand" OnRowDataBound="GridViewItems_RowDataBound">
                        <Columns>
                            <asp:BoundField HeaderText="Description" DataField="Description" />
                            <asp:TemplateField HeaderText="Friendly name">
                                <ItemTemplate>
                                    <asp:TextBox ID="TextBoxFriendlyName" runat="server" Text='<%# Eval("FriendlyName") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Wysiwyg">
                                <ItemTemplate>
                                    <asp:CheckBox ID="CheckBoxWysiwyg" runat="server" Checked='<%# bool.Parse((string)Eval("IsWysiwyg")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>       
                            <asp:TemplateField HeaderText="MultiLanguageContent">
                                <ItemTemplate>
                                    <asp:CheckBox ID="CheckBoxIsMultiLanguageContent" runat="server" Checked='<%# bool.Parse((string)Eval("IsMultiLanguageContent")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>     
                            <asp:TemplateField HeaderText="Show on list">
                                <ItemTemplate>
                                    <asp:CheckBox ID="CheckBoxShowOnList" runat="server" Checked='<%# bool.Parse((string)Eval("ShowOnList")) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>                    
                            <asp:BoundField HeaderText="Id" DataField="Id" />
                            <asp:BoundField HeaderText="Column type" DataField="ColumnType" /> 
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="CmdDelete" runat="server" Text="Delete" CommandArgument='<%# Eval("Id") + "," + Eval("ColumnType") %>' CommandName="delete" CssClass="btn btn-danger btn-xs" />
                                </ItemTemplate>
                            </asp:TemplateField>                    
                        </Columns>
                </asp:GridView>
                <div class="buttonsMiddle">
                    <asp:LinkButton ID="CmdSaveColumnChanges" runat="server" Text="Save" OnClick="CmdSaveColumnChanges_Click" CssClass="btn btn-success" />
                    <asp:LinkButton ID="CmdCancelColumnChanges" runat="server" Text="Cancel" OnClick="CmdCancelColumnChanges_Click" CssClass="btn btn-info" />
                </div>   

        </asp:View>
        <asp:View runat="server" ID="View3">
            <asp:Label ID="Label1" runat="server" Text="No configuration tables found. Do you want to create them?"></asp:Label>
            <asp:LinkButton ID="ButtonCreateTables" runat="server" Text="<span class='glyphicon glyphicon-plus'></span> Create config tables" onclick="ButtonCreateTables_Click" CssClass="btn btn-success" />	

        </asp:View>
    </asp:MultiView>

</asp:Content>
