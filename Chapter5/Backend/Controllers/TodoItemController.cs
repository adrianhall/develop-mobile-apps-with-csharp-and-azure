using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using Backend.DataObjects;
using Backend.Models;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using System.Collections.Generic;
using System;

namespace Backend.Controllers
{
    [Authorize]
    public class TodoItemController : TableController<TodoItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request, enableSoftDelete: true);
        }

        // GET tables/TodoItem
        public IQueryable<TodoItem> GetAllTodoItems()
        {
            return Query();
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TodoItem> GetTodoItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch)
        {
            var item = await UpdateAsync(id, patch);
            await PushToSyncAsync("todoitem", item.Id);
            return item;
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            TodoItem current = await InsertAsync(item);
            await PushToSyncAsync("todoitem", item.Id);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task DeleteTodoItem(string id)
        {
            await PushToSyncAsync("todoitem", id);
            await DeleteAsync(id);
        }

        private async Task PushToSyncAsync(string table, string id)
        {
            var appSettings = this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();
            var nhName = appSettings.NotificationHubName;
            var nhConnection = appSettings.Connections[MobileAppSettingsKeys.NotificationHubConnectionString].ConnectionString;

            // Create a new Notification Hub client
            var hub = NotificationHubClient.CreateClientFromConnectionString(nhConnection, nhName);

            // Create a template message
            var templateParams = new Dictionary<string, string>();
            templateParams["op"] = "sync";
            templateParams["table"] = table;
            templateParams["id"] = id;

            // Send the template message
            try
            {
                var result = await hub.SendTemplateNotificationAsync(templateParams);
                Configuration.Services.GetTraceWriter().Info(result.State.ToString());
            }
            catch (Exception ex)
            {
                Configuration.Services.GetTraceWriter().Error(ex.Message, null, "PushToSync Error");
            }
        }
    }
}