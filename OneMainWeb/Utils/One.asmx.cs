using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using One.Net.BLL;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for One
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class One : System.Web.Services.WebService
    {

        [WebMethod]
        public string ShortenUrl(string url)
        {
            return BRedirects.ShortenUrl(url);
        }
    }
}
