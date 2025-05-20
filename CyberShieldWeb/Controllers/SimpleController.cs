using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using CyberShield.BusinessLogic;

namespace CyberShieldWeb.Controllers
{
    public class SimpleController : Controller
    {
        private readonly ITestService _testService;

        public SimpleController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _testService = bl.GetTestService();
        }

        public ActionResult Index()
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            _testService.LogTestAccess("Simple-Index", username);
            
            return Content("This is the SimpleController. If you can see this, the basic routing is working.");
        }
    }
} 