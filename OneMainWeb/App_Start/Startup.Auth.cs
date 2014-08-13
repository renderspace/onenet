using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Configuration;

namespace OneMainWeb
{
    public partial class Startup {

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301883
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            // and also store information about a user logging in with a third party login provider.
            // This is required if your application allows users to login
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login.aspx")
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");


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
