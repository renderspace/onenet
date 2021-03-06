﻿using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

using One.Net.BLL;
using One.Net.BLL.Web;

using One.Net.BLL.WebControls;
using One.Net.BLL.Scaffold;
using One.Net.BLL.Model.Attributes;

namespace OneMainWeb.CommonModules
{
    public partial class VirtualTable : MModule
    {
        [Setting(SettingType.Int, DefaultValue = "0")]
        public int VirtualTableId { get { return GetIntegerSetting("VirtualTableId"); } }

        [Setting(SettingType.String)]
        public string ModuleSource { get { return GetStringSetting("ModuleSource"); } }

        [Setting(SettingType.String)]
        public string SortByColumn { get { return GetStringSetting("SortByColumn"); } }

        [Setting(SettingType.Bool, DefaultValue = "true")]
        public bool SortDescending { get { return GetBooleanSetting("SortDescending"); } }

        [Setting(SettingType.Int, DefaultValue = "10")]
        public int RecordsPerPage { get { return GetIntegerSetting("RecordsPerPage"); } }

        [Setting(SettingType.Bool, DefaultValue = "true")]
        public bool ShowPager { get { return GetBooleanSetting("ShowPager"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PostbackPager1.SelectedPage = 1;
                PostbackPager1.RecordsPerPage = RecordsPerPage;
                RepeaterDataBind();
            }
        }

        private void RepeaterDataBind()
        {
            var state = new ListingState
            {
                RecordsPerPage = RecordsPerPage,
                FirstRecordIndex = PostbackPager1.FirstRecordIndex,
                SortDirection = SortDescending ? SortDir.Descending : SortDir.Ascending,
                SortField = SortByColumn
            };

            var items = Data.ListItems(VirtualTableId, state);
            var allRecords = (int)items.ExtendedProperties["AllRecords"];

            PostbackPager1.TotalRecords = allRecords;
            PostbackPager1.DetermineData();
            PostbackPager1.Visible = allRecords > PostbackPager1.RecordsPerPage && ShowPager;

            RepeaterData.DataSource = items.Rows;
            RepeaterData.DataBind();
        }

        public void PostbackPager1_Command(object sender, CommandEventArgs e)
        {
            PostbackPager1.SelectedPage = Convert.ToInt32(e.CommandArgument);
            RepeaterDataBind();
        }
    }
}