using System;
using System.Web.Mvc;

namespace CyberShieldWeb.Controllers
{
    public class TestController : Controller
    {
        // GET: /Test
        public ActionResult Index()
        {
            return Content("Test controller is working. The routing system is functioning.");
        }
        
        // GET: /Test/Contact
        public ActionResult Contact()
        {
            return Content("This is a test contact action. If you can see this, routing to /Test/Contact works.");
        }
    }
} 