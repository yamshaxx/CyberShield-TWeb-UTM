using System;
using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using CyberShield.BusinessLogic;
using CyberShield.Domain.Model;

namespace CyberShieldWeb.Controllers
{
    [RoutePrefix("contact")]
    public class ContactController : Controller
    {
        private readonly IContactMessageService _contactMessageService;
        private readonly IErrorHandlingService _errorHandler;

        public ContactController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _contactMessageService = bl.GetContactMessageService();
            _errorHandler = bl.GetErrorHandlingService();
        }

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
            try
            {
                // Create the contact message
                var contactMessage = new ContactMessage
                {
                    Name = name,
                    Email = email,
                    Subject = subject,
                    Message = message,
                    SentDate = DateTime.Now,
                    IsRead = false
                };
                
                // Use the service to create the message
                bool success = _contactMessageService.CreateMessage(contactMessage);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Mesajul dumneavoastră a fost trimis cu succes. Vă vom contacta în curând.";
                }
                else
                {
                    TempData["ErrorMessage"] = "A apărut o eroare la trimiterea mesajului. Vă rugăm să încercați din nou.";
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "ContactController.SendMessage");
                TempData["ErrorMessage"] = "A apărut o eroare la trimiterea mesajului. Vă rugăm să încercați din nou.";
            }
            
            return RedirectToAction("Index");
        }
    }
} 