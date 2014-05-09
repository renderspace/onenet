using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class ArticleSingle : MModule
    {
        protected string ArticleListUri { get { return GetStringSetting("ArticleListUri"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 1;
            Time1.InnerHtml = "bla bla";
            H1Title.InnerHtml = "title";
            H2SubTitle.InnerHtml = "subtitle";
            Time2.Visible = false;
            SectionTeaser.InnerHtml = "teaser teaser teaser teaser teaser teaser teaser teaser ";
            SectionHtml.InnerHtml = "html html html html html html html html html html html ";
            DivReadon.Visible = !string.IsNullOrWhiteSpace(ArticleListUri);
        }
    }
}