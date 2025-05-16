using BlogModel = CyberShield.Domain.Model.Blog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CyberShield.Domain.Data;
using CyberShieldWeb.Models;
using System.Data.Entity;
using CyberShield.BusinessLogic.Interface;
using CyberShield.BusinessLogic.BL_Struct;
using UserModel = CyberShield.Domain.Model.User;

namespace CyberShieldWeb.Controllers
{
    [Authorize]
    public class SpecialistController : Controller
    {
        private CyberShieldContext _db;
        private readonly IContactMessageService _contactMessageService;
        
        // Lazy-load the database context to avoid initialization during controller construction
        private CyberShieldContext Db
        {
            get
            {
                if (_db == null)
                {
                    _db = new CyberShieldContext();
                }
                return _db;
            }
        }
        
        public SpecialistController()
        {
            _contactMessageService = new ContactMessageService();
        }
        
        [NonAction]
        private bool IsSpecialist()
        {
            if (!User.Identity.IsAuthenticated)
                return false;
                
            string username = User.Identity.Name;
            
            try
            {
                var user = Db.Users.FirstOrDefault(u => u.Username == username);
                if (user != null && user.IsSpecialist)
                {
                    return true;
                }
            }
            catch
            {
                // If database connection fails, try in-memory
                var user = InMemoryData.Users.FirstOrDefault(u => u.Username == username);
                if (user != null && user.IsSpecialist)
                {
                    return true;
                }
            }
            
            return false;
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
            if (!IsSpecialist())
                return RedirectToAction("Login", "Auth");
                
            try
            {
                string username = User.Identity.Name;
                var viewModel = new SpecialistDashboardViewModel
                {
                    Username = username
                };
                
                var user = Db.Users.FirstOrDefault(u => u.Username == username);
                if (user != null)
                {
                    viewModel.Email = user.Email;
                    
                    try
                    {
                        // Get all blog posts
                        var blogPosts = Db.BlogPosts
                            .OrderByDescending(b => b.PostedDate)
                            .ToList();
                            
                        foreach (var post in blogPosts)
                        {
                            viewModel.BlogPosts.Add(new SpecialistBlogViewModel
                            {
                                Id = post.Id,
                                Title = post.Title,
                                PostedDate = post.PostedDate,
                                CommentCount = post.Comments?.Count ?? 0
                            });
                        }
                        
                        // Get all appointments
                        var appointments = Db.Appointments
                            .OrderByDescending(a => a.PreferredDate)
                            .ToList();
                            
                        foreach (var appointment in appointments)
                        {
                            // Only add pending appointments to the waiting list
                            if (appointment.Status == "Pending" || string.IsNullOrEmpty(appointment.Status))
                            {
                                viewModel.Appointments.Add(new AppointmentViewModel
                                {
                                    Id = appointment.Id,
                                    Name = appointment.Name,
                                    Email = appointment.Email,
                                    Phone = appointment.Phone,
                                    Company = appointment.Company,
                                    ServiceType = appointment.ServiceType,
                                    PreferredDate = appointment.PreferredDate,
                                    Message = appointment.Message,
                                    Status = appointment.Status ?? "Pending",
                                    CreatedAt = appointment.CreatedAt
                                });
                            }
                            
                            // Confirmed appointments
                            if (appointment.Status == "Confirmed")
                            {
                                viewModel.ConfirmedAppointments.Add(new AppointmentViewModel
                                {
                                    Id = appointment.Id,
                                    Name = appointment.Name,
                                    Email = appointment.Email,
                                    Phone = appointment.Phone,
                                    Company = appointment.Company,
                                    ServiceType = appointment.ServiceType,
                                    PreferredDate = appointment.PreferredDate,
                                    Message = appointment.Message,
                                    Status = appointment.Status,
                                    CreatedAt = appointment.CreatedAt
                                });
                            }
                            
                            // Cancelled appointments
                            if (appointment.Status == "Cancelled")
                            {
                                viewModel.CancelledAppointments.Add(new AppointmentViewModel
                                {
                                    Id = appointment.Id,
                                    Name = appointment.Name,
                                    Email = appointment.Email,
                                    Phone = appointment.Phone,
                                    Company = appointment.Company,
                                    ServiceType = appointment.ServiceType,
                                    PreferredDate = appointment.PreferredDate,
                                    Message = appointment.Message,
                                    Status = appointment.Status,
                                    CreatedAt = appointment.CreatedAt
                                });
                            }
                        }
                        
                        // Get contact messages
                        var contactMessages = _contactMessageService.GetAllMessages().ToList();
                            
                        System.Diagnostics.Debug.WriteLine($"Found {contactMessages.Count} contact messages");
                        
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
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading appointments: {ex.Message}");
                        
                        // Fall back to in-memory data
                        try
                        {
                            LoadInMemoryData(viewModel);
                        }
                        catch (Exception memEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading in-memory data: {memEx.Message}");
                        }
                    }
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Specialist Dashboard: {ex.Message}");
                return View(new SpecialistDashboardViewModel
                {
                    Username = User.Identity.Name,
                    BlogPosts = new List<SpecialistBlogViewModel>(),
                    Appointments = new List<AppointmentViewModel>(),
                    ConfirmedAppointments = new List<AppointmentViewModel>(),
                    CancelledAppointments = new List<AppointmentViewModel>(),
                    ContactMessages = new List<ContactMessageViewModel>()
                });
            }
        }
        
        // Helper method to load in-memory data when database access fails
        private void LoadInMemoryData(SpecialistDashboardViewModel viewModel)
        {
            // Load blog posts from memory
            foreach (var post in InMemoryData.BlogPosts)
            {
                viewModel.BlogPosts.Add(new SpecialistBlogViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    PostedDate = post.PostedDate,
                    CommentCount = post.Comments?.Count ?? 0
                });
            }
            
            // Load appointments from memory
            foreach (var appointment in InMemoryData.Appointments)
            {
                // Only add pending appointments to the waiting list
                if (appointment.Status == "Pending" || string.IsNullOrEmpty(appointment.Status))
                {
                    viewModel.Appointments.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        Name = appointment.Name,
                        Email = appointment.Email,
                        Phone = appointment.Phone,
                        Company = appointment.Company,
                        ServiceType = appointment.ServiceType,
                        PreferredDate = appointment.PreferredDate,
                        Message = appointment.Message,
                        Status = appointment.Status ?? "Pending",
                        CreatedAt = appointment.CreatedAt
                    });
                }
                
