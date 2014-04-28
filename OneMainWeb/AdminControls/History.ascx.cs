using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

using One.Net.BLL;
using TwoControlsLibrary;

namespace OneMainWeb.AdminControls
{
    public partial class History : System.Web.UI.UserControl
    {
        private static readonly BAudit auditB = new BAudit();

        private bool showHistory;
        private int selectedItemId;
        private ContentGetter getContent;
        private string selectedGuid = string.Empty;

        public delegate BOInternalContent ContentGetter(int id);
        public delegate List<BOInternalContentAudit> AuditsLister(int id);
        public delegate BOInternalContentAudit AuditGetter(string guid);

        public event EventHandler<TypedEventArg<BOInternalContent>> RevertToAudit;

        protected void OnRevertToAudit(TypedEventArg<BOInternalContent> e)
        {
            if (RevertToAudit != null)
            {
                RevertToAudit(this, e);
            }
        }

        protected string SelectedGuid { get { return selectedGuid; } set { selectedGuid = value; } }

        [Bindable(false), Category("Behaviour"), DefaultValue("")]
        public ContentGetter GetContent { get { return getContent; } set { getContent = value; } }

        public string Img1Src
        {
            get { return Img1.Src; }
            set { Img1.Src = value; }
        }

        public string Img2Src
        {
            get { return Img2.Src; }
            set { Img2.Src = value; }
        }

        public int SelectedItemId { get { return selectedItemId; } set { selectedItemId = value; } }

        public bool ShowHistory { get { return showHistory; } 
            set 
            { 
                showHistory = value;
            } 
        }

        protected void CmdShowHistory_Click(object sender, EventArgs e)
        {
            ShowHistory = true;
            LoadHistory();
        }

        protected void CmdHideHistory_Click(object sender, EventArgs e)
        {
            ShowHistory = false;
            LoadHistory();
        }

        public void LoadHistory()
        {
            BOInternalContent textContentModel = GetContent(SelectedItemId);
            if (textContentModel != null)
            {
                PlaceHolderCollapsed.Visible = !ShowHistory;
                PlaceHolderExpanded.Visible = ShowHistory;

                if (ShowHistory)
                {
                    GridViewAudit.DataSource = auditB.ListAudits(textContentModel.ContentId.Value);
                    GridViewAudit.DataBind();
                }
            }
        }

        protected void PanelAuditInfo_WindowClosed(object sender, EventArgs e)
        {
            ModalPanel panel = (ModalPanel)sender;
            if (panel != null)
            {
                panel.Visible = false;
            }
        }

        private void ClosePreviousPanels()
        {
            if (!string.IsNullOrEmpty(SelectedGuid))
            {
                foreach (GridViewRow row in GridViewAudit.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        ModalPanel PanelAuditInfo = row.FindControl("PanelAuditInfo") as ModalPanel;
                        if (PanelAuditInfo != null)
                            PanelAuditInfo.Visible = false;
                    }
                }
            }
        }

        protected void GridViewAudit_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string command = e.CommandName;
            int rowIndex = FormatTool.GetInteger(e.CommandArgument);

            if (rowIndex < 0)
                return;

            Literal LiteralHiddenGuid = GridViewAudit.Rows[rowIndex].FindControl("LiteralHiddenGuid") as Literal;
            Input InputTitle = GridViewAudit.Rows[rowIndex].FindControl("InputTitle") as Input;
            Input InputSubTitle = GridViewAudit.Rows[rowIndex].FindControl("InputSubTitle") as Input;
            Input InputTeaser = GridViewAudit.Rows[rowIndex].FindControl("InputTeaser") as Input;
            Input InputHtml = GridViewAudit.Rows[rowIndex].FindControl("InputHtml") as Input;
            InfoLabel LabelDateModified = GridViewAudit.Rows[rowIndex].FindControl("LabelDateModified") as InfoLabel;
            InfoLabel LabelPrincipalModified = GridViewAudit.Rows[rowIndex].FindControl("LabelPrincipalModified") as InfoLabel;
            ModalPanel PanelAuditInfo = GridViewAudit.Rows[rowIndex].FindControl("PanelAuditInfo") as ModalPanel;


            if (LiteralHiddenGuid != null
                && InputTitle != null
                && InputSubTitle != null
                && InputTeaser != null
                && InputHtml != null
                && LabelDateModified != null
                && LabelPrincipalModified != null
                && PanelAuditInfo != null)
            {
                string guid = LiteralHiddenGuid.Text;

                BOInternalContentAudit audit =  auditB.GetAudit(guid);

                if (audit != null)
                {
                    switch (command)
                    {
                        case "ShowItem":
                            {
                                ClosePreviousPanels();
                                SelectedGuid = guid;
                                InputTitle.Value = audit.Title;
                                InputSubTitle.Value = audit.SubTitle;
                                InputTeaser.Value = audit.Teaser;
                                InputHtml.Value = audit.Html;
                                LabelDateModified.Value = audit.DateModified.Value.ToString();
                                LabelPrincipalModified.Value = audit.PrincipalModified;
                                PanelAuditInfo.Visible = true;
                                break;
                            }
                        case "RevertTo":
                            {
                                ClosePreviousPanels();
                                OnRevertToAudit(new TypedEventArg<BOInternalContent>(audit));
                                break;
                            }
                    }
                }
            }
        }

        protected void GridViewAudit_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.Cells.Count >= 3)
            {
                BOInternalContentAudit audit = e.Row.DataItem as BOInternalContentAudit;
                ModalPanel PanelAuditInfo = e.Row.FindControl("PanelAuditInfo") as ModalPanel;

                if (audit != null && PanelAuditInfo != null)
                {
                    PanelAuditInfo.Title = ResourceManager.GetString("$content_audit_item");
                }
            }
        }

        #region State methods

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object[] cSThis = new object[21];
            object cSBase = base.SaveControlState();

            cSThis[0] = cSBase;
            cSThis[1] = showHistory;
            cSThis[2] = selectedItemId;
            cSThis[3] = getContent;
            cSThis[4] = selectedGuid;

            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];

            showHistory = (bool) cSThis[1];
            selectedItemId = (int)cSThis[2];
            getContent = (ContentGetter) cSThis[3];
            selectedGuid = (string) cSThis[4];

            base.LoadControlState(cSBase);
        }

        #endregion State methods
    }

    public class TypedEventArg<T> : EventArgs
    {
        private T _Value;

        public TypedEventArg(T value)
        {
            _Value = value;
        }

        public T Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
    }
}