using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

//using MsSqlDBUtility;
using System.Web.Configuration;
using log4net;

using System.Web.Routing;
using System.ServiceModel.Activation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web.Http;

using One.Net.BLL;
using One.Net.BLL.Service;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Web.Optimization;
using Microsoft.AspNet.Identity.EntityFramework;
using OneMainWeb.Models;

namespace OneMainWeb
{
    public class Global : System.Web.HttpApplication
    {
        /*
        Application_Init and Application_Start
        Session_Start and Session_End
           * Application_BeginRequest
           * Application_AuthenticateRequest
           * Application_AuthorizeRequest
           * Application_ResolveRequestCache
           * Application_AcquireRequestState
           * Application_PreRequestHandlerExecute
           * Application_PreSendRequestHeaders
           * Application_PreSendRequestContent
           * <<code is executed>>
           * Application_PostRequestHandlerExecute
           * Application_ReleaseRequestState
           * Application_UpdateRequestCache
           * Application_EndRequest
       Application_Disposed and Application_End 
       */
        private readonly ILog log = LogManager.GetLogger("Global");
        //		private static string LOG_SOURCE = ConfigurationManager.AppSettings["Event Log Source"];

        public static IHttpModule Module = new RedirectModule();

        public override void Init()
        {
            base.Init();
            Module.Init(this);
        }    

        protected void Application_Start(object sender, EventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            Version version = this.GetType().BaseType.Assembly.GetName().Version;
            log.Info("-------------- One.NET " + version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision + " Application START --------------");

            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            /*
            var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            if (!rm.RoleExists("admin"))
            {
                rm.Create(new IdentityRole("admin"));
            } */



            //HttpContext ctx = HttpContext.Current;
            //Application["Config"] = WebConfigurationManager.OpenWebConfiguration(ctx.Request.ApplicationPath);

            TByNumberPathProvider pathProvider = new TByNumberPathProvider();
            System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(pathProvider);

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new IsoDateTimeConverter());

            
            RouteTable.Routes.Add(new ServiceRoute("FormService", new WebServiceHostFactory(), typeof(FormService)));

            //RouteTable.Routes.Add(new Route("Category/{action}/{categoryName}", new One.Net.BLL.Paths.CustomRouteHandler("2col.aspx")));
            /*
            RouteTable.Routes.MapPageRoute("EvalRoutes", "Evals/{type}/New.aspx", "~/spored", false);

            RouteTable.Routes.MapPageRoute("EvalRoutes2", "mijav", "~/Login.aspx");*/

            
        }

        protected void Application_End(object sender, EventArgs e)
        {
            log.Info("-------------- Application SHUTDOWN --------------");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (log.IsDebugEnabled)
            {
                string req = Request.Url.ToString();
                if (!(req.Contains("WebResource.axd") || req.Contains("_images") || req.Contains("_files")))
                {
                    log.Debug("BeginRequest " + req);
                }
                if (req.Contains(".asmx"))
                {
                    log.Debug("asmx Request " + req);
                    long position = Request.InputStream.Position;
                    Request.InputStream.Seek(0, SeekOrigin.Begin);

                    log.Debug("asmx Request Full read: " + Encoding.Default.GetString(ReadFully(Request.InputStream)));
                    Request.InputStream.Position = position;
                }
            }
        }


        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (log.IsDebugEnabled)
            {
                string req = Request.Url.ToString();
                if (!(req.Contains("WebResource.axd") || req.Contains("_images") || req.Contains("_files")))
                {
                    log.Debug("EndRequest " + req);
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception lastError = Server.GetLastError();
            Exception x = lastError.GetBaseException();

            if (x.Message.Contains("File does not exist"))
            {
                log.Fatal("Application_Error 404: " + Request.Url.ToString() , x);
            }
            else
            {
                log.Fatal("Application_Error", x);
            }
        }
    }
}