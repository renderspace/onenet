<%@ Page Language="C#" ValidateRequest="false" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Forms.aspx.cs" Inherits="OneMainWeb.Forms" Title="$forms"  EnableEventValidation="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <one:Notifier runat="server" ID="Notifier1" />
        <asp:MultiView runat="server" ID="MultiView1"  OnActiveViewChanged="tabMultiview_OnViewIndexChanged">
            <asp:View ID="View1" runat="server">
                 <div class="searchFull">
                   
                                <asp:Button ID="cmdShowAddForm" runat="server" Text="$add_form" OnClick="cmdShowAddForm_Click" />
                   
                  </div>
                  <div class="centerFull">
                    <div class="biggv">            
                            <asp:GridView ID="formGridView" runat="server" PageSize="10" PageIndex="0"
                                PagerSettings-Mode="NumericFirstLast"
                                PagerSettings-LastPageText="$last"
                                PagerSettings-FirstPageText="$first"
                                PagerSettings-PageButtonCount="7" 
                                AllowSorting="True" 
                                AllowPaging="True" 
                                AutoGenerateColumns="False"
                                DataSourceID="FormListSource" 
                                DataKeyNames="Id"
                                OnRowDataBound="formGridView_RowDataBound"
                                OnRowCommand="formGridView_RowCommand">
                                <Columns>
                                    <asp:TemplateField HeaderText="$form_id">
                                        <ItemTemplate>
                                                <%# Eval("Id") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="$form">
                                        <ItemTemplate>
                                                <%# Eval("Title") %>
                                                <%# String.IsNullOrEmpty(Eval("SubTitle") != null? Eval("SubTitle").ToString() : "") ? "" : ("<br/><em>" + Eval("SubTitle") + "</em>")%>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="$form_type">
                                        <ItemTemplate>
                                                <%# ResourceManager.GetString("$" + Eval("FormType").ToString()) %>
                                        </ItemTemplate>
                                    </asp:TemplateField> 
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                                <asp:LinkButton Text="$delete" CommandName="Delete" CommandArgument='<%# Eval("Id") %>' ID="cmdDelete" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>                           
                                    <asp:TemplateField HeaderText="$form_submission_count">
                                        <ItemTemplate>
                                                <span style="margin-left: 20px;" ><%# Eval("SubmissionCount") %></span>
                                        </ItemTemplate>
                                    </asp:TemplateField> 
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                                <asp:LinkButton Text="$copy_as_new" CommandName="CopyAsNew" CommandArgument='<%# Eval("Id") %>' ID="cmdCopyAsNew" runat="server" /><br />
                                        </ItemTemplate>
                                    </asp:TemplateField> 
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                                <asp:LinkButton Text="Export Aggregate" CommandName="ExportAggregateResults" CommandArgument='<%# Eval("Id") %>' ID="LinkButtonAggregate" runat="server" />
                                                <asp:LinkButton Text="Export All" CommandName="ExportResults" CommandArgument='<%# Eval("Id") %>' ID="LinkAll" runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                                <asp:ImageButton CommandName="EditForm"	CommandArgument='<%# Eval("Id") %>' ID="cmdEditButton" runat="server" Enabled='<%# !(bool)Eval("MissingTranslation") %>'  />
                                                <asp:LinkButton Text="$edit" CommandName="EditForm" CommandArgument='<%# Eval("Id") %>' ID="cmdEdit" Enabled='<%# !(bool)Eval("MissingTranslation") %>' runat="server" />
                                        </ItemTemplate>
                                    </asp:TemplateField>                            
                                </Columns>
                                <PagerSettings FirstPageText="$first" LastPageText="$last" Mode="NumericFirstLast"
                                    PageButtonCount="7" />
                            </asp:GridView>
                           <asp:ObjectDataSource MaximumRowsParameterName="recordsPerPage" StartRowIndexParameterName="firstRecordIndex"
                                EnablePaging="True" ID="FormListSource" runat="server" SelectMethod="Select"
                                TypeName="OneMainWeb.FormHelper" DeleteMethod="DeleteForm" OnSelecting="FormListSource_Selecting" SelectCountMethod="SelectCount" SortParameterName="sortBy">
                                <SelectParameters>
                                    <asp:Parameter Name="sortDirection" DefaultValue="ASC" Type="string" />
                                </SelectParameters> 
                           </asp:ObjectDataSource>
                    </div>            
                </div>  <!-- centerFull -->

            </asp:View>
            <asp:View ID="View2" runat="server">
                <div class="searchFull">
                 </div>
                    <div class="centerStructure formStructure">     
                        <asp:PlaceHolder ID="plhAddForm" runat="server">
		                    <two:Input ID="txtAddForm" runat="server" Text="$add_new_form" />
		                    <asp:button CssClass="addbutton" id="cmdAddForm" Runat="server" Text="$add_new_form_button" OnClick="cmdAddForm_Click" />                    
		                </asp:PlaceHolder>
		        
		                <asp:PlaceHolder id="plhAddSection" runat="server">
		                    <two:Input Required="false" ID="txtAddSection" runat="server" text="$add_section" />
		                    <asp:button CssClass="addbutton" id="cmdAddSection" Runat="server" Text="$add_section_button" OnClick="cmdAddSection_Click" />                                            
		                </asp:PlaceHolder>
		        
		                <asp:PlaceHolder ID="plhAddQuestion" runat="server">
		                    <two:Input ID="txtAddQuestion" Required="false" runat="server" Text="$add_question" />
		                    <asp:button	CssClass="addbutton" id="cmdAddQuestion" Runat="server" Text="$add_question_button" OnClick="cmdAddQuestion_Click" />                                                                    			            
		                </asp:PlaceHolder>
		                <div style="width: 100%;">&nbsp;</div>
		                <br style="clear: both;" />                
                        <div id="treeHolder" class="treeHolder">                            
	                        <asp:TreeView OnAdaptedSelectedNodeChanged="FormTree_SelectedNodeChanged" OnSelectedNodeChanged="FormTree_SelectedNodeChanged" ID="FormTree" runat="server" 
		                        BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" Width="270" />
                        </div>		
           	        </div>        
		            <div class="mainEditor formEditor">
		                <div class="contentEntry">
		                    <asp:PlaceHolder ID="plhUpdateForm" runat="server">
		                        <two:Input Required="false" ID="txtFormName" runat="server" Text="$form_name" />
		                        <two:Input Required="false" ID="InputFormPrivateName" runat="server" Text="$form_private_name" />
		                        <two:Input Required="false" ID="txtFormThankYouNote" runat="server" Text="$form_thank_you_note" Rows="3" TextMode="multiLine" />
		                        <two:Input Required="false" ID="txtFormDescription" runat="server" Text="$form_desc" Rows="3" TextMode="multiLine" />
		                
                                <div class="select">
                                    <asp:Label AssociatedControlID="ddlFormTypes" runat="server" ID="lblFormTypes"
                                        Text="$form_type" />
                                    <asp:DropDownList AppendDataBoundItems="False" 
                                        ID="ddlFormTypes" runat="server" OnDataBound="ddlFormTypes_DataBound" DataSourceID="FormTypesSource" AutoPostBack="true" OnSelectedIndexChanged="ddlFormTypes_SelectedIndexChanged"
                                        />
                                    <asp:ObjectDataSource ID="FormTypesSource" runat="server"
                                        SelectMethod="ListFormTypes" TypeName="OneMainWeb.FormHelper">
                                        <SelectParameters>
                                        </SelectParameters>
                                    </asp:ObjectDataSource>                        
                                </div>
                        
                                <div class="select">
                                    <asp:Label AssociatedControlID="ddlUpdateSectionTypes" runat="server" ID="lblUpdateSectionTypes"
                                        Text="$section_type" />
                                    <asp:DropDownList AppendDataBoundItems="False" 
                                        ID="ddlUpdateSectionTypes" OnDataBound="ddlUpdateSectionTypes_DataBound" runat="server" DataSourceID="SectionTypesSource"
                                        />
                                    <asp:ObjectDataSource ID="SectionTypesSource" runat="server"
                                        SelectMethod="ListSectionTypes" TypeName="OneMainWeb.FormHelper">
                                        <SelectParameters>
                                        </SelectParameters>
                                    </asp:ObjectDataSource>    
                                </div> 
                         
                                <two:LabeledCheckBox runat="server" Text="$allow_multiple_submissions" ID="chkAllowMultipleSubmissions" />
                                <two:LabeledCheckBox runat="server" Text="$allow_modify_in_submission" ID="chkAllowModifyInSubmission" />                                                
                                <two:ValidInput Required="false" ID="txtSendTo" runat="server" Text="$form_send_to" ValidationType="email" />
                                <two:Input Required="false" ID="InputCompletionRedirect" runat="server" Text="$form_completion_redirect" />
                                <div class="save">
		                            <asp:button	id="cmdUpdateForm" Runat="server" CssClass="button" Text="$update_form" OnClick="cmdUpdateForm_Click" />
		                        </div>
		                    </asp:PlaceHolder>
		                    <asp:PlaceHolder ID="plhUpdateSection" runat="server">
		                        <two:Input Required="false" runat="server" ID="txtSectionName" Text="$section_name" />
		                        <two:Input Required="false" ID="InputSectionDescription" runat="server" Text="$section_desc" Rows="3" TextMode="multiLine" />
                                <two:Input Required="false" ID="InputSectionOnClientClick" runat="server" Text="$section_on_client_click" />
                                <div class="save">
		                            <asp:button	id="cmdUpdateSection" Runat="server" CssClass="button" Text="$update_section" OnClick="cmdUpdateSection_Click" />			            
		                            <asp:button	id="cmdDeleteSection" Runat="server" CssClass="button" Text="$mark_section_as_deleted" OnClick="cmdDeleteSection_Click" />			            		                    
		                            <asp:button	id="cmdUnDeleteSection" Runat="server" CssClass="button" Text="$undelete_section" OnClick="cmdUnDeleteSection_Click" />			            		                    		                    
		                        </div>
		                    </asp:PlaceHolder>
		                    <asp:PlaceHolder ID="plhUpdateQuestion" runat="server">
		                        <two:input id="txtQuestionText" runat="server" Text="$question_text" />
		                        <two:input Required="false" TextMode="MultiLine" Rows="3" ID="txtQuestionDescription" runat="server" Text="$question_description" />
                                <two:LabeledCheckBox runat="server" Text="$question_requires_answer" ID="chkAnswerIsRequired" />
                                <div class="radiobuttonlist" id="divFrontEndQuestionTypes" runat="server">
                                    <asp:Label CssClass="radiobuttonlistTitle" ID="lblFrontEndQuestionTypes" runat="server" Text="$user_question_types" />
                                    <asp:RadioButtonList OnSelectedIndexChanged="radFrontEndQuestionTypes_SelectedIndexChanged" ID="radFrontEndQuestionTypes" AutoPostBack="true" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Right" />
                                </div>

                                <div class="separate_input" id="separateInput" runat="server">
                                    <two:ValidInput ID="txtNumberOfRows" runat="server" Text="$number_of_lines_for_editing" ValidationType="numeric" />
                                    <two:ValidInput ID="txtMaxChars" runat="server" Text="$max_chars" ValidationType="numeric" />
                                    <two:ValidInput ID="txtMaximumFileSize" runat="server" Text="$maximum_file_size" ValidationType="numeric" />                                                    
                                    <div class="checkboxlist" id="divAllowedMimeTypes" runat="server">
                                        <asp:Label CssClass="checkboxlistTitle" ID="lblAllowedMimeTypes" runat="server" Text="$allowed_mime_types" />
                                        <asp:CheckBoxList ID="chkAllowedMimeTypes" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Right"  />
                                    </div>
                            
                                    <div class="radiobuttonlist" id="divAnswerPresentationTypes" runat="server">
                                        <asp:Label CssClass="radiobuttonlistTitle" ID="lblAnswerPresentationTypes" runat="server" Text="$type_of_information" />
                                        <asp:RadioButtonList AutoPostBack="true" ID="radAnswerPresentationTypes" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Right" OnSelectedIndexChanged="radAnswerPresentationTypes_SelectIndexChanged" />
                                    </div>       
                            
                                    <two:LabeledCheckBox ID="chkAllowBlankAnswersInMenu" runat="server" Text="$allow_blank_answers_in_menu" />
                                    <two:LabeledCheckBox ID="chkFirstAnswerIsFake" runat="server" Text="$first_answer_in_menu_is_fake" />
                                </div>
                        
                                <two:Input ID="txtAnswers" runat="server" Text="$answers_one_per_line" Rows="5" TextMode="MultiLine" />                                                                                                    
                                <div class="save">
		                    
		                            <asp:button	id="cmdDeleteQuestion" Runat="server" CssClass="button" Text="$mark_question_as_deleted" OnClick="cmdDeleteQuestion_Click" />			            		                    		                    
		                            <asp:button	id="cmdUnDeleteQuestion" Runat="server" CssClass="button" Text="$undelete_question" OnClick="cmdUnDeleteQuestion_Click" />			            		                    		                    		                    
		                            <span>&nbsp;&nbsp;</span>
		                            <asp:button	id="cmdUpdateQuestion" Runat="server" CssClass="button" Text="$update_question" OnClick="cmdUpdateQuestion_Click" />			            
		                        </div>
		                    </asp:PlaceHolder>
                        </div>
                    </div>
                    <div class="searchFull" id="overallButtons" runat="server">
                        <p class="save">
                            <asp:Button ID="cmdCancelButton" OnClick="cmdCancelButton_Click" runat="server" Text="$cancel" />
                            <asp:Button ID="cmdSaveForm" OnClick="cmdSaveForm_Click" runat="server" Text="Save" />
                            <asp:Button ID="cmdSaveFormAndClose" OnClick="cmdSaveFormAndClose_Click" runat="server" Text="Save and close" />
                        </p> 				    
                    </div>       
            </asp:View>
        </asp:MultiView>
</asp:Content>


