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

namespace OneMainWeb
{
    public partial class FileManager : OneBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                filemng2.ExpandTree = ExpandTree;
            }
        }

        protected void chkExpandTree_CheckedChanged(object sender, EventArgs e)
        {
            filemng2.ExpandTree = ExpandTree;
            filemng2.LoadControls();
        }
    }
}
