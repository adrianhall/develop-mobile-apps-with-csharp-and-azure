using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Backend.DataObjects;
using Microsoft.Azure.Mobile.Server;

namespace Backend.Controllers
{
    public class TodoItemController : TableController<TodoItem>
    {
        private const string connectionString = "MS_AzureStorageAccountConnectionString";
        private const string tableName = "TodoItem";

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            ODataValidationSettings validationSettings = new ODataValidationSettings()
            {
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                AllowedFunctions = AllowedFunctions.None,
                AllowedQueryOptions = AllowedQueryOptions.Filter
                    | AllowedQueryOptions.Top
                    | AllowedQueryOptions.Select,
                AllowedLogicalOperators = AllowedLogicalOperators.Equal
                    | AllowedLogicalOperators.And
                    | AllowedLogicalOperators.Or
                    | AllowedLogicalOperators.Not
                    | AllowedLogicalOperators.GreaterThan
                    | AllowedLogicalOperators.GreaterThanOrEqual
                    | AllowedLogicalOperators.LessThan
                    | AllowedLogicalOperators.LessThanOrEqual
                    | AllowedLogicalOperators.NotEqual
            };

            ODataQuerySettings querySettings = new ODataQuerySettings()
            {
                PageSize = 50
            };

            DomainManager = new StorageDomainManager<TodoItem>(
                connectionString,
                tableName,
                Request,
                validationSettings,
                querySettings,
                enableSoftDelete: true);
        }

        // GET tables/TodoItem
        public async Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync(ODataQueryOptions query)
        {
            return await QueryAsync(query);
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<SingleResult<TodoItem>> GetTodoItemAsync(string id)
        {
            return await LookupAsync(id);
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<TodoItem> PatchTodoItemAsync(string id, Delta<TodoItem> patch)
        {
            return await UpdateAsync(id, patch);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task DeleteTodoItemAsync(string id)
        {
            await DeleteAsync(id);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostTodoItemAsync(TodoItem item)
        {
            if (item.Id == null || item.Id.Equals("'',''"))
            {
                item.Id = GenerateUniqueId();
            }
            TodoItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        private string GenerateUniqueId()
        {
            var partitionId = "1";
            var rowId = Guid.NewGuid().ToString("N");
            return $"'{partitionId}','{rowId}'";
        }
    }
}