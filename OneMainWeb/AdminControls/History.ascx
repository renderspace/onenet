<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="History.ascx.cs" Inherits="OneMainWeb.AdminControls.History" %>


<asp:PlaceHolder id="PlaceHolderHistory" runat="server">
    <div class="instanceAllSettings">
        <asp:PlaceHolder ID="PlaceHolderCollapsed" Visible="true" runat="server">
             <div class="extline">
                <span>
                    <asp:LinkButton ID="CmdShowHistory" OnClick="CmdShowHistory_Click" runat="server"  Text="$show_history" ValidationGroup="none"  />&nbsp;&nbsp;
                    <img id="Img1" alt="" runat="server" />
                </span>
            </div>
        </asp:PlaceHolder>	
        <asp:PlaceHolder ID="PlaceHolderExpanded" Visible="false" runat="server">
             <div class="extline">
                <span>
                    <asp:LinkButton ID="CmdHideHistory" OnClick="CmdHideHistory_Click" runat="server" Text="$hide_history" ValidationGroup="none" />&nbsp;&nbsp;
                    <img id="Img2" alt="" runat="server" />
                </span>
            </div>
            <div class="smallestgv">  
	            <asp:GridView   ID="GridViewAudit" 
	                            AutoGenerateColumns="False" 
	                            runat="server"
	                            CssClass="smallestgv"
	                            DataKeyNames="AuditGuid"
	                            OnRowCommand="GridViewAudit_RowCommand"
	                            OnRowDataBound="GridViewAudit_RowDataBound">
	                <Columns>
			            <asp:TemplateField HeaderText="$date_saved" SortExpression="date_saved">
				            <ItemTemplate><%# Eval("DateModified")%><asp:Literal ID="LiteralHiddenGuid" runat="server" Text='<%# Eval("AuditGuid") %>' Visible="false" ></asp:Literal></ItemTemplate>
			            </asp:TemplateField>
			            <asp:TemplateField HeaderText="$principal_saved_by" SortExpression="principal_saved_by">
				            <ItemTemplate><%# Eval("PrincipalModified")%></ItemTemplate>
			            </asp:TemplateField>							        					            
	                    <asp:TemplateField>
	                        <ItemTemplate>
	                            <asp:LinkButton ValidationGroup="no-validation-group" ID="CmdViewHistoryItem" CommandName="ShowItem" CommandArgument='<%# Container.DataItemIndex %>' runat="server" Text="$view" />
	                            
                                <two:modalpanel ShowCloseButton="true" CloseButtonImageUrl="/AdminControls/toolbar_close.gif" OnWindowClosed="PanelAuditInfo_WindowClosed" Visible="false" id="PanelAuditInfo" runat="server" >
                                    <div class="outerBorder">
                                        <div class="innerBorder">
                                            <two:Input ReadOnly="true" ID="InputTitle" Text="$title" runat="server" Value='<%# Eval("Title") %>' />	    
                                            <two:Input ReadOnly="true" ID="InputSubTitle" Text="$subtitle" runat="server" Value='<%# Eval("SubTitle") %>' />	    
                                            <two:Input TextMode="MultiLine" Rows="3" ReadOnly="true" ID="InputTeaser" Text="$teaser" runat="server" Value='<%# Eval("Teaser") %>' />	    
                                            <two:Input TextMode="MultiLine" Rows="3" ReadOnly="true" ID="InputHtml" Text="$html" runat="server" Value='<%# Eval("Html") %>' />	    
                                            <two:InfoLabel ID="LabelDateModified" Text="$date_saved" runat="server" Value='<%# Eval("DateModified") %>' />	    
                                            <two:InfoLabel ID="LabelPrincipalModified" Text="$principal_saved_by" runat="server" Value='<%# Eval("PrincipalModified") %>' />	    
                                            <asp:LinkButton ValidationGroup="no-validation-group" ID="CmdRevertTo" runat="server" Text="$revert_to" CommandName="RevertTo" CommandArgument='<%# Container.DataItemIndex %>' />
                                        </div>
                                    </div>
                                </two:modalpanel> 
	                            
	                        </ItemTemplate>
	                    </asp:TemplateField>
	                </Columns>
	            </asp:GridView>
            </div>
        </asp:PlaceHolder>
    </div>
</asp:PlaceHolder>