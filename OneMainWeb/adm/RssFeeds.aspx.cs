using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Reflection;

using One.Net.BLL.Model;
using One.Net.BLL;
using One.Net.BLL.WebConfig;


namespace OneMainWeb
{
    public partial class RssFeeds : OneBasePage
    {
        private static readonly BRssFeed rssFeedB = new BRssFeed();
        private bool diagnosticsValid = true;

        protected BORssFeed SelectedRssFeed
        {
            get { return ViewState["SelectedRssFeed"] as BORssFeed; }
            set { ViewState["SelectedRssFeed"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Notifier1.Visible = true;

            if (!IsPostBack)
            {
                RunDiagnostics();
            }
        }

        private void RunDiagnostics()
        {
            if (RssConfiguration.Configuration == null)
            {
                Notifier1.ExceptionMessage =
                    ResourceManager.GetString("$rss_configuration_element_missing_from_web_config");
                Notifier1.ExceptionName = ResourceManager.GetString("$error_loading");
                Notifier1.Visible = true;
                AddRssFeed.Enabled = false;
                diagnosticsValid = false;
            }
            else if (RssConfiguration.Configuration.RssProviders == null)
            {
                Notifier1.ExceptionMessage =
                    ResourceManager.GetString("$missing_rssConfiguration_providers_section_from_web_config");
                Notifier1.ExceptionName = ResourceManager.GetString("$error_loading");
                Notifier1.Visible = true;
                AddRssFeed.Enabled = false;
                diagnosticsValid = false;
            }
            else if (RssConfiguration.Configuration.RssProviders.Count == 0)
            {
                Notifier1.ExceptionMessage =
                    ResourceManager.GetString("$missing_rssConfiguration_providers_from_web_config");
                Notifier1.ExceptionName = ResourceManager.GetString("$error_loading");
                Notifier1.Visible = true;
                AddRssFeed.Enabled = false;
                diagnosticsValid = false;
            }

            RssConfigProviderCollection configProviders = rssFeedB.RetreiveProviderCollection();

            foreach (RssConfigProvider configProvider in configProviders)
            {
                try
                {
                    rssFeedB.ListUnCachedCategories(configProvider.Name);
                }
                catch (System.Data.SqlClient.SqlException sqex)
                {
                    diagnosticsValid = false;
                    MultiView1.ActiveViewIndex = 0;
                    Notifier1.ExceptionMessage = ((Exception)sqex).Message;
                    Notifier1.ExceptionName = ResourceManager.GetString("$error_loading");
                    Notifier1.Visible = true;
                    AddRssFeed.Enabled = false;
                }
            }
        }

        protected void ddlProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedProvider = ddlProviders.SelectedValue;

            if (!string.IsNullOrEmpty(selectedProvider))
            {
                List<BORssCategory> categories = rssFeedB.ListUnCachedCategories(selectedProvider);
                if (categories != null)
                {
                    chlCategories.DataSource = categories;
                    chlCategories.DataTextField = "Title";
                    chlCategories.DataValueField = "Id";
                    chlCategories.DataBind();

                    foreach (int cat in SelectedRssFeed.Categories)
                    {
                        foreach (ListItem item in chlCategories.Items)
                        {
                            if (cat == Int32.Parse( item.Value))
                                item.Selected = true;
                        }
                    }
                }
                else
                {
                    throw new Exception(ResourceManager.GetString("$rss_categories_loading_failed"));
                }
            }            
        }

        protected void TabMultiview_OnViewIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (((MultiView) sender).ActiveViewIndex == 0)
                {
                    FeedsGridView.DataBind();

                    foreach (GridViewRow row in FeedsGridView.Rows)
                    {
                        LinkButton cmdDelete = row.Cells[4].FindControl("cmdDelete") as LinkButton;
                        LinkButton cmdEdit = row.Cells[5].FindControl("cmdEdit") as LinkButton;

                        if (cmdDelete != null && cmdEdit != null)
                        {
                            cmdDelete.Enabled = diagnosticsValid;
                            cmdEdit.Enabled = diagnosticsValid;
                        }                        
                    }

                }
                else if (((MultiView) sender).ActiveViewIndex == 1)
                {
                    RssConfigProviderCollection configProviders = rssFeedB.RetreiveProviderCollection();

                    if (SelectedRssFeed != null)
                    {
                        ddlProviders.DataSource = configProviders;
                        ddlProviders.DataTextField = "Name";
                        ddlProviders.DataValueField = "Name";
                        ddlProviders.DataBind();

                        if (ddlProviders.Items.FindByValue(SelectedRssFeed.Type) != null)
                            ddlProviders.SelectedValue = SelectedRssFeed.Type;
                        else
                            ddlProviders.SelectedIndex = 0;

                        string selectedProvider = ddlProviders.SelectedValue;

                        if (!string.IsNullOrEmpty(selectedProvider))
                        {
                            List<BORssCategory> categories = rssFeedB.ListUnCachedCategories(selectedProvider);
                            if (categories != null)
                            {
                                chlCategories.DataSource = categories;
                                chlCategories.DataTextField = "Title";
                                chlCategories.DataValueField = "Id";
                                chlCategories.DataBind();

                                foreach (int cat in SelectedRssFeed.Categories)
                                {
                                    foreach (ListItem item in chlCategories.Items)
                                    {
                                        if ( cat == Int32.Parse( item.Value) )
                                            item.Selected = true;
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("Categories loading failed.");
                            }
                        }

                        InputTitle.Text = SelectedRssFeed.Title;
                        InputLinkToSingle.Text = SelectedRssFeed.LinkToSingle;
                        InputLinkToList.Text = SelectedRssFeed.LinkToList;
                        InputDescription.Text = SelectedRssFeed.Description;
                    }

                    if (SelectedRssFeed != null && SelectedRssFeed.Id.HasValue)
                    {
                        InsertUpdateButton.Text = ResourceManager.GetString("$update_rss_feed");
                        InsertUpdateCloseButton.Text = ResourceManager.GetString("$update_rss_feed_and_close");
                    }
                    else
                    {
                        InsertUpdateButton.Text = ResourceManager.GetString("$add_rss_feed");
                        InsertUpdateCloseButton.Text = ResourceManager.GetString("$add_rss_feed_and_close");
                    }
                }
            }
            catch (Exception ex)
            {
                Notifier1.Visible = true;
                Notifier1.ExceptionName = "$error_loading";
                Notifier1.ExceptionMessage = ex.Message;
                Notifier1.ExceptionMessage += "<br/>" + ex.StackTrace;
            }
        }

        protected void FeedsGridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = Int32.Parse(FeedsGridView.SelectedDataKey.Value.ToString());
            SelectedRssFeed = rssFeedB.GetUnCached(id);

            MultiView1.ActiveViewIndex = 1;
        }

