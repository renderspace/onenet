<%@ Page Title="One.NET Websites" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Website.aspx.cs" Inherits="OneMainWeb.adm.Website" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Register TagPrefix="one" TagName="OneSettings" Src="~/AdminControls/OneSettings.ascx" %>
<%@ Register src="~/AdminControls/LastChangeAndHistory.ascx" tagname="LastChangeAndHistory" tagprefix="uc2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <one:Notifier runat="server" ID="Notifier1" />

    <asp:MultiView runat="server" ID="MultiView1" OnActiveViewChanged="MultiView1_ActiveViewChanged" ActiveViewIndex="0">
        <asp:View runat="server">
            <div class="adminSection">
			    <div class="col-md-2">
                    <asp:LinkButton ID="ButtonStartWizard" runat="server" onclick="ButtonStartWizard_Click"  text="<span class='glyphicon glyphicon-plus'></span> New website" CssClass="btn btn-success" />
			    </div>
            </div>
            <asp:GridView	ID="GridViewWebsites"
					runat="server"
					CssClass="table table-hover"
					AutoGenerateColumns="false"
					AllowSorting="false"
					DataKeyNames="Id" OnSelectedIndexChanged="GridViewWebsites_SelectedIndexChanged">
		        <Columns>
                    <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <img src='/Utils/Favicon.ashx?id=<%# Eval("id") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Website name" DataField="DisplayName" />
                    <asp:BoundField HeaderText="PreviewUrl" DataField="PreviewUrl" />
                    <asp:BoundField HeaderText="ProductionUrl" DataField="ProductionUrl" />
                    <asp:BoundField HeaderText="Last Changed" DataField="DisplayLastChanged" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton Text='<span class="glyphicon glyphicon-pencil"></span> Edit' CommandName="Select" CommandArgument='<%# Eval("Id") %>' ID="cmdEdit" runat="server" CssClass="btn btn-info btn-xs  " />
					    </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>


            <asp:PlaceHolder runat="server" ID="PlaceHolderTemplates">

                <script>

                    $(document).ready(function () {
                        $('.deleteTemplate').click(function (e) {
                            var answer = confirm('Are you sure?');
                            if (!answer) {
                                e.preventDefault();
                            }
                        });
                    });

                </script>

                <div class="adminSection validationGroup">
                    <asp:TextBox runat="server" ID="TextBoxTemplate" MaxLength="255" placeholder="New template name" CssClass="required"></asp:TextBox>
                    <asp:LinkButton ID="LinkButtonAddTemplate" runat="server" OnClick="LinkButtonAddTemplate_Click"  text="<span class='glyphicon glyphicon-plus'></span> Add template" CssClass="btn btn-success causesValidation" />
                </div>
                <asp:GridView	ID="GridViewTemplates"
					    runat="server"
					    CssClass="table table-hover"
					    AutoGenerateColumns="false"
					    AllowSorting="false"
					    DataKeyNames="Id"
                        OnSelectedIndexChanged="GridViewTemplates_SelectedIndexChanged"
                        OnRowDeleting="GridViewTemplates_RowDeleting">
		            <Columns>
                        <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true" />
                        <asp:BoundField HeaderText="Name" DataField="Name" />
                        <asp:BoundField HeaderText="Type" DataField="Type" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton Text='<span class="glyphicon glyphicon-pencil"></span> Edit' CommandName="Select" CommandArgument='<%# Eval("Id") %>' ID="cmdEdit" runat="server" CssClass="btn btn-info btn-xs  " />
					        </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton Text='<span class="glyphicon glyphicon-trash"></span> Delete' CommandName="Delete" CommandArgument='<%# Eval("Id") %>' ID="cmdDelete" runat="server" CssClass="deleteTemplate btn btn-danger btn-xs  " />
					        </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>


                <div class="adminSection validationGroup">
                    <asp:TextBox runat="server" ID="TextBoxPlaceholder" MaxLength="255" placeholder="New placeholder name" CssClass="required"></asp:TextBox>
                    <asp:LinkButton ID="LinkButtonAddPlaceholder" runat="server" OnClick="LinkButtonAddPlaceholder_Click"  text="<span class='glyphicon glyphicon-plus'></span> Add placeholder" CssClass="btn btn-success causesValidation" />
                </div>
                <asp:GridView	ID="GridViewPlaceholders"
					    runat="server"
					    CssClass="table table-hover"
					    AutoGenerateColumns="false"
					    AllowSorting="false"
					    DataKeyNames="Id" >
		            <Columns>
                        <asp:BoundField HeaderText="Id" DataField="Id" ReadOnly="true" />
                        <asp:BoundField HeaderText="Name" DataField="Name" />
                    </Columns>
                </asp:GridView>
            </asp:PlaceHolder>

        </asp:View>
        <asp:View runat="server">
             <div class="adminSection form-horizontal validationGroup">

                 <asp:Panel runat="server" CssClass="adminSection" ID="PanelEmptyDatabase">
			        <h4>Database doesn't contain any websites. This looks like a new installation. Please add your first website below. Some additional configuration may be required.</h4>
                </asp:Panel>

                <div class="form-group">
                    <label class="col-sm-3 control-label">Website name</label>
                    <div class="col-sm-9">
                        <asp:TextBox ValidationGroup="website" Text="" ID="InputTitle" runat="server" CssClass="form-control required" MaxLength="255" placeholder="The name of your website. Not the URL, but the name. (i.e. 'IMDb' not 'imdb.com'.) Used at least by Facebook, possible other uses, too." />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Language</label>
                    <div class="col-sm-9">
                        <asp:DropDownList ID="DropDownList1" ValidationGroup="website" runat="server" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Preview website address</label>
                    <div class="col-sm-9">
                        <asp:TextBox ValidationGroup="website" Text="" ID="TextBoxPreviewUrl" runat="server" CssClass="form-control required" MaxLength="255" placeholder="typically http://sitename.w.renderspace.net (in general: http://preview.example.com)" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="TextBoxPreviewUrl" CssClass="text-danger" ErrorMessage="Preview website address is required." />
                        <asp:RegularExpressionValidator runat="server" ControlToValidate="TextBoxPreviewUrl" CssClass="text-danger" ErrorMessage="Preview website URL must start with http." 
                            ValidationExpression="^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Production website address</label>
                    <div class="col-sm-9">
                        <asp:TextBox ValidationGroup="website" Text="" ID="TextBoxProductionUrl" runat="server" CssClass="form-control url" MaxLength="255" placeholder="http://www.example.com" />
                    </div>
                </div>
                 <div class="form-group">
                        <div class="col-sm-offset-3 col-sm-9">
                            <div class="checkbox">
                                <label>
                                    <asp:CheckBox runat="server" ID="CheckboxManualCopy" ClientIDMode="Static" />Manual server configuration (just add website to menu).
                                </label>
                            </div>
                        </div>
                    </div>

                 <asp:Placeholder runat="server" ID="PlaceholderNewDatabase">
                    <div class="form-group">
                        <div class="col-sm-offset-3 col-sm-9">
                            <div class="checkbox">
                                <label>
                                    <asp:CheckBox runat="server" ID="CheckboxNewDatabase" CssClass="j_control_new_database" ClientIDMode="Static" /> Create new database (you'll need admin password for database).
                                </label>
                            </div>
                        </div>
                    </div>
                     <div class="new_database_fields" style="display: none;">
                        <div class="form-group">
                            <label class="col-sm-3 control-label">Server name</label>
                            <div class="col-sm-9">
                            <asp:TextBox ValidationGroup="website" Text="" ID="TextBoxServer" runat="server" CssClass="form-control" MaxLength="255" placeholder="Microsoft SQL server name. Example: server.example.com" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-sm-3 control-label">Username</label>
                            <div class="col-sm-9">
                            <asp:TextBox ValidationGroup="website" Text="" ID="TextBoxUsername" runat="server" CssClass="form-control" MaxLength="255" placeholder="Database username. Provided by system administrator. Example: sa" />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-sm-3 control-label">Password</label>
                            <div class="col-sm-9">
                            <asp:TextBox ValidationGroup="website" Text="" ID="TextBoxPassword" runat="server" CssClass="form-control" MaxLength="255" placeholder="Database password. Provided by system administrator." />
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-sm-3 control-label">New database name</label>
                            <div class="col-sm-9">
                            <asp:TextBox ValidationGroup="website" Text="" ID="TextBoxDatabaseName" runat="server" CssClass="form-control" MaxLength="255" placeholder="Make sure that the name is in agreement with company rules. Example: name should match client's name." />
                            </div>
                        </div>
                     </div>
                </asp:Placeholder>

                 <div class="form-group">
                    <div class="col-sm-offset-3 col-sm-9">
                        <asp:LinkButton  ValidationGroup="website" id="ButtonAdd" runat="server" OnClick="ButtonAdd_Click" text="<span class='glyphicon glyphicon-plus'></span> Create website" CssClass="btn btn-success causesValidation" />
                    </div>
               </div>
            </div>
        </asp:View>
        <asp:View ID="View1" runat="server">
             <div class="adminSection form-horizontal validationGroup">
                 <div class="form-group">
                        <label class="col-sm-3 control-label">ID</label> 
                        <div class="col-sm-9">
                            <asp:Label runat="server" ID="LabelID"></asp:Label>
                        </div>
                    </div>
                 <div class="form-group">
                        <label class="col-sm-3 control-label">Language</label> 
                        <div class="col-sm-9">
                            <asp:Label runat="server" ID="LabelLanguage"></asp:Label>
                        </div>
                    </div>

                  <div class="form-group">
                        <label class="col-sm-3 control-label">PreviewUrl</label> 
                        <div class="col-sm-9">
                            <asp:Label runat="server" ID="LabelPreviewUrl"></asp:Label>
                        </div>
                    </div>
                  <div class="form-group">
                        <label class="col-sm-3 control-label">ProductionUrl</label> 
                        <div class="col-sm-9">
                            <asp:Label runat="server" ID="LabelProductionUrl"></asp:Label>
                        </div>
                    </div>
                 <div class="form-group">
                    <label class="col-sm-3 control-label">Website name</label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ID="TextBoxTitle" MaxLength="255" ValidationGroup="PageSett" CssClass="form-control required" placeholder="The name of your website. Not the URL, but the name. (i.e. 'IMDb' not 'imdb.com'.) Used at least by Facebook, possible other uses, too."></asp:TextBox>
                    </div>
                </div>
                 <div class="form-group">
                    <label class="col-sm-3 control-label">Description</label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ID="TextBoxDescription" MaxLength="4000" TextMode="MultiLine" Rows="3" ValidationGroup="PageSett" CssClass="form-control required" placeholder="Short description of the website."></asp:TextBox>
                    </div>
                </div>
                 <div class="form-group">
                    <label class="col-sm-3 control-label">Default og:image</label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ID="TextBoxOgImage" MaxLength="255" ValidationGroup="PageSett" CssClass="form-control" placeholder="Default image used by Facebook when sharing. Use at least 1200 x 630 pixels and absolute path."></asp:TextBox>
                    </div>
                </div>
                 <one:onesettings ID="OneSettingsWebsite" runat="server" Mode="Page" Text="Website settings" DisplayCommands="false" />	
                  <div class="form-group">
                        <div class="col-sm-3">
						    <uc2:LastChangeAndHistory ID="LastChangeAndHistory1" runat="server" />
                        </div>
                        <div class="col-sm-9">
                            <asp:LinkButton	id="ButtonSave" Runat="server"	CssClass="btn-success btn causesValidation" Text="Save website"  ValidationGroup="PageSett" OnClick="ButtonSave_Click" />
                        </div>
				    </div>
              </div>
        </asp:View>
        <asp:View ID="View2" runat="server">
            <div class="adminSection form-horizontal validationGroup">
                <div class="form-group">
                    <label class="col-sm-3 control-label">ID</label> 
                    <div class="col-sm-9">
                        <asp:Label runat="server" ID="LabelTemplateId"></asp:Label>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Name</label> 
                    <div class="col-sm-9">
                        <asp:TextBox ValidationGroup="template" Text="" ID="TextBoxTemplateName" runat="server" CssClass="form-control required" MaxLength="255" placeholder="Template name" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Type</label> 
                    <div class="col-sm-9">
                        <asp:TextBox ValidationGroup="template" Text="" ID="TextBoxTemplateType" runat="server" CssClass="form-control required" MaxLength="255" placeholder="Template type" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Content</label>
                    <div class="col-sm-9">
                        <asp:TextBox runat="server" ID="TextBoxTemplateContent" MaxLength="4000" TextMode="MultiLine" Rows="3" ValidationGroup="template" CssClass="form-control" placeholder="Template content"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-sm-12">
                        <asp:LinkButton	id="LinkButtonSaveTemplate" Runat="server"	CssClass="btn-success btn causesValidation" Text="Save template"  ValidationGroup="template" OnClick="ButtonSaveTemplate_Click" />
                    </div>
                </div>
            </div>
        </asp:View>
    </asp:MultiView>
     

	
</asp:Content>