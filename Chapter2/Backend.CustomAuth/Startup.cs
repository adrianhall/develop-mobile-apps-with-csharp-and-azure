using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Backend.CustomAuth.Startup))]

namespace Backend.CustomAuth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}