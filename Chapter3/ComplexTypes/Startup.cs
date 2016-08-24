using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ComplexTypes.Startup))]

namespace ComplexTypes
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}