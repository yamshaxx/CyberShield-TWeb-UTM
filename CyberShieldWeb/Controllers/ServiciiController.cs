using System;
using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;

namespace CyberShieldWeb.Controllers
{
    public class ServiciiController : Controller
    {
        private readonly IServiciiService _serviciiService;
        private readonly IErrorHandlingService _errorHandler;

        public ServiciiController()
        {
            var bl = new BL.BusinessLogic();
            _serviciiService = bl.GetServiciiService();
            _errorHandler = bl.GetErrorHandlingService();
        }
        
        // GET: Servicii
        public ActionResult Index()
        {
            return View();
        }

        // GET: Servicii/TestareaPenetrarii
        public ActionResult TestareaPenetrarii()
        {
            return View();
        }

        // GET: Servicii/Consultanta
        public ActionResult Consultanta()
        {
            return View();
        }

        // GET: Servicii/InginerieSociala
        public ActionResult InginerieSociala()
        {
            return View("InginerieaSociala");
        }

        // GET: Servicii/ConformitateGDPR
        public ActionResult ConformitateGDPR()
        {
            return View();
        }

        // GET: Servicii/Programeaza
        public ActionResult Programeaza()
        {
            return View();
        }

        // POST: Servicii/SubmitProgramare
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitProgramare(string name, string email, string phone, string company, string serviceType, string preferredDate, string message)
        {
            try
            {
                string username = User.Identity.IsAuthenticated ? User.Identity.Name : null;
                
                bool success = _serviciiService.SubmitAppointment(name, email, phone, company, serviceType, preferredDate, message, username);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Solicitarea dumneavoastră de programare a fost trimisă cu succes. Vă vom contacta în curând pentru confirmare.";
                }
                else
                {
                    TempData["ErrorMessage"] = "A apărut o eroare la procesarea cererii dumneavoastră. Vă rugăm să încercați din nou.";
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "ServiciiController.SubmitProgramare");
                TempData["ErrorMessage"] = "A apărut o eroare la procesarea cererii dumneavoastră. Vă rugăm să încercați din nou.";
            }
            
            // If the user is authenticated, redirect to Dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            else
            {
                return RedirectToAction("Programeaza");
            }
        }
    }
} 
