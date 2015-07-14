using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OneMainWeb.Startup))]
namespace OneMainWeb
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
