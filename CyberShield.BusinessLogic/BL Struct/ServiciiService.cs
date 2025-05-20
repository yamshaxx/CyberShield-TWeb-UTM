using System;
using System.Linq;
using System.Text.RegularExpressions;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Data;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class ServiciiService : IServiciiService
    {
        private readonly IErrorHandlingService _errorHandler;
        private readonly IValidationService _validationService;
        private readonly CyberShieldContext _db;

        public ServiciiService(IErrorHandlingService errorHandler, IValidationService validationService)
        {
            _errorHandler = errorHandler;
            _validationService = validationService;
            _db = new CyberShieldContext();
        }

        public bool SubmitAppointment(string name, string email, string phone, string company, 
            string serviceType, string preferredDate, string message, string username = null)
        {
            try
            {
                // Validate appointment data
                if (!ValidateAppointmentData(name, email, phone, serviceType, preferredDate, out string validationError))
                {
                    _errorHandler?.LogError($"Appointment validation failed: {validationError}", "ServiciiService.SubmitAppointment");
                    return false;
                }

                // Determine user ID if authenticated
                int userId = 0;
                if (!string.IsNullOrEmpty(username))
                {
                    System.Diagnostics.Debug.WriteLine($"User is authenticated: {username}");
                    
                    // Try to find the user in database
                    var user = _db.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        userId = user.Id;
                        System.Diagnostics.Debug.WriteLine($"Found user in database: ID = {userId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"User not found in database, checking in-memory");
                        // Try to find in memory
                        var memoryUser = InMemoryData.Users.FirstOrDefault(u => u.Username == username);
                        if (memoryUser != null)
                        {
                            userId = memoryUser.Id;
                            System.Diagnostics.Debug.WriteLine($"Found user in memory: ID = {userId}");
                        }
                    }
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
                if (userId == 0 && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(email))
                {
                    var userByEmail = _db.Users.FirstOrDefault(u => u.Email == email);
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
                    _db.Appointments.Add(appointment);
                    _db.SaveChanges();
                    savedToDb = true;
                    System.Diagnostics.Debug.WriteLine($"Appointment saved to database for service {serviceType}");
                }
                catch (Exception dbEx)
                {
                    _errorHandler?.LogError(dbEx, "ServiciiService.SubmitAppointment - Database save failed");
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

                return true;
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "ServiciiService.SubmitAppointment");
                return false;
            }
        }

        public string[] GetServiceTypes()
        {
            return new string[]
            {
                "Testarea Penetrarii",
                "Audit de Securitate",
                "Consultanta in Securitate",
                "Inginerie Sociala",
                "Conformitate GDPR"
            };
        }

        public bool ValidateAppointmentData(string name, string email, string phone, 
            string serviceType, string preferredDate, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validate name
            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = "Numele este obligatoriu";
                return false;
            }

            if (name.Length < 2 || name.Length > 50)
            {
                errorMessage = "Numele trebuie să aibă între 2 și 50 de caractere";
                return false;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email-ul este obligatoriu";
                return false;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errorMessage = "Formatul email-ului nu este valid";
                return false;
            }

            // Validate phone
            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Numărul de telefon este obligatoriu";
                return false;
            }

            if (!Regex.IsMatch(phone, @"^[\d\s\-\+\(\)]{10,15}$"))
            {
                errorMessage = "Formatul numărului de telefon nu este valid";
                return false;
            }

            // Validate service type
            if (string.IsNullOrWhiteSpace(serviceType))
            {
                errorMessage = "Tipul de serviciu este obligatoriu";
                return false;
            }

            var validServiceTypes = GetServiceTypes();
            if (!validServiceTypes.Contains(serviceType))
            {
                errorMessage = "Tipul de serviciu selectat nu este valid";
                return false;
            }

            // Validate preferred date
            if (string.IsNullOrWhiteSpace(preferredDate))
            {
                errorMessage = "Data preferată este obligatorie";
                return false;
            }

            if (!DateTime.TryParse(preferredDate, out DateTime appointmentDate))
            {
                errorMessage = "Formatul datei nu este valid";
                return false;
            }

            if (appointmentDate <= DateTime.Now)
            {
                errorMessage = "Data programării trebuie să fie în viitor";
                return false;
            }

            if (appointmentDate > DateTime.Now.AddMonths(6))
            {
                errorMessage = "Data programării nu poate fi mai mult de 6 luni în viitor";
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
} 