using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Web;
using One.Net.BLL.Utility;

namespace One.Net.BLL.WebControls
{
    public class Pager : WebControl, INamingContainer 
    {
        public const string REQUEST_PAGE_ID = "pid";

        #region Variables

        // Local variables to hold the page numbers layout & style
        int maxColsPerRow;
        int numPagesShown;
        int selectedPage;
        int totalRecords;
        int recordsPerPage;
        int pageCount;

        string containerCssClass;
        string listCssClass;
        string pagerTitle;
        string pagerSubTitle;

        bool showFirstLastLinks;
        bool showPrevNextLinks;

        #endregion Variables

        #region Properties

        /// <summary>
        /// Get/Set the page count;
        /// </summary>
        [Browsable(false), Category("Misc"), DefaultValue(""), Description("Get/Set the page count.")]
        public int PageCount
        {
            get { return pageCount; }
            set { pageCount = value; }
        }
        
        /// <summary>
        /// Get/Set the currently selected page.
        /// </summary>
        [Browsable(false), Category("Misc"), DefaultValue(""), Description("Get/Set the currently selected page.")]
        public int SelectedPage
        {
            get { return selectedPage; }
            set { selectedPage = value; }
        }

        /// <summary>
        /// Get/Set the total number of records.
        /// </summary>
        [Browsable(false), Category("Misc"), DefaultValue(""), Description("Get/Set the max records in grid.")]
        public int TotalRecords
        {
            get { return totalRecords; }
            set { totalRecords = value; }
        }

        /// <summary>
        /// Get/Set the number of records to show per page.
        /// </summary>
        [Browsable(false), Category("Misc"), DefaultValue(""), Description("Get/Set the records per page.")]
        public int RecordsPerPage
        {
            get { return recordsPerPage; }
            set { recordsPerPage = value; }
        }

        /// <summary>
        /// Read only property. returns the zero based index of the first record to be fetched.
        /// </summary>
        [Category("Misc"), Description("Get the zero based index of the first record to be fetched.")]
        public int FirstRecordIndex
        {
            get
            {
                int answer = (selectedPage * recordsPerPage) - recordsPerPage;
                return (answer < 0 ? 0 : answer);
            }
        }

        /// <summary>
        /// Read only property. returns the zero based index of the last record to be fetched.
        /// </summary>
        [Category("Misc"), Description("Get the zero based index of the last record to be fetched.")]
        public int LastRecordIndex
        {
            get
            {
                int answer = (selectedPage * recordsPerPage);
                if (answer >= totalRecords)
                    answer = totalRecords - 1;
                return answer;
            }
        }

        /// <summary>
        /// Show/Hide the first/last links of pager
        /// </summary>
        [Category("One Pager"), DefaultValue("false"), Description("Show/Hide the first/last links of pager")]
        public bool ShowFirstLastLinks
        {
            get { return showFirstLastLinks; }
            set { showFirstLastLinks = value; }
        }

        /// <summary>
        /// Show/Hide the prev/next links of pager
        /// </summary>
        [Category("One Pager"), DefaultValue("false"), Description("Show/Hide the prev/next links of pager")]
        public bool ShowPrevNextLinks
        {
            get { return showPrevNextLinks; }
            set { showPrevNextLinks = value; }
        }

        /// <summary>
        /// Get/Set the CSS class of the container
        /// </summary>
        [Category("One Pager"), DefaultValue(""), Description("Get/Set the CSS class of the container")]
        public string ContainerCssClass
        {
            get { return containerCssClass; }
            set { containerCssClass = value; }
        }

        /// <summary>
        /// Get/Set the CSS class of the list element
        /// </summary>
        [Category("One Pager"), DefaultValue(""), Description("Get/Set the CSS class of the list element")]
        public string ListCssClass
        {
            get { return listCssClass; }
            set { listCssClass = value; }
        }

        /// <summary>
        /// Get/Set the pager title
        /// </summary>
        [Category("One Pager"), DefaultValue(""), Description("Get/Set the pager title")]
        public string PagerTitle
        {
            get { return pagerTitle; }
            set { pagerTitle = value; }
        }

