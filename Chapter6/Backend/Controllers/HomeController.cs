using System.Linq;
using System.Web.Mvc;
using Backend.Models;

namespace Backend.Controllers
{
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
    }
}