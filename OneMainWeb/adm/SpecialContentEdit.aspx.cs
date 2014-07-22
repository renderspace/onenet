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
using System.Threading;
using One.Net.BLL;
using OneMainWeb.AdminControls;


namespace OneMainWeb.adm
{
    public partial class SpecialContentEdit : OneBasePage
    {
        protected override void OnInit(EventArgs e)
        {
            TextSpecialContent1.SelectedWebSiteId = SelectedWebSiteId;
            TextSpecialContent1.SelectedPageId = SelectedPageId;

            TextSpecialContent1.EnableXHTMLValidator = EnableXHTMLValidator;
            TextSpecialContent1.ExpandTree = ExpandTree;
            TextSpecialContent1.SelectedPageId = SelectedPageId;
            TextSpecialContent1.IsSpecialContent = true;
        }  
    }
}
