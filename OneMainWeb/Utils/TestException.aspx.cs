using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.Utils
{
    public partial class TestException : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            throw new Exception("test");
        }
    }
}