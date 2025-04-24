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

namespace CyberShieldWeb.Controllers
{
    [Authorize]
    public class SpecialistController : Controller
    {
        private CyberShieldContext _db;
        
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
        
        [NonAction]
        private bool IsSpecialist()
        {
            if (!User.Identity.IsAuthenticated)
                return false;
                
            // First check if the user has the Specialist role directly
            if (User.IsInRole("Specialist"))
                return true;
                
            // If role check fails, fall back to database check
            var username = User.Identity.Name;
            var user = Db.Users.FirstOrDefault(u => u.Username == username);
            
            // If the user is a specialist in the database but not in the authentication ticket,
            // update the session to include the Specialist role for future requests
            if (user != null && user.IsSpecialist)
            {
                System.Diagnostics.Debug.WriteLine($"User {username} is a specialist in database but not in auth ticket");
                return true;
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
                        // Get blog posts by this specialist
                        var posts = Db.BlogPosts
                            .Where(p => p.Author == username)
                            .OrderByDescending(p => p.PostedDate)
                            .ToList();
                            
                        System.Diagnostics.Debug.WriteLine($"Found {posts.Count} blog posts for specialist {username}");
                        
                        foreach (var post in posts)
                        {
                            viewModel.BlogPosts.Add(new SpecialistBlogViewModel
                            {
                                Id = post.Id,
                                Title = post.Title,
                                PostedDate = post.PostedDate,
                                Category = post.Category,
                                CommentCount = post.Comments != null ? post.Comments.Count : 0
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading blog posts: {ex.Message}");
                    }
                    
                    try
                    {
                        // Get all pending appointments for the specialist to accept
                        var pendingAppointments = Db.Appointments
                            .Where(a => a.Status == "Pending")
                            .OrderBy(a => a.PreferredDate)
                            .ToList();
                            
                        System.Diagnostics.Debug.WriteLine($"Found {pendingAppointments.Count} pending appointments");
                        
                        foreach (var appointment in pendingAppointments)
                        {
                            viewModel.Appointments.Add(new AppointmentViewModel
                            {
                                Id = appointment.Id,
                                Name = appointment.Name,
                                Email = appointment.Email,
                                Phone = appointment.Phone,
                                ServiceType = appointment.ServiceType,
                                PreferredDate = appointment.PreferredDate,
                                Message = appointment.Message,
                                Status = appointment.Status,
                                CreatedAt = appointment.CreatedAt
                            });
                        }
                        
                        // Get all confirmed appointments
                        var confirmedAppointments = Db.Appointments
                            .Where(a => a.Status == "Confirmed")
                            .OrderBy(a => a.PreferredDate)
                            .ToList();
                            
                        System.Diagnostics.Debug.WriteLine($"Found {confirmedAppointments.Count} confirmed appointments");
                        
                        foreach (var appointment in confirmedAppointments)
                        {
                            viewModel.ConfirmedAppointments.Add(new AppointmentViewModel
                            {
                                Id = appointment.Id,
                                Name = appointment.Name,
                                Email = appointment.Email,
                                Phone = appointment.Phone,
                                ServiceType = appointment.ServiceType,
                                PreferredDate = appointment.PreferredDate,
                                Message = appointment.Message,
                                Status = appointment.Status,
                                CreatedAt = appointment.CreatedAt
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading appointments: {ex.Message}");
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
                    ConfirmedAppointments = new List<AppointmentViewModel>()
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