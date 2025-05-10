using System.Web.Mvc;

namespace CyberShieldWeb.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            return Content("This is the Help controller index action. The routing system is working for this controller.");
        }
    }
} 