        /// <summary>
        /// Get/Set the pager sub title
        /// </summary>
        [Category("One Pager"), DefaultValue(""), Description("Get/Set the pager subtitle")]
        public string PagerSubTitle
        {
            get { return pagerSubTitle; }
            set { pagerSubTitle = value; }
        }

        /// <summary>
        /// Get/Set the numbers of columns to use in each row(0 - unlimited).
        /// </summary>
        [Category("One Pager"), DefaultValue(""), Description("Get/Set the numbers of columns to use in each row(0 - unlimited).")]
        public int MaxColsPerRow
        {
            get { return maxColsPerRow; }
            set { maxColsPerRow = value; }
        }

        /// <summary>
        /// Get/Set the maximum page numbers to display in the pager
        /// </summary>				
        [Category("One Pager"), DefaultValue(""), Description("Get/Set the maximum page numbers to display in the pager.")]
        public int NumPagesShown
        {
            get { return numPagesShown; }
            set { numPagesShown = value; }
        }

        #endregion Properties

        public void DetermineData()
        {
            try
            {
                pageCount = totalRecords / recordsPerPage;

                if ((totalRecords % recordsPerPage) > 0)
                {
                    pageCount++;
                }

                selectedPage = ((selectedPage > pageCount || selectedPage <= 0) ? 0 : selectedPage);

                if (selectedPage > pageCount)
                {
                    selectedPage = pageCount;
                }
            }
            catch
            {
                pageCount = 0;
                selectedPage = 0;
            }
        }

        #region Control's State Management

        protected override void  OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
 	        base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object cSBase = base.SaveControlState();
            object[] cSThis = new object[12];

            cSThis[0] = cSBase;
            cSThis[1] = maxColsPerRow;
            cSThis[2] = numPagesShown;
            cSThis[3] = selectedPage;
            cSThis[4] = totalRecords;
            cSThis[5] = recordsPerPage;
            cSThis[6] = containerCssClass;
            cSThis[7] = pagerTitle;
            cSThis[8] = pagerSubTitle;
            cSThis[9] = listCssClass;
            cSThis[10] = showFirstLastLinks;
            cSThis[11] = showPrevNextLinks;

            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];

            cSBase = cSThis[0];
            maxColsPerRow = (int)cSThis[1];
            numPagesShown = (int)cSThis[2];
            selectedPage = (int)cSThis[3];
            totalRecords = (int)cSThis[4];
            recordsPerPage = (int)cSThis[5];
            containerCssClass = (string)cSThis[6];
            pagerTitle = (string)cSThis[7];
            pagerSubTitle = (string)cSThis[8];
            listCssClass = (string)cSThis[9];
            showFirstLastLinks = (bool)cSThis[10];
            showPrevNextLinks = (bool)cSThis[11];

            DetermineData();

