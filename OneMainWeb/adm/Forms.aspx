<%@ Page Language="C#" ValidateRequest="false" MasterPageFile="~/OneMain.Master" AutoEventWireup="true" CodeBehind="Forms.aspx.cs" Inherits="Forms.adm.Forms" Title="One.NET Forms"  EnableEventValidation="false" %>
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
            <asp:GridView ID="GridViewForms" runat="server" CssClass="table table-hover"
                AllowSorting="True" 
                AutoGenerateColumns="False"
                DataKeyNames="Id"
                OnRowDataBound="GridViewForms_RowDataBound"
                OnRowCommand="GridViewForms_RowCommand">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <input id="chkAll" onclick="SelectAllCheckboxes(this);" runat="server" type="checkbox" />
                        </HeaderTemplate>								    
						<ItemTemplate>
							<asp:Literal ID="litId" Visible="false" runat="server" Text='<%# Eval("Id") %>' />
							<asp:CheckBox ID="chkFor" runat="server" Text="" />
						</ItemTemplate>
					</asp:TemplateField>
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
                            <asp:HyperLink ID="HyperLinkExport1" runat="server" NavigateUrl='/adm/ExcelExport.ashx?id={0}&type=form_agregate' Text="Export Aggregate" CssClass="btn btn-default btn-xs" />	
                            <asp:HyperLink ID="HyperLinkExport2" runat="server" NavigateUrl='/adm/ExcelExport.ashx?id={0}&type=form_all_submissions' Text="Export All" CssClass="btn btn-default btn-xs" />	
                                   
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                                <asp:LinkButton Text='<span class=\"glyphicon glyphicon-pencil\"></span> Edit' CommandName="EditForm" CommandArgument='<%# Eval("Id") %>' ID="cmdEdit" runat="server" CssClass="btn btn-info btn-xs" />	
                        </ItemTemplate>
                    </asp:TemplateField>                            
                </Columns>
            </asp:GridView>
            <div class="text-center">
                <two:PostbackPager id="TwoPostbackPager1" OnCommand="TwoPostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
            </div>	  
            <asp:Panel runat="server" ID="PanelGridButtons" CssClass="form-group">
                <div class="col-sm-12">
                    <asp:LinkButton CssClass="btn btn-danger deleteAll" ID="ButtonDelete" OnClick="ButtonDelete_Click" runat="server" CausesValidation="false" Text="<span class='glyphicon glyphicon-trash'></span> Delete selected" />
                </div>
            </asp:Panel>
            <asp:Panel runat="server" ID="PanelNoResults"  CssClass="col-md-12">
                    <div class="alert alert-info" role="alert">
                    No forms found in database.
                        </div>
            </asp:Panel>
        </asp:View>
        <asp:View ID="View2" runat="server">
                <div class="col-md-3">
                    <section class="module tall">
                        <asp:Panel ID="plhAddForm" runat="server" CssClass="form-group validationGroup">
		                    <asp:TextBox formnovalidate="formnovalidate" ID="txtAddForm" runat="server" placeholder="new form name"  CssClass="required" MaxLength="255"/>
		                    <asp:LinkButton formnovalidate="formnovalidate" CssClass="btn btn-success causesValidation" Text="<span class='glyphicon glyphicon-plus'></span> Add"  id="cmdAddForm" Runat="server" OnClick="cmdAddForm_Click" />
		                </asp:Panel>
		        
		                <asp:Panel id="plhAddSection" runat="server" CssClass="form-group validationGroup">
		                    <asp:TextBox formnovalidate="formnovalidate"  ID="txtAddSection" runat="server" placeholder="new section name" CssClass="required" MaxLength="255"/>
		                    <asp:LinkButton formnovalidate="formnovalidate"  id="cmdAddSection" Runat="server" CssClass="btn btn-success causesValidation" Text="<span class='glyphicon glyphicon-plus'></span> Add"  OnClick="cmdAddSection_Click" />                                            
		                </asp:Panel>
		        
		                <asp:Panel ID="plhAddQuestion" runat="server" CssClass="form-group validationGroup">
		                    <asp:TextBox formnovalidate="formnovalidate" ID="txtAddQuestion"  runat="server" placeholder="new question" CssClass="required" MaxLength="255"/>
		                    <asp:LinkButton formnovalidate="formnovalidate"	 id="cmdAddQuestion" Runat="server" CssClass="btn btn-success causesValidation" Text="<span class='glyphicon glyphicon-plus'></span> Add"  OnClick="cmdAddQuestion_Click" />                                                                    			            
		                </asp:Panel>
		                    
                        <div class="treeview">
	                        <asp:TreeView OnAdaptedSelectedNodeChanged="FormTree_SelectedNodeChanged" OnSelectedNodeChanged="FormTree_SelectedNodeChanged" ID="FormTree" runat="server" />
                        </div>	
                    </section>
           	    </div>        
		        <div class="col-md-9">
                    <div class="adminSection form-horizontal">
		                <asp:PlaceHolder ID="plhUpdateForm" runat="server">
                            <div class="validationGroup">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Form name</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox formnovalidate="formnovalidate"  ID="txtFormName" runat="server" CssClass="form-control required" MaxLength="255"/>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Private name</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox formnovalidate="formnovalidate"  ID="InputFormPrivateName" runat="server" CssClass="form-control" MaxLength="255"/>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Thank you note</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox formnovalidate="formnovalidate"  ID="txtFormThankYouNote" runat="server" Rows="3" TextMode="multiLine"  CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Description</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox formnovalidate="formnovalidate"  ID="txtFormDescription" runat="server" Rows="3" TextMode="multiLine"  CssClass="form-control" />
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
                                        SelectMethod="ListFormTypes" TypeName="Forms.adm.FormHelper">
                                        <SelectParameters>
                                        </SelectParameters>
                                    </asp:ObjectDataSource>
                                        </div>                        
                                </div>
                        
                                <div class="form-group">
                                    <asp:Label AssociatedControlID="ddlUpdateSectionTypes" runat="server" ID="lblUpdateSectionTypes"  CssClass="col-sm-3 control-label"
                                        Text="Section type" />
                                        <div class="col-sm-9">
                                    <asp:DropDownList formnovalidate="formnovalidate" AppendDataBoundItems="False" 
                                        ID="ddlUpdateSectionTypes" OnDataBound="ddlUpdateSectionTypes_DataBound" runat="server" DataSourceID="SectionTypesSource"
                                            CssClass="form-control" />
                                    <asp:ObjectDataSource ID="SectionTypesSource" runat="server"
                                        SelectMethod="ListSectionTypes" TypeName="Forms.adm.FormHelper">
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
                                        <asp:TextBox formnovalidate="formnovalidate" type="text" runat="server" ID="txtSendTo" CssClass="form-control" MaxLength="1000" />
                                    </div>
                                </div>

                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Redirect to URL after completion</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox formnovalidate="formnovalidate" ID="InputCompletionRedirect" runat="server" CssClass="form-control absrelurl" MaxLength="255" />
                                    </div>
                                </div>

                                <div class="form-group">
                                    <div class="col-sm-offset-3 col-sm-9">
		                                <asp:button formnovalidate="formnovalidate"	id="cmdUpdateForm" Runat="server" CssClass="btn btn-success causesValidation" Text="Save form metadata" OnClick="cmdUpdateForm_Click" />
                                    </div>
		                        </div>
                            </div>
		                </asp:PlaceHolder>

		                <asp:PlaceHolder ID="plhUpdateSection" runat="server">
                            <div class="validationGroup">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Section name</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox formnovalidate="formnovalidate"   runat="server" ID="txtSectionName"  CssClass="form-control required" MaxLength="255" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Description</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox  formnovalidate="formnovalidate"  ID="InputSectionDescription" runat="server"   Rows="3" TextMode="multiLine" CssClass="form-control" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Javascript onClientClick</label>
                                    <div class="col-sm-9">
                                        <asp:TextBox formnovalidate="formnovalidate"  ID="InputSectionOnClientClick" runat="server"   CssClass="form-control" MaxLength="255" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-offset-3 col-sm-9">
		                                <asp:button formnovalidate="formnovalidate"	id="cmdUpdateSection" Runat="server"  Text="Save section" OnClick="cmdUpdateSection_Click" CssClass="causesValidation btn btn-success" />			            
		                            <asp:button formnovalidate="formnovalidate"	id="cmdDeleteSection" Runat="server"  Text="Delete section" OnClick="cmdDeleteSection_Click" CssClass="btn btn-danger" />			            		                    
		                            <asp:button formnovalidate="formnovalidate"	id="cmdUnDeleteSection" Runat="server"  Text="Undelete section" OnClick="cmdUnDeleteSection_Click" CssClass="btn btn-default" />			            		                    		                    
                                    </div>
		                        </div>
                            </div>
		                </asp:PlaceHolder>
		                <asp:PlaceHolder ID="plhUpdateQuestion" runat="server">
                            <div class="validationGroup">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">Question</label>
                                    <div class="col-sm-9">
		                            <asp:TextBox formnovalidate="formnovalidate" id="txtQuestionText" runat="server" CssClass="form-control required" MaxLength="255" />
                                    </div>
                                    </div>

                                    <div class="form-group">
                                    <label class="col-sm-3 control-label">Description</label>
                                        <div class="col-sm-9">
		                                <asp:TextBox formnovalidate="formnovalidate" TextMode="MultiLine" Rows="3" ID="txtQuestionDescription" runat="server" CssClass="form-control" />
                                        </div>
                                    </div>

                                <div class="checkbox">
                                    <label class="col-sm-offset-3 col-sm-9">
                                        <asp:CheckBox runat="server" ID="chkAnswerIsRequired"  />
                                        Answer required
                                    </label>
                                </div>    


                                <div  id="divFrontEndQuestionTypes" runat="server" class="form-group">
                                    <label class="col-sm-3 control-label">user_question_type</label>
                                        <div class="col-sm-9">
                                        <asp:RadioButtonList OnSelectedIndexChanged="radFrontEndQuestionTypes_SelectedIndexChanged" ID="radFrontEndQuestionTypes" AutoPostBack="true" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Right" />
                                        </div>
                                </div>

                                <asp:PlaceHolder runat="server" ID="separateInput">
                                        <asp:Panel runat="server" ID="PanelNoOfLines" class="form-group">
                                        <label class="col-sm-3 control-label">Number of lines</label>
                                        <div class="col-sm-9">
                                            <asp:TextBox formnovalidate="formnovalidate" ID="txtNumberOfRows" runat="server" type="number" CssClass="form-control" />
                                        </div>
                                    </asp:Panel>
                                        <div class="form-group">
                                        <label class="col-sm-3 control-label">Max characters</label>
                                        <div class="col-sm-9">
                                            <asp:TextBox formnovalidate="formnovalidate" ID="txtMaxChars" runat="server" placeholder="chars" type="number" />
                                        </div>
                                    </div>
                                        <asp:Panel runat="server" ID="PanelMaxFileSize" class="form-group">
                                        <label class="col-sm-3 control-label">Maximum file size</label>
                                        <div class="col-sm-9">
                                            <asp:TextBox formnovalidate="formnovalidate" ID="txtMaximumFileSize" runat="server" type="number" CssClass="form-control" />    
                                        </div>
                                    </asp:Panel>
                                        
                                    <div id="divAllowedMimeTypes" runat="server" class="form-group">
                                        <label class="col-sm-3 control-label">Allowed MIME types</label>
                                        
                                        <div class="col-sm-9">
                                        <asp:CheckBoxList ID="chkAllowedMimeTypes" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Right"  />
                                            </div>
                                    </div>
                            
                                    <div id="divAnswerPresentationTypes" runat="server" class="form-group">
                                        <label class="col-sm-3 control-label">Type</label>
                                        <div class="col-sm-9">
                                        <asp:RadioButtonList AutoPostBack="true" ID="radAnswerPresentationTypes" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Right" OnSelectedIndexChanged="radAnswerPresentationTypes_SelectIndexChanged" />
                                            </div>
                                    </div>       

                                    <div class="checkbox">
                                        <label class="col-sm-offset-3 col-sm-9">
                                            <asp:CheckBox runat="server" ID="chkAllowBlankAnswersInMenu"  />
                                            Allow blank answers in menu
                                        </label>
                                    </div>   

                                    <div class="checkbox">
                                        <label class="col-sm-offset-3 col-sm-9">
                                            <asp:CheckBox runat="server" ID="chkFirstAnswerIsFake"  />
                                            Nonselectable first option in menu
                                        </label>
                                    </div>   
                                </asp:PlaceHolder>

                                <asp:Panel runat="server" ID="PanelAnswersList" class="form-group">
                                        <label class="col-sm-3 control-label">Answers one per line</label>
                                        <div class="col-sm-9">
                                            <asp:TextBox formnovalidate="formnovalidate" ID="txtAnswers" runat="server" Rows="5" TextMode="MultiLine" CssClass="form-control required" />
                                        </div>
                                    </asp:Panel>                        
                                <div class="form-group">
                                    <div class="col-sm-offset-3 col-sm-9">
		                                <asp:LinkButton formnovalidate="formnovalidate"	id="cmdDeleteQuestion" Runat="server" Text="Delete question" OnClick="cmdDeleteQuestion_Click" CssClass="btn btn-default" />			            		                    		                    
		                                <asp:LinkButton formnovalidate="formnovalidate"	id="cmdUnDeleteQuestion" Runat="server" Text="Undelete question" OnClick="cmdUnDeleteQuestion_Click" CssClass="btn btn-danger" />			            		                    		                    		                    
		                                <asp:LinkButton formnovalidate="formnovalidate"	id="cmdUpdateQuestion" Runat="server" Text="Save question" OnClick="cmdUpdateQuestion_Click" CssClass="causesValidation btn btn-success" />	
		                            </div>
		                        </div>
                            </div>
		                </asp:PlaceHolder>
                        <div class="form-group" id="overallButtons" runat="server">
                                <div class="col-sm-offset-3 col-sm-9">
                                <asp:LinkButton formnovalidate="formnovalidate" ID="cmdCancelButton" OnClick="cmdCancelButton_Click" runat="server" Text="Cancel" CssClass="btn btn-default" />
                                <asp:LinkButton formnovalidate="formnovalidate" ID="cmdSaveForm" OnClick="cmdSaveForm_Click" runat="server" Text="Save this form" CssClass="btn btn-success" />
                                <asp:LinkButton formnovalidate="formnovalidate" ID="cmdSaveFormAndClose" OnClick="cmdSaveFormAndClose_Click" runat="server" Text="Save and close this form" CssClass="btn btn-success" />
                                </div>
                        </div>    
                    </div>
                </div>
        </asp:View>
    </asp:MultiView>
</asp:Content>


