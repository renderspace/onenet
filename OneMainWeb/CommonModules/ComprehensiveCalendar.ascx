<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ComprehensiveCalendar.ascx.cs" Inherits="OneMainWeb.CommonModules.ComprehensiveCalendar" %>
<%@ Register TagPrefix="two" Namespace="TwoControlsLibrary" Assembly="TwoControlsLibrary" %>
<%@ Import Namespace="One.Net.BLL"  %>
<script language="javascript" type="text/javascript">
function CheckUnCheck(Select, categoryDivId){    

  var categoryDiv = document.getElementById(categoryDivId);
  var checkBoxList;
  for(var i = 0; i < categoryDiv.childNodes.length; i++) {
    if ( categoryDiv.childNodes[i].nodeName == 'DIV')
       checkBoxList = categoryDiv.childNodes[i]
  }
  
  var checkBoxes = checkBoxList.getElementsByTagName("input");

  for(var i = 0; i < checkBoxes.length; i++) {
    checkBoxes[i].checked = Select;
  }
}
</script>

<h1><%=Translate("comprehansive_calendar_title") %></h1>
<asp:PlaceHolder ID="PlaceHolderSearch" runat="server">

        <asp:RadioButtonList id="RadioButtonListTimeFrame" RepeatDirection="Vertical" RepeatLayout="Flow" CssClass="timeframe" TextAlign="Right" runat="server" />
         <div id="categoryDiv">
			<asp:CheckBoxList id="CheckBoxListCategories" RepeatDirection="Vertical" RepeatLayout="Flow" TextAlign="Right" CssClass="categories" runat="server" />
	        <span class="mark">
	            <a onclick="CheckUnCheck(true, 'categoryDiv');return false" href="#"><%=Translate("cc_mark_all") %></a>
	            <a onclick="CheckUnCheck(false, 'categoryDiv');return false" href="#"><%=Translate("cc_unmark_all") %></a>
	        </span>
		</div>
    <div class="filter">
        <two:Input ValidationGroup="CC1" required="false" id="InputFilter" runat="server" Text="cc_filter_place" />
        <span class="button"><asp:LinkButton ValidationGroup="CC1" ID="CmdFilter" OnClick="CmdFilter_Click" runat="server" Text="cc_filter" CssClass="filterButton" /></span>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder ID="PlaceHolderList" runat="server">

        <asp:Repeater ID="RepeaterEvents" OnItemDataBound="RepeaterEvents_ItemDataBound" runat="server">
            <ItemTemplate>
                <div class="eventItem">
                    <h1><asp:LinkButton ID="CmdShowEvent" runat="server" OnClick="CmdShowEvent_Click" CommandArgument='<%# Eval("Id") %>' Text='<%# Eval("Title")%>' /></h1>
                    <p class="date"><%# ((DateTime)Eval("BeginDate")).ToShortDateString() %></p>
                    <h2><%# Eval("SubTitle") %></h2>
                    <p class="teaser"><%# Eval("Teaser") %></p>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <two:PostbackPager id="EventPager" OnCommand="EventPager_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	    

</asp:PlaceHolder>

<asp:PlaceHolder ID="PlaceHolderNoItemsInList" runat="server">
    <div>
        <%= Translate("no_results_label") %>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder id="PlaceHolderSingle" runat="server">
    <div class="singleEvent">
        <h1><%=SelectedEvent.Title %></h1>
        <h2><%=SelectedEvent.SubTitle%></h2>
        <%=SelectedEvent.Html%>
    </div>
</asp:PlaceHolder>

