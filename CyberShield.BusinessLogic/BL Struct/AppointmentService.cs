using System;
using System.Collections.Generic;
using System.Linq;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Data;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class AppointmentService : IAppointmentService
    {
        private readonly CyberShieldContext _db;
        private readonly IErrorHandlingService _errorHandler;
        private readonly IValidationService _validationService;
        
        public AppointmentService()
        {
            _db = new CyberShieldContext();
        }
        
        public AppointmentService(IErrorHandlingService errorHandler, IValidationService validationService = null)
        {
            _db = new CyberShieldContext();
            _errorHandler = errorHandler;
            _validationService = validationService;
        }

        #region CRUD Operations
        
        public BlogModel.Appointment GetAppointmentById(int id)
        {
            try
            {
                return _db.Appointments.FirstOrDefault(a => a.Id == id);
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAppointmentById));
                return null;
            }
        }

        public IEnumerable<BlogModel.Appointment> GetAllAppointments()
        {
            try
            {
                return _db.Appointments.OrderByDescending(a => a.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAllAppointments));
                return new List<BlogModel.Appointment>();
            }
        }

        public IEnumerable<BlogModel.Appointment> GetAppointmentsByUser(int userId)
        {
            try
            {
                return _db.Appointments
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAppointmentsByUser));
                return new List<BlogModel.Appointment>();
            }
        }

        public IEnumerable<BlogModel.Appointment> GetAppointmentsByStatus(string status)
        {
            try
            {
                return _db.Appointments
                    .Where(a => a.Status == status)
                    .OrderBy(a => a.PreferredDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAppointmentsByStatus));
                return new List<BlogModel.Appointment>();
            }
        }

        public bool CreateAppointment(BlogModel.Appointment appointment, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // Validate appointment
                if (!ValidateAppointment(appointment, out errorMessage))
                {
                    return false;
                }
                
                // Set creation date if not set
                if (appointment.CreatedAt == default)
                {
                    appointment.CreatedAt = DateTime.Now;
                }
                
                // Set status to pending if not set
                if (string.IsNullOrEmpty(appointment.Status))
                {
                    appointment.Status = "Pending";
                }
                
                // Add appointment to database
                _db.Appointments.Add(appointment);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(CreateAppointment));
                errorMessage = "An error occurred while creating the appointment";
                return false;
            }
        }

        public bool UpdateAppointment(BlogModel.Appointment appointment, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var existingAppointment = _db.Appointments.FirstOrDefault(a => a.Id == appointment.Id);
                if (existingAppointment == null)
                {
                    errorMessage = "Appointment not found";
                    return false;
                }
                
                // Validate appointment
                if (!ValidateAppointment(appointment, out errorMessage))
                {
                    return false;
                }
                
                // Update appointment properties
                existingAppointment.Name = appointment.Name;
                existingAppointment.Email = appointment.Email;
                existingAppointment.Phone = appointment.Phone;
                existingAppointment.Company = appointment.Company;
                existingAppointment.ServiceType = appointment.ServiceType;
                existingAppointment.PreferredDate = appointment.PreferredDate;
                existingAppointment.Message = appointment.Message;
                existingAppointment.Status = appointment.Status;
                
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(UpdateAppointment));
                errorMessage = "An error occurred while updating the appointment";
                return false;
            }
        }

        public bool DeleteAppointment(int appointmentId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var appointment = _db.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                if (appointment == null)
                {
                    errorMessage = "Appointment not found";
                    return false;
                }
                
                _db.Appointments.Remove(appointment);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(DeleteAppointment));
                errorMessage = "An error occurred while deleting the appointment";
                return false;
            }
        }
        
        #endregion
        
        #region Appointment Management
        
        public bool AcceptAppointment(int appointmentId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var appointment = _db.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                if (appointment == null)
                {
                    errorMessage = "Appointment not found";
                    return false;
                }
                
                // Check if appointment is already accepted
                if (appointment.Status == "Confirmed")
                {
                    errorMessage = "Appointment is already confirmed";
                    return false;
                }
                
                // Change status to confirmed
                appointment.Status = "Confirmed";
                _db.SaveChanges();
                
                // Send confirmation notification
                SendAppointmentConfirmation(appointmentId, out string notificationError);
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(AcceptAppointment));
                errorMessage = "An error occurred while accepting the appointment";
                return false;
            }
        }

        public bool CancelAppointment(int appointmentId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var appointment = _db.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                if (appointment == null)
                {
                    errorMessage = "Appointment not found";
                    return false;
                }
                
                // Check if appointment is already cancelled
                if (appointment.Status == "Cancelled")
                {
                    errorMessage = "Appointment is already cancelled";
                    return false;
                }
                
                // Change status to cancelled
                appointment.Status = "Cancelled";
                _db.SaveChanges();
                
                // Send cancellation notification
                SendAppointmentCancellation(appointmentId, out string notificationError);
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(CancelAppointment));
                errorMessage = "An error occurred while cancelling the appointment";
                return false;
            }
        }

        public bool RescheduleAppointment(int appointmentId, DateTime newDate, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var appointment = _db.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                if (appointment == null)
                {
                    errorMessage = "Appointment not found";
                    return false;
                }
                
                // Validate new appointment date
                if (_validationService != null)
                {
                    if (!_validationService.ValidateAppointmentTime(newDate, out errorMessage))
                    {
                        return false;
                    }
                }
                else
                {
                    // Basic validation if validation service is not available
                    if (newDate <= DateTime.Now)
                    {
                        errorMessage = "Appointment date must be in the future";
                        return false;
                    }
                }
                
                // Check if the time slot is available
                if (!IsTimeSlotAvailable(newDate))
                {
                    errorMessage = "The selected time slot is not available";
                    return false;
                }
                
                // Update appointment date
                appointment.PreferredDate = newDate;
                
                // If the appointment was cancelled, set it back to pending
                if (appointment.Status == "Cancelled")
                {
                    appointment.Status = "Pending";
                }
                
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(RescheduleAppointment));
                errorMessage = "An error occurred while rescheduling the appointment";
                return false;
            }
        }
        
        #endregion
        
        #region Search and Filtering
        
        public IEnumerable<BlogModel.Appointment> GetAppointmentsByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _db.Appointments
                    .Where(a => a.PreferredDate >= startDate && a.PreferredDate <= endDate)
                    .OrderBy(a => a.PreferredDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAppointmentsByDateRange));
                return new List<BlogModel.Appointment>();
            }
        }

        public IEnumerable<BlogModel.Appointment> GetPaginatedAppointments(int pageNumber, int pageSize, out int totalCount)
        {
            totalCount = 0;
            
            try
            {
                totalCount = _db.Appointments.Count();
                
                return _db.Appointments
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetPaginatedAppointments));
                return new List<BlogModel.Appointment>();
            }
        }

        public IEnumerable<BlogModel.Appointment> GetUpcomingAppointments(int days)
        {
            try
            {
                var endDate = DateTime.Now.AddDays(days);
                
                return _db.Appointments
                    .Where(a => a.PreferredDate >= DateTime.Now && a.PreferredDate <= endDate && 
                                a.Status != "Cancelled")
                    .OrderBy(a => a.PreferredDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetUpcomingAppointments));
                return new List<BlogModel.Appointment>();
            }
        }

        public IEnumerable<BlogModel.Appointment> GetPendingAppointments()
        {
            try
            {
                return _db.Appointments
                    .Where(a => a.Status == "Pending")
                    .OrderBy(a => a.PreferredDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetPendingAppointments));
                return new List<BlogModel.Appointment>();
            }
        }

        public IEnumerable<BlogModel.Appointment> GetConfirmedAppointments()
        {
            try
            {
                return _db.Appointments
                    .Where(a => a.Status == "Confirmed")
                    .OrderBy(a => a.PreferredDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetConfirmedAppointments));
                return new List<BlogModel.Appointment>();
            }
        }
        
        #endregion
        
        #region Validation and Availability
        
        public bool IsTimeSlotAvailable(DateTime proposedTime, int durationMinutes = 60)
        {
            try
            {
                // Check if there are any appointments that overlap with the proposed time
                var endTime = proposedTime.AddMinutes(durationMinutes);
                
                return !_db.Appointments
                    .Where(a => a.Status != "Cancelled" && 
                                a.PreferredDate < endTime && 
                                proposedTime < a.PreferredDate.AddMinutes(durationMinutes))
                    .Any();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(IsTimeSlotAvailable));
                return false;
            }
        }

        public IEnumerable<DateTime> GetAvailableTimeSlots(DateTime date, int intervalMinutes = 60)
        {
            try
            {
                var availableSlots = new List<DateTime>();
                
                // Define working hours (9 AM to 5 PM)
                var startTime = new DateTime(date.Year, date.Month, date.Day, 9, 0, 0);
                var endTime = new DateTime(date.Year, date.Month, date.Day, 17, 0, 0);
                
                // Skip weekends
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    return availableSlots;
                }
                
                // Get all appointments for the specified date
                var appointmentsForDate = _db.Appointments
                    .Where(a => a.Status != "Cancelled" && 
                                a.PreferredDate.Date == date.Date)
                    .ToList();
                
                // Check each time slot
                for (var slot = startTime; slot < endTime; slot = slot.AddMinutes(intervalMinutes))
                {
                    bool isAvailable = true;
                    
                    foreach (var appointment in appointmentsForDate)
                    {
                        // Check if this appointment overlaps with the current slot
                        var appointmentEndTime = appointment.PreferredDate.AddMinutes(intervalMinutes);
                        
                        if (slot < appointmentEndTime && appointment.PreferredDate < slot.AddMinutes(intervalMinutes))
                        {
                            isAvailable = false;
                            break;
                        }
                    }
                    
                    if (isAvailable)
                    {
                        availableSlots.Add(slot);
                    }
                }
                
                return availableSlots;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAvailableTimeSlots));
                return new List<DateTime>();
            }
        }

        public bool ValidateAppointment(BlogModel.Appointment appointment, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Validate using the validation service if available
            if (_validationService != null)
            {
                // Name validation
                if (string.IsNullOrEmpty(appointment.Name))
                {
                    errorMessage = "Name is required";
                    return false;
                }
                
                // Email validation
                if (!_validationService.ValidateEmail(appointment.Email, out errorMessage))
                {
                    return false;
                }
                
                // Phone validation
                if (!_validationService.ValidatePhoneNumber(appointment.Phone, out errorMessage))
                {
                    return false;
                }
                
                // Company validation (optional)
                if (!string.IsNullOrEmpty(appointment.Company) && 
                    !_validationService.ValidateCompanyName(appointment.Company, out errorMessage))
                {
                    return false;
                }
                
                // Service type validation
                if (!_validationService.ValidateServiceType(appointment.ServiceType, out errorMessage))
                {
                    return false;
                }
                
                // Appointment time validation
                if (!_validationService.ValidateAppointmentTime(appointment.PreferredDate, out errorMessage))
                {
                    return false;
                }
            }
            else
            {
                // Basic validation if validation service is not available
                if (string.IsNullOrEmpty(appointment.Name))
                {
                    errorMessage = "Name is required";
                    return false;
                }
                
                if (string.IsNullOrEmpty(appointment.Email))
                {
                    errorMessage = "Email is required";
                    return false;
                }
                
                if (string.IsNullOrEmpty(appointment.Phone))
                {
                    errorMessage = "Phone is required";
                    return false;
                }
                
                if (string.IsNullOrEmpty(appointment.ServiceType))
                {
                    errorMessage = "Service type is required";
                    return false;
                }
                
                if (appointment.PreferredDate <= DateTime.Now)
                {
                    errorMessage = "Appointment date must be in the future";
                    return false;
                }
                
                // Check if the time slot is available
                if (!IsTimeSlotAvailable(appointment.PreferredDate))
                {
                    errorMessage = "The selected time slot is not available";
                    return false;
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region Notifications
        
        public bool SendAppointmentConfirmation(int appointmentId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var appointment = _db.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                if (appointment == null)
                {
                    errorMessage = "Appointment not found";
                    return false;
                }
                
                // In a real system, this would send an email or other notification
                // For now, just log it
                LogError($"Appointment confirmation sent for appointment ID {appointmentId}", "Notification");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(SendAppointmentConfirmation));
                errorMessage = "An error occurred while sending the confirmation";
                return false;
            }
        }

        public bool SendAppointmentReminder(int appointmentId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var appointment = _db.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                if (appointment == null)
                {
                    errorMessage = "Appointment not found";
                    return false;
                }
                
                // In a real system, this would send an email or other notification
                // For now, just log it
                LogError($"Appointment reminder sent for appointment ID {appointmentId}", "Notification");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(SendAppointmentReminder));
                errorMessage = "An error occurred while sending the reminder";
                return false;
            }
        }

        public bool SendAppointmentCancellation(int appointmentId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var appointment = _db.Appointments.FirstOrDefault(a => a.Id == appointmentId);
                if (appointment == null)
                {
                    errorMessage = "Appointment not found";
                    return false;
                }
                
                // In a real system, this would send an email or other notification
                // For now, just log it
                LogError($"Appointment cancellation sent for appointment ID {appointmentId}", "Notification");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(SendAppointmentCancellation));
                errorMessage = "An error occurred while sending the cancellation";
                return false;
            }
        }
        
        #endregion
        
        #region Statistics
        
        public int GetTotalAppointmentCount()
        {
            try
            {
                return _db.Appointments.Count();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetTotalAppointmentCount));
                return 0;
            }
        }

        public int GetPendingAppointmentCount()
        {
            try
            {
                return _db.Appointments.Count(a => a.Status == "Pending");
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetPendingAppointmentCount));
                return 0;
            }
        }

        public int GetConfirmedAppointmentCount()
        {
            try
            {
                return _db.Appointments.Count(a => a.Status == "Confirmed");
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetConfirmedAppointmentCount));
                return 0;
            }
        }

        public int GetCancelledAppointmentCount()
        {
            try
            {
                return _db.Appointments.Count(a => a.Status == "Cancelled");
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetCancelledAppointmentCount));
                return 0;
            }
        }

        public Dictionary<string, int> GetAppointmentCountByServiceType()
        {
            try
            {
                return _db.Appointments
                    .GroupBy(a => a.ServiceType)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAppointmentCountByServiceType));
                return new Dictionary<string, int>();
            }
        }

        public Dictionary<DateTime, int> GetAppointmentCountByDay(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _db.Appointments
                    .Where(a => a.PreferredDate.Date >= startDate.Date && a.PreferredDate.Date <= endDate.Date)
                    .GroupBy(a => a.PreferredDate.Date)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAppointmentCountByDay));
                return new Dictionary<DateTime, int>();
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LogError(Exception ex, string methodName)
        {
            if (_errorHandler != null)
            {
                _errorHandler.LogError(ex, $"AppointmentService.{methodName}");
            }
            else
            {
                // Fallback logging
                System.Diagnostics.Debug.WriteLine($"Error in AppointmentService.{methodName}: {ex.Message}");
            }
        }
        
        private void LogError(string message, string source)
        {
            if (_errorHandler != null)
            {
                _errorHandler.LogError(message, source);
            }
            else
            {
                // Fallback logging
                System.Diagnostics.Debug.WriteLine($"{source}: {message}");
            }
        }
        
        #endregion
    }
} 