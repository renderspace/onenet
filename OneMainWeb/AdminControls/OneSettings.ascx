<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OneSettings.ascx.cs" Inherits="OneMainWeb.AdminControls.OneSettings" %>
<%@ Register TagPrefix="two" Namespace="One.Net.BLL.WebControls" Assembly="One.Net.BLL" %>
<%@ Import Namespace="One.Net.BLL" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="One.Net.BLL" %>
<details class="form-horizontal validationGroup">
    <summary>Advanced settings</summary>
    <asp:Repeater ID="RepeaterSettings" EnableViewState="true" runat="server" OnItemDataBound="RepeaterSettings_ItemDataBound">
        <ItemTemplate>
            <asp:Literal ID="LiteralKey" runat="server" Visible="false" Text='<%# ((KeyValuePair<string, BOSetting>)Container.DataItem).Key %>'></asp:Literal>
            <asp:Panel runat="server" CssClass="checkbox" ID="PanelCheckbox">
                <label class="col-sm-offset-3 col-sm-9">
                    <asp:CheckBox runat="server" ID="CheckBox1" />
                    <%# ((KeyValuePair<string, BOSetting>)Container.DataItem).Value.Name %>
                </label>
            </asp:Panel> 
            <asp:Panel runat="server" CssClass="form-group" ID="PanelInput">
                <label class="col-sm-3 control-label"><%# ((KeyValuePair<string, BOSetting>)Container.DataItem).Value.Name %> </label>
                    
                <div class="col-sm-9">
                    <asp:TextBox ID="TextBox1" runat="server"   CssClass="form-control" ValidationGroup="ModuleInstanceSettings" Visible="false" />
                    <asp:Label  runat="server" ID="LabelValue" Visible="false"></asp:Label>
                    <asp:Label  runat="server" ID="LabelHiddenInfo" Visible="false"></asp:Label>
                    <asp:DropDownList ID="DropDownList1" runat="server"  Visible="false"></asp:DropDownList>
                    <asp:Panel runat="server" Visible="false" ID="PanelFile">
                        <p class="imageFileUploadStatus"><asp:FileUpload runat="server"  CssClass="imageFileUploadWithPreview" ID="FileUploadFromSettings" />
                            <asp:Literal runat="server" ID="LiteralFileDisplay"></asp:Literal>
                        </p>
                        
                        <script>
                            
                        </script>

                    </asp:Panel>

                </div>
                </asp:Panel>       
        </ItemTemplate>
    </asp:Repeater>
    <asp:Panel runat="server" ID="PanelCommands"  CssClass="form-group">
        <div class="col-sm-offset-3 col-sm-9">
            <asp:LinkButton ID="cmdSaveChanges" runat="server" OnClick="cmdSaveChanges_Click" Text="Save" ValidationGroup="ModuleInstanceSettings"  CssClass="btn btn-success causesValidation" />
        </div>
    </asp:Panel>
</details>