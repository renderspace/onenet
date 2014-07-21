<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Website.aspx.cs" Inherits="OneMainWeb.adm.Website" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <one:Notifier runat="server" ID="notifier" />
    <div class="topCommands">
        <asp:LinkButton ID="LinkButtonWebsites" runat="server" OnClick="LinkButtonWebsites_Click">Websites</asp:LinkButton>
        <asp:LinkButton ID="LinkButtonCopy" runat="server" OnClick="LinkButtonCopy_Click">Copy website structure</asp:LinkButton>
        <asp:LinkButton ID="LinkButtonRecursive" runat="server" OnClick="LinkButtonRecursive_Click">Recursive delete</asp:LinkButton>
    </div>

        <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="TabMultiview_OnViewIndexChanged">
            <asp:View ID="View1" runat="server">
                <div class="centerFull">
			        <div class="biggv">  
				        <asp:GridView	ID="GridViewWebsites"
								        runat="server"
								        CssClass="gv"
								        AutoGenerateColumns="false"
								        AllowPaging="false"
								        AllowSorting="false"
								        DataKeyNames="Id"
                                        OnRowEditing="GridViewWebsites_RowEditing"
                                        OnRowUpdating="GridViewWebsites_RowUpdating"
                                        OnRowCancelingEdit="GridViewWebsites_RowCancelingEdit">
					        <Columns>
                                <asp:BoundField HeaderText="$id" DataField="Id" ReadOnly="true" />
                                <asp:BoundField HeaderText="$title" DataField="Title" />
                                <asp:BoundField HeaderText="$lcid" DataField="LanguageId" ReadOnly="true" />
						        <asp:TemplateField HeaderText="$last_changed_by">
							        <ItemTemplate><%# Eval("PrincipalModified") == null || string.IsNullOrEmpty(Eval("PrincipalModified").ToString()) ? Eval("PrincipalCreated") : Eval("PrincipalModified")%></ItemTemplate>
						        </asp:TemplateField>
						        <asp:TemplateField HeaderText="$last_change_date">
							        <ItemTemplate><%# Eval("DateModified") == null || !((DateTime?)Eval("DateModified")).HasValue ? ((DateTime)Eval("DateCreated")).ToShortDateString() : ((DateTime?)Eval("DateModified")).Value.ToShortDateString() %></ItemTemplate>
						        </asp:TemplateField>
                                <asp:CommandField HeaderText="$edit" ShowEditButton="true" CancelText="$cancel" EditText="$edit"
                                    UpdateText="$update" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
                <div class="searchFull">
                    <two:Input Required="true" ValidationGroup="website" Text="$title" ID="InputTitle" runat="server" />
                    <div class="select">
                        <asp:Label ID="LabelLanguage" runat="server" AssociatedControlID="DropDownList1" Text="$language" />
                        <asp:DropDownList ID="DropDownList1" ValidationGroup="website" runat="server" />
                    </div>
                    <div class="save">
                        <asp:Button Text="$add" ValidationGroup="website" id="ButtonAdd" runat="server" OnClick="ButtonAdd_Click" />
                    </div>
                </div>
            </asp:View>
            <asp:View ID="View2" runat="server">
                <div class="centerFull">
                    <div class="centerStructure">
                        <div id="treeHolder" runat="server" class="treeHolder">
                            <asp:TreeView OnUnload="TreeView1_Unload" ID="TreeView1" runat="server" BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" OnSelectedNodeChanged="TreeView1_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeView1_SelectedNodeChanged" Width="270" ExpandDepth="3">
                            </asp:TreeView>
                        </div>
                    </div>            
                    <div class="mainEditor">
                        <asp:Label ID="LabelEmptyWebSites" runat="server" AssociatedControlID="CheckBoxListEmptyWebSites" Text="$empty_websites" />
                        <asp:CheckBoxList ID="CheckBoxListEmptyWebSites" runat="server"></asp:CheckBoxList>
                    </div>
                    <div class="save">
                        <asp:Button ID="ButtonCopy" runat="server" text="$copy_pages_to_site" OnClick="ButtonCopy_Click" />
                    </div>
                </div>
            </asp:View>
            <asp:View ID="View3" runat="server">
                <div class="centerFull">
                    <div class="centerStructure">
                        <div id="treeHolder2" runat="server" class="treeHolder">
                            <asp:TreeView OnUnload="TreeView2_Unload" ID="TreeView2" runat="server" BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" OnSelectedNodeChanged="TreeView2_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeView2_SelectedNodeChanged" Width="270" ExpandDepth="3">
                            </asp:TreeView>
                        </div>
                    </div>   
                    <div class="mainEditor">
                        <two:InfoLabel ID="InfoLabelPageName" runat="server" Text="$selected_page" /><br />
                        <two:InfoLabel ID="InfoLabelPageId" runat="server" Text="$selected_page_id" />                                        
                    </div>                         
                    <div class="save">
                        <asp:Button OnPreRender="ButtonDelete_PreRender" ID="ButtonDelete" runat="server" text="Delete node and everything below" OnClick="ButtonDelete_Click" CssClass="delete-btn" />
                    </div>
                </div>
            </asp:View>
        </asp:MultiView>
</asp:Content>
