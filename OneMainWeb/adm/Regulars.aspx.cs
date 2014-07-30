using One.Net.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.adm
{
    public partial class Regulars : OneBasePage
    {
        private static readonly BArticle articleB = new BArticle();

        protected BORegular SelectedRegular
        {
            get { return ViewState["SelectedRegular"] as BORegular; }
            set { ViewState["SelectedRegular"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                regularGridView.DataSource = articleB.ListRegulars(new ListingState(SortDir.Ascending, ""), ShowUntranslated, null, null);
                regularGridView.DataBind();
            }
        }

        protected void Multiview1_ActiveViewChanged(object sender, EventArgs e)
        {
            if (((MultiView)sender).ActiveViewIndex == 1)
            {
                TxtRegularContent.UseCkEditor = true;

                if (SelectedRegular != null)
                {
                    TxtRegularContent.Title = SelectedRegular.Title;
                    TxtRegularContent.SubTitle = SelectedRegular.SubTitle;
                    TxtRegularContent.Teaser = SelectedRegular.Teaser;
                    TxtRegularContent.Html = SelectedRegular.Html;
                }
            }
        }

        protected void cmdAddRegular_Click(object sender, EventArgs e)
        {
            BORegular regular = new BORegular();

            regular.Title = TextBoxRegular.Text;
            regular.SubTitle = regular.Teaser = regular.Html = "";
            regular.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;
            regular.ContentId = null;

            articleB.ChangeRegular(regular);

            regularGridView.DataBind();
        }

        protected void cmdDeleteRegular_Click(object sender, EventArgs e)
        {
            LinkButton button = sender as LinkButton;

            if (button != null)
            {
                int id = Int32.Parse(button.CommandArgument);
                articleB.DeleteRegular(id);
                regularGridView.DataBind();
            }
        }

        protected void regularGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null && grid.SelectedValue != null)
            {
                Multiview1.ActiveViewIndex = 1;
                SelectedRegular = articleB.GetRegular(Int32.Parse(grid.SelectedValue.ToString()));
            }
        }

        private void SaveRegular(bool close)
        {
            try
            {
                SelectedRegular.Html = TxtRegularContent.Html;
                SelectedRegular.Title = TxtRegularContent.Title;
                SelectedRegular.Teaser = TxtRegularContent.Teaser;
                SelectedRegular.SubTitle = TxtRegularContent.SubTitle;
                SelectedRegular.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;

                articleB.ChangeRegular(SelectedRegular);

                Notifier1.Message = "Saved";

                if (close)
                {
                    SelectedRegular = null;
                    Multiview1.ActiveViewIndex = 0;
                }
                else
                {
                    Multiview1.ActiveViewIndex = 1;
                }
            }
            catch (Exception ex)
            {
                Notifier1.Visible = true;
                Notifier1.ExceptionName = "Error saving";
                Notifier1.ExceptionMessage = ex.Message;
                Notifier1.ExceptionMessage += "<br/>" + ex.StackTrace;
            }
        }

        protected void RegularCancelButton_Click(object sender, EventArgs e)
        {
            Multiview1.ActiveViewIndex = 0;
        }


        protected void RegularInsertUpdateButton_Click(object sender, EventArgs e)
        {
            SaveRegular(false);
        }

        protected void RegularInsertUpdateCloseButton_Click(object sender, EventArgs e)
        {
            SaveRegular(true);
        }
        /*
        protected void ObjectDataSourceRegularList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["showUntranslated"] = ShowUntranslated;
            if (e.ExecutingSelectCount)
                e.InputParameters.Clear();
        }

        protected void ObjectDataSourceRegularSource_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["showUntranslated"] = ShowUntranslated;
        }*/
        
    }
}