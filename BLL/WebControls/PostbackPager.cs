using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Collections.Generic;

namespace One.Net.BLL.WebControls
{
    public class PostbackPager : WebControl, INamingContainer, IPostBackEventHandler 
    {

        #region PostBack Stuff

        private static readonly object EventCommand = new object();

        public event CommandEventHandler Command
        {
            add { Events.AddHandler(EventCommand, value); }
            remove { Events.RemoveHandler(EventCommand, value); }
        }

        protected virtual void OnCommand(CommandEventArgs e)
        {
            CommandEventHandler clickHandler = (CommandEventHandler)Events[EventCommand];
            if (clickHandler != null) clickHandler(this, e);
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            OnCommand(new CommandEventArgs(this.UniqueID, Convert.ToInt32(eventArgument)));
        }

        #endregion

        public const string REQUEST_PAGE_ID = "pid";

        #region Variables

        // Local variables to hold the page numbers layout & style
        int maxColsPerRow;
        int numPagesShown;
        int selectedPage;
        int totalRecords;
        int recordsPerPage;
        int pageCount;

        string pagerTitle;
        string pagerSubTitle;

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
            if (recordsPerPage == 0)
            {
                pageCount = 0;
                selectedPage = 0;
            }
            else
            {
                pageCount = totalRecords/recordsPerPage;

                if ((totalRecords%recordsPerPage) > 0)
                    pageCount++;

                selectedPage = ((selectedPage > pageCount || selectedPage <= 0) ? 0 : selectedPage);

                if (selectedPage > pageCount)
                    selectedPage = pageCount;
            }
        }

        #region Control's State Management

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object cSBase = base.SaveControlState();
            object[] cSThis = new object[8];

            cSThis[0] = cSBase;
            cSThis[1] = maxColsPerRow;
            cSThis[2] = numPagesShown;
            cSThis[3] = selectedPage;
            cSThis[4] = totalRecords;
            cSThis[5] = recordsPerPage;
            cSThis[6] = pagerTitle;
            cSThis[7] = pagerSubTitle;

            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;
            object cSBase = cSThis[0];
            maxColsPerRow = (int)cSThis[1];
            numPagesShown = (int)cSThis[2];
            selectedPage = (int)cSThis[3];
            totalRecords = (int)cSThis[4];
            recordsPerPage = (int)cSThis[5];
            pagerTitle = (string)cSThis[6];
            pagerSubTitle = (string)cSThis[7];

            DetermineData();

            base.LoadControlState(cSBase);
        }

        #endregion Control's State Management

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "pagination");
            writer.RenderBeginTag("ul");
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            //base.RenderEndTag(writer);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (pageCount > 1)
            {
                int lowerHalf = this.SelectedPage - (numPagesShown / 2);
                int upperHalf = this.SelectedPage + (numPagesShown / 2);

                if (lowerHalf < 1)
                    lowerHalf = 1;
                if (upperHalf > pageCount - 1)
                    upperHalf = pageCount - 1;
                if ( lowerHalf > upperHalf )
                    upperHalf = lowerHalf;

                int fromPage = lowerHalf;
                int toPage = upperHalf;

                if (fromPage > 1)
                {
                    RenderStaticPageLinkSpan(writer, 1, " firststatic");
                    RenderDots(writer, "li", "disabled");
                }

                for (int i = fromPage; i <= toPage; i++)
                {   
                    if (i != SelectedPage)
                    {
                        writer.RenderBeginTag("li");
                        writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, (i).ToString()));
                    }
                    else
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "active");
                        writer.RenderBeginTag("li");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write(i);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                if (toPage < pageCount)
                {
                    RenderDots(writer, "li", "disabled");
                    RenderStaticPageLinkSpan(writer, pageCount, " laststatic");
                }
            }
        }

        private void RenderStaticPageLinkSpan(HtmlTextWriter writer, int page, string cssClass)
        {
            if (page != SelectedPage)
            {
                writer.RenderBeginTag("li");
                writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, (page).ToString()));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "active");
                writer.RenderBeginTag("li");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }

            writer.Write(page);

            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private static void RenderDots(HtmlTextWriter writer, string tag, string cssClass)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
            writer.RenderBeginTag(tag);
            writer.RenderBeginTag("a");
            writer.Write("...");
            writer.RenderEndTag();
            writer.RenderEndTag();   
        }
    }
}
