using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CyberShieldWeb.Controllers
{
    [RoutePrefix("contact")]
    public class ContactController : Controller
    {
        // GET: Contact
        [Route("")]
        public ActionResult Index()
        {
            return View();
        }
        
        // POST: Contact/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(string name, string email, string subject, string message)
        {
            // This would typically send an email or store the message in a database
            // For now, just redirecting with a success message
            TempData["SuccessMessage"] = "Mesajul dumneavoastră a fost trimis cu succes. Vă vom contacta în curând.";
            return RedirectToAction("Index");
        }
    }
} 