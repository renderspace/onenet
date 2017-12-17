using Microsoft.AspNet.Identity;
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
using System.Security.Cryptography;
using System.Text;

namespace OneMainWeb.Models
{
    // You can add User data for the user by adding more properties to your User class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class OneNetUser : IdentityUser
    {
        public string SelectedWebSiteId { get; set; }
        public string SelectedPageId { get; set; }

        public bool IsGoogleAuthenticatorEnabled { get; set; }

        public string GoogleAuthenticatorSecretKey { get; set; }

        /*
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }*/
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
            this.EmailService = new OneNetEmailService();
            this.PasswordHasher = new SqlPasswordHasher();
        }
    }

    public class OneNetEmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            // convert IdentityMessage to a MailMessage
            var m = new MailMessage();
            m.Subject = message.Subject;
            m.Body = message.Body;
            m.To.Add(message.Destination);
            m.IsBodyHtml = true;

            using (var client = new SmtpClient()) // SmtpClient configuration comes from config file
            {
                await client.SendMailAsync(m);
            }
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

    /// <summary>
    /// This class is used for transitioning from legacy membership provider code to new identity system provider
    /// when migrating passwords from aspnet_Users table to AspNetUsers table.
    /// </summary>
    public class SqlPasswordHasher : PasswordHasher
    {
        public override string HashPassword(string password)
        {
            return base.HashPassword(password);
        }

        public override PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (!string.IsNullOrEmpty(hashedPassword) && !string.IsNullOrEmpty(providedPassword))
            {
                string[] passwordProperties = hashedPassword.Split('|');
                if (passwordProperties.Length != 3)
                {
                    return base.VerifyHashedPassword(hashedPassword, providedPassword);
                }
                else
                {
                    string passwordHash = passwordProperties[0];
                    int passwordformat = 1;
                    string salt = passwordProperties[2];
                    if (String.Equals(EncryptPassword(providedPassword, passwordformat, salt), passwordHash, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return PasswordVerificationResult.SuccessRehashNeeded;
                    }
                    else
                    {
                        return PasswordVerificationResult.Failed;
                    }
                }
            }
            return PasswordVerificationResult.Failed;
        }

        // This is copied from the existing SQL providers and is provided only for back-compat.
        private string EncryptPassword(string pass, int passwordFormat, string salt)
        {
            if (passwordFormat == 0) // MembershipPasswordFormat.Clear
                return pass;

            byte[] bIn = Encoding.Unicode.GetBytes(pass);
            byte[] bSalt = Convert.FromBase64String(salt);
            byte[] bRet = null;

            if (passwordFormat == 1)
            { // MembershipPasswordFormat.Hashed 
                HashAlgorithm hm = HashAlgorithm.Create("SHA1");
                if (hm is KeyedHashAlgorithm)
                {
                    KeyedHashAlgorithm kha = (KeyedHashAlgorithm)hm;
                    if (kha.Key.Length == bSalt.Length)
                    {
                        kha.Key = bSalt;
                    }
                    else if (kha.Key.Length < bSalt.Length)
                    {
                        byte[] bKey = new byte[kha.Key.Length];
                        Buffer.BlockCopy(bSalt, 0, bKey, 0, bKey.Length);
                        kha.Key = bKey;
                    }
                    else
                    {
                        byte[] bKey = new byte[kha.Key.Length];
                        for (int iter = 0; iter < bKey.Length; )
                        {
                            int len = Math.Min(bSalt.Length, bKey.Length - iter);
                            Buffer.BlockCopy(bSalt, 0, bKey, iter, len);
                            iter += len;
                        }
                        kha.Key = bKey;
                    }
                    bRet = kha.ComputeHash(bIn);
                }
                else
                {
                    byte[] bAll = new byte[bSalt.Length + bIn.Length];
                    Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
                    Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
                    bRet = hm.ComputeHash(bAll);
                }
            }

            return Convert.ToBase64String(bRet);
        }
    }
}