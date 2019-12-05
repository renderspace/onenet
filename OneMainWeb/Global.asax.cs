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
using NLog;

using System.Web.Routing;
using System.ServiceModel.Activation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


using One.Net.BLL;
using One.Net.BLL.Service;
using Microsoft.AspNet.Identity;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;
using OneMainWeb.Models;
using OneMainWeb.Base;
using System.Collections.Specialized;
using One.Net.BLL.Web;
using System.Data.Entity;

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
        protected static Logger log = LogManager.GetCurrentClassLogger();

        public static IHttpModule Module = new RedirectModule();

        public override void Init()
        {
            base.Init();
            Module.Init(this);
        }    

        protected void Application_Start(object sender, EventArgs e)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            Version version = this.GetType().BaseType.Assembly.GetName().Version;
            log.Info("-------------- One.NET " + version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision + " Application START --------------");
            Database.SetInitializer<ApplicationDbContext>(null);


            TByNumberPathProvider pathProvider = new TByNumberPathProvider();
            System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(pathProvider);

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            serializerSettings.Converters.Add(new IsoDateTimeConverter());

            RouteConfig.ReloadRoutes(RouteTable.Routes);
            log.Info("-------------- MVC ROUTES ADDED --------------");

            SiteMap.SiteMapResolve += new SiteMapResolveEventHandler(Provider_SiteMapResolve);
        }

        SiteMapNode Provider_SiteMapResolve(object sender, SiteMapResolveEventArgs e)
        {
            if (e.Context.CurrentHandler is PresentBasePage)
            {
                var title = ((PresentBasePage)e.Context.CurrentHandler).MenuTitle;
                if (string.IsNullOrWhiteSpace(title))
                    return null;

                title = title.StripHtmlTags();
                if (SiteMap.CurrentNode == null)
                    return null;
                SiteMapNode currentNode = SiteMap.CurrentNode.Clone(true);
                SiteMapNode tempNode = currentNode;
                tempNode.Title = title;
                return tempNode;
            }
            else
                return null;
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
                //if (!(req.Contains("_images") || req.Contains("_files") || req.Contains("/Scripts") || req.Contains("/JavaScript")))
                if (req.Contains(".aspx"))
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
                if (req.Contains(".aspx") || req.EndsWith("/"))
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
                log.Fatal(x, "Application_Error 404: " + Request.Url.ToString());
            }
            else
            {
                log.Fatal(x, "Application_Error");
            }
        }
    }
}