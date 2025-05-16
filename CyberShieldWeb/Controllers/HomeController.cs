using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CyberShield.Domain.Data;
using CyberShieldWeb.Models;
using BlogModel = CyberShield.Domain.Model.Blog;
using UserModel = CyberShield.Domain.Model.User;
using CyberShield.BusinessLogic.Interface;
using CyberShield.BusinessLogic.BL_Struct;
using CyberShield.Domain.Model;

namespace CyberShieldWeb.Controllers
{
    public class HomeController : Controller
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
        
        public HomeController()
        {
            _contactMessageService = new ContactMessageService();
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
                    // Set success message
                    TempData["SuccessMessage"] = "Mesajul dumneavoastră a fost trimis cu succes. Vă vom contacta în curând.";
                }
                else
                {
                    TempData["ErrorMessage"] = "A apărut o eroare la trimiterea mesajului. Vă rugăm să încercați din nou.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving contact message: {ex.Message}");
                // Continue without error to user - we don't want to show database issues to users
            }
            
            return RedirectToAction("Contact");
        }

        // GET: Dashboard
        [Authorize]
        public ActionResult Dashboard()
        {
            try
            {
                string username = User.Identity.Name;
                var viewModel = new UserDashboardViewModel
                {
                    Username = username
                };
                
                // Try to find user in the database
                UserModel.User user = null;
                bool userFoundInDb = false;
                
                try
                {
                    user = Db.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        viewModel.Email = user.Email;
                        userFoundInDb = true;

                        try 
                        {
                            // Get user's comments from database
                            var dbComments = Db.Comments
                                .Include(c => c.BlogPost)
                                .Where(c => c.UserId == user.Id)
                                .OrderByDescending(c => c.PostedAt)
                                .ToList();
                            
                            foreach (var comment in dbComments)
                            {
                                viewModel.Comments.Add(new UserCommentViewModel
                                {
                                    Id = comment.Id,
                                    BlogPostId = comment.BlogPostId,
                                    BlogPostTitle = comment.BlogPost != null ? comment.BlogPost.Title : "Unknown Post",
                                    Content = comment.Content,
                                    PostedAt = comment.PostedAt
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading comments: {ex.Message}");
                            // Continue to appointments
                        }

                        try
                        {
                            // Get user's appointments from database
                            var dbAppointments = Db.Appointments
                                .Where(a => a.UserId == user.Id)
                                .OrderByDescending(a => a.PreferredDate)
                                .ToList();
                            
                            System.Diagnostics.Debug.WriteLine($"Found {dbAppointments.Count} appointments in database for user {user.Username} (ID: {user.Id})");
                            
                            foreach (var appointment in dbAppointments)
                            {
                                System.Diagnostics.Debug.WriteLine($"DB Appointment: ID={appointment.Id}, ServiceType={appointment.ServiceType}, Date={appointment.PreferredDate}");
                                if (appointment.Status == "Pending" || string.IsNullOrEmpty(appointment.Status))
                                {
                                    viewModel.Appointments.Add(new UserAppointmentViewModel
                                    {
                                        Id = appointment.Id,
                                        ServiceType = appointment.ServiceType,
                                        PreferredDate = appointment.PreferredDate,
                                        Status = appointment.Status ?? "Pending",
                                        CreatedAt = appointment.CreatedAt
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading appointments from database: {ex.Message}");
                            // Now try to load in-memory appointments
                            LoadInMemoryData(viewModel);
                        }
                        
                        try
                        {
                            // Get user's sent messages using the service
                            var contactMessages = _contactMessageService.GetMessagesByEmail(user.Email);
                                
                            System.Diagnostics.Debug.WriteLine($"Found {contactMessages.Count()} contact messages for user {user.Username} (Email: {user.Email})");
                            
                            foreach (var message in contactMessages)
                            {
                                viewModel.SentMessages.Add(new UserContactMessageViewModel
                                {
                                    Id = message.Id,
                                    Subject = message.Subject,
                                    Message = message.Message,
                                    SentDate = message.SentDate,
                                    IsRead = message.IsRead
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading contact messages: {ex.Message}");
                            // Continue without messages if there's an error
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Database error in Dashboard: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                        if (dbEx.InnerException.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner inner exception: {dbEx.InnerException.InnerException.Message}");
                        }
                    }
                    // Continue to in-memory fallback
                }
                
                // If user not found in database, try in-memory
                if (!userFoundInDb)
                {
                    user = InMemoryData.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        viewModel.Email = user.Email;
                        
                        // Get user's comments from in-memory
                        var memoryComments = InMemoryData.Comments
                            .Where(c => c.UserId == user.Id)
                            .OrderByDescending(c => c.PostedAt)
                            .ToList();
                        
                        foreach (var comment in memoryComments)
                        {
                            var blogPost = InMemoryData.BlogPosts.FirstOrDefault(b => b.Id == comment.BlogPostId);
                            
                            viewModel.Comments.Add(new UserCommentViewModel
                            {
                                Id = comment.Id,
                                BlogPostId = comment.BlogPostId,
                                BlogPostTitle = blogPost != null ? blogPost.Title : "Unknown Post",
                                Content = comment.Content,
                                PostedAt = comment.PostedAt
                            });
                        }
                        
                        // Load in-memory appointments
                        LoadInMemoryData(viewModel);
                        
                        // Load in-memory contact messages
                        LoadInMemoryContactMessages(user.Email, viewModel);
                    }
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Dashboard: {ex.Message}");
                return View(new UserDashboardViewModel
                {
                    Username = User.Identity.Name
                });
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && _db != null)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
        
        private void LoadInMemoryData(UserDashboardViewModel viewModel)
        {
            // Load blog posts, appointments, and messages from memory
            // Using the same filtering logic for appointments
        }
        
        private void LoadInMemoryContactMessages(string userEmail, UserDashboardViewModel viewModel)
        {
            // Get user's contact messages from in-memory
            var memoryMessages = InMemoryData.ContactMessages
                .Where(m => m.Email == userEmail)
                .OrderByDescending(m => m.SentDate)
                .ToList();
            
            System.Diagnostics.Debug.WriteLine($"Found {memoryMessages.Count} contact messages in memory for user with email: {userEmail}");
            
            foreach (var message in memoryMessages)
            {
                viewModel.SentMessages.Add(new UserContactMessageViewModel
                {
                    Id = message.Id,
                    Subject = message.Subject,
                    Message = message.Message,
                    SentDate = message.SentDate,
                    IsRead = message.IsRead
                });
            }
        }
    }
}