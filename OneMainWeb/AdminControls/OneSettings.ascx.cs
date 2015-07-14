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
            Databind();

            base.LoadControlState(cSBase);
        }

        protected void cmdSaveChanges_Click(object sender, EventArgs e)
        {
            Save(e);
        }

        public void LoadSettingsControls(Dictionary<string, BOSetting> _settings)
        {
            Settings = _settings;
        }

        public void Databind()
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

        protected void RepeaterSettings_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var CheckBox1 = e.Item.FindControl("CheckBox1") as CheckBox;
                var TextBox1 = e.Item.FindControl("TextBox1") as TextBox;
                var LiteralKey = e.Item.FindControl("LiteralKey") as Literal;

                
                var LabelKey = e.Item.FindControl("LabelKey") as Label;
                var LabelValue = e.Item.FindControl("LabelValue") as Label;

                var DropDownList1 = e.Item.FindControl("DropDownList1") as DropDownList;
                var LiteralFileDisplay = e.Item.FindControl("LiteralFileDisplay") as Literal;
                var LabelHiddenInfo = e.Item.FindControl("LabelHiddenInfo") as Label;
                var PanelFile = e.Item.FindControl("PanelFile") as Panel;
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
                        if (setting.HasOptions || setting.Type == "ImageTemplate")
                        {
                            PanelInput.Visible = true;
                            DropDownList1.Visible = true;
                            if (setting.Type == "ImageTemplate")
                            {
                                var templates = BWebsite.ListTemplates("ImageTemplate");
                                templates.Add(new BOTemplate { Name = "No image", Id = -1 });
                                DropDownList1.DataSource = templates;
                                DropDownList1.DataTextField = "Name";
                                DropDownList1.DataValueField = "Id";
                                if (templates.Where(t => t.Id.ToString() == setting.Value).FirstOrDefault() != null )
                                    DropDownList1.SelectedValue = setting.Value;
                            }
                            else
                            {
                                DropDownList1.DataSource = setting.Options;
                                DropDownList1.DataTextField = "Value";
                                DropDownList1.DataValueField = "Key";
                                if (setting.Options.ContainsKey(setting.Value))
                                    DropDownList1.SelectedValue = setting.Value;
                            }
                            if (string.IsNullOrWhiteSpace(DropDownList1.SelectedValue))
                                DropDownList1.ForeColor = Color.Red;

                            LabelHiddenInfo.Text = setting.Value;
                            DropDownList1.DataBind();
                        } 
                        else
                        {
                            switch (setting.Type)
                            {
                                case "Int":
                                    {
                                        TextBox1.CssClass = "form-control number";
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
                                        TextBox1.CssClass = "form-control absrelurl";
                                        PanelInput.Visible = true;
                                        TextBox1.Visible = true;
                                        break;
                                    }
                                case "Image":
                                    {
                                        PanelInput.Visible = true;
                                        PanelFile.Visible = true;
                                        if (!string.IsNullOrWhiteSpace(setting.Value))
                                        {
                                            LiteralFileDisplay.Text = "<img src=\"data:image/x-icon;base64," + setting.Value + " \">";
                                        } 
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
                                    PanelInput.Visible = true;
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
                                    { 
                                        var val = FormatTool.GetLong(TextBox1.Text);
                                        setting.Value = val.ToString();
                                    }
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
                            case "Image":
                                {
#warning this will not work for multiple images on same page.
                                    if (Request.Files.Count > 0 && Request.Files[0] != null)
                                    {
                                        var postedFile = Request.Files[0];
                                        if (postedFile.InputStream.Length > 0)
                                        { 
                                            byte[] fileData = new Byte[postedFile.InputStream.Length];
                                            postedFile.InputStream.Read(fileData, 0, (int)postedFile.InputStream.Length);
                                            postedFile.InputStream.Close();
                                            setting.Value = Convert.ToBase64String(fileData);
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    if (setting.HasOptions || setting.Type == "ImageTemplate")
                                        setting.Value = DropDownList1.SelectedValue;
                                    else
                                        setting.Value = TextBox1.Text;
                                    break;
                                }
                        }
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
                    }
                    SettingsForSaving.Add(setting.Name, setting);
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
    }
}