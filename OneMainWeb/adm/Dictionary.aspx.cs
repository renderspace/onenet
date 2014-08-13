using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using One.Net.BLL;

using OneMainWeb.AdminControls;

namespace OneMainWeb
{
    public partial class Dictionary : OneBasePage
    {
        protected static BContent contentB = new BContent();

        protected BODictionaryEntry SelectedDictionaryEntry
        {
            get { return ViewState["SelectedDictionaryEntry"] as BODictionaryEntry; }
            set { ViewState["SelectedDictionaryEntry"] = value; }
        }

        

        protected string GridViewSortExpression
        {
            get
            {
                if (ViewState["sortField"] == null)
                    ViewState["sortField"] = "";
                return (string)ViewState["sortField"];
            }
            set { ViewState["sortField"] = value; }
        }

        protected string SearchTermNoResults
        {
            get
            {
                if (ViewState["SearchReturnedNoResults"] == null)
                    ViewState["SearchReturnedNoResults"] = "";
                return (string)ViewState["SearchReturnedNoResults"];
            }
            set { ViewState["SearchReturnedNoResults"] = value; }
        }

        

        protected void Page_Load(object sender, EventArgs e)
        {
            Notifier1.Visible = true;
            LabelNoResults.Text = "$no_search_results";

            if (!IsPostBack)
            {
                SearchTermNoResults = "";

                MultiView1.ActiveViewIndex = 0;

                TwoPostbackPager1.RecordsPerPage = GridViewPageSize;
                TwoPostbackPager1.SelectedPage = 1;
                LoadAll(false);
            }
        }

        protected void tabMultiview_OnViewIndexChanged(object sender, EventArgs e)
        {
            LastChangeAndHistory1.SelectedContentId = 0;
            LastChangeAndHistory1.Text = "";

            if (((MultiView)sender).ActiveViewIndex == 0)
            {
                if (IsPostBack)
                    LoadAll(false);
            }
            else if (((MultiView)sender).ActiveViewIndex == 1)
            {
                txtTextContent.UseCkEditor = true;

                if (SelectedDictionaryEntry != null && !string.IsNullOrEmpty(SelectedDictionaryEntry.KeyWord))
                {
                    LastChangeAndHistory1.SelectedContentId = SelectedDictionaryEntry.ContentId.Value;
                    LastChangeAndHistory1.Text = SelectedDictionaryEntry.DisplayLastChanged;
                    LastChangeAndHistory1.SelectedLanguageId = SelectedDictionaryEntry.LanguageId;

                    txtTextContent.Title = SelectedDictionaryEntry.Title;
                    txtTextContent.SubTitle = SelectedDictionaryEntry.SubTitle;
                    txtTextContent.Teaser = SelectedDictionaryEntry.Teaser;
                    txtTextContent.Html = SelectedDictionaryEntry.Html;
                    LabelKeyword.Text = SelectedDictionaryEntry.KeyWord;

                    txtKeyword.Visible = false;
                    LabelKeyword.Visible = true;
                    InsertUpdateButton.Text = "Save";
                    InsertUpdateCloseButton.Text = "Save and close";
                }
                else
                {
                    txtKeyword.Visible = true;
                    LabelKeyword.Visible = false;
                    InsertUpdateButton.Text = "Insert";
                    InsertUpdateCloseButton.Text = "Insert and close";
                }
            }
            else if (((MultiView)sender).ActiveViewIndex == 2)
            {
                RadioButtonListWriteTypes.Items.Clear();
                RadioButtonListWriteTypes.Items.Add(new ListItem("overwrite", "0"));
                RadioButtonListWriteTypes.Items.Add(new ListItem("dont_overwrite", "1"));
                RadioButtonListWriteTypes.SelectedIndex = 1;
            }
        }

        protected void GridViewEntries_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (GridViewSortExpression == e.SortExpression)
            {
                GridViewSortDirection = SortDir.Ascending == GridViewSortDirection
                                            ? SortDir.Descending
                                            : SortDir.Ascending;
            }
            else
            {
                GridViewSortExpression = e.SortExpression;
                GridViewSortDirection = SortDir.Ascending;
            }
            LoadAll(false);
        }

