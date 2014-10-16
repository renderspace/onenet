using One.Net.BLL;
using One.Net.BLL.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OneMainWeb.CommonModules
{
    public partial class MagnificGallery : MModule
    {

        protected BOImageTemplate ThumbTemplate { get { return GetImageTemplate("ThumbTemplate"); } }

        protected BOImageTemplate ImageTemplate { get { return GetImageTemplate("ImageTemplate"); } }

        protected int FolderId { get { return GetIntegerSetting("FolderId"); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            var fileB = new BFileSystem();
            var files = fileB.List(FolderId).Where(f => f.IsImage);
            LiteralMessage.Text = "";
            if (ThumbTemplate != null && ThumbTemplate != null)
            {
                RepeaterImages.DataSource = files;
                RepeaterImages.DataBind();
            }
            else
            {
                LiteralMessage.Text = "ThumbTemplate and/or ImageTemplate must be defined.";
            }
        }

        protected void RepeaterImages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var file = e.Item.DataItem as BOFile;
            if (file != null)
            {
                var LiteralImageTag = e.Item.FindControl("LiteralImageTag") as Literal;
                LiteralImageTag.Text = "<a href=\"" + ImageTemplate.RenderImageLink(file.RelativeUrl) + "\">" + ThumbTemplate.RenderHtml(file.Alt, file.RelativeUrl, "") + "</a>";

                var LiteralCaption = e.Item.FindControl("LiteralCaption") as Literal;

                if (file.Content != null && LiteralCaption != null)
                {
                    LiteralCaption.Text = file.Content.Teaser;
                }
            }
        }
    }
}