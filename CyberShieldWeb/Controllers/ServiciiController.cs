using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CyberShield.Domain.Data;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShieldWeb.Controllers
{
    public class ServiciiController : Controller
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
                // First, determine if the user is authenticated
                int userId = 0;
                if (User.Identity.IsAuthenticated)
                {
                    System.Diagnostics.Debug.WriteLine($"User is authenticated: {User.Identity.Name}");
                    
                    // Try to find the user in database
                    var user = Db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                    if (user != null)
                    {
                        userId = user.Id;
                        System.Diagnostics.Debug.WriteLine($"Found user in database: ID = {userId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"User not found in database, checking in-memory");
                        // Try to find in memory
                        var memoryUser = InMemoryData.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                        if (memoryUser != null)
                        {
                            userId = memoryUser.Id;
                            System.Diagnostics.Debug.WriteLine($"Found user in memory: ID = {userId}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"User not found in memory either!");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"User is not authenticated");
                }

                // Parse the date
                DateTime appointmentDate;
                if (!DateTime.TryParse(preferredDate, out appointmentDate))
                {
                    appointmentDate = DateTime.Now.AddDays(2); // Default to 2 days from now if parsing fails
                }

                // Create the appointment
                var appointment = new BlogModel.Appointment
                {
                    UserId = userId,
                    Name = name,
                    Email = email,
                    Phone = phone,
                    Company = company ?? "",
                    ServiceType = serviceType,
                    PreferredDate = appointmentDate,
                    Message = message ?? "",
                    CreatedAt = DateTime.Now,
                    Status = "Pending"
                };
                
                System.Diagnostics.Debug.WriteLine($"Creating appointment with UserId={userId}, ServiceType={serviceType}, Date={appointmentDate}");

                // If the user is logged in but we couldn't find their ID, use their email to look them up
                if (userId == 0 && User.Identity.IsAuthenticated && !string.IsNullOrEmpty(email))
                {
                    var userByEmail = Db.Users.FirstOrDefault(u => u.Email == email);
                    if (userByEmail != null)
                    {
                        userId = userByEmail.Id;
                        appointment.UserId = userId;
                        System.Diagnostics.Debug.WriteLine($"Found user by email: ID = {userId}");
                    }
                    else
                    {
                        var memoryUserByEmail = InMemoryData.Users.FirstOrDefault(u => u.Email == email);
                        if (memoryUserByEmail != null)
                        {
                            userId = memoryUserByEmail.Id;
                            appointment.UserId = userId;
                            System.Diagnostics.Debug.WriteLine($"Found user by email in memory: ID = {userId}");
                        }
                    }
                }

                // Try to save to database first
                bool savedToDb = false;
                try
                {
                    Db.Appointments.Add(appointment);
                    Db.SaveChanges();
                    savedToDb = true;
                    System.Diagnostics.Debug.WriteLine($"Appointment saved to database for service {serviceType}");
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving appointment to database: {dbEx.Message}");
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

                // If database save failed, add to in-memory
                if (!savedToDb)
                {
                    // Assign an ID for the in-memory appointment
                    if (InMemoryData.Appointments.Any())
                    {
                        appointment.Id = InMemoryData.Appointments.Max(a => a.Id) + 1;
                    }
                    else
                    {
                        appointment.Id = 1;
                    }
                    
                    InMemoryData.Appointments.Add(appointment);
                    System.Diagnostics.Debug.WriteLine($"Appointment saved to in-memory for service {serviceType}");
                }

                TempData["SuccessMessage"] = "Solicitarea dumneavoastră de programare a fost trimisă cu succes. Vă vom contacta în curând pentru confirmare.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SubmitProgramare: {ex.Message}");
                TempData["ErrorMessage"] = "A apărut o eroare la procesarea cererii dumneavoastră. Vă rugăm să încercați din nou.";
            }
            
            // If the user is authenticated, redirect to Dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            
            return RedirectToAction("Programeaza");
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
