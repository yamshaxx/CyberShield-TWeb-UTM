using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;

namespace CyberShieldWeb.Controllers
{
    public class TestController : Controller
    {
        private readonly ITestService _testService;

        public TestController()
        {
            var bl = new BL.BusinessLogic();
            _testService = bl.GetTestService();
        }

        // GET: /Test
        public ActionResult Index()
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            _testService.LogTestAccess("Index", username);
            
            string content = _testService.GetTestContent();
            return Content(content);
        }
        
        // GET: /Test/Contact
        public ActionResult Contact()
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            _testService.LogTestAccess("Contact", username);
            
            string content = _testService.GetTestContactContent();
            return Content(content);
        }
    }
} 