        protected void AddRssFeed_Click(object sender, EventArgs e)
        {
            SelectedRssFeed = new BORssFeed();

            MultiView1.ActiveViewIndex = 1;
        }

        protected void FeedsGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            
        }

        protected void ObjectDataSourceRssFeedList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (e.ExecutingSelectCount)
                e.InputParameters.Clear();            
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            SelectedRssFeed = null;
            MultiView1.ActiveViewIndex = 0;      
        }

        protected void InsertUpdateButton_Click(object sender, EventArgs e)
        {
            SaveRssFeed();
            Notifier1.Message = ResourceManager.GetString("$item_saved");

            MultiView1.ActiveViewIndex = 1;
        }

        protected void InsertUpdateCloseButton_Click(object sender, EventArgs e)
        {
            SaveRssFeed();

            Notifier1.Message = ResourceManager.GetString("$item_saved");

            SelectedRssFeed = null;
            MultiView1.ActiveViewIndex = 0;
        }

        private void SaveRssFeed()
        {
            SelectedRssFeed.Title = InputTitle.Text;
            SelectedRssFeed.Description = InputDescription.Text;
            SelectedRssFeed.Type = ddlProviders.SelectedValue;
            SelectedRssFeed.LinkToList = InputLinkToList.Text;
            SelectedRssFeed.LinkToSingle = InputLinkToSingle.Text;
            string categories = "";
            foreach (ListItem item in chlCategories.Items)
            {
                if (item.Selected)
                    categories += item.Value + ",";
            }
            categories = categories.TrimEnd(',');

            SelectedRssFeed.Categories = StringTool.SplitStringToIntegers(categories);
            SelectedRssFeed.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;

            rssFeedB.Change(SelectedRssFeed);
        }
    }

    [Serializable]
    public class RssFeedDataSource
    {
        private static readonly BRssFeed rssFeedB = new BRssFeed();

        public List<BORssFeed> SelectRssFeeds(int recordsPerPage, int firstRecordIndex, string sortBy)
        {
            ListingState state = 
                new ListingState(recordsPerPage, firstRecordIndex,
                                 (sortBy.Contains("ASC") || !sortBy.Contains("DESC")
                                      ? SortDir.Ascending
                                      : SortDir.Descending), sortBy.Replace("DESC", "").Replace("ASC", ""));
            PagedList<BORssFeed> rssFeeds = rssFeedB.List(state);
            HttpContext.Current.Items["feedCount"] = rssFeeds.AllRecords;
            return rssFeeds;
        }

        public int SelectRssFeedCount()
        {
            return (int)HttpContext.Current.Items["feedCount"];
        }

        public void Delete(int id)
        {
            rssFeedB.Delete(id);
        }
    }
}
