<%@ Control Language="C#" ViewStateMode="Disabled" AutoEventWireup="true" CodeBehind="VirtualTable.ascx.cs" Inherits="OneMainWeb.CommonModules.VirtualTable" %>
<%@ Register TagPrefix="bll" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="System.Data" %>

<asp:Repeater runat="server" ID="RepeaterData">
    <HeaderTemplate>
        <table class="table">
    </HeaderTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
    <ItemTemplate>
        <tr>
            <td><%# ((DataRow)Container.DataItem)[0] %></td>
        </tr>
    </ItemTemplate>
</asp:Repeater>
<bll:PostbackPager id="PostbackPager1" OnCommand="PostbackPager1_Command" runat="server" MaxColsPerRow="11" NumPagesShown="10" />	
