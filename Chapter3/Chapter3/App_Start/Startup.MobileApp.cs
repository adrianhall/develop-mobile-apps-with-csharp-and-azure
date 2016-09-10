using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using Chapter3.DataObjects;
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
                List<Tag> tags = new List<Tag>
                {
                    new Tag { Id = NewGuid(), TagName = "Tag-1" },
                    new Tag { Id = NewGuid(), TagName = "Tag-2" },
                    new Tag { Id = NewGuid(), TagName = "Tag-3" }
                };
                context.Set<Tag>().AddRange(tags);

                List<Message> messages = new List<Message>
                {
                    new Message { Id = NewGuid(), Text = "Message-1", Tags = tags },
                    new Message { Id = NewGuid(), Text = "message-2", Tags = new List<Tag>() }
                };
                context.Set<Message>().AddRange(messages);

                base.Seed(context);
            }

            private string NewGuid()
            {
                return Guid.NewGuid().ToString();
            }
        }

    }
}

