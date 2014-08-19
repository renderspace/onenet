using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Linq;

using System.ComponentModel;

using One.Net.BLL;
using DropDownList = System.Web.UI.WebControls.DropDownList;
using One.Net.BLL.WebControls;


namespace OneMainWeb.AdminControls
{
    public partial class OneSettings : System.Web.UI.UserControl
    {

        public event EventHandler SettingsSaved;

        protected void OnSettingsSaved(EventArgs e)
        {
            if (SettingsSaved != null)
            {
                SettingsSaved(this, e);
            }
        }


        static readonly BWebsite webSiteB = new BWebsite();

        public enum SettingMode { None, Website, Page, Module }
        public enum SettingType { Integer = 1, Boolean, Decimal, String, URL, PhysicalPath, VirtualPath, Color, RegularExpression, Email, DateTime, GraphicalMeasure }

        //public class SettingsEventArgs : EventArgs
        //{
        //    string error;
        //    public SettingsEventArgs() { this.error = ""; }
        //    public SettingsEventArgs(string error) { this.error = error; }
        //    public string Error { get { return error; } }
        //}

        //public delegate void SettingsChangeHandler(object sender, SettingsEventArgs e);
        //public event SettingsChangeHandler SettingsChange;

        [Bindable(true), Category("Data"), DefaultValue("True")]
        public bool DisplayCommands { get; set; }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public Dictionary<string, BOSetting> Settings { get; set; }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public int ItemId { get; set; }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public SettingMode Mode { get; set; }

        [Bindable(false), Category("Data"), DefaultValue("")]
        protected bool ShowSettings { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            PanelCommands.Visible = DisplayCommands;
        }

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object cSBase = base.SaveControlState();
            object[] cSThis = new object[6];

            cSThis[0] = cSBase;
            cSThis[1] = ShowSettings;
            cSThis[2] = Settings;
            cSThis[3] = ItemId;
            cSThis[4] = Mode;
            cSThis[5] = DisplayCommands;

            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];
            ShowSettings = (bool)cSThis[1];
            Settings = (Dictionary<string, BOSetting>)cSThis[2];
            ItemId = (int)cSThis[3];
            Mode = (SettingMode)cSThis[4];
            DisplayCommands = (bool)cSThis[5];
            LoadSettings();

