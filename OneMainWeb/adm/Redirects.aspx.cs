using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Threading;
using One.Net.BLL;

namespace OneMainWeb
{
    public partial class Redirects : OneBasePage
    {
        protected BORedirect CurrentItem
        {
            get { return (BORedirect)ViewState["CurrentItem"]; }
            set { ViewState["CurrentItem"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Notifier1.Visible = true;

            if (!IsPostBack)
            {
                MultiView1.ActiveViewIndex = 0;
            }
        }

        protected void tabMultiview_OnViewIndexChanged(object sender, EventArgs e)
        {   
            if (((MultiView)sender).ActiveViewIndex == 0)
            {
                GridViewRedirects.DataBind();
            }
            else if (((MultiView)sender).ActiveViewIndex == 1)
            {
                if (CurrentItem == null)
                {
                    InputFromLink.Text = "";
                    InputToLink.Text = "";
                }
                else
                {
                    InputFromLink.Text = CurrentItem.FromLink;
                    InputToLink.Text = CurrentItem.ToLink;
                }
            }
        }

        protected void CmdSave_Click(object sender, EventArgs e)
        {
            if (CurrentItem == null)
                CurrentItem = new BORedirect();

            CurrentItem.FromLink = InputFromLink.Text;
            CurrentItem.ToLink = InputToLink.Text;

            var redirect = CurrentItem;
            BRedirects.Change(CurrentItem);

            var button = sender as Button;
            if (button.CommandName == "SAVE_CLOSE")
            {
                MultiView1.ActiveViewIndex = 0;
                GridViewRedirects.DataBind();
            }
        }

        protected void CmdCancel_Click(object sender, EventArgs e)
        {
            CurrentItem = null;
            MultiView1.ActiveViewIndex = 0;
        }

        protected void cmdShowAddRedirect_Click(object sender, EventArgs e)
        {
            CurrentItem = null;
            MultiView1.ActiveViewIndex = 1;
        }

        protected void RedirectListSource_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
        {
            GridViewRedirects.DataBind();
        }

        protected void GridViewRedirects_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
            {
                CurrentItem = BRedirects.Get(int.Parse(e.CommandArgument.ToString()));

                if (CurrentItem != null && CurrentItem.Id.HasValue)
                {
                    MultiView1.ActiveViewIndex = 1;
                }
            }
        }

        protected void RedirectListSource_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
                e.InputParameters.Clear();
        }
    }

    public class RedirectHelper
    {
        public int SelectCount()
        {
            return (int)HttpContext.Current.Items["rowCount"];
        }

        public PagedList<BORedirect> Select(int recordsPerPage, int firstRecordIndex, string sortDirection, string sortBy)
        {
            if (string.IsNullOrEmpty(sortBy.Trim()))
                sortBy = "id";
            var list = BRedirects.List(new ListingState(recordsPerPage, firstRecordIndex, (sortDirection.ToLower() == "asc" ? SortDir.Ascending : SortDir.Descending), sortBy));
            HttpContext.Current.Items["rowCount"] = list.AllRecords;
            return list;
        }

        public void DeleteRedirect(int Id)
        {
            BRedirects.Delete(Id);
        }

        public BORedirect Get(int Id)
        {
            return BRedirects.Get(Id);
        }
    }
}
