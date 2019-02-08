using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Backend.DataObjects;
using Backend.Helpers;
using Backend.Models;
using Microsoft.Azure.Mobile.Server;

namespace Backend.Controllers
{
    [AuthorizeClaims("groups", "01f214a9-af1f-4bdd-938f-3f16749aef0e")]
    public class TodoItemController : TableController<TodoItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TodoItem>(context, Request);
        }

        public IQueryable<TodoItem> GetAllTodoItems() => Query();

        public SingleResult<TodoItem> GetTodoItem(string id) => Lookup(id);

        public Task<TodoItem> PatchTodoItem(string id, Delta<TodoItem> patch) => UpdateAsync(id, patch);

        public async Task<IHttpActionResult> PostTodoItem(TodoItem item)
        {
            //if (!await IsAuthorizedAsync())
            //{
            //    return Unauthorized();
            //}
            TodoItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        public Task DeleteTodoItem(string id) => DeleteAsync(id);

        //async Task<bool> IsAuthorizedAsync()
        //{
        //    var identity = await User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(Request);
        //    var countofGroups = identity.UserClaims
        //        .Where(c => c.Type.Equals("groups") && c.Value.Equals("01f214a9-af1f-4bdd-938f-3f16749aef0e"))
        //        .Count();
        //    return (countofGroups > 0);
        //}
    }
}