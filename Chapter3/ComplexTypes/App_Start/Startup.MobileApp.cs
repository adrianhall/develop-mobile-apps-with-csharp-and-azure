using System.Configuration;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using ComplexTypes.Models;
using Owin;
using System.Collections.Generic;
using ComplexTypes.DataObjects;
using ComplexTypes.Types;
using Newtonsoft.Json;
using System.Diagnostics;
using System;

namespace ComplexTypes
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer(new MobileServiceInitializer());

            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            if (string.IsNullOrEmpty(settings.HostName))
            {
                app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
                {
                    // This middleware is intended to be used locally for debugging. By default, HostName will
                    // only have a value when running in an App Service application.
                    SigningKey = ConfigurationManager.AppSettings["SigningKey"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
                    ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
                    TokenHandler = config.GetAppServiceTokenHandler()
                });
            }

            app.UseWebApi(config);
        }
    }

    public class MobileServiceInitializer : CreateDatabaseIfNotExists<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext context)
        {
            List<Track> tracks = new List<Track>
            {
                new Track {
                    Id = Guid.NewGuid().ToString(),
                    Location = new Position { Longitude = 1.0, Latitude = 1.0 }
                },
                new Track {
                    Id = Guid.NewGuid().ToString(),
                    Location = new Position { Longitude = 89.6, Latitude = 77.4 }
                }
            };
            context.Set<Track>().AddRange(tracks);

            string json = JsonConvert.SerializeObject(tracks);
            Debug.WriteLine($"JSON Serialization: {json}");

            base.Seed(context);
        }
    }
}

