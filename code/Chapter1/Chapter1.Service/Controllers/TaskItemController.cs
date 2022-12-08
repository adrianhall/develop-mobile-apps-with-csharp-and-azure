using Chapter1.Service.Database;
using Chapter1.Service.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;

namespace Chapter1.Service.Controllers
{
    [Route("tables/taskitem")]
    public class TaskItemController : TableController<TaskItemDTO>
    {
        public TaskItemController(AppDbContext context) : base()
        {
            Repository = new EntityTableRepository<TaskItemDTO>(context);
        }
    }
}