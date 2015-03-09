using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using One.Net.BLL;
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class TemplateContent : MModule
    {
        protected int TemplateId { get { return GetIntegerSetting("TemplateId"); } }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}