using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace One.Net.BLL.Paths
{
    public class DownloadHandler : IHttpHandler
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        private bool IsDownload = false;

        public void ProcessRequest(HttpContext context)
        {
            var virtualPath = context.Request.Path;

            if (context.Request.Params["d"] != null)
                IsDownload = FormatTool.GetBoolean(context.Request.Params["d"]);

            try
            {
                using (Stream iStream = VirtualPathProvider.OpenFile(virtualPath))
                {
                    TImageHandler.WriteStream(iStream, context, virtualPath, IsDownload);
                }
            }
            catch (DirectoryNotFoundException ioex)
            {
                log.Error("Directory not found: " + virtualPath, ioex);
                log.Debug("UrlReferrer: " + context.Request.UrlReferrer);
                context.Response.AddHeader("Content-Type", "text/html");
                context.Response.StatusCode = 404;
                context.Response.Write("<html><body><h1 style=\"color: Red;\">404:  Directory Not Found</h1><h2>DownloadHandler</h2></body></html>");
            }
            catch (FileNotFoundException ioex)
            {
                log.Error("File not found: " + virtualPath, ioex);
                log.Debug("UrlReferrer: " + context.Request.UrlReferrer);
                context.Response.AddHeader("Content-Type", "text/html");
                context.Response.StatusCode = 404;
                context.Response.Write("<html><body><h1 style=\"color: Red;\">404:  File Not Found</h1><h2>DownloadHandler</h2></body></html>");
            }
            catch (Exception ex)
            {
                log.Fatal("Uknown error: " + virtualPath, ex);
                log.Fatal("Url: " + context.Request.Url.ToString());
                log.Fatal("UrlReferrer: " + context.Request.UrlReferrer);
                context.Response.AddHeader("Content-Type", "text/html");
                context.Response.StatusCode = 500;
                context.Response.Write("<html><body><h1 style=\"color: Red;\">500:  Unkown internal server error.</h1><h2>DownloadHandler</h2></body></html>");
            }
            context.Response.End();
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
