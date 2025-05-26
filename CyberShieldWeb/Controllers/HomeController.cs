using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;
using CyberShieldWeb.Models;

namespace CyberShieldWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IContactMessageService _contactMessageService;
        private readonly IDashboardService _dashboardService;
        private readonly IErrorHandlingService _errorHandler;
        
        public HomeController()
        {
            var bl = new BL.BusinessLogic();
            _contactMessageService = bl.GetContactMessageService();
            _dashboardService = bl.GetDashboardService();
            _errorHandler = bl.GetErrorHandlingService();
        }
        
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        // GET: Contact functionality added to Home controller for "Contactati-ne"
        public ActionResult Contact()
        {
            // Render the Contact/Index view from the Contact folder
            return View("~/Views/Contact/Index.cshtml");
        }
        
        // POST: Home/SendMessage (for contact form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(string name, string email, string subject, string message)
        {
            try
            {
                // Create the contact message using the service
                var contactMessage = new CyberShield.Domain.Model.ContactMessage
                {
                    Name = name,
                    Email = email,
                    Subject = subject,
                    Message = message,
                    SentDate = DateTime.Now,
                    IsRead = false
                };
                
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
                _errorHandler?.LogError(ex, "HomeController.SendMessage");
                TempData["ErrorMessage"] = "A apărut o eroare la trimiterea mesajului. Vă rugăm să încercați din nou.";
            }
            
            return RedirectToAction("Index");
        }

        // GET: Dashboard
        [Authorize]
        public ActionResult Dashboard()
        {
            try
            {
                string username = User.Identity.Name;
                
                // Get dashboard data from service
                var dashboardData = _dashboardService.GetUserDashboardData(username);
                
                // Convert to view model
                var viewModel = new UserDashboardViewModel
                {
                    Username = username
                };
                
                // Use reflection to extract data from anonymous object
                var dashboardType = dashboardData.GetType();
                var emailProperty = dashboardType.GetProperty("Email");
                var commentsProperty = dashboardType.GetProperty("Comments");
                var appointmentsProperty = dashboardType.GetProperty("Appointments");
                var sentMessagesProperty = dashboardType.GetProperty("SentMessages");
                
                if (emailProperty != null)
                {
                    viewModel.Email = emailProperty.GetValue(dashboardData)?.ToString() ?? "";
                }
                
                // Process comments
                if (commentsProperty != null)
                {
                    var comments = commentsProperty.GetValue(dashboardData) as IEnumerable<object>;
                    if (comments != null)
                    {
                        foreach (var comment in comments)
                        {
                            var commentType = comment.GetType();
                            var commentViewModel = new UserCommentViewModel
                            {
                                Id = (int)(commentType.GetProperty("Id")?.GetValue(comment) ?? 0),
                                BlogPostId = (int)(commentType.GetProperty("BlogPostId")?.GetValue(comment) ?? 0),
                                BlogPostTitle = commentType.GetProperty("BlogPostTitle")?.GetValue(comment)?.ToString() ?? "Unknown",
                                Content = commentType.GetProperty("Content")?.GetValue(comment)?.ToString() ?? "",
                                PostedAt = (DateTime)(commentType.GetProperty("PostedAt")?.GetValue(comment) ?? DateTime.Now)
                            };
                            viewModel.Comments.Add(commentViewModel);
                        }
                    }
                }
                
                // Process appointments
                if (appointmentsProperty != null)
                {
                    var appointments = appointmentsProperty.GetValue(dashboardData) as IEnumerable<CyberShield.Domain.Model.Blog.Appointment>;
                    if (appointments != null)
                    {
                        foreach (var appointment in appointments)
                        {
                            viewModel.Appointments.Add(new UserAppointmentViewModel
                            {
                                Id = appointment.Id,
                                ServiceType = appointment.ServiceType,
                                PreferredDate = appointment.PreferredDate,
                                Status = appointment.Status,
                                CreatedAt = appointment.CreatedAt
                            });
                        }
                    }
                }
                
                // Process sent messages
                if (sentMessagesProperty != null)
                {
                    var sentMessages = sentMessagesProperty.GetValue(dashboardData) as IEnumerable<CyberShield.Domain.Model.ContactMessage>;
                    if (sentMessages != null)
                    {
                        foreach (var sentMessage in sentMessages)
                        {
                            viewModel.SentMessages.Add(new UserContactMessageViewModel
                            {
                                Id = sentMessage.Id,
                                Subject = sentMessage.Subject,
                                Message = sentMessage.Message,
                                SentDate = sentMessage.SentDate,
                                IsRead = sentMessage.IsRead
                            });
                        }
                    }
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "HomeController.Dashboard");
                return View(new UserDashboardViewModel
                {
                    Username = User.Identity.Name ?? ""
                });
            }
        }
    }
}