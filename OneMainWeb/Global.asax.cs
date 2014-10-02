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
using OneMainWeb.Base;
using System.Collections.Specialized;

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
            // RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var publishFlag = PresentBasePage.ReadPublishFlag();
            if (!publishFlag)
            {
                IdentityManager.CreateRoleIfNotExists("admin");
                IdentityManager.CreateRoleIfNotExists("ContentEdit");
                IdentityManager.CreateRoleIfNotExists("all");
                IdentityManager.CreateRoleIfNotExists("Structure");
                IdentityManager.CreateRoleIfNotExists("Publish");
                IdentityManager.CreateRoleIfNotExists("Scaffold");
                IdentityManager.CreateRoleIfNotExists("Forms");
                IdentityManager.CreateRoleIfNotExists("Articles");
                IdentityManager.CreateRoleIfNotExists("Subscriptions");
                IdentityManager.CreateRoleIfNotExists("Dictionary");
                IdentityManager.CreateRoleIfNotExists("FileManager");
                IdentityManager.CreateRoleIfNotExists("Redirects");
                IdentityManager.CreateRoleIfNotExists("Website");

                var websiteB = new BWebsite();
                var list = websiteB.List();
                foreach (var w in list)
                {
                    var previewUrl = w.PreviewUrl;
                    if (!string.IsNullOrWhiteSpace(previewUrl))
                    {
                        IdentityManager.CreateRoleIfNotExists(previewUrl);
                    }
                }
                log.Info("-------------- CreateRoleIfNotExists FINISHED --------------");
            }

            

            //HttpContext ctx = HttpContext.Current;
            //Application["Config"] = WebConfigurationManager.OpenWebConfiguration(ctx.Request.ApplicationPath);

            TByNumberPathProvider pathProvider = new TByNumberPathProvider();
            System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(pathProvider);

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new IsoDateTimeConverter());

            RouteConfig.ReloadRoutes(RouteTable.Routes);
            log.Info("-------------- MVC ROUTES ADDED --------------");
            /*

           //RouteTable.Routes.Add(new Route("Category/{action}/{categoryName}", new One.Net.BLL.Paths.CustomRouteHandler("2col.aspx")));
           
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
                if (!(req.Contains("_images") || req.Contains("_files") || req.Contains("/Scripts") || req.Contains("/JavaScript")))
                {
                    log.Debug("BeginRequest " + req + " / UserAgent: " + Request.UserAgent);
                }
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (log.IsDebugEnabled)
            {
                string req = Request.Url.ToString();
                if (!(req.Contains("_images") || req.Contains("_files") || req.Contains("/Scripts") || req.Contains("/JavaScript")))
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