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
                    TextBox ValidInput1 = item.FindControl("ValidInput1") as TextBox;
                    Label KeyLabel1 = item.FindControl("KeyLabel1") as Label;
                    LabeledCheckBox CheckBox1 = item.FindControl("CheckBox1") as LabeledCheckBox;
                    DropDownList DropDownList1 = item.FindControl("DropDownList1") as DropDownList;

                    BOSetting setting = Settings[KeyLabel1.Text];

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
                                        setting.Value = FormatTool.GetInteger(ValidInput1.Text).ToString();
                                    break;
                                }
                            case "Bool":
                                {
                                    setting.Value = CheckBox1.Checked.ToString();
                                    break;
                                }
                            case "PageId":
                                {
                                    setting.Value = FormatTool.GetInteger(ValidInput1.Text).ToString();
                                    break;
                                }
                            default:
                                {
                                    if (setting.HasOptions)
                                        setting.Value = DropDownList1.SelectedValue;
                                    else
                                        setting.Value = ValidInput1.Text;
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
                                setting.Value = ValidInput1.Text;
                                break;
                            default: break;
                        }
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
                LabeledCheckBox CheckBox1 = e.Item.FindControl("CheckBox1") as LabeledCheckBox;
                TextBox ValidInput1 = e.Item.FindControl("ValidInput1") as TextBox;
                Label KeyLabel1 = e.Item.FindControl("KeyLabel1") as Label;

                var PanelInfo = e.Item.FindControl("PanelInfo") as Panel;
                var LabelKey = e.Item.FindControl("LabelKey") as Label;
                var LabelValue = e.Item.FindControl("LabelValue") as Label;

                DropDownList DropDownList1 = e.Item.FindControl("DropDownList1") as DropDownList;
                Panel PanelSelect1 = e.Item.FindControl("PanelSelect1") as Panel;
                Label LabelHiddenInfo = e.Item.FindControl("LabelHiddenInfo") as Label;

                if (e.Item.DataItem != null)
                {
                    CheckBox1.Visible = false;
                    ValidInput1.Visible = false;
                    PanelInfo.Visible = false;


                    BOSetting setting = ((KeyValuePair<string, BOSetting>)e.Item.DataItem).Value;

                    if (setting.UserVisibility != BOSetting.USER_VISIBILITY_SPECIAL && setting.UserVisibility != BOSetting.USER_VISIBILITY_MULTILINE)
                    {
                        if (setting.HasOptions)
                        {
                            PanelSelect1.Visible = true;
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
                                        if (setting.UserVisibility == BOSetting.USER_VISIBILITY_COMMON)
                                        {
                                        }
                                        ValidInput1.Visible = true;
                                        break;
                                    }
                                case "String":
                                    {
                                        if (setting.UserVisibility == BOSetting.USER_VISIBILITY_COMMON)
                                        {
                                        }
                                        ValidInput1.Visible = true;
                                        break;
                                    }
                                case "Bool":
                                    {
                                        if (setting.UserVisibility == BOSetting.USER_VISIBILITY_COMMON)
                                        {
                                            CheckBox1.ContainerCssClass = "checkbox checkbox_common";
                                        }
                                        CheckBox1.Visible = true;
                                        CheckBox1.Checked = FormatTool.GetBoolean(setting.Value);
                                        break;
                                    }
                                case "PageId":
                                    {
                                        ValidInput1.Visible = true;
                                        break;
                                    }
                                default:
                                    {
                                        PanelInfo.Visible = true;
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
                                    ValidInput1.Visible = true;
                                    ValidInput1.Rows = 5;
                                    ValidInput1.TextMode = TextBoxMode.MultiLine;
                                    break;
                                }
                            default: break;
                        }
                    }
                    else
                    {
                        PanelInfo.Visible = true;
                    }
                }
            }
        }

        protected void RepeaterSettings_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var setting = (KeyValuePair<string, BOSetting>) e.Item.DataItem;

                    var LabelKey = e.Item.FindControl("LabelKey") as Label;
                    var LabelValue = e.Item.FindControl("LabelValue") as Label;
                    var KeyLabel1 = e.Item.FindControl("KeyLabel1") as Label;

                    LabelKey.Text = KeyLabel1.Text = setting.Key;
            }
        }
    }
}