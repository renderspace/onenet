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

        public enum DisplayTypes { Span = 0, DefinitionList = 1 }
        public const string REQUEST_PAGE_ID = "pid";

        #region Variables

        // Local variables to hold the page numbers layout & style
        int maxColsPerRow;
        int numPagesShown;
        int selectedPage;
        int totalRecords;
        int recordsPerPage;
        int pageCount;

        string containerClass;
        string pagerTitle;
        string pagerSubTitle;

        DisplayTypes displayType;

        #endregion Variables

        #region Properties

        /// <summary>
        /// Get/Set Display Type Of Page (span, dl, ...)
        /// </summary>
        [Category("Misc"), DefaultValue(DisplayTypes.Span), Description("Get/Set Display Type Of Page (span, dl, ...)")]
        public DisplayTypes DisplayType
        {
            get { return displayType; }
            set { displayType = (DisplayTypes)Enum.Parse(typeof(DisplayTypes), value.ToString()); }
        }

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
        /// Get/Set the CSS class of the container
        /// </summary>
        [Category("One Pager"), DefaultValue(""), Description("Get/Set the CSS class of the container")]
        public string ContainerCssClass
        {
            get { return containerClass; }
            set { containerClass = value; }
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
            object[] cSThis = new object[10];

            cSThis[0] = cSBase;
            cSThis[1] = maxColsPerRow;
            cSThis[2] = numPagesShown;
            cSThis[3] = selectedPage;
            cSThis[4] = totalRecords;
            cSThis[5] = recordsPerPage;
            cSThis[6] = containerClass;
            cSThis[7] = pagerTitle;
            cSThis[8] = pagerSubTitle;
            cSThis[9] = displayType;

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
            containerClass = (string)cSThis[6];
            pagerTitle = (string)cSThis[7];
            pagerSubTitle = (string)cSThis[8];
            displayType = (DisplayTypes)Enum.Parse(typeof(DisplayTypes), cSThis[9].ToString());

            DetermineData();

            base.LoadControlState(cSBase);
        }

        #endregion Control's State Management

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, string.IsNullOrEmpty(containerClass) ? "pager" : containerClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            //base.RenderBeginTag(writer);
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

                if (lowerHalf < 2)
                    lowerHalf = 2;
                if (upperHalf > pageCount - 1)
                    upperHalf = pageCount - 1;
                if ( lowerHalf > upperHalf )
                    upperHalf = lowerHalf;

                int fromPage = lowerHalf;
                int toPage = upperHalf;

                if (DisplayType == DisplayTypes.Span)
                {
                    RenderPrevNextButtonSpan(writer, SelectedPage - 1, "prev", true); // render prev button

                    RenderStaticPageLinkSpan(writer, 1, " firststatic");

                    RenderDots(writer, HtmlTextWriterTag.Span, "dots");

                    for (int i = fromPage; i <= toPage; i++)
                    {
                        string cssClassBuilder = "";
                        if (i == fromPage)
                        {
                            cssClassBuilder = " first";
                        }
                        if (i == toPage)
                        {
                            cssClassBuilder = " last";
                        }

                        if (i != SelectedPage)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "norp" + cssClassBuilder);
                            writer.RenderBeginTag(HtmlTextWriterTag.Span);
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, (i).ToString()));
                            writer.RenderBeginTag(HtmlTextWriterTag.A);
                        }
                        else
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "selp" + cssClassBuilder);
                            writer.RenderBeginTag(HtmlTextWriterTag.Span);
                        }

                        writer.Write(i);

                        writer.RenderEndTag();

                        if (i != SelectedPage)
                        {
                            writer.RenderEndTag();
                        }
                    }

                    if (toPage < pageCount)
                    {
                        RenderDots(writer, HtmlTextWriterTag.Span, "dots");
                        RenderStaticPageLinkSpan(writer, pageCount, " laststatic");
                    }

                    RenderPrevNextButtonSpan(writer, SelectedPage + 1, "next", false); // render next button
                }
                else if (DisplayType == DisplayTypes.DefinitionList)
                {
                    if (!string.IsNullOrEmpty(containerClass))
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, ContainerCssClass);
                    writer.RenderBeginTag(HtmlTextWriterTag.Dl);

                    if (!string.IsNullOrEmpty(PagerTitle))
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Dt);
                        writer.Write(PagerTitle);
                        writer.RenderEndTag();
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Dd);
                    writer.RenderBeginTag(HtmlTextWriterTag.P);

                    if (!string.IsNullOrEmpty(PagerSubTitle))
                        writer.Write(PagerSubTitle);

                    RenderPrevNextButtonDlElement(writer, selectedPage - 1, true);

                    RenderStaticPageLinkDlElement(writer, 1);

                    RenderDots(writer, HtmlTextWriterTag.Strong, "dots");

                    for (int i = fromPage; i <= toPage; i++)
                    {
                        if (i != SelectedPage)
                        {
                            writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, (i).ToString()));
                            writer.RenderBeginTag(HtmlTextWriterTag.A);
                        }
                        else if (i == SelectedPage)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Strong);
                        }

                        writer.Write(i);
                        writer.RenderEndTag();
                        writer.Write(" | ");
                    }

                    if (toPage < pageCount)
                    {
                        RenderDots(writer, HtmlTextWriterTag.Strong, "dots");
                        RenderStaticPageLinkDlElement(writer, pageCount);
                    }

                    RenderPrevNextButtonDlElement(writer, selectedPage + 1, false);

                    writer.RenderEndTag(); // </p>
                    writer.RenderEndTag(); // </dd>
                    writer.RenderEndTag(); // </dl>                    
                }
            }
        }

        private void RenderPrevNextButtonDlElement(HtmlTextWriter writer, int page, bool isPrev)
        {
            if (page < 1)
                page = 1;
            else if (page > pageCount)
                page = pageCount;

            if ((selectedPage > 1 && isPrev) || (selectedPage < pageCount && !isPrev))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href,
                                    Page.ClientScript.GetPostBackClientHyperlink(this, (page).ToString()));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }

            if ((selectedPage > 1 && isPrev) || (selectedPage < pageCount && !isPrev))
            {
                writer.RenderEndTag();
            }
        }

        private void RenderPrevNextButtonSpan(HtmlTextWriter writer, int page, string cssClass, bool isPrev)
        {
            if (page < 1)
                page = 1;
            else if (page > pageCount)
                page = pageCount;

            writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            if ((selectedPage > 1 && isPrev) || (selectedPage < pageCount && !isPrev))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href,
                                    Page.ClientScript.GetPostBackClientHyperlink(this, (page).ToString()));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }

            if ((selectedPage > 1 && isPrev) || (selectedPage < pageCount && !isPrev))
            {
                writer.RenderEndTag();
            }
            writer.RenderEndTag();
        }

        private void RenderStaticPageLinkSpan(HtmlTextWriter writer, int page, string cssClass)
        {
            if (page != SelectedPage)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "norp" + cssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, (page).ToString()));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "selp" + cssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
            }

            writer.Write(page);

            writer.RenderEndTag();

            if (page != SelectedPage)
            {
                writer.RenderEndTag();
            }            
        }

        private void RenderStaticPageLinkDlElement(HtmlTextWriter writer, int page)
        {
            if (page != SelectedPage)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink(this, (page).ToString()));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
            }
            else
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Strong);
            }

            writer.Write(page);
            writer.RenderEndTag();            
        }

        private static void RenderDots(HtmlTextWriter writer, HtmlTextWriterTag tag, string cssClass)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
            writer.RenderBeginTag(tag);
            writer.Write("...");
            writer.RenderEndTag();            
        }
    }
}
