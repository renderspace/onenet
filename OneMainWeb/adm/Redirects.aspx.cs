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
                TwoPostbackPager1.RecordsPerPage = GridViewPageSize;
                TwoPostbackPager1.SelectedPage = 1;
                GridViewSortExpression = "id";
                GridViewSortDirection = SortDir.Descending;
                Redirects_DataBind();
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

        private void Redirects_DataBind()
        {
            Redirects_DataBind("", "");
        }

        private void Redirects_DataBind(string searchBy, string regularsFilter)
        {
            ListingState state = new ListingState();
            state.RecordsPerPage = GridViewPageSize;
            state.SortDirection = GridViewSortDirection;
            state.FirstRecordIndex = (TwoPostbackPager1.SelectedPage - 1) * GridViewPageSize;
            state.SortField = GridViewSortExpression;

            var redirects = BRedirects.List(state);

            TwoPostbackPager1.TotalRecords = redirects.AllRecords;
            TwoPostbackPager1.DetermineData();

            GridViewRedirects.DataSource = redirects;
            GridViewRedirects.DataBind();

            PanelGridButtons.Visible = (redirects.Count != 0);
            TwoPostbackPager1.Visible = (redirects.Count != 0);
            GridViewRedirects.Visible = (redirects.Count != 0);
            PanelNoResults.Visible = (redirects.Count == 0);
        }

        protected void CmdSave_Click(object sender, EventArgs e)
        {
            if (CurrentItem == null)
                CurrentItem = new BORedirect();

            CurrentItem.FromLink = InputFromLink.Text;
            CurrentItem.ToLink = InputToLink.Text;

            var redirect = CurrentItem;
            BRedirects.Change(CurrentItem);

            var button = sender as LinkButton;
            if (button.CommandName == "SAVE_CLOSE")
            {
                MultiView1.ActiveViewIndex = 0;
                GridViewRedirects.DataBind();
            }
        }

        protected void cmdShowAddRedirect_Click(object sender, EventArgs e)
        {
            CurrentItem = null;
            MultiView1.ActiveViewIndex = 1;
        }

        protected void RedirectListSource_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
                e.InputParameters.Clear();
        }

        protected void ButtonDelete_Click(object sender, EventArgs e)
        {
            int deletedCount = 0;
            var list = GetCheckedIds();
            foreach (var i in list)
            {
                BRedirects.Delete(i);
                deletedCount++;
            }
            if (deletedCount > 0)
            {
                Notifier1.Title = string.Format("Deleted {0} redirects", deletedCount);
                Redirects_DataBind();
            }
        }

        public void TwoPostbackPager1_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            Redirects_DataBind();
        }

        protected void GridViewRedirects_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridViewSorting(e);
            Redirects_DataBind();
        }

        protected void GridViewRedirects_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null && grid.SelectedValue != null)
            {
                CurrentItem = BRedirects.Get(Int32.Parse(grid.SelectedValue.ToString()));
                MultiView1.ActiveViewIndex = 1;
            }
        }

        protected IEnumerable<int> GetCheckedIds()
        {
            var result = new List<int>();
            foreach (GridViewRow row in GridViewRedirects.Rows)
            {
                var chkForPublish = row.FindControl("chkFor") as CheckBox;
                var litRedirectId = row.FindControl("litId") as Literal;

                if (litRedirectId != null && chkForPublish != null && chkForPublish.Checked)
                {
                    int redirectId = FormatTool.GetInteger(litRedirectId.Text);
                    if (redirectId > 0)
                    {
                        result.Add(redirectId);
                    }
                }
            }
            return result;
        }

        protected void ButtonEditById_Click(object sender, EventArgs e)
        {
            var id = 0;
            int.TryParse(TextBoxSearch.Text, out id);
            CurrentItem = BRedirects.Get(id);

            if (CurrentItem != null)
            {
                MultiView1.ActiveViewIndex = 1;
            }
            else
            {
                Notifier1.Warning = "ID not found.";
            }
        }

        

        protected void LinkButtonExport_Click(object sender, EventArgs e)
        {
            var redirects = BRedirects.List(new ListingState(100000, 0, SortDir.Ascending, "id"));

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + "Redirects.xls\";");
            Response.ContentEncoding = System.Text.Encoding.GetEncoding(1250);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Charset = "";

            // Prepare to export the data
            System.IO.StringWriter strw = new System.IO.StringWriter();

            // START head
            strw.GetStringBuilder().Append(
                @"<html xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:x=""urn:schemas-microsoft-com:office:excel"" xmlns=""http://www.w3.org/TR/REC-html40"">
                      <head>
                            <meta http-equiv=Content-Type content=""text/html; charset=windows-1250"">
                            <meta name=ProgId content=Excel.Sheet>
                            <meta name=Generator content=""Microsoft Excel 11"">
                            <style>
                                <!-- 

                                .general {color:black; font-size:13.0pt; font-weight:400;}
                                .generalsmall {color:black; font-size:9.0pt; font-weight:bold;}
                                .question { background:#CCCCFF; color:black; font-size:13.0pt; font-weight:400; }
                                .openAnswer { 	background:lime; color:black; font-size:13.0pt; font-weight:400; }
                                .singleAnswer { background:#FF9900; color:black; font-size:13.0pt; font-weight:400; }
                                .multipleChoiceAnswer { background:#FF6600; color:black; font-size:13.0pt; font-weight:400; }

                                -->
                            </style>
                      </head>
                      <body><div id=""STI_5961"" align=center x:publishsource=""Excel"">");
            // END head

            // START DETAIL

            strw.GetStringBuilder().Append(
                @"<table border=""1px""><tr>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + "From" + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + "To" + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + "Created" + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + "Id" + "</th>");
            strw.GetStringBuilder().Append("</tr>");

            foreach (var r in redirects)
            {
                strw.GetStringBuilder().Append(@"<tr><td class=""general"" align=""center"">" + r.FromLink + @"</td>");
                strw.GetStringBuilder().Append(@"<td class=""general"" align=""center"">" + r.ToLink + @"</td>");
                strw.GetStringBuilder().Append(@"<td class=""general"" align=""center"">" + r.Created.ToShortDateString() + @"</td>");
                strw.GetStringBuilder().Append(@"<td class=""general"" align=""center"">" + r.Id + @"</td></tr>");
            }
            strw.GetStringBuilder().Append("</table><br />");
            // END DETAIL

            // START tail
            strw.GetStringBuilder().Append("</div></body></html>");
            // END tail

            Response.Write(strw.ToString());
            Response.End();
        }
    }
}
