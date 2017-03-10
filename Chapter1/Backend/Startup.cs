using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Backend.Startup))]

namespace Backend
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}