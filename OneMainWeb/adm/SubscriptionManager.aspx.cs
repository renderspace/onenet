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

        protected void Page_Load(object sender, EventArgs e)
        {
            cmdDeleteUnconfirmedSubscriptions.OnClientClick = @"return confirm('" + ResourceManager.GetString("$label_confirm_delete_unconfirmed_subscriptions") + @"');";
			cmdDeleteSubscriptions.OnClientClick = @"return confirm('" + ResourceManager.GetString("$label_confirm_delete_subscriptions") + @"');";
			if (!IsPostBack)
			{
            }
        }

        private void ddlSubscriptionFilter_DataBind()
        {
            ddlSubscriptionFilter.Items.Clear();
            ddlSubscriptionFilter.Items.Add(new ListItem(ResourceManager.GetString("$started_subscription"), SUBSCRIPTION_TYPE_STARTED.ToString()));
            ddlSubscriptionFilter.Items.Add(new ListItem(ResourceManager.GetString("$confirmed_subscription"), SUBSCRIPTION_TYPE_CONFIRMED.ToString()));
            ddlSubscriptionFilter.Items.Add(new ListItem(ResourceManager.GetString("$unsubscribed"), SUBSCRIPTION_TYPE_UNSUBSCRIBED.ToString()));
        }

        private static void ddlNewsletter_DataBind(ListControl ddl)
        {
            ddl.DataSource = newsletterB.ListNewsletters(null);
            ddl.DataTextField = "Name";
            ddl.DataValueField = "Id";
            ddl.DataBind();
        }

        protected void tabMultiview_OnViewIndexChanged(object sender, EventArgs e)
        {
            if (((MultiView)sender).ActiveViewIndex == 0)
            {
                plhCSLSubs.Visible = false;
                plhSubscriptions.Visible = true;

                ddlSubscriptionFilter_DataBind();
                ddlNewsletter_DataBind(ddlNewsletters);
                ddlNewsletter_DataBind(ddlNewsletterFilter);
                subscriptionGridView.DataBind();
            }
            else if (((MultiView)sender).ActiveViewIndex == 1)
            {

            }
            else if (((MultiView)sender).ActiveViewIndex == 2)
            {
                ddlNewsletter_DataBind(DropDownListNewsletters);
            }
        }

        protected void CmdExportSubscriptions_Click(object sender, EventArgs e)
        {
            PagedList<BONewsLtrSub> subscriptions = newsletterB.ListSubscriptions(Int32.Parse(ddlNewsletters.SelectedValue), new ListingState(null, null, SortDir.Ascending, "email"), Int32.Parse(ddlSubscriptionFilter.SelectedValue));

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + ResourceManager.GetString("$export_newsletter_subscriptions_file_name") + ".xls\";");
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
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + ResourceManager.GetString("$subscription_id") + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + ResourceManager.GetString("$email") + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + ResourceManager.GetString("$subscribed") + "</th>");
            strw.GetStringBuilder().Append(@"<th class=""generalsmall"">" + ResourceManager.GetString("$confirmed") + "</th>");
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
            plhSubscriptions.Visible = true;
            subscriptionGridView.DataBind();
        }

        protected void cmdDisplayCSLSubscriptions_Click(object sender, EventArgs e)
        {
            plhCSLSubs.Visible = true;
            plhSubscriptions.Visible = false;

            int newsletterId = FormatTool.GetInteger(ddlNewsletters.SelectedValue);
            int subscriptionType = FormatTool.GetInteger(ddlSubscriptionFilter.SelectedValue);

            List<BONewsLtrSub> subs = newsletterB.ListSubscriptions(newsletterId, new ListingState(null, null, SortDir.Ascending, null), subscriptionType);

            StringBuilder emailBuilder = new StringBuilder();

            foreach (BONewsLtrSub sub in subs)
            {
                emailBuilder.Append(sub.Email + "; ");
            }
            txtCSV.Text = emailBuilder.ToString();
        }

        protected void SubscriptionSource_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
            }
        }

        protected void subscriptionGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if ( e.CommandName == "UnSubscribe" )
            {
                int rowIndex = Int32.Parse(e.CommandArgument.ToString());

                Literal litHash = subscriptionGridView.Rows[rowIndex].FindControl("litHash") as Literal;
                Literal litSubId = subscriptionGridView.Rows[rowIndex].FindControl("litSubId") as Literal;

                if (litHash != null && litSubId != null)
                {
                    int subId = FormatTool.GetInteger(litSubId.Text);
                    string hash = litHash.Text;

                    if (newsletterB.Unsubscribe(subId, hash) == NewsLtrSubRes.Success)
                        Notifier1.Message = ResourceManager.GetString("$sucessively_unsubscribed_sub");
                    subscriptionGridView.DataBind();   
                }
            }
        }

		protected void cmdDeleteSubscriptions_Click(object sender, EventArgs e)
		{
			Button button = sender as Button;
			if (button != null && ddlNewsletterFilter.SelectedIndex > -1 && chkDeleteAll.Checked)
			{
				int newsletterId = int.Parse(ddlNewsletterFilter.SelectedValue);
				int countDeleted = 0;
				List<BONewsLtrSub> subscriptions = newsletterB.ListSubscriptions(newsletterId, new ListingState(null, null, SortDir.Ascending, null), -1);
				foreach (BONewsLtrSub sub in subscriptions)
				{
					newsletterB.DeleteSubscription(sub.SubscriptionId);
					countDeleted++;
				}
				MultiView1.ActiveViewIndex = 1;
				Notifier1.Visible = true;
				Notifier1.Message = "deleted " + countDeleted + " subscriptions from newsletter: " + ddlNewsletterFilter.SelectedItem.Text;
			}
			else if (!chkDeleteAll.Checked)
			{
				Notifier1.Visible = true;
				Notifier1.ExceptionName = ResourceManager.GetString("$label_delete_all_precaution_title");
				Notifier1.ExceptionMessage = ResourceManager.GetString("$label_delete_all_precaution_text");
			}
		}

        protected void cmdDeleteUnconfirmedSubscriptions_Click(object sender, EventArgs e)
        {
            int newsletterId = FormatTool.GetInteger( ddlNewsletterFilter.SelectedValue );
            DateTime fromDate = DateTime.Now.AddDays(-Int32.Parse(txtBackFromDays.Text));

            if (newsletterId > -1 && fromDate != DateTime.MinValue)
            {
                int countDeleted = 0;
                List<BONewsLtrSub> unconfirmedSubscriptions = newsletterB.ListSubscriptions(newsletterId, new ListingState(null, null, SortDir.Ascending, null), SUBSCRIPTION_TYPE_STARTED);
                foreach (BONewsLtrSub sub in unconfirmedSubscriptions)
                {
                    if (sub.DateSubscribed < fromDate)
                    {
                        newsletterB.DeleteSubscription(sub.SubscriptionId);
                        countDeleted++;
                    }
                }

                MultiView1.ActiveViewIndex = 1;
                Notifier1.Visible = true;
                Notifier1.Message = "deleted " + countDeleted + " unconfirmed subscriptions that were subscribed before " + fromDate.ToString("dd.MM.yyyy");
            }
            else
            {
                Notifier1.Visible = true;
                Notifier1.Message = ResourceManager.GetString("$delete_failed");
            }
        }

        protected void CmdImport_Click(object sender, EventArgs e)
        {
            int newsletterId = FormatTool.GetInteger(DropDownListNewsletters.SelectedValue);

            if ( newsletterId > 0)
            {
                string rawEmails = InputEmails.Text;
                string[] emails = rawEmails.Split('\n'); // split csv contents by new line

                StringBuilder strBuilder = new StringBuilder();
                int countSuccess = 0;
                int countFailure = 0;

                foreach (string rawEmail in emails)
                {
                    string email = rawEmail.Trim();

                    if (!string.IsNullOrEmpty(email) && IsValidEmail(email))
                    {
                        NewsLtrSubRes res = newsletterB.SubscribeAndConfirm(email, newsletterId, System.Net.IPAddress.Parse(Request.UserHostAddress));

                        if (res == NewsLtrSubRes.Failed)
                        {
                            strBuilder.Append("Processing failed for email address: [" + email + "]<br />");
                            countFailure++;
                        }
                        else if (res == NewsLtrSubRes.AlreadyExists)
                        {
                            strBuilder.Append("Duplicate email address detected [" + email + "] and not processed <br />");
                            countFailure++;
                        }
                        else if (res == NewsLtrSubRes.Success)
                            countSuccess++;
                    }
                    else
                    {
                        strBuilder.Append("Invalid email address detected [" + email + "] and not processed <br />");
                        countFailure++;
                    }
                }

                strBuilder.Append("<br />Successfully processed, subscribed and confirmed [" + countSuccess + "] email addresses.<br />");
                strBuilder.Append("Failed to process [" + countFailure + "] email addresses.");

                Notifier1.Message = strBuilder.ToString();
            }
        }

        private static bool IsValidEmail(string email)
        {
            Regex reg = new Regex(@"^(([\w\-\.]+@([A-Za-z0-9]([\w\-])*\.){1,2}([a-zA-Z]([\w\-]){1,3}));*)+"); // regex pattern from TwoControlsLibrary
            return reg.IsMatch(email);
        }
    }

    public class SubscriptionHelper
    {
        protected static BNewsLtr newsletterB = new BNewsLtr();

        public PagedList<BONewsLtrSub> ListSubscriptions(int newsletterId, int firstRecordIndex, int recordsPerPage, int subscriptionType, string sortBy)
        {
            PagedList<BONewsLtrSub> subscriptions = newsletterB.ListSubscriptions(newsletterId, new ListingState(recordsPerPage, firstRecordIndex, (sortBy.ToLower().Contains("asc") || !sortBy.ToLower().Contains("desc") ? SortDir.Ascending : SortDir.Descending), sortBy.ToLower().Replace("desc", "").Replace("asc", "")), subscriptionType);
            HttpContext.Current.Items["rowCount"] = subscriptions.AllRecords;
            return subscriptions;
        }

        public int GetSubscriptionCount()
        {
            return (int)HttpContext.Current.Items["rowCount"];
        }
    }
}
