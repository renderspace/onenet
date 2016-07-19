using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using One.Net.BLL;

using One.Net.BLL.Web;
using One.Net.Forms;
using One.Net.BLL.Model.Attributes;


namespace OneMainWeb.CommonModules
{
    public partial class FormSelector : MModule
    {
        private static readonly BForm formsB = new BForm();

        [Setting(SettingType.CSInteger)]
        public List<int> FormIds { get { return GetIntegerListSetting("FormIds"); } }

        protected int SelectedFormId
        {
            get { return FormatTool.GetInteger(Session["SelectedFormId" + this.InstanceId]); }
            set { Session["SelectedFormId" + this.InstanceId] = value; }
        }

        protected List<BOForm> FormList
        {
            get { return ViewState["FormList"] as List<BOForm>; }
            set { ViewState["FormList"] = value; }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadControls();
            }
        }

        private void LoadControls()
        {
            lblForms.Text = Translate("select_form_for_contact");

            if (FormList == null)
            {
                FormList = new List<BOForm>();

                foreach (int id in FormIds)
                {
                    BOForm form = formsB.Get(id);
                    if (form != null)
                    {
                        FormList.Add(form);
                    }
                }
            }

            ddlForms.DataSource = FormList;
            ddlForms.DataTextField = "Title";
            ddlForms.DataValueField = "Id";
            ddlForms.DataBind();

            ddlForms.Items.Insert(0, new ListItem(Translate("select_form_for_sending"), "-1"));

            if (ddlForms.Items.FindByValue(SelectedFormId.ToString()) != null)
            {
                ddlForms.SelectedValue = SelectedFormId.ToString();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (SelectedFormId > -1)
            {
                Control control = LoadControl("~/CommonModules/Form.ascx");
                MModule m = (MModule)control;
                m.InstanceId = this.InstanceId;
                m.RelativePageUri = this.RelativePageUri;
                m.Settings = new Dictionary<string, BOSetting>();
                m.Settings.Add("FormId", new BOSetting("FormId", SettingTypeEnum.Int, SelectedFormId.ToString(), VisibilityEnum.NORMAL));
                m.Settings.Add("UploadFolderId", new BOSetting("UploadFolderId", SettingTypeEnum.Int, "0", VisibilityEnum.NORMAL));
                PlaceHolderAcutalForm.Controls.Add(control);
            }
            else
            {
                PlaceHolderAcutalForm.Controls.Clear();
            }

            base.OnInit(e);
        }

        protected void ddlForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedFormId = FormatTool.GetInteger(ddlForms.SelectedValue);
            Response.Redirect(Request.Url.AbsoluteUri);
        }
    }
}