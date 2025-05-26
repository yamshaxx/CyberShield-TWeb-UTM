using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;

namespace CyberShieldWeb.Controllers
{
    public class HelpController : Controller
    {
        private readonly IHelpService _helpService;

        public HelpController()
        {
            var bl = new BL.BusinessLogic();
            _helpService = bl.GetHelpService();
        }

        // GET: Help
        public ActionResult Index()
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            _helpService.LogHelpRequest("Index", username);
            
            string content = _helpService.GetHelpContent();
            return Content(content);
        }
    }
} 