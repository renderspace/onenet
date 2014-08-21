using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using One.Net.BLL;
using One.Net.BLL.Web;

namespace OneMainWeb.CommonModules
{
    public partial class BCarousel : MModule
    {
        public BCarousel()
        {
            ExtraAttributes = new Dictionary<string, string>();
            ExtraAttributes.Add("data-ride", "carousel");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var fileB = new BFileSystem();
            var files = fileB.List(GetIntegerSetting("FolderId")).Where(f => f.IsImage);

            RepeaterImages.DataSource = files;
            RepeaterImages.DataBind();
            RepeaterButtons.DataSource = files;
            RepeaterButtons.DataBind();
        }

        protected void RepeaterImages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var file = e.Item.DataItem as BOFile;
            if (file != null)
            {
                var LiteralImage = e.Item.FindControl("LiteralImage") as Literal;
                var css = e.Item.ItemIndex == 0 ? "class=\"active item\"" : "class=\"item\"";
                LiteralImage.Text = "<div " + css + "><img src=\"" + file.RelativeUrl + "\" alt=\"" + file.EncodedAlt + "\" >";
                if (file.Content != null)
                {

                    LiteralImage.Text += "<div class=\"carousel-caption\">";
                    if (!string.IsNullOrWhiteSpace(file.Content.Title))
                    {
                        LiteralImage.Text += "<h3>" + file.Content.Title + "</h3>";
                    }
                    if (!string.IsNullOrWhiteSpace(file.Content.Teaser))
                    {
                        LiteralImage.Text += "<p>" + file.Content.Teaser + "</p>";
                    }
                    LiteralImage.Text += "</div>";
                }
               LiteralImage.Text += "</div>";
            }
            
        }

        protected void RepeaterButtons_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var LiteralNavig = e.Item.FindControl("LiteralNavig") as Literal;
            var css = e.Item.ItemIndex == 0 ? "class=\"active\"" : "";
            LiteralNavig.Text = "<li data-target=\"#" + CustomClientID + "\" data-slide-to=\"" + e.Item.ItemIndex + "\" " + css + "></li>";
        }
    }
}