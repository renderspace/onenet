using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using One.Net.BLL;
using OneMainWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;

namespace OneMainWeb.adm
{
    public class AuthorizationHelper
    {
        UserManager<OneNetUser> manager = null;
        OneNetUser currentUser = null;

        protected HttpContext Context { get; set; }
        protected HttpSessionState Session { get { return Context.Session; } }

        public AuthorizationHelper(HttpContext context)
        {
            Context = context;
            manager = new UserManager<OneNetUser>(new UserStore<OneNetUser>(new ApplicationDbContext()));
            currentUser = manager.FindById(Thread.CurrentPrincipal.Identity.GetUserId());
        }

        public int SelectedPageId
        {
            get
            {
                if (Session["SelectedPageId"] == null && currentUser != null)
                {
                        Session["SelectedPageId"] = currentUser.SelectedPageId;
                }
                var r = 0;
                if (Session["SelectedPageId"] == null ||
                    string.IsNullOrWhiteSpace(Session["SelectedPageId"].ToString()) ||
                    !int.TryParse(Session["SelectedPageId"].ToString(), out r))
                {
                    Session["SelectedPageId"] = "0";
                }
                return int.Parse(Session["SelectedPageId"].ToString());
            }
            set
            {
                Session["SelectedPageId"] = value;
                if (currentUser != null)
                {
                    currentUser.SelectedPageId = value.ToString();
                    manager.Update(currentUser);
                }
            }
        }

        public int SelectedWebSiteId
        {
            get
            {
                if (Session["SelectedWebSiteId"] == null && currentUser != null)
                {
                    Session["SelectedWebSiteId"] = currentUser.SelectedWebSiteId;
                }
                var r = 0;
                if (Session["SelectedWebSiteId"] == null ||
                    string.IsNullOrWhiteSpace(Session["SelectedWebSiteId"].ToString()) ||
                    !int.TryParse(Session["SelectedWebSiteId"].ToString(), out r))
                {
                    Session["SelectedWebSiteId"] = Int32.Parse(ConfigurationManager.AppSettings["WebSiteId"].ToString());
                }
                r = int.Parse(Session["SelectedWebSiteId"].ToString());
                CheckWebSiteRole(r);
                return r;
            }
            set
            {
                Session["SelectedWebSiteId"] = value.ToString();
                if (currentUser != null)
                {
                    currentUser.SelectedWebSiteId = value.ToString();
                    manager.Update(currentUser);
                }
            }
        }

        public bool HasPublishRights
        {
            get
            {
                bool publishRole = manager != null ? manager.IsInRole(Thread.CurrentPrincipal.Identity.GetUserId(), "publisher") : Roles.IsUserInRole("publisher");
                return publishRole;
            }
        }

        private void CheckWebSiteRole(int webSiteId)
        {
            var webSiteB = new BWebsite();
            // the following code is used to fix specific issue where web.config specified website is 
            // loaded on first load to users that may not have permissions to view this website.
            var website = webSiteB.Get(webSiteId);
            if (website != null)
            {
                Thread.CurrentThread.CurrentCulture = website.Culture;
                /*
                AuthenticationSection authenticationSection = ConfigurationManager.GetSection("system.web/authentication") as AuthenticationSection;
                AuthenticationMode currentMode = AuthenticationMode.Windows;
                if (authenticationSection != null)
                    currentMode = authenticationSection.Mode;

                bool adminRole = manager != null ? manager.IsInRole(Thread.CurrentPrincipal.Identity.GetUserId(), "admin") : Roles.IsUserInRole("admin");
                if (!(adminRole || currentMode == AuthenticationMode.Windows || Roles.IsUserInRole("website_" + website.Title.ToLower())))
                {
                    // website with id SelectedWebSiteId is not accessible to user - so use another website id.
                    List<BOWebSite> webSiteList = webSiteB.List();
                    foreach (BOWebSite webSite2 in webSiteList)
                    {
                        if (Roles.IsUserInRole("website_" + webSite2.Title.ToLower()))
                        {
                            SelectedWebSiteId = webSite2.Id;
                            break;
                        }
                    }
                }*/
            }
        }
    }
}