            base.LoadControlState(cSBase);
        }

        #endregion Control's State Management

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, string.IsNullOrEmpty(containerCssClass) ? "pager" : containerCssClass);
            writer.RenderBeginTag("section");
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (pageCount > 1)
            {
                int lowerHalf = this.SelectedPage - (numPagesShown / 2);
                int upperHalf = this.SelectedPage + (numPagesShown / 2);

                if (lowerHalf < 1)
                    lowerHalf = 1;
                if (upperHalf > pageCount)
                    upperHalf = pageCount;
                if (upperHalf < numPagesShown && numPagesShown < pageCount)
                    upperHalf = numPagesShown;

                int fromPage = lowerHalf;
                int toPage = upperHalf;

                writer.AddAttribute(HtmlTextWriterAttribute.Class, string.IsNullOrEmpty(listCssClass) ? "" : listCssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                if (ShowFirstLastLinks && pageCount > 1)
                {
                    var pagerUrlBuilder = new UrlBuilder(this.Page.Request.Url.AbsoluteUri);
                    pagerUrlBuilder.QueryString.Remove(REQUEST_PAGE_ID + base.ID);
                    // remove the previous key/value for this pager from any newly created uris
                    pagerUrlBuilder.QueryString.Add(REQUEST_PAGE_ID + base.ID, "1");

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "first");
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, pagerUrlBuilder.ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write("&laquo;");
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                if (ShowPrevNextLinks && pageCount > 1 && SelectedPage > 1)
                {
                    var pagerUrlBuilder = new UrlBuilder(this.Page.Request.Url.AbsoluteUri);
                    pagerUrlBuilder.QueryString.Remove(REQUEST_PAGE_ID + base.ID);
                    // remove the previous key/value for this pager from any newly created uris
                    pagerUrlBuilder.QueryString.Add(REQUEST_PAGE_ID + base.ID, (SelectedPage - 1).ToString());

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "previous");
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, pagerUrlBuilder.ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write("&laquo;");
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                for (int i = fromPage; i <= toPage; i++)
                {
                    string cssClassBuilder = "";

                    if (!ShowFirstLastLinks)
                    {
                        if (i == fromPage)
                        {
                            cssClassBuilder = " first";
                        }
                        if (i == toPage)
                        {
                            cssClassBuilder = " last";
                        }
                    }

                    var pagerUrlBuilder = new UrlBuilder(this.Page.Request.Url.AbsoluteUri);
                    pagerUrlBuilder.QueryString.Remove(REQUEST_PAGE_ID + base.ID);
                    // remove the previous key/value for this pager from any newly created uris
                    pagerUrlBuilder.QueryString.Add(REQUEST_PAGE_ID + base.ID, i.ToString());

                    if (i != SelectedPage)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "" + cssClassBuilder);
                        writer.RenderBeginTag(HtmlTextWriterTag.Li);
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, pagerUrlBuilder.ToString());
                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                    }
                    if (i == SelectedPage)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "current" + cssClassBuilder);
                        writer.RenderBeginTag(HtmlTextWriterTag.Li);
                        writer.RenderBeginTag(HtmlTextWriterTag.Em);
                    }

                    writer.Write(i);
                    if (i == SelectedPage)
                    {
                        writer.RenderEndTag();
                    }    

                    writer.RenderEndTag();

                    if (i != SelectedPage)
                    {
                        writer.RenderEndTag();
                    }
                }
                
                if (ShowPrevNextLinks && pageCount > 1 && SelectedPage < pageCount)
                {
                    var pagerUrlBuilder = new UrlBuilder(this.Page.Request.Url.AbsoluteUri);
                    pagerUrlBuilder.QueryString.Remove(REQUEST_PAGE_ID + base.ID);
                    // remove the previous key/value for this pager from any newly created uris
                    pagerUrlBuilder.QueryString.Add(REQUEST_PAGE_ID + base.ID, (SelectedPage + 1).ToString());

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "next");
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, pagerUrlBuilder.ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write("&raquo;");
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                if (ShowFirstLastLinks && pageCount > 1)
                {
                    var pagerUrlBuilder = new UrlBuilder(this.Page.Request.Url.AbsoluteUri);
                    pagerUrlBuilder.QueryString.Remove(REQUEST_PAGE_ID + base.ID);
                    // remove the previous key/value for this pager from any newly created uris
                    pagerUrlBuilder.QueryString.Add(REQUEST_PAGE_ID + base.ID, pageCount.ToString());

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "last");
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, pagerUrlBuilder.ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write("&raquo;");
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                /*
                 * 
                 * <li class="first"><span>&laquo;</span></li>	
		        <li class="previous"><span>&lsaquo;</span></li>		

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "next");
                writer.RenderBeginTag(HtmlTextWriterTag.Li);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, "");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write("&rsaquo;");
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "last");
                writer.RenderBeginTag(HtmlTextWriterTag.Li);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, "");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write("&raquo;");
                writer.RenderEndTag(); */

                writer.RenderEndTag();
                
            }
        }
    }
}
