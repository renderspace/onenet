using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

using TwoControlsLibrary;
using One.Net.BLL;
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class ComprehensiveCalendar : MModule
    {
        private static readonly BEvent eventB = new BEvent();
        private enum EventTimes { Today = 0, FromToday, Tomorrow, ThisWeek, NextWeek, ThisMonth, NextMonth, SixMonths }

        protected string CategoryList { get { return GetStringSetting("CategoryList"); } }
        protected string SortByColumn { get { return GetStringSetting("SortByColumn"); } }
        protected bool SortDescending { get { return GetBooleanSetting("SortDescending"); } }
        protected int RecordsPerPage { get { return GetIntegerSetting("RecordsPerPage"); } }

        private BOEvent selectedEvent = null;
        protected BOEvent SelectedEvent
        {
            get { return selectedEvent; }
            set { selectedEvent = value; }
        }

        private string selectedCategoryIds = string.Empty;
        public string SelectedCategoryIds
        {
            get { return selectedCategoryIds; }
            set { selectedCategoryIds = value; }
        }

        private int selectedEventTime = 7;
        protected int SelectedEventTime
        {
            get { return selectedEventTime; }
            set { selectedEventTime = value; }
        }

        private string selectedFilter = string.Empty;
        protected string SelectedFilter
        {
            get { return selectedFilter; }
            set { selectedFilter = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RadioButtonListTimeFrame_DataBind();
                CheckBoxListCategories_DataBind();
                CmdFilter.Text = Translate(CmdFilter.Text);
                InputFilter.Text = Translate(InputFilter.Text);

                LoadControls();
            }
        }

        private void LoadControls()
        {
            PlaceHolderNoItemsInList.Visible = PlaceHolderList.Visible = PlaceHolderSingle.Visible = false;

            if (SelectedEvent != null)
            {
                PlaceHolderSingle.Visible = true;
            }
            else
            {
                DateTime? from = null;
                DateTime? to = null;

                if (SelectedEventTime < 0 || SelectedEventTime > 7)
                    SelectedEventTime = 0;
                EventTimes eventTime = (EventTimes)SelectedEventTime;

                switch (eventTime)
                {
                    case EventTimes.Today:
                        from = DateTime.Today;
                        to = DateTime.Today.AddDays(1).AddMilliseconds(-1);
                        break;
                    case EventTimes.FromToday:
                        {
                            from = DateTime.Today;
                            to = null;
                        }
                        break;
                    case EventTimes.Tomorrow:
                        from = DateTime.Today.AddDays(1);
                        to = DateTime.Today.AddDays(2).AddMilliseconds(-1);
                        break;
                    case EventTimes.ThisWeek:
                        from = DateTime.Today.AddDays(-((int) DateTime.Today.DayOfWeek) + 1);
                        if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                            from = DateTime.Today.AddDays(-6);
                        to = from.Value.AddDays(7).AddMilliseconds(-1);
                        break;
                    case EventTimes.NextWeek:
                        {
                            from = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek) + 1);
                            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                                from = DateTime.Today.AddDays(-6);
                            from = from.Value.AddDays(7);
                            to = from.Value.AddDays(7).AddMilliseconds(-1);
                        } break;
                    case EventTimes.ThisMonth:
                        {
                            from = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                            to = from.Value.AddMonths(1).AddMilliseconds(-1);
                        }
                        break;
                    case EventTimes.NextMonth:
                        {
                            from = DateTime.Today.AddDays(1 - DateTime.Today.Day).AddMonths(1);
                            to = from.Value.AddMonths(1).AddSeconds(-1);
                        }
                        break;
                    case EventTimes.SixMonths:
                        {
                            from = DateTime.Today.AddDays(1 - DateTime.Today.Day);
                            to = from.Value.AddMonths(6).AddMilliseconds(-1);
                        }
                        break;
                }

                string filter = SelectedFilter;

                ListingState listingState = new ListingState(
                    RecordsPerPage,
                    EventPager.FirstRecordIndex,
                    SortDescending ? SortDir.Descending : SortDir.Ascending,
                    SortByColumn
                    );

                PagedList<BOEvent> events = new PagedList<BOEvent>();
                if (!string.IsNullOrEmpty(SelectedCategoryIds))
                    events = eventB.List(SelectedCategoryIds, false, listingState, from, to, null, filter, null);

                if (events.AllRecords > 0)
                {
                    PlaceHolderList.Visible = true;
                    EventPager.TotalRecords = events.AllRecords;
                    EventPager.DetermineData();
                    RepeaterEvents.DataSource = events;
                    RepeaterEvents.DataBind();
                }
                else
                {
                    PlaceHolderNoItemsInList.Visible = true;
                }
            }
        }

        private void RadioButtonListTimeFrame_DataBind()
        {
            RadioButtonListTimeFrame.Items.Clear();
            RadioButtonListTimeFrame.Items.Add(new ListItem(Translate("cc_today"), ((int)EventTimes.Today).ToString()));
            RadioButtonListTimeFrame.Items.Add(new ListItem(Translate("cc_this_week"), ((int)EventTimes.ThisWeek).ToString()));
            RadioButtonListTimeFrame.Items.Add(new ListItem(Translate("cc_next_week"), ((int)EventTimes.NextWeek).ToString()));
            RadioButtonListTimeFrame.Items.Add(new ListItem(Translate("cc_this_month"), ((int)EventTimes.ThisMonth).ToString()));
            RadioButtonListTimeFrame.Items.Add(new ListItem(Translate("cc_six_months"), ((int)EventTimes.SixMonths).ToString()));
            RadioButtonListTimeFrame.SelectedValue = ((int)EventTimes.SixMonths).ToString();
        }

        private void CheckBoxListCategories_DataBind()
        {
            CheckBoxListCategories.DataSource = eventB.ListCategories(BuildCategoriesFilter(CategoryList), false);
            CheckBoxListCategories.DataTextField = "Title";
            CheckBoxListCategories.DataValueField = "Id";
            CheckBoxListCategories.DataBind();

            SelectedCategoryIds = CategoryList;
            string[] cats = CategoryList.Split(',');
            foreach (string cat in cats)
            {
                ListItem item = CheckBoxListCategories.Items.FindByValue(cat.Trim());
                if ( item != null)
                    item.Selected = true;
            }
        }

        protected void CmdShowEvent_Click(object sender, EventArgs e)
        {
            LinkButton CmdShowEvent = sender as LinkButton;
            if ( CmdShowEvent != null )
            {
                int eventId = Int32.Parse(CmdShowEvent.CommandArgument);
                SelectedEvent = eventB.Get(eventId, false);
                LoadControls();
            }
        }

        protected void CmdFilter_Click(object sender, EventArgs e)
        {
            SelectedEvent = null;
            SelectedCategoryIds = string.Empty;

            foreach (ListItem item in CheckBoxListCategories.Items)
            {
                if (item.Selected)
                {
                    if (string.IsNullOrEmpty(SelectedCategoryIds))
                        SelectedCategoryIds = item.Value;
                    else
                        SelectedCategoryIds += "," + item.Value;
                }
            }

            SelectedFilter = InputFilter.Value;
            SelectedEventTime = FormatTool.GetInteger(RadioButtonListTimeFrame.SelectedValue);

            EventPager.SelectedPage = 1;
            EventPager.RecordsPerPage = RecordsPerPage;

            LoadControls();
        }

        public void EventPager_Command(object sender, CommandEventArgs e)
        {
            EventPager.SelectedPage = Convert.ToInt32(e.CommandArgument);
            LoadControls();
        }

        public void RepeaterEvents_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            BOEvent boundEvent = e.Item.DataItem as BOEvent;
            LinkButton CmdShowEvent = e.Item.FindControl("CmdShowEvent") as LinkButton;
            if (boundEvent != null && CmdShowEvent != null)
            {
                if (string.IsNullOrEmpty(boundEvent.Html))
                    CmdShowEvent.Enabled = false;
                else
                    CmdShowEvent.Enabled = true;
            }
        }

        private static List<int> BuildCategoriesFilter(string list)
        {
            if (!string.IsNullOrEmpty(list))
            {
                string[] cats = list.Split(',');
                List<int> catsList = new List<int>();
                foreach (string s in cats)
                {
                    catsList.Add(Int32.Parse(s));
                }
                return catsList;
            }
            else
            {
                return new List<int>();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            object[] cSThis = new object[5];
            object cSBase = base.SaveControlState();

            cSThis[0] = cSBase;
            cSThis[1] = selectedEvent;
            cSThis[2] = selectedEventTime;
            cSThis[3] = selectedFilter;
            cSThis[4] = selectedCategoryIds;

            return cSThis;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] cSThis = (object[])savedState;

            object cSBase = cSThis[0];
            selectedEvent = (BOEvent)cSThis[1];
            selectedEventTime = (int)cSThis[2];
            selectedFilter = (string)cSThis[3];
            selectedCategoryIds = (string)cSThis[4];

            LoadControls();

            base.LoadControlState(cSBase);
        }


    }
}