                // Confirmed appointments
                if (appointment.Status == "Confirmed")
                {
                    viewModel.ConfirmedAppointments.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        Name = appointment.Name,
                        Email = appointment.Email,
                        Phone = appointment.Phone,
                        Company = appointment.Company,
                        ServiceType = appointment.ServiceType,
                        PreferredDate = appointment.PreferredDate,
                        Message = appointment.Message,
                        Status = appointment.Status,
                        CreatedAt = appointment.CreatedAt
                    });
                }
                
                // Cancelled appointments
                if (appointment.Status == "Cancelled")
                {
                    viewModel.CancelledAppointments.Add(new AppointmentViewModel
                    {
                        Id = appointment.Id,
                        Name = appointment.Name,
                        Email = appointment.Email,
                        Phone = appointment.Phone,
                        Company = appointment.Company,
                        ServiceType = appointment.ServiceType,
                        PreferredDate = appointment.PreferredDate,
                        Message = appointment.Message,
                        Status = appointment.Status,
                        CreatedAt = appointment.CreatedAt
                    });
                }
            }
            
            // Load contact messages from memory
            foreach (var message in InMemoryData.ContactMessages)
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
        
        // GET: Specialist/CreateBlogPost
        public ActionResult CreateBlogPost()
        {
            if (!IsSpecialist())
                return RedirectToAction("Login", "Auth");
                
            return View();
        }
        
        // POST: Specialist/CreateBlogPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in content
        public ActionResult CreateBlogPost(CreateBlogPostViewModel model)
        {
            if (!IsSpecialist())
                return RedirectToAction("Login", "Auth");
                
            if (ModelState.IsValid)
            {
                try
                {
                    var blogPost = new BlogModel.BlogPost
                    {
                        Title = model.Title,
                        Author = User.Identity.Name,
                        PostedDate = DateTime.Now,
                        Content = model.Content,
                        Summary = model.Summary,
                        Category = model.Category,
                        ImageUrl = model.ImageUrl
                    };
                    
                    Db.BlogPosts.Add(blogPost);
                    Db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Articolul a fost creat cu succes!";
                    return RedirectToAction("Dashboard");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating blog post: {ex.Message}");
                    ModelState.AddModelError("", "A apărut o eroare la crearea articolului. Încercați din nou.");
                }
            }
            
            return View(model);
        }
        
        // POST: Specialist/AcceptAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AcceptAppointment(int id)
        {
            if (!IsSpecialist())
                return RedirectToAction("Login", "Auth");
                
            try
            {
                var appointment = Db.Appointments.Find(id);
                if (appointment != null)
                {
                    appointment.Status = "Confirmed";
                    Db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Consultația a fost acceptată cu succes!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Consultația nu a fost găsită.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error accepting appointment: {ex.Message}");
                TempData["ErrorMessage"] = "A apărut o eroare la acceptarea consultației.";
            }
            
            return RedirectToAction("Dashboard");
        }
        
        // POST: Specialist/CancelAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelAppointment(int id)
        {
            if (!IsSpecialist())
                return RedirectToAction("Login", "Auth");
                
            try
            {
                var appointment = Db.Appointments.Find(id);
                if (appointment != null)
                {
                    appointment.Status = "Cancelled";
                    Db.SaveChanges();
                    
                    TempData["SuccessMessage"] = "Consultația a fost anulată.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Consultația nu a fost găsită.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cancelling appointment: {ex.Message}");
                TempData["ErrorMessage"] = "A apărut o eroare la anularea consultației.";
            }
            
            return RedirectToAction("Dashboard");
        }
        
        // POST: Specialist/MarkContactMessageAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkContactMessageAsRead(int id)
        {
            if (!IsSpecialist())
                return RedirectToAction("Login", "Auth");
                
            try
            {
                bool success = _contactMessageService.MarkAsRead(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Mesajul a fost marcat ca citit.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Mesajul nu a fost găsit.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking message as read: {ex.Message}");
                TempData["ErrorMessage"] = "A apărut o eroare la marcarea mesajului ca citit.";
            }
            
            return RedirectToAction("Dashboard");
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && _db != null)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 