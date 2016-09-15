using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using Backend.DataObjects;
using Backend.Models;
using System.Security.Claims;
using System.Net;
using Backend.Extensions;

namespace Backend.Controllers
{
    [Authorize]
    public class TodoItemController : TableController<TodoItem>
    {
        /// <summary>
        /// Initialize the controller
        /// </summary>
        /// <param name="controllerContext">The controller context</param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request, enableSoftDelete: true);
        }

        /// <summary>
        /// The UserId of the user making the current request
        /// </summary>
        public string UserId
            => ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier).Value;

        /// <summary>
        /// Validate the the owner of the record is the current user
        /// </summary>
        public void ValidateOwner(string id)
        {
            var result = Lookup(id).Queryable.PerUserFilter(UserId).FirstOrDefault<TodoItem>();
            if (result == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// OData Query Endpoint - GetAll
        /// </summary>
        public IQueryable<TodoItem> GetAllTodoItems()
        {
            return Query().PerUserFilter(UserId);
        }

        /// <summary>
        /// OData Query Endpoint: GetSingle
        /// </summary>
        /// <param name="id">The Id of the record to retrieve</param>
        public SingleResult<TodoItem> GetTodoItem(string id)
        {
            return SingleResult.Create<TodoItem>(Lookup(id).Queryable.PerUserFilter(UserId));
        }

        /// <summary>
        /// OData PATCH Endpoint
        /// </summary>
        /// <param name="id">The Id of the record to update</param>
        /// <param name="patch">The provided patch to the record</param>
        public Task<TodoItem> PatchTodoItemAsync(string id, Delta<TodoItem> patch)
        {
            ValidateOwner(id);
            return UpdateAsync(id, patch);
        }

        /// <summary>
        /// OData POST/INSERT Endpoint
        /// </summary>
        /// <param name="item">The item to insert</param>
        public async Task<IHttpActionResult> PostTodoItemAsync(TodoItem item)
        {
            item.UserId = UserId;
            TodoItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        /// <summary>
        /// OData DELETE Endpoint
        /// </summary>
        /// <param name="id">The id of the record to delete</param>
        public Task DeleteTodoItemAsync(string id)
        {
            ValidateOwner(id);
            return DeleteAsync(id);
        }
    }
}