<%@ Page Language="C#" ValidateRequest="false" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Forms.aspx.cs" Inherits="OneMainWeb.Forms" Title="$forms"  EnableEventValidation="false" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Register TagPrefix="one" TagName="Notifier" Src="~/AdminControls/Notifier.ascx" %>
<%@ Import Namespace="One.Net.BLL"%>
<%@ OutputCache Location="None" VaryByParam="None" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <one:Notifier runat="server" ID="Notifier1" />
        <asp:MultiView runat="server" ID="MultiView1"  OnActiveViewChanged="Multiview1_ActiveViewChanged">
            <asp:View ID="View1" runat="server">
                <div class="adminSection">
                     <div class="col-md-2">
                         <asp:LinkButton ID="cmdShowAddForm" runat="server" text="<span class='glyphicon glyphicon-plus'></span> Add" OnClick="cmdShowAddForm_Click" CssClass="btn btn-success" />			
			        </div>
                    
                </div>
                <asp:GridView ID="formGridView" runat="server" CssClass="table table-hover"
                    AllowSorting="True" 
                    AutoGenerateColumns="False"
                    DataKeyNames="Id"
                    OnRowDataBound="formGridView_RowDataBound"
                    OnRowCommand="formGridView_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Id">
                            <ItemTemplate>
                                    <%# Eval("Id") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Title">
                            <ItemTemplate>
                                    <%# Eval("Title") %>
                                    <%# String.IsNullOrEmpty(Eval("SubTitle") != null? Eval("SubTitle").ToString() : "") ? "" : ("<br/><em>" + Eval("SubTitle") + "</em>")%>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Type">
                            <ItemTemplate>
                                    <%# Eval("FormType").ToString() %>
                            </ItemTemplate>
                        </asp:TemplateField>                          
                        <asp:TemplateField HeaderText="Submissions">
                            <ItemTemplate>
                                    <span style="margin-left: 20px;" ><%# Eval("SubmissionCount") %></span>
                            </ItemTemplate>
                        </asp:TemplateField> 
                        <asp:TemplateField>
                            <ItemTemplate>
                                    <asp:LinkButton Text="Copy as new" CommandName="CopyAsNew" CommandArgument='<%# Eval("Id") %>' ID="cmdCopyAsNew" runat="server" CssClass="btn btn-info btn-xs" />	
                            </ItemTemplate>
                        </asp:TemplateField> 
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:HyperLink ID="HyperLinkExport1" runat="server" NavigateUrl='/adm/ExcelExport.ashx?id=<%# Eval("Id") %>&type=form_agregate' Text="Export Aggregate" CssClass="btn btn-default btn-xs" />	
                                <asp:HyperLink ID="HyperLinkExport2" runat="server" NavigateUrl='/adm/ExcelExport.ashx?id=<%# Eval("Id") %>&type=form_all_submissions' Text="Export All" CssClass="btn btn-default btn-xs" />	
                                   
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                    <asp:LinkButton Text='<span class=\"glyphicon glyphicon-pencil\"></span> Edit' CommandName="EditForm" CommandArgument='<%# Eval("Id") %>' ID="cmdEdit" Enabled='<%# !(bool)Eval("MissingTranslation") %>' runat="server" CssClass="btn btn-info btn-xs" />	
                            </ItemTemplate>
                        </asp:TemplateField>                            
                    </Columns>
                </asp:GridView>
                <div class="text-center">
                    <two:PostbackPager id="TwoPostbackPager1" OnCommand="TwoPostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
                </div>	  
            </asp:View>
            <asp:View ID="View2" runat="server">
                 <div class="col-md-3">
                     <div class="adminSection">
                            <asp:PlaceHolder ID="plhAddForm" runat="server">
		                        <asp:TextBox ID="txtAddForm" runat="server" Text="$add_new_form" />
		                        <asp:button CssClass="addbutton" id="cmdAddForm" Runat="server" Text="$add_new_form_button" OnClick="cmdAddForm_Click" />                    
		                    </asp:PlaceHolder>
		        
		                    <asp:PlaceHolder id="plhAddSection" runat="server">
		                        <asp:TextBox Required="false" ID="txtAddSection" runat="server" text="$add_section" />
		                        <asp:button CssClass="addbutton" id="cmdAddSection" Runat="server" Text="$add_section_button" OnClick="cmdAddSection_Click" />                                            
		                    </asp:PlaceHolder>
		        
		                    <asp:PlaceHolder ID="plhAddQuestion" runat="server">
		                        <asp:TextBox ID="txtAddQuestion" Required="false" runat="server" Text="$add_question" />
		                        <asp:button	CssClass="addbutton" id="cmdAddQuestion" Runat="server" Text="$add_question_button" OnClick="cmdAddQuestion_Click" />                                                                    			            
		                    </asp:PlaceHolder>
		                    <div style="width: 100%;">&nbsp;</div>
		                    <br style="clear: both;" />                
                            <div id="treeHolder" class="treeView">                            
	                            <asp:TreeView OnAdaptedSelectedNodeChanged="FormTree_SelectedNodeChanged" OnSelectedNodeChanged="FormTree_SelectedNodeChanged" ID="FormTree" runat="server" 
		                            BackColor="#F3F2EF" SelectedNodeStyle-BackColor="Gray" Width="270" />
                            </div>	
                        </div>	
           	        </div>        
		            <div class="col-md-9">
                        <div class="adminSection form-horizontal">
		                    <asp:PlaceHolder ID="plhUpdateForm" runat="server">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Form name</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox  ID="txtFormName" runat="server" CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Private name</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox Required="false" ID="InputFormPrivateName" runat="server" CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Thank you note</label>
                                    <div class="col-sm-9">
                                       <asp:TextBox Required="false" ID="txtFormThankYouNote" runat="server" Rows="3" TextMode="multiLine"  CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Description</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox Required="false" ID="txtFormDescription" runat="server" Rows="3" TextMode="multiLine"  CssClass="form-control" />
                                    </div>
                                </div>
		                        
		                
                                 <div class="form-group">
                                    <asp:Label AssociatedControlID="ddlFormTypes" runat="server" ID="lblFormTypes" CssClass="col-sm-3 control-label"
                                        Text="Form type" />
                                     <div class="col-sm-9">
                                    <asp:DropDownList AppendDataBoundItems="False" 
                                        ID="ddlFormTypes" runat="server" OnDataBound="ddlFormTypes_DataBound" DataSourceID="FormTypesSource" AutoPostBack="true" OnSelectedIndexChanged="ddlFormTypes_SelectedIndexChanged"
                                         CssClass="form-control" />
                                    <asp:ObjectDataSource ID="FormTypesSource" runat="server"
                                        SelectMethod="ListFormTypes" TypeName="OneMainWeb.FormHelper">
                                        <SelectParameters>
                                        </SelectParameters>
                                    </asp:ObjectDataSource>
                                        </div>                        
                                </div>
                        
                                <div class="form-group">
                                    <asp:Label AssociatedControlID="ddlUpdateSectionTypes" runat="server" ID="lblUpdateSectionTypes"  CssClass="col-sm-3 control-label"
                                        Text="Section type" />
                                     <div class="col-sm-9">
                                    <asp:DropDownList AppendDataBoundItems="False" 
                                        ID="ddlUpdateSectionTypes" OnDataBound="ddlUpdateSectionTypes_DataBound" runat="server" DataSourceID="SectionTypesSource"
                                         CssClass="form-control" />
                                    <asp:ObjectDataSource ID="SectionTypesSource" runat="server"
                                        SelectMethod="ListSectionTypes" TypeName="OneMainWeb.FormHelper">
                                        <SelectParameters>
                                        </SelectParameters>
                                    </asp:ObjectDataSource>
                                        </div>    
                                </div> 

                                <div class="checkbox">
                                    <label class="col-sm-offset-3 col-sm-9">
                                        <asp:CheckBox runat="server" ID="chkAllowMultipleSubmissions" />
                                        Allow multiple submissions
                                    </label>
                                </div>
                                <div class="checkbox">
                                    <label class="col-sm-offset-3 col-sm-9">
                                        <asp:CheckBox runat="server" ID="chkAllowModifyInSubmission"  />
                                        Allow_modify_in_submission
                                    </label>
                                </div>                                            

                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Sent to email</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox type="email" runat="server" ID="txtSendTo" CssClass="form-control" />
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Redirect to URL after completion</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox type="url" ID="InputCompletionRedirect" runat="server" CssClass="form-control" />
                                    </div>
                                </div>

                                <div class="form-group">
                                    <div class="col-sm-offset-3 col-sm-9">
		                                <asp:button	id="cmdUpdateForm" Runat="server" CssClass="button" Text="$update_form" OnClick="cmdUpdateForm_Click" />
                                    </div>
		                        </div>
		                    </asp:PlaceHolder>

		                    <asp:PlaceHolder ID="plhUpdateSection" runat="server">
		                        <asp:TextBox Required="false" runat="server" ID="txtSectionName" Text="$section_name" />
		                        <asp:TextBox Required="false" ID="InputSectionDescription" runat="server" Text="$section_desc" Rows="3" TextMode="multiLine" />
                                <asp:TextBox Required="false" ID="InputSectionOnClientClick" runat="server" Text="$section_on_client_click" />
                                <div class="save">
		                            <asp:button	id="cmdUpdateSection" Runat="server" CssClass="button" Text="$update_section" OnClick="cmdUpdateSection_Click" />			            
		                            <asp:button	id="cmdDeleteSection" Runat="server" CssClass="button" Text="$mark_section_as_deleted" OnClick="cmdDeleteSection_Click" />			            		                    
		                            <asp:button	id="cmdUnDeleteSection" Runat="server" CssClass="button" Text="$undelete_section" OnClick="cmdUnDeleteSection_Click" />			            		                    		                    
		                        </div>
		                    </asp:PlaceHolder>
		                    <asp:PlaceHolder ID="plhUpdateQuestion" runat="server">

                                <label>Question</label>
		                        <asp:TextBox id="txtQuestionText" runat="server"  />

                                <label>Description</label>
		                        <asp:TextBox Required="false" TextMode="MultiLine" Rows="3" ID="txtQuestionDescription" runat="server" />

                                <two:LabeledCheckBox runat="server" Text="$question_requires_answer" ID="chkAnswerIsRequired" />
                                <div class="radiobuttonlist" id="divFrontEndQuestionTypes" runat="server">
                                    <asp:Label CssClass="radiobuttonlistTitle" ID="lblFrontEndQuestionTypes" runat="server" Text="$user_question_types" />
                                    <asp:RadioButtonList OnSelectedIndexChanged="radFrontEndQuestionTypes_SelectedIndexChanged" ID="radFrontEndQuestionTypes" AutoPostBack="true" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Right" />
                                </div>

                                <div class="separate_input" id="separateInput" runat="server">
                                    <div class="form-input">
                                        <div class="col-sm-3">
                                            <label>$number_of_lines_for_editing</label>
                                        </div>
                                        <div class="col-sm-9">
                                            <asp:TextBox ID="txtNumberOfRows" runat="server" type="number" />
                                        </div>
                                    </div>
                                    <div class="form-input">
                                        <div class="col-sm-3">
                                             <label>$max_chars</label>
                                        </div>
                                        <div class="col-sm-9">
                                            <asp:TextBox ID="txtMaxChars" runat="server" Text="$max_chars" type="number" />
                                        </div>
                                    </div>
                                    <div class="form-input">
                                        <div class="col-sm-3">
                                             <label>$maximum_file_size</label>
                                        </div>
                                        <div class="col-sm-9">
                                            <asp:TextBox ID="txtMaximumFileSize" runat="server" type="number"/>       
                                        </div>
                                    </div>
                                    
                                    
                                    
                                    
                                    
                                                                                 
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
                        
                                <asp:TextBox ID="txtAnswers" runat="server" Text="$answers_one_per_line" Rows="5" TextMode="MultiLine" />                                                                                                    
                                <div class="save">
		                    
		                            <asp:button	id="cmdDeleteQuestion" Runat="server" CssClass="button" Text="$mark_question_as_deleted" OnClick="cmdDeleteQuestion_Click" />			            		                    		                    
		                            <asp:button	id="cmdUnDeleteQuestion" Runat="server" CssClass="button" Text="$undelete_question" OnClick="cmdUnDeleteQuestion_Click" />			            		                    		                    		                    
		                            <span>&nbsp;&nbsp;</span>
		                            <asp:button	id="cmdUpdateQuestion" Runat="server" CssClass="button" Text="$update_question" OnClick="cmdUpdateQuestion_Click" />			            
		                        </div>
		                    </asp:PlaceHolder>
                             <div class="form-group" id="overallButtons" runat="server">
                                 <div class="col-sm-offset-3 col-sm-9">
                                    <asp:LinkButton ID="cmdCancelButton" OnClick="cmdCancelButton_Click" runat="server" Text="Cancel" CssClass="btn btn-default" />
                                    <asp:LinkButton ID="cmdSaveForm" OnClick="cmdSaveForm_Click" runat="server" Text="Save" CssClass="btn btn-success" />
                                    <asp:LinkButton ID="cmdSaveFormAndClose" OnClick="cmdSaveFormAndClose_Click" runat="server" Text="Save and close" CssClass="btn btn-success" />
                                 </div>
                            </div>    
                        </div>
                    </div>
            </asp:View>
        </asp:MultiView>
</asp:Content>


