using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace One.Net.Forms.Controls
{
    /// <summary>
    /// Summary description for ProgressBar.
    /// </summary>
    public class ProgressBar : WebControl
    {
        private int percentage = 0;
        private string parentCssClass;
        private string childCssClass;

        public int Percentage
        {
            get { return percentage; }
            set { percentage = Math.Max(0, Math.Min(value, 100)); }
        }

        public string OuterCssClass
        {
            get { return parentCssClass; }
            set { parentCssClass = value; }
        }

        public string InnerCssClass
        {
            get { return childCssClass; }
            set { childCssClass = value; }
        }

        protected override void Render(HtmlTextWriter output)
        {
            string percentageBar = string.Empty;
            if (Percentage == 0)
                percentageBar = "<div class=\"" + OuterCssClass + "\"></div>";
            else
                percentageBar = "<div class=\"" + OuterCssClass + "\">" +
                                    "<div class=\"" + InnerCssClass + "\" style=\"width: " + Percentage.ToString() + "%; \"></div>" +
                                "</div>";

            output.Write(percentageBar);
        }
    }
}
