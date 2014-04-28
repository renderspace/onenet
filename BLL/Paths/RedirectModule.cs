using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Web;
using TwoControlsLibrary;

namespace One.Net.BLL
{
    public class RedirectModule : IHttpModule
    {
        protected static readonly ILog log = LogManager.GetLogger("RedirectHandler");
        public string ModuleName { get { return "RedirectHandler"; } }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += delegate(object sender, EventArgs e)
            {
                var app = (HttpApplication)sender;
                if (app.Context == null) return;

                var redirect = BRedirects.Find(app.Context.Request.RawUrl);
                if ( redirect != null)
                {
                    if (!string.IsNullOrEmpty(redirect.ToLink))
                    {
                        if (app.Request["redirect"] == null)
                        {
                            log.Info(string.Format("Replacing {0} with {1}", redirect.FromLink, redirect.ToLink));
                            var uri = new Uri(redirect.ToLink, UriKind.RelativeOrAbsolute);
                            UrlBuilder builder = null;
                            if (uri.IsAbsoluteUri)
                                builder = new UrlBuilder(redirect.ToLink);
                            else
                            {
                                builder = new UrlBuilder(app.Request.Url);
                                builder.QueryString.Clear();
                                builder.Path = redirect.ToLink;
                                builder = new UrlBuilder(HttpUtility.UrlDecode(builder.ToString()));
                            }
                            builder.QueryString["redirect"] = true.ToString();
                            app.Response.Status = "301 Moved Permanently";
                            app.Response.AddHeader("Location", builder.ToString()); 
                        }
                    }
                }
            };
        }

        public void Dispose() { }
    }
}
