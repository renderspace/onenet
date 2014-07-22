<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ScaffoldConfig.ascx.cs" Inherits="OneMainWeb.AdminControls.ScaffoldConfig" %>

<%@ Register TagPrefix="bll" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="cc1" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>

    <div class="top">
        <cc1:Notifier runat="server" ID="Notifier1" />
    </div>  

    <asp:PlaceHolder runat="server" ID="PlaceHolderList">
        <div class="physical_ListBox">
            <asp:ListBox ID="ListBoxPhysical" runat="server" DataTextField="FriendlyName" DataValueField="StartingPhysicalTable" Rows="15">
            </asp:ListBox>
            <div class="add">
                <asp:Button ID="ButtonAdd" runat="server" Text="&gt;" onclick="ButtonAdd_Click" />
            </div>
        </div>

        <div class="listing">
            <asp:GridView ID="GridViewVirtualTables" runat="server" AllowSorting="false" AutoGenerateColumns="false" 
                onselectedindexchanged="GridViewVirtualTables_SelectedIndexChanged" 
                OnRowDeleted="GridViewVirtualTables_RowDeleted"
                OnRowCommand="GridViewVirtualTables_RowCommand"
                OnRowDeleting="GridViewVirtualTables_Deleting"
                OnRowDataBound="GridViewVirtualTables_RowDataBound">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button CommandArgument='<%# Eval("Id") %>' CommandName="Delete" ID="CmdDelete" runat="server" Text="$delete" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="StartingPhysicalTable" HeaderText="$physical_table" />
                    <asp:TemplateField HeaderText="Show">
                        <ItemTemplate>
                            <asp:CheckBox ID="CheckBoxShowOnMenu" runat="server" Checked='<%# Eval("ShowOnMenu") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="$name">
                        <ItemTemplate>
                            <asp:TextBox ID="TextBoxFriendlyName" runat="server" Text='<%# Eval("FriendlyName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="$where_condition" >
                        <ItemTemplate>
                            <asp:TextBox ID="TextBoxWhereCondition" runat="server" Text='<%# Eval("Condition") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>  
                    <asp:TemplateField HeaderText="$order_by">
                        <ItemTemplate>
                            <asp:DropDownList runat="server" ID="DropDownListOrder" />
                        </ItemTemplate>
                    </asp:TemplateField>              
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="CmdSelect" runat="server" Text="$edit_columns" CommandArgument='<%# Eval("Id") %>' CommandName="select" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <div class="buttonsMiddle">
                <asp:Button ID="CmdSave" runat="server" Text="Save" OnClick="CmdSave_Click" CssClass="save-btn" />
                <asp:Button ID="CmdCancel" runat="server" Text="$cancel" OnClick="CmdCancel_Click" />
            </div>     
        </div>   
        
    </asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="PlaceHolderSingle">
        <div class="physical_ListBox">
            <asp:ListBox ID="ListBoxPhysicalColumns" runat="server" DataTextField="Description" DataValueField="Name" Rows="15" >
            </asp:ListBox>
            <div class="add">
                <asp:Button ID="CmdAddAllColumns" runat="server" Text="$add_all_columns" OnClick="CmdAddAllColumns_Click" />
                <asp:Button ID="CmdAddColumn" runat="server" Text="$add_column" OnClick="CmdAddColumn_Click" />                    
            </div>                
        </div>
    
        <div class="relationship_ListBox">
            <asp:ListBox ID="ListBoxOneToManyVirtualTables" AutoPostBack="true" OnSelectedIndexChanged="ListBoxOneToManyVirtualTables_SelectedIndexChanged" runat="server" DataTextField="Description" DataValueField="Id" Rows="8" />
            
            <asp:ListBox ID="ListBoxRelationshipDisplayColumn" runat="server" DataTextField="Description" DataValueField="Name" />
            
            <div class="add">
                <asp:Button ID="CmdAddRelationship" runat="server" Text="$add_relationship" OnClick="CmdAddRelationship_Click" />
            </div>
        </div>

        <asp:GridView ID="GridViewItems" runat="server" AllowSorting="True" 
            AutoGenerateColumns="false" OnRowDeleting="GridViewItems_RowDeleting" OnRowDeleted="GridViewItems_RowDeleted" OnRowCommand="GridViewItems_RowCommand" OnRowDataBound="GridViewItems_RowDataBound">
                <Columns>
                    <asp:BoundField HeaderText="Description" DataField="Description" />
                    <asp:TemplateField HeaderText="$friendly_name">
                        <ItemTemplate>
                            <asp:TextBox ID="TextBoxFriendlyName" runat="server" Text='<%# Eval("FriendlyName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="$show_on_list">
                        <ItemTemplate>
                            <asp:CheckBox ID="CheckBoxShowOnList" runat="server" Checked='<%# bool.Parse((string)Eval("ShowOnList")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>                    
<%--                    <asp:TemplateField HeaderText="DbType">
                        <ItemTemplate>
                            <asp:DropDownList ID="DropDownListDbType" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>--%>
                    <asp:BoundField HeaderText="Id" DataField="Id" />
                    <asp:BoundField HeaderText="$column_type" DataField="ColumnType" /> 
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Button ID="CmdDelete" runat="server" Text="$delete" CommandArgument='<%# Eval("Id") + "," + Eval("ColumnType") %>' CommandName="delete" />
                        </ItemTemplate>
                    </asp:TemplateField>                    
                </Columns>
        </asp:GridView>
        <div class="buttonsMiddle">
            <asp:Button ID="CmdSaveColumnChanges" runat="server" Text="Save" OnClick="CmdSaveColumnChanges_Click" />
            <asp:Button ID="CmdCancelColumnChanges" runat="server" Text="Cancel" OnClick="CmdCancelColumnChanges_Click" />
        </div>           
    </asp:PlaceHolder> 