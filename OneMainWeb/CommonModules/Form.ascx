<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Form.ascx.cs" Inherits="OneMainWeb.CommonModules.Form" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ Import Namespace="One.Net.BLL.Forms" %>

<asp:PlaceHolder ID="plhForm" runat="server" />
<asp:PlaceHolder ID="plhResults" runat="server">
    <div runat="server" id="DivFormTitle" class="formTitle"></div>
   
    <asp:Panel ID="divQuestionaireThankYouNote" runat="server" Visible="false" CssClass="ThankYouNote">
        <asp:Literal ID="lblThankYouNote" runat="server"></asp:Literal>
    </asp:Panel>
    
    <asp:Repeater ID="rptPollResults" runat="server" OnItemDataBound="rptPollResults_ItemDataBound">
        <ItemTemplate>
            <div class="question"><span><%# Eval("Question.Title") %></span></div>
            <asp:Repeater ID="rptPollSubmittedAnswers" runat="server" OnItemDataBound="rptPollSubmittedAnswers_ItemDataBound">
                <ItemTemplate>
                    <div class="singlePollAnswer">
				        <div class="answer"><%# DataBinder.Eval(Container.DataItem, "Answer.Title") %></div>	
				        <div class="percentage">									
					        <two:ProgressBar id="progressBar" runat="server" />	
					        <asp:Label runat="server" ID="LabelPercentage"></asp:Label>
				        </div>
				    </div>
                </ItemTemplate>
            </asp:Repeater>
        </ItemTemplate>
    </asp:Repeater>
        
</asp:PlaceHolder>

<asp:Panel ID="PanelCommands" runat="server" CssClass="Commands">
    <asp:LinkButton ID="cmdPrev" runat="server" Text="prev" OnClick="cmdPrev_Click" CssClass="FormsCommandPrev" />    
    <asp:LinkButton ID="cmdNext" runat="server" Text="next" OnClick="cmdNext_Click" CssClass="FormsCommandNext" />    
    <asp:LinkButton ID="cmdSubmit" runat="server" Text="submit" OnClick="cmdSubmit_Click" CssClass="FormsCommandSubmit" />    
</asp:Panel>
