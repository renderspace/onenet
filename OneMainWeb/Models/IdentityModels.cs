﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System.Web;
using System;
using OneMainWeb.Models;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Web.ModelBinding;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.AspNet.Identity.Owin;
using System.Net.Mail;

namespace OneMainWeb.Models
{
    // You can add User data for the user by adding more properties to your User class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class OneNetUser : IdentityUser
    {
        public string SelectedWebSiteId { get; set; }
        public string SelectedPageId { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<OneNetUser>
    {
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
            
        }
        public DbSet<IdentityUserLogin> Logins { get; set; }

        public List<OneNetUser> GetUsers([Control]string ddlRole)
        {
            var context = new ApplicationDbContext();
            var users = context.Users.Where(x => x.Roles.Select(y => y.RoleId).Contains(ddlRole)).ToList();

            return users;
        }

        /*
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // reference: modelBuilder.Configurations.Add( <EFConfiguration>(new EFConfiguration()); 
        }*/
    }

    public static class IdentityManager
    {
        public static bool RoleExists(string name)
        {
            var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            return rm.RoleExists(name);
        }

        public static bool AddUserToRole(string userName, string roleName)
        {
            var um = new UserManager<OneNetUser>(new UserStore<OneNetUser>(new ApplicationDbContext()));
            var user = um.FindByName(userName);
            var idResult = um.AddToRole(user.Id, roleName);
            return idResult.Succeeded;

        }

        public static void ClearUserRoles(string userName)
        {
            var um = new UserManager<OneNetUser>(new UserStore<OneNetUser>(new ApplicationDbContext()));
            var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            var user = um.FindByName(userName);
            var currentRoles = new List<IdentityUserRole>();
            currentRoles.AddRange(user.Roles);
            foreach (var role in currentRoles)
            {
                var r = rm.FindById(role.RoleId);
                um.RemoveFromRole(user.Id, r.Name);
            }
        }

        public static ICollection<IdentityUserRole> ListUserRoles(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;
            var um = new UserManager<OneNetUser>(new UserStore<OneNetUser>(new ApplicationDbContext()));
            return um.FindByName(userName).Roles;
        }

        public static IEnumerable<OneNetUser> ListUsers()
        {
            var context = new ApplicationDbContext();
            var users = context.Users.ToList();
            return users;
        }

        public static IEnumerable<IdentityRole> ListRoles()
        {
            var context = new ApplicationDbContext();
            var roles = context.Roles.ToList();
            return roles;
        }

        public static void CreateRoleIfNotExists(string roleName)
        {
            var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            var role = rm.FindByName(roleName);

            if (role == null)
            {
                role = new IdentityRole(roleName);
                var roleresult = rm.Create(role);
            }
        }
    }

    public class UserManager : UserManager<OneNetUser> 
    {
        public UserManager() : base(new UserStore<OneNetUser>(new ApplicationDbContext())) 
        {
            var dataProtectionProvider = Startup.DataProtectionProvider;
            this.UserTokenProvider = new DataProtectorTokenProvider<OneNetUser>(dataProtectionProvider.Create("ASP.NET Identity"));
        }
    }
}

namespace OneMainWeb
{
    public static class IdentityHelper
    {
        // Used for XSRF when linking external logins
        public const string XsrfKey = "XsrfId";
        public const string CodeKey = "code";
        public const string UserIdKey = "userId";

        public static void SignIn(UserManager manager, OneNetUser user, bool isPersistent)
        {
            IAuthenticationManager authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = manager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
            authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        public const string ProviderNameKey = "providerName";
        public static string GetProviderNameFromRequest(HttpRequest request)
        {
            return request[ProviderNameKey];
        }

        public static string GetExternalLoginRedirectUrl(string accountProvider)
        {
            return "/Account/RegisterExternalLogin.aspx?" + ProviderNameKey + "=" + accountProvider;
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        public static void RedirectToReturnUrl(string returnUrl, HttpResponse response)
        {
            if (!String.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
            {
                response.Redirect(returnUrl);
            }
            else
            {
                response.Redirect("~/");
            }
        }

        public static string GetResetPasswordRedirectUrl(string code, string userId, HttpRequest request)
        {
            var absoluteUri = "/Account/RecoverPassword.aspx?" + CodeKey + "=" + HttpUtility.UrlEncode(code) + "&" + UserIdKey + "=" + HttpUtility.UrlEncode(userId);
            return new Uri(request.Url, absoluteUri).AbsoluteUri.ToString();
        }
    }
}