        private void LoadAll(bool searching)
        {
            ListingState state = new ListingState();

            state.RecordsPerPage = GridViewPageSize;
            state.SortDirection = GridViewSortDirection;
            state.FirstRecordIndex = (TwoPostbackPager1.SelectedPage - 1) * GridViewPageSize;
            state.SortField = GridViewSortExpression;

            PagedList<BODictionaryEntry> entries = contentB.ListDictionaryEntries(state, ShowUntranslated, TextBoxSearch.Text);

            TwoPostbackPager1.TotalRecords = entries.AllRecords;
            TwoPostbackPager1.DetermineData();
            GridViewEntries.DataSource = entries;
            GridViewEntries.DataBind();

            if (!string.IsNullOrEmpty(TextBoxSearch.Text) && entries.AllRecords == 0)
            {
                if (!ShowUntranslated)
                    Notifier1.Warning = "$you_can_get_more_results_if_tick_show_untranslated";

                GridViewEntries.Visible = false;
                LabelNoResults.Visible = true;
                SearchTermNoResults = TextBoxSearch.Text;
            }
            else
            {
                GridViewEntries.Visible = true;
                LabelNoResults.Visible = false;
                SearchTermNoResults = "";
            }

            TwoPostbackPager1.Visible = entries.AllRecords > 0;
        }

        public void TwoPostbackPager1_Command(object sender, CommandEventArgs e)
        {
            TwoPostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            LoadAll(false);
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            SelectedDictionaryEntry = null;
            MultiView1.ActiveViewIndex = 0;
        }

        protected void InsertUpdateButton_Click(object sender, EventArgs e)
        {
            SaveDictionaryEntry(false);
        }

        protected void InsertUpdateCloseButton_Click(object sender, EventArgs e)
        {
            SaveDictionaryEntry(true);
        }

        private void SaveDictionaryEntry(bool close)
        {
            try
            {
                // we're inserting
                if (null == SelectedDictionaryEntry) 
                {
                    SelectedDictionaryEntry = new BODictionaryEntry();
                    SelectedDictionaryEntry.KeyWord = txtKeyword.Text;
                }

                SelectedDictionaryEntry.Title = txtTextContent.Title;
                SelectedDictionaryEntry.SubTitle = txtTextContent.SubTitle;
                SelectedDictionaryEntry.Teaser = txtTextContent.Teaser;
                SelectedDictionaryEntry.Html = txtTextContent.Html;
                SelectedDictionaryEntry.LanguageId = Thread.CurrentThread.CurrentCulture.LCID;

                BContent.ResultCode result = contentB.ChangeDictionaryEntry(SelectedDictionaryEntry);

                switch(result)
                {
                    case BContent.ResultCode.OK:
                        Notifier1.Message = "$dictionary_entry_saved";
                        break;
                    default:
                        Notifier1.Warning = "$dictionary_entry_not_saved";
                        close = false;
                        break;
                }

                if (close)
                {
                    SelectedDictionaryEntry = null;
                    txtTextContent.Title = "";
                    txtTextContent.SubTitle = "";
                    txtTextContent.Teaser = "";
                    txtTextContent.Html = "";
                    LabelKeyword.Text = "";

                    MultiView1.ActiveViewIndex = 0;
                }
                else
                {
                    MultiView1.ActiveViewIndex = 1;
                }
            }
            catch (Exception ex)
            {
                Notifier1.Visible = true;
                Notifier1.ExceptionName = "$error_saving";
                Notifier1.ExceptionMessage = ex.Message;
                Notifier1.ExceptionMessage += "<br/>" + ex.StackTrace;
            }
        }


        protected void cmdSearch_Click(object sender, EventArgs e)
        {
            TwoPostbackPager1.SelectedPage = 1;
            LoadAll(true);
        }

