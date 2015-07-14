<%@ Page Title="One.NET copy" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="StructureCopy.aspx.cs" Inherits="OneMainWeb.adm.StructureCopy" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="notifier" />
    <div class="col-md-3">
            <section class="module tall">
                <div class="treeview">
                    <asp:TreeView OnUnload="TreeView1_Unload" ID="TreeView1" runat="server" BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" OnSelectedNodeChanged="TreeView1_SelectedNodeChanged" OnAdaptedSelectedNodeChanged="TreeView1_SelectedNodeChanged" Width="270" ExpandDepth="3">
                    </asp:TreeView>
                </div>
            </section>      
    </div>
      <div class="col-md-9">
             <div class="page-and-module-settings"> 

                     <section class="module page-settings">
                           <div class="pageproperties form-horizontal">

                               <div class="form-group">
                                   <div class="col-sm-4">
						                    <asp:Label ID="LabelEmptyWebSites" runat="server" AssociatedControlID="CheckBoxListEmptyWebSites" Text="Empty websites" />
                                    </div>
                                    <div class="col-sm-8">
                                        
                                        <asp:CheckBoxList ID="CheckBoxListEmptyWebSites" runat="server"></asp:CheckBoxList>
                                        </div>
                               </div>


                               <div class="form-group">
                                    <div class="col-sm-4">
						      
                                    </div>

                                    <div class="col-sm-8">
					                   <asp:Button ID="ButtonCopy" runat="server" text="Copy pages to site" OnClick="ButtonCopy_Click" />
                                    </div>
				                </div>
                            </div>
                         </section>
                    </div>

        </div>
                    

</asp:Content>
