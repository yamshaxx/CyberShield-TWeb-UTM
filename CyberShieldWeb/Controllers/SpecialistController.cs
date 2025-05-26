using System;
using System.Linq;
using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;
using CyberShieldWeb.Models;

namespace CyberShieldWeb.Controllers
{
    [Authorize]
    public class SpecialistController : Controller
    {
        private readonly IContactMessageService _contactMessageService;
        private readonly IDashboardService _dashboardService;
        private readonly IAuth _authService;
        private readonly IErrorHandlingService _errorHandler;
        
        public SpecialistController()
        {
            var bl = new BL.BusinessLogic();
            _contactMessageService = bl.GetContactMessageService();
            _dashboardService = bl.GetDashboardService();
            _authService = bl.GetAuthBL();
            _errorHandler = bl.GetErrorHandlingService();
        }
        
        [NonAction]
        private bool IsSpecialist()
        {
            if (!User.Identity.IsAuthenticated)
                return false;
                
            string username = User.Identity.Name;
            return _dashboardService.IsUserSpecialist(username);
        }
        
        // GET: Specialist
        public ActionResult Index()
        {
            if (!IsSpecialist())
                return RedirectToAction("Login", "Auth");
                
            return View();
        }
        
        // GET: Specialist/Dashboard
        public ActionResult Dashboard()
        {
            try
            {
                if (!IsSpecialist())
                    return RedirectToAction("Login", "Auth");
                
                string username = User.Identity.Name;
                
                // Get specialist dashboard data from service
                var dashboardData = _dashboardService.GetSpecialistDashboardData(username);
                
                // Convert to view model
                var viewModel = new SpecialistDashboardViewModel
                {
                    Username = username
                };
                
                // Use reflection to extract data from anonymous object
                var dashboardType = dashboardData.GetType();
                var totalAppointmentsProperty = dashboardType.GetProperty("TotalAppointments");
                var recentAppointmentsProperty = dashboardType.GetProperty("RecentAppointments");
                var contactMessagesProperty = dashboardType.GetProperty("ContactMessages");
                var pendingAppointmentsProperty = dashboardType.GetProperty("PendingAppointments");
                
                if (totalAppointmentsProperty != null)
                {
                    viewModel.TotalAppointments = (int)(totalAppointmentsProperty.GetValue(dashboardData) ?? 0);
                }
                
                // Process recent appointments
                if (recentAppointmentsProperty != null)
                {
                    var recentAppointments = recentAppointmentsProperty.GetValue(dashboardData) as System.Collections.Generic.IEnumerable<CyberShield.Domain.Model.Blog.Appointment>;
                    if (recentAppointments != null)
                    {
                        foreach (var appointment in recentAppointments)
                        {
                            viewModel.RecentAppointments.Add(new AppointmentViewModel
                            {
                                Id = appointment.Id,
                                ClientName = appointment.Name,
                                ServiceType = appointment.ServiceType,
                                PreferredDate = appointment.PreferredDate,
                                Status = appointment.Status,
                                CreatedAt = appointment.CreatedAt
                            });
                        }
                    }
                }
                
                // Process contact messages
                if (contactMessagesProperty != null)
                {
                    var contactMessages = contactMessagesProperty.GetValue(dashboardData) as System.Collections.Generic.IEnumerable<CyberShield.Domain.Model.ContactMessage>;
                    if (contactMessages != null)
                    {
                        foreach (var message in contactMessages)
                        {
                            viewModel.ContactMessages.Add(new ContactMessageViewModel
                            {
                                Id = message.Id,
                                Name = message.Name,
                                Email = message.Email,
                                Subject = message.Subject,
                                Message = message.Message,
                                SentDate = message.SentDate,
                                IsRead = message.IsRead
                            });
                        }
                    }
                }
                
                // Process pending appointments
                if (pendingAppointmentsProperty != null)
                {
                    var pendingAppointments = pendingAppointmentsProperty.GetValue(dashboardData) as System.Collections.Generic.IEnumerable<CyberShield.Domain.Model.Blog.Appointment>;
                    if (pendingAppointments != null)
                    {
                        foreach (var appointment in pendingAppointments)
                        {
                            viewModel.PendingAppointments.Add(new AppointmentViewModel
                            {
                                Id = appointment.Id,
                                ClientName = appointment.Name,
                                ServiceType = appointment.ServiceType,
                                PreferredDate = appointment.PreferredDate,
                                Status = appointment.Status,
                                CreatedAt = appointment.CreatedAt
                            });
                        }
                    }
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "SpecialistController.Dashboard");
                return View(new SpecialistDashboardViewModel
                {
                    Username = User.Identity.Name ?? ""
                });
            }
        }
        
        // GET: Specialist/Appointments
        public ActionResult Appointments()
        {
            if (!IsSpecialist())
                return RedirectToAction("Login", "Auth");
                
            return View();
        }
        
        // GET: Specialist/Messages
        public ActionResult Messages()
        {
            try
            {
                if (!IsSpecialist())
                    return RedirectToAction("Login", "Auth");
                
                var messages = _contactMessageService.GetAllMessages();
                
                var viewModel = new ContactMessagesViewModel
                {
                    Messages = messages.Select(m => new ContactMessageViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Email = m.Email,
                        Subject = m.Subject,
                        Message = m.Message,
                        SentDate = m.SentDate,
                        IsRead = m.IsRead
                    }).ToList()
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "SpecialistController.Messages");
                return View(new ContactMessagesViewModel());
            }
        }
    }
} 