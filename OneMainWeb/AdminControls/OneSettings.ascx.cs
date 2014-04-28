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

using System.ComponentModel;


using One.Net.BLL;
using DropDownList=System.Web.UI.WebControls.DropDownList;
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

        private Dictionary<string, BOSetting> settings;
        private int itemId;
        private SettingMode settingMode;
        private bool showSettings;

        static readonly BWebsite webSiteB = new BWebsite();

        public enum SettingMode { None, Website, Page, Module }
        public enum SettingType { Integer = 1, Boolean, Decimal, String, URL, PhysicalPath, VirtualPath, Color, RegularExpression, Email, DateTime, GraphicalMeasure }

        //public class SettingsEventArgs : EventArgs
        //{
        //    string error;
        //    public SettingsEventArgs() { this.error = string.Empty; }
        //    public SettingsEventArgs(string error) { this.error = error; }
        //    public string Error { get { return error; } }
        //}

        //public delegate void SettingsChangeHandler(object sender, SettingsEventArgs e);
        //public event SettingsChangeHandler SettingsChange;

        [Bindable(true), Category("Data"), DefaultValue("")]
        public Dictionary<string, BOSetting> Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public int ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public SettingMode Mode
        {
            get { return settingMode; }
            set { settingMode = value; }
        }

        [Bindable(true), Category("Data"), DefaultValue("")]
        public string Text
        {
            set { cmdShowModuleSettings.Text = value; }
        }

        [Bindable(false), Category("Data"), DefaultValue("")]
        protected bool ShowSettings
        {
            get { return showSettings; }
            set { showSettings = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Img1.Src = Page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), "OneMainWeb.Res.extend-down.gif");
            Img2.Src = Page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), "OneMainWeb.Res.extend-up.gif");
        }

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object cSBase = base.SaveControlState();
            object[] cSThis = new object[5];

            cSThis[0] = cSBase;
            cSThis[1] = showSettings;
            cSThis[2] = settings;
            cSThis[3] = itemId;
            cSThis[4] = settingMode;

            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];
            showSettings = (bool)cSThis[1];
            settings = (Dictionary<string, BOSetting>) cSThis[2];
            itemId = (int) cSThis[3];
            settingMode = (SettingMode) cSThis[4];

            LoadSettings();

            base.LoadControlState(cSBase);
        }

        protected void cmdShowModuleSettings_Click(object sender, EventArgs e)
        {
            ShowSettings = true;

            switch (this.Mode)
            {
                case  SettingMode.Module:
                    this.Settings = webSiteB.GetModuleInstance(itemId).Settings;
                    break;
                case  SettingMode.Page:
                    this.Settings = webSiteB.GetPage(itemId).Settings;
                    break;
                case  SettingMode.Website:
                    this.Settings = webSiteB.Get(itemId).Settings;
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

        public void Collapse()
        {
            if (showSettings)
            {
                plhCollapsed.Visible = true;
                plhExpanded.Visible = false;
            }
            else
            {
                plhCollapsed.Visible = false;
                plhExpanded.Visible = false;
            }
        }

        protected void cmdSaveChanges_Click(object sender, EventArgs e)
        {
            if (ItemId > 0 && Settings != null)
            {
                Dictionary<string, BOSetting> SettingsForSaving = new Dictionary<string, BOSetting>();

                foreach (RepeaterItem item in rptSettings.Items)
                {
                    ValidInput ValidInput1 = item.FindControl("ValidInput1") as ValidInput;
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
                                        setting.Value = FormatTool.GetInteger(ValidInput1.Value).ToString();
                                    break;
                                }
                            case "Bool":
                                {
                                    setting.Value = CheckBox1.Checked.ToString();
                                    break;
                                }
                            case "PageId":
                                {
                                    setting.Value = FormatTool.GetInteger(ValidInput1.Value).ToString();
                                    break;
                                }
                            default:
                                {
                                    if (setting.HasOptions)
                                        setting.Value = DropDownList1.SelectedValue;
                                    else
                                        setting.Value = ValidInput1.Value;
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
                                setting.Value = ValidInput1.Value;
                                break;
                            default:break;
                        }
                        SettingsForSaving.Add(setting.Name, setting);
                    }
                }

//                ShowSettings = false;
//                LoadSettings();

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

        public void LoadSettingsControls(Dictionary<string, BOSetting> _settings)
        {
            if (itemId > 0)
            {
                plhCollapsed.Visible = !ShowSettings;
                plhExpanded.Visible = ShowSettings;
            }
            else
            {
                plhCollapsed.Visible = plhExpanded.Visible = false;
            }
        	Settings = _settings;
        }

        public void LoadSettings()
        {
            if (itemId > 0)
            {
                plhCollapsed.Visible = !ShowSettings;
                plhExpanded.Visible = ShowSettings;
                rptSettings.DataSource = Settings;
                rptSettings.DataBind();
            }
            else
            {
                plhCollapsed.Visible = plhExpanded.Visible = false;
            }
        }

        protected void rptSettings_ItemCreated(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                LabeledCheckBox CheckBox1 = e.Item.FindControl("CheckBox1") as LabeledCheckBox;
                ValidInput ValidInput1 = e.Item.FindControl("ValidInput1") as ValidInput;
                Label KeyLabel1 = e.Item.FindControl("KeyLabel1") as Label;
                InfoLabel InfoLabel1 = e.Item.FindControl("InfoLabel1") as InfoLabel;
                DropDownList DropDownList1 = e.Item.FindControl("DropDownList1") as DropDownList;
                Panel PanelSelect1 = e.Item.FindControl("PanelSelect1") as Panel;
                Label LabelHiddenInfo = e.Item.FindControl("LabelHiddenInfo") as Label;

                if (e.Item.DataItem != null)
                {
                    CheckBox1.Visible = false;
                    ValidInput1.Visible = false;
                    InfoLabel1.Visible = false;
                    

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
                                            ValidInput1.ContainerCssClass = "input input_common";
                                        }
                                        ValidInput1.ValidationType = "integer";
                                        ValidInput1.Visible = true;
                                        break;
                                    }
                                case "String":
                                    {
                                        if (setting.UserVisibility == BOSetting.USER_VISIBILITY_COMMON)
                                        {
                                            ValidInput1.ContainerCssClass = "input input_common";
                                        }
                                        ValidInput1.ValidationType = "";
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
                                        ValidInput1.ValidationType = "integer";
                                        break;
                                    }
                                default:
                                    {
                                        InfoLabel1.Visible = true;
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
                                    ValidInput1.ContainerCssClass = "input input_multiline";
                                    ValidInput1.ValidationType = "";
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
                        InfoLabel1.Visible = true;
                    }
                }
            }
        }
    }
}