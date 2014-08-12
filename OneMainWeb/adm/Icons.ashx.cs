using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using One.Net.BLL;

namespace OneMainWeb.adm
{
    /// <summary>
    /// Summary description for Icons
    /// </summary>
    public class Icons : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Content-Type", "Image/gif");

            var ext = context.Request["extension"];
            var result = BFileSystem.GetFileIcon(ext);
            context.Response.Write(result);
            context.Response.Flush();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}