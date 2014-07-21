<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OneSettings.ascx.cs" Inherits="OneMainWeb.AdminControls.OneSettings" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="One.Net.BLL" %>
<div class="oneSettings">
    <details>
        <summary>Advanced settings</summary>

        <asp:ValidationSummary ID="ValidationSummary1" runat="server" ValidationGroup="ModuleInstanceSettings" />
        <asp:Repeater ID="RepeaterSettings" EnableViewState="true" runat="server" OnItemCreated="rptSettings_ItemCreated" OnItemDataBound="RepeaterSettings_ItemDataBound">
            <ItemTemplate>
                <asp:Label ID="KeyLabel1" runat="server" Visible="false" Text='<%# ((KeyValuePair<string, BOSetting>)Container.DataItem).Key %>'></asp:Label>
                <two:ValidInput ID="ValidInput1" runat="server" Visible="false"  Text='<%# ResourceManager.GetString("$" + ((KeyValuePair<string, BOSetting>)Container.DataItem).Value.Name) %>' Value='<%# ((KeyValuePair<string, BOSetting>)Container.DataItem).Value.Value %>' ValidationGroup="ModuleInstanceSettings" Required="false" />
                <two:LabeledCheckBox ID="CheckBox1" runat="server" Visible="false" Text='<%# ResourceManager.GetString("$" + ((KeyValuePair<string, BOSetting>)Container.DataItem).Value.Name) %>' />

                <asp:Panel runat="server" CssClass="infoLabel" ID="PanelInfo">
                    <asp:Label runat="server" ID="LabelKey" AssociatedControlID="LabelValue"></asp:Label>
                    <asp:Label  runat="server" ID="LabelValue"></asp:Label>
                </asp:Panel>
                
                <asp:Panel ID="PanelSelect1" runat="server" Visible="false" CssClass="select">
                    <asp:Label ID="Label1" runat="server" Text='<%# ((KeyValuePair<string, BOSetting>)Container.DataItem).Key %>'></asp:Label>
                    <asp:DropDownList ID="DropDownList1" runat="server" ></asp:DropDownList>
                    <!-- <asp:Label ID="LabelHiddenInfo" runat="server"></asp:Label> -->
                </asp:Panel>
                
            </ItemTemplate>
        </asp:Repeater>
        <asp:Panel runat="server" ID="PanelCommands" CssClass="commands">
            <asp:Button ID="cmdSaveChanges" runat="server" OnClick="cmdSaveChanges_Click" Text="Save" ValidationGroup="ModuleInstanceSettings"  CssClass="save-btn" />
        </asp:Panel>
    </details>
</div>
