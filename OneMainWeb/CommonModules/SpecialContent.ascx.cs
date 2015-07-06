using System;
using System.Data;
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
using One.Net.BLL.Model.Attributes;

namespace OneMainWeb.CommonModules
{
    public partial class SpecialContent : MModule
    {
        private static readonly BTextContent specialContentB = new BTextContent();

        BOInternalContent specialContent;

        [Setting(SettingType.Int, DefaultValue = "-1", Visibility = SettingVisibility.SPECIAL)]
        public int ContentId { get { throw new Exception("Not intended to be used directly"); } }

        protected override void OnLoad(EventArgs e)
        {
            specialContent = specialContentB.GetTextContent(InstanceId);
            base.OnLoad(e);
        }

        protected override void Render(HtmlTextWriter output)
        {
            if (specialContent != null && specialContent.IsComplete)
            {
                if (specialContent.ProcessedHtml.Length > 0)
                {
                    output.Write(specialContent.ProcessedHtml);
                }
            }
        }
    }
}