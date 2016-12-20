using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Backend.DataObjects;
using Backend.Models;

namespace Backend.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private MobileServiceContext context;

        public HomeController()
        {
            context = new MobileServiceContext();
        }

        public ActionResult Index()
        {
            var list = context.TodoItems.ToList();
            return View(list);
        }

        [HttpPost]
        /* [ValidateAntiForgeryToken] */
        public async Task<ActionResult> Create([Bind(Include = "Text")]TodoItem item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    item.Id = Guid.NewGuid().ToString("N");
                    context.TodoItems.Add(item);
                    await context.SaveChangesAsync();
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Unable to save changes.");
            }
            return RedirectToAction("Index");
        }
    }
}