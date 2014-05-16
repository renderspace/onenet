using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using log4net;
using One.Net.BLL.Scaffold;

namespace OneMainWeb.Utils
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class BambooOneToManyData : IHttpHandler
    {
        static readonly ILog log = LogManager.GetLogger(typeof(BambooOneToManyData));

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            var sb = new StringBuilder();

            try
            {
                int limit = 10;
                Int32.TryParse(context.Request.QueryString["limit"], out limit);
                var relationId = 0;
                Int32.TryParse(context.Request.QueryString["relationId"], out relationId);
                var query = context.Request.QueryString["q"];

                var tags = Data.GetForeignKeyOptions(relationId, query, limit);
                foreach (var key in tags.Keys)
                {
                    // pipe is used as separator, so let's remove it
                    var value = tags[key].Replace("|", "").Trim();
                    var id = key;
					sb.Append(value).Append("|").Append(id).Append("\n");
                }
            }
            catch (Exception ex)
            {
                log.Error("OneToManyData", ex);
            }
            
            context.Response.Write(sb.ToString());
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
