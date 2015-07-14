using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using One.Net.BLL;

namespace OneMainWeb
{
    public partial class SubscriptionManager : OneBasePage
    {
        public const int SUBSCRIPTION_TYPE_STARTED = 1;
        public const int SUBSCRIPTION_TYPE_CONFIRMED = 2;
        public const int SUBSCRIPTION_TYPE_UNSUBSCRIBED = 3;

        protected static BNewsLtr newsletterB = new BNewsLtr();

        protected int SelectedSubscriptionFilterId
        {
            get
            {
                int subscriptionFilterId = FormatTool.GetInteger(ddlSubscriptionFilter.SelectedValue);
                return subscriptionFilterId;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
			if (!IsPostBack)
			{
                plhCSLSubs.Visible = false;
                TwoPostbackPager1.Visible = false;
                TwoPostbackPager1.RecordsPerPage = GridViewPageSize;
                TwoPostbackPager1.SelectedPage = 1;
                GridViewSubscriptions.Visible = false;
                GridViewSortExpression = "id";
                GridViewSortDirection = SortDir.Descending;
                ddlSubscriptionFilter_DataBind();
                ddlNewsletter_DataBind(ddlNewsletters);
            }
        }

        private void ddlSubscriptionFilter_DataBind()
        {
            ddlSubscriptionFilter.Items.Clear();
            ddlSubscriptionFilter.Items.Add(new ListItem("Started subscriptions", SUBSCRIPTION_TYPE_STARTED.ToString()));
            ddlSubscriptionFilter.Items.Add(new ListItem("Confirmed subscriptions", SUBSCRIPTION_TYPE_CONFIRMED.ToString()));
            ddlSubscriptionFilter.Items.Add(new ListItem("Unsubscribed", SUBSCRIPTION_TYPE_UNSUBSCRIBED.ToString()));
        }

        private static void ddlNewsletter_DataBind(ListControl ddl)
        {
            ddl.DataSource = newsletterB.ListNewsletters(null);
            ddl.DataTextField = "Name";
            ddl.DataValueField = "Id";
            ddl.DataBind();
        }

        private void Subscriptions_DataBind()
        {
            plhCSLSubs.Visible = false;
            ListingState state = new ListingState();
            state.RecordsPerPage = GridViewPageSize;
            state.SortDirection = GridViewSortDirection;
            state.FirstRecordIndex = (TwoPostbackPager1.SelectedPage - 1) * GridViewPageSize;
            state.SortField = GridViewSortExpression;

            int newsletterId = 0;
            int.TryParse(ddlNewsletters.SelectedValue, out newsletterId);

            PagedList<BONewsLtrSub> subscriptions = newsletterB.ListSubscriptions(newsletterId, state, SelectedSubscriptionFilterId);
            TwoPostbackPager1.TotalRecords = subscriptions.AllRecords;
            TwoPostbackPager1.DetermineData();
            GridViewSubscriptions.DataSource = subscriptions;
            GridViewSubscriptions.DataBind();

            TwoPostbackPager1.Visible = (subscriptions.Count != 0);
            GridViewSubscriptions.Visible = (subscriptions.Count != 0);
            PanelNoResults.Visible = (subscriptions.Count == 0);
        }

        public void TwoPostbackPager1_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            Subscriptions_DataBind();
        }

        protected void CmdExportSubscriptions_Click(object sender, EventArgs e)
        {
            PagedList<BONewsLtrSub> subscriptions = newsletterB.ListSubscriptions(Int32.Parse(ddlNewsletters.SelectedValue), new ListingState(100000, 0, SortDir.Ascending, "email"), SelectedSubscriptionFilterId);

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + "Newsletter subscriptions" + ddlNewsletters.SelectedValue + ".xls\";");
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
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + "Subscription_id" + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + "Email" + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + "Subscribed" + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + "Confirmed" + "</th>");
            strw.GetStringBuilder().Append("</tr>");

            foreach (BONewsLtrSub sub in subscriptions)
            {
                strw.GetStringBuilder().Append(@"<tr><td class=""general"" align=""center"">" + sub.SubscriptionId.ToString() + @"</td>");
                strw.GetStringBuilder().Append(@"<td class=""general"" align=""center"">" + sub.Email + @"</td>");
                strw.GetStringBuilder().Append(@"<td class=""general"" align=""center"">" + sub.DateSubscribed.ToShortDateString() + @"</td>");
                strw.GetStringBuilder().Append(@"<td class=""general"" align=""center"">" +
                                               (sub.DateConfirmed.HasValue
                                                   ? sub.DateConfirmed.Value.ToShortDateString()
                                                   : "") + @"</td></tr>");
            }
            strw.GetStringBuilder().Append("</table><br />");
            // END DETAIL

            // START tail
            strw.GetStringBuilder().Append("</div></body></html>");
            // END tail

            Response.Write(strw.ToString());
            Response.End();
        }

        protected void cmdDisplaySubscriptions_Click(object sender, EventArgs e)
        {
            plhCSLSubs.Visible = false;
            Subscriptions_DataBind();
        }

        protected void cmdDisplayCSLSubscriptions_Click(object sender, EventArgs e)
        {
            plhCSLSubs.Visible = true;
            TwoPostbackPager1.Visible = false;
            GridViewSubscriptions.Visible = false;
            PanelNoResults.Visible = false;

            int newsletterId = FormatTool.GetInteger(ddlNewsletters.SelectedValue);

            List<BONewsLtrSub> subs = newsletterB.ListSubscriptions(newsletterId, new ListingState(10000, 0, SortDir.Ascending, null), SelectedSubscriptionFilterId);

            StringBuilder emailBuilder = new StringBuilder();

            foreach (BONewsLtrSub sub in subs)
            {
                emailBuilder.Append(sub.Email + "; ");
            }
            txtCSV.Text = emailBuilder.ToString();
        }

        protected void GridViewSubscriptions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if ( e.CommandName == "UnSubscribe" )
            {
                int rowIndex = Int32.Parse(e.CommandArgument.ToString());

                Literal litHash = GridViewSubscriptions.Rows[rowIndex].FindControl("litHash") as Literal;
                Literal litSubId = GridViewSubscriptions.Rows[rowIndex].FindControl("litSubId") as Literal;

                if (litHash != null && litSubId != null)
                {
                    int subId = FormatTool.GetInteger(litSubId.Text);
                    string hash = litHash.Text;

                    if (newsletterB.Unsubscribe(subId, hash) == NewsLtrSubRes.Success)
                        Notifier1.Message = "$sucessively_unsubscribed_sub";
                    Subscriptions_DataBind();   
                }
            }
        }


        private static bool IsValidEmail(string email)
        {
            Regex reg = new Regex(@"^(([\w\-\.]+@([A-Za-z0-9]([\w\-])*\.){1,2}([a-zA-Z]([\w\-]){1,3}));*)+"); // regex pattern from TwoControlsLibrary
            return reg.IsMatch(email);
        }
    }
}
