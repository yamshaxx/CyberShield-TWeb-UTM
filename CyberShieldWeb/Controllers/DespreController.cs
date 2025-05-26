using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;

namespace CyberShieldWeb.Controllers
{
    public class DespreController : Controller
    {
        private readonly IDespreService _despreService;

        public DespreController()
        {
            var bl = new BL.BusinessLogic();
            _despreService = bl.GetDespreService();
        }

        public ActionResult Index()
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            _despreService.LogPageVisit("Index", username);
            
            return View();
        }
    }
} 