            base.LoadControlState(cSBase);
        }

        protected void cmdShowModuleSettings_Click(object sender, EventArgs e)
        {
            ShowSettings = true;

            switch (this.Mode)
            {
                case SettingMode.Module:
                    this.Settings = webSiteB.GetModuleInstance(ItemId).Settings;
                    break;
                case SettingMode.Page:
                    this.Settings = webSiteB.GetPage(ItemId).Settings;
                    break;
                case SettingMode.Website:
                    this.Settings = webSiteB.Get(ItemId).Settings;
                    break;
            }

            LoadSettings();
        }

        protected void cmdHideModuleSettings_Click(object sender, EventArgs e)
        {
            Settings = null;
            ShowSettings = false;
            LoadSettings();
        }

        public void Save(EventArgs e = null)
        {
            if (ItemId > 0 && Settings != null)
            {
                Dictionary<string, BOSetting> SettingsForSaving = new Dictionary<string, BOSetting>();

                foreach (RepeaterItem item in RepeaterSettings.Items)
                {
                    TextBox TextBox1 = item.FindControl("TextBox1") as TextBox;
                    var LiteralKey = item.FindControl("LiteralKey") as Literal;
                    var CheckBox1 = item.FindControl("CheckBox1") as CheckBox;
                    DropDownList DropDownList1 = item.FindControl("DropDownList1") as DropDownList;

                    BOSetting setting = Settings[LiteralKey.Text];

                    if (setting.UserVisibility != BOSetting.USER_VISIBILITY_SPECIAL && setting.UserVisibility != BOSetting.USER_VISIBILITY_MULTILINE)
                    {
                        switch (setting.Type)
                        {
                            case "Int":
                                {
                                    if (setting.HasOptions)
                                    {
                                        if (setting.Options.ContainsKey(DropDownList1.SelectedValue))
                                            setting.Value = FormatTool.GetInteger(DropDownList1.SelectedValue).ToString();
                                    }
                                    else
                                        setting.Value = FormatTool.GetInteger(TextBox1.Text).ToString();
                                    break;
                                }
                            case "Bool":
                                {
                                    setting.Value = CheckBox1.Checked.ToString();
                                    break;
                                }
                            case "PageId":
                                {
                                    setting.Value = FormatTool.GetInteger(TextBox1.Text).ToString();
                                    break;
                                }
                            default:
                                {
                                    if (setting.HasOptions)
                                        setting.Value = DropDownList1.SelectedValue;
                                    else
                                        setting.Value = TextBox1.Text;
                                    break;
                                }
                        }
                        SettingsForSaving.Add(setting.Name, setting);
                    }
                    else if (setting.UserVisibility == BOSetting.USER_VISIBILITY_MULTILINE)
                    {
                        switch (setting.Type)
                        {
                            case "String":
                                setting.Value = TextBox1.Text;
                                break;
                            default: break;
                        }
                        SettingsForSaving.Add(setting.Name, setting);
                    }
                    else if (setting.UserVisibility == BOSetting.USER_VISIBILITY_SPECIAL)
                    {
                        SettingsForSaving.Add(setting.Name, setting);
                    }
                }

                switch (Mode)
                {
                    case SettingMode.Page:
                        {
                            BOPage page = webSiteB.GetPage(ItemId);
                            page.Settings = SettingsForSaving;
                            webSiteB.ChangePage(page);
                            break;
                        }
                    case SettingMode.Website:
                        {
                            webSiteB.ChangeSettings(SettingsForSaving, ItemId);
                            break;
                        }
                    case SettingMode.Module:
                        {
                            webSiteB.ChangeModuleInstanceSettings(SettingsForSaving, ItemId);
                            break;
                        }
                }

                ShowSettings = false;
                OnSettingsSaved(e);
            }
        }

        protected void cmdSaveChanges_Click(object sender, EventArgs e)
        {
            Save(e);
        }

        public void LoadSettingsControls(Dictionary<string, BOSetting> _settings)
        {
            Settings = _settings;
        }

        public void LoadSettings()
        {
            if (ItemId > 0)
            {
                RepeaterSettings.DataSource = Settings;
                RepeaterSettings.DataBind();
                Visible = Settings.Where(s => s.Value.IsVisible).Count() > 0;
            }
            else
            {
                Visible = false;
            }
        }

        protected void rptSettings_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var CheckBox1 = e.Item.FindControl("CheckBox1") as CheckBox;
                var TextBox1 = e.Item.FindControl("TextBox1") as TextBox;
                var LiteralKey = e.Item.FindControl("LiteralKey") as Literal;

                
                var LabelKey = e.Item.FindControl("LabelKey") as Label;
                var LabelValue = e.Item.FindControl("LabelValue") as Label;

                var DropDownList1 = e.Item.FindControl("DropDownList1") as DropDownList;
                
                var LabelHiddenInfo = e.Item.FindControl("LabelHiddenInfo") as Label;

                var PanelCheckbox = e.Item.FindControl("PanelCheckbox") as Panel;
                var PanelInput = e.Item.FindControl("PanelInput") as Panel;

                if (e.Item.DataItem != null)
                {
                    TextBox1.Visible = false;
                    PanelCheckbox.Visible = false;
                    PanelInput.Visible = false;


                    BOSetting setting = ((KeyValuePair<string, BOSetting>)e.Item.DataItem).Value;
                    TextBox1.Text = setting.Value;
                    LabelValue.Text = setting.Value;

                    if (setting.UserVisibility != BOSetting.USER_VISIBILITY_SPECIAL && setting.UserVisibility != BOSetting.USER_VISIBILITY_MULTILINE)
                    {
                        if (setting.HasOptions)
                        {
                            DropDownList1.Visible = true;
                            DropDownList1.DataTextField = "Value";
                            DropDownList1.DataValueField = "Key";
                            DropDownList1.DataSource = setting.Options;
                            if (setting.Options.ContainsKey(setting.Value))
                                DropDownList1.SelectedValue = setting.Value;
                            else
                                DropDownList1.ForeColor = Color.Red;

                            LabelHiddenInfo.Text = setting.Value;
                        }
                        else
                        {
                            switch (setting.Type)
                            {
                                case "Int":
                                    {
                                        TextBox1.CssClass = "form-control digits";
                                        PanelInput.Visible = true;
                                        TextBox1.Visible = true;
                                        break;
                                    }
                                case "String":
                                    {
                                        PanelInput.Visible = true;
                                        TextBox1.Visible = true;
                                        break;
                                    }
                                case "Bool":
                                    {
                                        PanelCheckbox.Visible = true;
                                        CheckBox1.Checked = FormatTool.GetBoolean(setting.Value);
                                        break;
                                    }
                                case "Url":
                                    {
                                        TextBox1.CssClass = "form-control url";
                                        PanelInput.Visible = true;
                                        TextBox1.Visible = true;
                                        break;
                                    }
                                default:
                                    {
                                        PanelInput.Visible = true;
                                        TextBox1.Visible = true;
                                        break;
                                    }
                            }
                        }
                    }
                    else if (setting.UserVisibility == BOSetting.USER_VISIBILITY_MULTILINE)
                    {
                        switch (setting.Type)
                        {
                            case "String":
                                {
                                    TextBox1.Visible = true;
                                    TextBox1.Rows = 5;
                                    TextBox1.TextMode = TextBoxMode.MultiLine;
                                    break;
                                }
                            default: break;
                        }
                    }
                    else
                    {
                        LabelValue.Visible = true;
                    }
                }
            }
        }
    }
}