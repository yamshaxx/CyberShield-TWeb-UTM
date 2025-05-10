using System.Web.Mvc;

namespace CyberShieldWeb.Controllers
{
    public class ContactBasicController : Controller
    {
        public ActionResult Index()
        {
            return Content("This is the ContactBasic controller. If you can see this message, the routing is working for this controller.");
        }
    }
} 