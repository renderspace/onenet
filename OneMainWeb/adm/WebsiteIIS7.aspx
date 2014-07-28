<%@ Page Title="" Language="C#" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="WebsiteIIS7.aspx.cs" Inherits="OneMainWeb.adm.WebsiteIIS7" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="System.Security" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<%=System.Security.Principal.WindowsIdentity.GetCurrent().Name %>
    <script type="text/javascript" src="/JavaScript/jquery-1.6.1.min.js"></script>
    <script type="text/javascript">

        function ValidateDatabaseMode(source, arguments) {
            if ($('#<%=DropDownListDatabaseMode.ClientID %>').val().length == 0) {
                arguments.IsValid = false;
            } else {
                arguments.IsValid = true;
            }
        }

        function ValidateLcid(source, arguments) {
            if ($('#<%=DropDownListLcid.ClientID %>').val().length == 0) {
                arguments.IsValid = false;
            } else {
                arguments.IsValid = true;
            }
        }

        function ValidateDatabaseName(source, arguments) {
            if ($('#<%=DropDownListExistingDatabaseName.ClientID %>').val().length == 0) {
                arguments.IsValid = false;
            } else {
                arguments.IsValid = true;
            }
        }
            
    </script>
    <one:Notifier runat="server" ID="notifier" />
    <asp:Wizard DisplaySideBar="false" DisplayCancelButton="false" ID="Wizard1" runat="server" OnNextButtonClick="Wizard1_NextButtonClick" OnActiveStepChanged="Wizard1_ActiveStepChanged">
        <StartNavigationTemplate>
            <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" Text="$next" />
        </StartNavigationTemplate>
        <StepNavigationTemplate>
            <asp:Button ID="StepPreviousButton" runat="server" CommandName="MovePrevious" Text="$prev" />
            <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" Text="$next" />
        </StepNavigationTemplate>
        <WizardSteps>
            <asp:WizardStep Title="$database_mode" runat="server" StepType="Start">
                <div class="searchFull">
                    <div class="select">
                        <asp:Label runat="server" AssociatedControlID="DropDownListDatabaseMode" text="$select_database_mode" />
                        <asp:DropDownList ValidationGroup="database_mode" id="DropDownListDatabaseMode" runat="server">
                            <asp:ListItem Value="" Text="$select_mode" />
                            <asp:ListItem Value="new" Text="$new" />
                            <asp:ListItem Value="existing" Text="$existing" />
                        </asp:DropDownList>
                        <asp:CustomValidator ValidateEmptyText="true" runat="server" ClientValidationFunction="ValidateDatabaseMode" ControlToValidate="DropDownListDatabaseMode" ErrorMessage="*" ValidationGroup="database_mode" />
                    </div>
                    <two:Input Required="true" ID="InputDbServer" runat="server" Text="$db_server" ValidationGroup="database_mode" />
                    <two:Input Required="true" ID="InputMasterDbUsername" runat="server" Text="$master_db_username" ValidationGroup="database_mode" />
                    <two:Input TextMode="Password" Required="true" ID="InputMasterDbPassword" runat="server" Text="$master_db_password" ValidationGroup="database_mode" />
                </div>
            </asp:WizardStep>
            <asp:WizardStep StepType="Step" runat="server" Title="$database_new">
                <div class="searchFull">
                    <two:Input Required="true" ID="InputSqlPhysicalPath" runat="server" Text="$db_sql_physical_path" ValidationGroup="database_new" Value="c:\Program Files\Microsoft SQL Server\MSSQL.1\MSSQL\Data\" />
                    <two:Input Required="true" ID="InputNewDbName" runat="server" Text="$new_db_name" ValidationGroup="database_new" />
                </div>
            </asp:WizardStep>
            <asp:WizardStep StepType="Step" runat="server" Title="$database_existing">
                <div class="searchFull">    
                    <div class="select">
                        <asp:Label runat="server" AssociatedControlID="DropDownListExistingDatabaseName" ID="LabelExistingDatabaseName" Text="$existing_database_name" />
                        <asp:DropDownList ValidationGroup="database_existing" ID="DropDownListExistingDatabaseName" runat="server" />
                        <asp:CustomValidator runat="server" ValidationGroup="database_existing" ClientValidationFunction="ValidateDatabaseName" ControlToValidate="DropDownListExistingDatabaseName" ErrorMessage="*" />
                    </div>
                    <two:Input Required="true" ID="InputExistingDbUsername" runat="server" Text="$existing_db_username" ValidationGroup="database_existing" />
                    <two:Input TextMode="Password" Required="true" ID="InputExistingDbPassword" runat="server" Text="$existing_db_password" ValidationGroup="database_existing" />
                </div>
            </asp:WizardStep>
            <asp:WizardStep Title="$website_step" runat="server" StepType="Step">
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
                                <asp:CommandField HeaderText="$edit" ShowEditButton="true" CancelText="$cancel" EditText="$edit"
                                    UpdateText="$update" />
                                <asp:BoundField HeaderText="$id" DataField="Id" ReadOnly="true" />
                                <asp:BoundField HeaderText="$title" DataField="Title" ReadOnly="true" />
                                <asp:BoundField HeaderText="$host_headers" DataField="HostHeader" />
                                <asp:BoundField HeaderText="$lcid" DataField="LanguageId" ReadOnly="true" />
						        <asp:TemplateField HeaderText="$last_changed_by">
							        <ItemTemplate><%# Eval("PrincipalModified") == null || string.IsNullOrEmpty(Eval("PrincipalModified").ToString()) ? Eval("PrincipalCreated") : Eval("PrincipalModified")%></ItemTemplate>
						        </asp:TemplateField>
						        <asp:TemplateField HeaderText="$last_change_date">
							        <ItemTemplate><%# Eval("DateModified") == null || !((DateTime?)Eval("DateModified")).HasValue ? ((DateTime)Eval("DateCreated")).ToShortDateString() : ((DateTime?)Eval("DateModified")).Value.ToShortDateString() %></ItemTemplate>
						        </asp:TemplateField>
                                
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
                <div class="searchFull">
                    <two:Input ValidationGroup="website" ID="InputAppPoolName" runat="server" Text="$app_pool_name" />
                    <two:Input ValidationGroup="website" ID="InputSiteSpecificName" runat="server" Text="$site_specific_name" />
                    <span><two:Input ValidationGroup="website" ID="InputWebSiteName" runat="server" Text="$web_site_name" />.w.renderspace.net</span>
                    <two:Input ValidationGroup="website" ID="InputHostHeader" runat="server" Text="$web_site_host_headers" />
                    <div class="select">
                        <asp:Label ID="LabelLanguage" runat="server" AssociatedControlID="DropDownListLcid" Text="$language" />
                        <asp:DropDownList ValidationGroup="website" ID="DropDownListLcid" runat="server" />
                        <asp:CustomValidator runat="server" ClientValidationFunction="ValidateLcid" ControlToValidate="DropDownListLcid" ErrorMessage="*" ValidationGroup="website" />
                    </div>
                    <div class="save">
                        <asp:Button runat="server" ID="ButtonAddWebsite" ValidationGroup="website" OnClick="ButtonAddWebsite_Click" Text="$add_website" />
                    </div>
                </div>
            </asp:WizardStep>
        </WizardSteps>
    </asp:Wizard>
</asp:Content>
