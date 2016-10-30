using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using Backend.DataObjects;
using Backend.Models;
using Microsoft.Azure.Mobile.Server.Config;
using Owin;

namespace Backend
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

