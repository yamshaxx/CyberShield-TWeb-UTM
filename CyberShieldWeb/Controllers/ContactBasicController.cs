using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;

namespace CyberShieldWeb.Controllers
{
    public class ContactBasicController : Controller
    {
        private readonly ITestService _testService;

        public ContactBasicController()
        {
            var bl = new BL.BusinessLogic();
            _testService = bl.GetTestService();
        }

        public ActionResult Index()
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            _testService.LogTestAccess("ContactBasic-Index", username);
            
            return Content("This is the ContactBasic controller. If you can see this message, the routing is working for this controller.");
        }
    }
} 