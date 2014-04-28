using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web;
using System.IO;
using TwoControlsLibrary;
using System.Web.UI.WebControls;

namespace One.Net.BLL
{
    [Serializable]
    public class BOIntContFlash : BOIntCont
    {
        private bool unitIsPixel;
        public bool UnitIsPixel { get { return unitIsPixel; } set { unitIsPixel = value; } }

        private int width = 0;
        public int Width { get {return width;} set {width=value;} }

        private int height = 0;
        public int Height { get { return height; } set { height = value; } }
        
        private NameValueCollection parameters = new NameValueCollection();
        public NameValueCollection Parameters { get { return parameters; } set { parameters = value; } }

        private NameValueCollection flashVars = new NameValueCollection();
        public NameValueCollection FlashVars { get { return flashVars; } set { flashVars = value; } }

        public string Movie { get; set; }
        public string Id { get; set; }

        public string RenderHtml(int? parentContentId, int idx)
        {
            string clientId = "swfobject_contentid_" + (parentContentId.HasValue ? parentContentId.Value.ToString() : "") + "_idx_" + idx;

            StringBuilder sb = new StringBuilder();

            using (HtmlTextWriter writer = new HtmlTextWriter(new System.IO.StringWriter(sb)))
            {
                SwfObject.RenderHtml(writer, clientId, "", FlashVars, Parameters, null, null, Movie, UnitIsPixel ? Unit.Pixel(Width) : Unit.Percentage(Width), UnitIsPixel ? Unit.Pixel(Height) : Unit.Percentage(Height), 7); 
            }

            return sb.ToString();
        }

        public static string GetFlashVarKey(string raw)
        {
            int index = raw.IndexOf('=');
            return raw.Substring(0, index);
        }

        public static string GetFlashVarValue(string raw)
        {
            int index = raw.IndexOf('=');
            return raw.Substring(index + 1);
        }
    }
}
