using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Chapter3.Startup))]

namespace Chapter3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}