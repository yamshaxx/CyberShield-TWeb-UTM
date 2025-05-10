using System.Web.Mvc;

namespace CyberShieldWeb.Controllers
{
    public class SimpleController : Controller
    {
        public ActionResult Index()
        {
            return Content("This is the SimpleController. If you can see this, the basic routing is working.");
        }
    }
} 