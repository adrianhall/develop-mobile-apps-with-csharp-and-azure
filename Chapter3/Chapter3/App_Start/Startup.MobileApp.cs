using System.Data.Entity;
using System.Web.Http;
using Chapter3.Models;
using Microsoft.Azure.Mobile.Server.Config;
using Owin;

namespace Chapter3
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();
            var mobileConfig = new MobileAppConfiguration();

            mobileConfig
                .AddTablesWithEntityFramework()
                .ApplyTo(httpConfig);

            // Map Routes via attribute
            httpConfig.MapHttpAttributeRoutes();

            Database.SetInitializer(new MobileServiceInitializer());

            app.UseWebApi(httpConfig);
        }

        public class MobileServiceInitializer : CreateDatabaseIfNotExists<MobileServiceContext>
        {
            protected override void Seed(MobileServiceContext context)
            {
                base.Seed(context);
            }
        }
    }
}

