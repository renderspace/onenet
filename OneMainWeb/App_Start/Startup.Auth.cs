using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using NLog;
using One.Net.BLL;
using Owin;
using System;
using System.Configuration;
using System.Reflection;
using System.Web.Hosting;

namespace OneMainWeb
{

    public partial class Startup {

        protected static Logger log = LogManager.GetCurrentClassLogger();


        private void OnResponseSignIn(CookieResponseSignInContext ctx)
        {
            log.Info("OnResponseSignIn: " + ctx.Identity.IsAuthenticated);
        }

        private void OnApplyRedirect(CookieApplyRedirectContext ctx)
        {
            log.Info("OnApplyRedirect");
        }

        

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301883
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseKentorOwinCookieSaver();


            // Enable the application to use a cookie to store information for the signed in user
            // and also store information about a user logging in with a third party login provider.
            // This is required if your application allows users to login
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                ExpireTimeSpan = TimeSpan.FromMinutes(70),
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login.aspx")
                // CookieSecure = CookieSecureOption.Always
                /*
                 * Provider = new CookieAuthenticationProvider
        {
            OnValidateIdentity = SecurityStampValidator
                .OnValidateIdentity<UserManager, ApplicationUser, int>(
                    validateInterval: TimeSpan.FromMinutes(30),
                    regenerateIdentityCallback: (manager, user) => user.GenerateUserIdentityAsync(manager)
        }
                 * */
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);


            var FacebookAppId = ConfigurationManager.AppSettings["FacebookAppId"];
            var FacebookAppSecret = ConfigurationManager.AppSettings["FacebookAppSecret"];
            if (!string.IsNullOrWhiteSpace(FacebookAppId) && !string.IsNullOrWhiteSpace(FacebookAppSecret))
            {
                app.UseFacebookAuthentication(
                   appId: FacebookAppId,
                   appSecret: FacebookAppSecret);
            }

            var GoogleClientId = ConfigurationManager.AppSettings["GoogleClientId"];
            var GoogleClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];

            if (!string.IsNullOrWhiteSpace(GoogleClientId) && !string.IsNullOrWhiteSpace(GoogleClientSecret))
            {
                var googleOptions = new Microsoft.Owin.Security.Google.GoogleOAuth2AuthenticationOptions
                {

                    ClientId = GoogleClientId,
                    ClientSecret = GoogleClientSecret,
                    CallbackPath = new PathString("/callbacks/google"), // /callbacks/google this is never called by MVC 
                    /*
                    Provider = new GoogleOAuth2AuthenticationProvider
                    {
                        OnAuthenticated = (context) =>
                        {
                            context.Identity.AddClaim(new Claim("picture", context.User.GetValue("picture").ToString()));
                            context.Identity.AddClaim(new Claim("profile", context.User.GetValue("profile").ToString()));
                            return Task.FromResult(0);
                        }

                    }*/
                };

                googleOptions.Scope.Add("email");
                app.UseGoogleAuthentication(googleOptions);
            }
        }
    }
}
