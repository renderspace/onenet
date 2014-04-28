using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class ArticleList : MModule
    {

        #region Settings
        protected string RegularsList { get { return GetStringSetting("RegularsList"); } }
        protected string SortByColumn { get { return GetStringSetting("SortByColumn"); } }
        protected bool SortDescending { get { return GetBooleanSetting("SortDescending"); } }
        protected int RecordsPerPage { get { return GetIntegerSetting("RecordsPerPage"); } }
        protected bool ShowPager { get { return GetBooleanSetting("ShowPager"); } }
        protected bool ShowDate { get { return GetBooleanSetting("ShowDate"); } }
        protected bool ShowTitle { get { return GetBooleanSetting("ShowTitle"); } }
        protected bool ShowSubTitle { get { return GetBooleanSetting("ShowSubTitle"); } }
        protected bool ShowTeaser { get { return GetBooleanSetting("ShowTeaser"); } }
        protected bool ShowHtml { get { return GetBooleanSetting("ShowHtml"); } }
        protected string SingleArticleUri { get { return GetStringSetting("SingleArticleUri"); } }
        protected int OffSet { get { return GetIntegerSetting("OffSet"); } }


        #endregion Settings

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}