        protected void cmdAddDictionaryEntry_Click(object sender, EventArgs e)
        {
            SelectedDictionaryEntry = null;
            txtKeyword.Text = SearchTermNoResults;
            txtTextContent.Title = "";
            txtTextContent.SubTitle = "";
            txtTextContent.Teaser = "";
            txtTextContent.Html = "";
            LabelKeyword.Text= SearchTermNoResults;
            MultiView1.ActiveViewIndex = 1;
        }

        protected void GridViewEntries_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null)
            {
                string keyword = ((Label)grid.Rows[e.RowIndex].FindControl("LabelKeyword")).Text;
                if (!string.IsNullOrEmpty(keyword))
                {
                    if(contentB.DeleteDictionaryEntry(keyword))
                    {
                        LoadAll(false);
                        Notifier1.Message = "$deleted";
                    }
                    else
                    {
                        Notifier1.Warning = "$delete_failed";
                    }
                }
            }
        }

        protected void GridViewEntries_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridView grid = sender as GridView;
            if (grid != null && grid.SelectedValue != null)
            {
                SelectedDictionaryEntry = contentB.GetDictionaryEntry(grid.SelectedValue.ToString());
                if (null == SelectedDictionaryEntry || SelectedDictionaryEntry.MissingTranslation)
                {
                    SelectedDictionaryEntry = new BODictionaryEntry();
                    SelectedDictionaryEntry.KeyWord = grid.SelectedValue.ToString();
                }
                MultiView1.ActiveViewIndex = 1;
            }
        }

        protected void GridViewEntries_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            ImageButton cmdEditButton = e.Row.FindControl("cmdEditButton") as ImageButton;
            if (null != cmdEditButton)
                cmdEditButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(OneMainWeb.OneMain), "OneMainWeb.Res.edit.gif");
        }

        protected IEnumerable<string> GetCheckedKeywords()
        {
            var result = new List<string>();
            foreach (GridViewRow row in GridViewEntries.Rows)
            {
                CheckBox chkForPublish = row.FindControl("chkFor") as CheckBox;
                Literal litArticleId = row.FindControl("litId") as Literal;

                if (litArticleId != null && chkForPublish != null && chkForPublish.Checked && !string.IsNullOrEmpty(litArticleId.Text))
                {
                    result.Add(litArticleId.Text);
                }
            }
            return result;
        }

        protected void ButtonDelete_Click(object sender, EventArgs e)
        {
            int deletedCount = 0;
            var list = GetCheckedKeywords();
            foreach (var keyword in list)
            {
                if (contentB.DeleteDictionaryEntry(keyword))
                {
                    deletedCount++;
                }
            }
            if (deletedCount > 0)
            {
                Notifier1.Title = string.Format("Deleted {0} keywords", deletedCount);
                GridViewEntries.DataBind();
            }
        }

        protected void CmdImport_Click(object sender, EventArgs e)
        {
            if (FileUploadImport.HasFile)
            {
                var fileStr = "";

                using (StreamReader reader = new StreamReader(FileUploadImport.PostedFile.InputStream))
                    fileStr = reader.ReadToEnd();

                bool overwrite = RadioButtonListWriteTypes.SelectedValue == "0";

                var doc = new XmlDocument();

                using (StringReader reader = new StringReader(fileStr))
                {
                    bool validFile = false;
                    try
                    {
                        doc.Load(reader);
                        validFile = true;
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                    if (validFile)
                    {
                        int countImport = 0;
                        int countFail = 0;
                        var entryNodeList = doc.GetElementsByTagName("entry");
                        foreach (XmlElement entry in entryNodeList)
                        {
                            var keyword = entry.GetAttribute("keyword");
                            if (!string.IsNullOrEmpty(keyword))
                            {
                                var translationList = entry.GetElementsByTagName("translation");
                                foreach (XmlElement translation in translationList)
                                {
                                    var languageId = FormatTool.GetInteger(translation.GetAttribute("language_id"));
                                    var titleList = translation.GetElementsByTagName("description");
                                    var subtitleList = translation.GetElementsByTagName("subtitle");
                                    var teaserList = translation.GetElementsByTagName("teaser");
                                    var htmlList = translation.GetElementsByTagName("html");

                                    if (languageId > 0 && titleList.Count > 0 && subtitleList.Count > 0 && teaserList.Count > 0 && htmlList.Count > 0)
                                    {
                                        BODictionaryEntry de = contentB.GetDictionaryEntry(keyword, languageId);

                                        if (de == null)
                                        {
                                            // keyword or translation doesn't exist so enter it.
                                            de = new BODictionaryEntry();
                                            de.KeyWord = keyword;
                                            de.Title = titleList[0].InnerText;
                                            de.SubTitle = subtitleList[0].InnerText;
                                            de.Teaser = teaserList[0].InnerText;
                                            de.Html = htmlList[0].InnerText;
                                            de.LanguageId = languageId;
                                            contentB.ChangeDictionaryEntry(de);
                                            countImport++;
                                        }
                                        else
                                        {
                                            // keyword and translation exists... so overwrite if told to do so
                                            if (overwrite)
                                            {
                                                de.Title = titleList[0].InnerText;
                                                de.SubTitle = subtitleList[0].InnerText;
                                                de.Teaser = teaserList[0].InnerText;
                                                de.Html = htmlList[0].InnerText;
                                                contentB.ChangeDictionaryEntry(de);
                                                countImport++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        countFail++;
                                    }
                                }
                            }
                            else
                            {
                                countFail++;
                            }
                        }

                        Notifier1.Message = "$successfully_import_count" + countImport;
                        Notifier1.Message += "<br />" + "$fail_import_count" + countFail;
                    }
                    else
                    {

                        Notifier1.Warning = "$invalid_import_file_or_failed_to_read";
                    }
                }
            }
            else
            {
                Notifier1.Warning = "$import_file_not_selected";
            }
        }

        protected void CmdExport_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.Buffer = false;
            Response.ContentType = "text/xml";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" +
                "$dictionary_export" + DateTime.Now.ToShortDateString() + ".xml\";");

            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(Get().OuterXml.ToString());
            Response.Flush();
            Response.End();
        }

        private XmlDocument Get()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(dec);

            XmlElement dictionary = doc.CreateElement("dictionary");
            doc.AppendChild(dictionary);

            List<List<BODictionaryEntry>> entries = contentB.ListAllDictionaryEntries();

            if (entries.Count > 0)
            {
                foreach (List<BODictionaryEntry> innerList in entries)
                {
                    XmlElement entry = doc.CreateElement("entry");
                    bool keywordSet = false;

                    foreach (BODictionaryEntry e in innerList)
                    {
                        if (!keywordSet)
                        {
                            entry.SetAttribute("keyword", e.KeyWord);
                            keywordSet = true;
                        }

                        XmlElement translation = doc.CreateElement("translation");
                        translation.SetAttribute("language_id", e.LanguageId.ToString());

                        XmlElement title = doc.CreateElement("description");
                        XmlText translationText1 = doc.CreateTextNode(e.Title);
                        title.AppendChild(translationText1);
                        translation.AppendChild(title);

                        XmlElement subTitle = doc.CreateElement("subtitle");
                        XmlText translationText3 = doc.CreateTextNode(e.SubTitle);
                        subTitle.AppendChild(translationText3);
                        translation.AppendChild(subTitle);

                        XmlElement teaser = doc.CreateElement("teaser");
                        XmlText translationText2 = doc.CreateTextNode(e.Teaser);
                        teaser.AppendChild(translationText2);
                        translation.AppendChild(teaser);

                        XmlElement html = doc.CreateElement("html");
                        XmlText translationText4 = doc.CreateTextNode(e.Html);
                        html.AppendChild(translationText4);
                        translation.AppendChild(html);

                        entry.AppendChild(translation);
                    }

                    dictionary.AppendChild(entry);
                }
            }

            return doc;
        }

        protected void LinkButtonKeywords_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
        }

        protected void LinkButtonExport_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 2;
        }
    }
}
