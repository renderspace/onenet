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
                chkExpandTree.Checked = ExpandTree;
                CheckBoxEnableDelete.Checked = EnableDelete;
                filemng2.ExpandTree = ExpandTree;
                filemng2.EnableDelete = EnableDelete;
            }
        }

        protected void chkExpandTree_CheckedChanged(object sender, EventArgs e)
        {
            ExpandTree = chkExpandTree.Checked;
            filemng2.ExpandTree = ExpandTree;
            filemng2.LoadControls();
        }

        protected void ChkEnableDelete_CheckedChanged(object seneder, EventArgs e)
        {
            EnableDelete = CheckBoxEnableDelete.Checked;
            filemng2.EnableDelete = EnableDelete;
            filemng2.LoadControls();
            filemng2.fileGrid_ForceDataBind();
        }
    }
}
