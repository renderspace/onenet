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

        public bool IsInRole(string roleName)
        {
            var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            var role = rm.FindByName(roleName);
            return currentUser.Roles.Where(r => r.RoleId == role.Id).Any();
        }

        public IEnumerable<BOWebSite> ListAllowedWebsites()
        { 
             var webSiteB = new BWebsite();
             var list = webSiteB.List();


            var roles = IdentityManager.ListRoles();
            if (roles.Where(r => r.Name == "admin").Count() == 1)
                return list;

            var websiteroles = new List<string>();
            foreach(var r in roles.Where(r => r.Name.StartsWith("http")))
            {
                websiteroles.Add(r.Name);            
            }

            var result = list.Where(w => !string.IsNullOrWhiteSpace(w.PreviewUrl) && websiteroles.Contains(w.PreviewUrl));
            return result;
        }

        public BOWebSite SelectedWebSite
        {
            get
            {
                var website = ListAllowedWebsites().Where(w => w.Id == SelectedWebSiteId).FirstOrDefault();
                if (website == null)
                {
                    SelectedWebSiteId = int.Parse(ConfigurationManager.AppSettings["WebSiteId"].ToString());
                    SelectedPageId = 0;
                    website = ListAllowedWebsites().Where(w => w.Id == SelectedWebSiteId).FirstOrDefault();
                }
                if (website == null)
                {
                    SelectedWebSiteId = 0;
                }
                else
                {
                    Thread.CurrentThread.CurrentCulture = website.Culture;
                }
                return website;
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
    }
}