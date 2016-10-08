using System.Configuration;
using System.Diagnostics;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Owin;

namespace Backend
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            // Enable Tracing
            config.EnableSystemDiagnosticsTracing();

            new MobileAppConfiguration()
                .AddTables()
                .ApplyTo(config);

            #region Local Settings
            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();
            Debug.WriteLine("Connection Strings defined:");
            foreach (var cs in settings.Connections)
            {
                Debug.WriteLine($"{cs.Key} = {cs.Value.ConnectionString}");
            }

            // This middleware is intended to be used locally for debugging. By default, HostName will
            // only have a value when running in an App Service application.
            if (string.IsNullOrEmpty(settings.HostName))
            {
                app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
                {
                    SigningKey = ConfigurationManager.AppSettings["SigningKey"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
                    ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
                    TokenHandler = config.GetAppServiceTokenHandler()
                });
            }
            #endregion

            app.UseWebApi(config);
        }
    }
}

