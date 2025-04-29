using System;
using System.Collections.Generic;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShield.BusinessLogic.Interface
{
    public interface IAppointmentService
    {
        // Appointment CRUD operations
        BlogModel.Appointment GetAppointmentById(int id);
        IEnumerable<BlogModel.Appointment> GetAllAppointments();
        IEnumerable<BlogModel.Appointment> GetAppointmentsByUser(int userId);
        IEnumerable<BlogModel.Appointment> GetAppointmentsByStatus(string status);
        bool CreateAppointment(BlogModel.Appointment appointment, out string errorMessage);
        bool UpdateAppointment(BlogModel.Appointment appointment, out string errorMessage);
        bool DeleteAppointment(int appointmentId, out string errorMessage);
        
        // Appointment management
        bool AcceptAppointment(int appointmentId, out string errorMessage);
        bool CancelAppointment(int appointmentId, out string errorMessage);
        bool RescheduleAppointment(int appointmentId, DateTime newDate, out string errorMessage);
        
        // Appointment search and filtering
        IEnumerable<BlogModel.Appointment> GetAppointmentsByDateRange(DateTime startDate, DateTime endDate);
        IEnumerable<BlogModel.Appointment> GetPaginatedAppointments(int pageNumber, int pageSize, out int totalCount);
        IEnumerable<BlogModel.Appointment> GetUpcomingAppointments(int days);
        IEnumerable<BlogModel.Appointment> GetPendingAppointments();
        IEnumerable<BlogModel.Appointment> GetConfirmedAppointments();
        
        // Appointment validation and availability
        bool IsTimeSlotAvailable(DateTime proposedTime, int durationMinutes = 60);
        IEnumerable<DateTime> GetAvailableTimeSlots(DateTime date, int intervalMinutes = 60);
        bool ValidateAppointment(BlogModel.Appointment appointment, out string errorMessage);
        
        // Appointment notifications
        bool SendAppointmentConfirmation(int appointmentId, out string errorMessage);
        bool SendAppointmentReminder(int appointmentId, out string errorMessage);
        bool SendAppointmentCancellation(int appointmentId, out string errorMessage);
        
        // Appointment statistics
        int GetTotalAppointmentCount();
        int GetPendingAppointmentCount();
        int GetConfirmedAppointmentCount();
        int GetCancelledAppointmentCount();
        Dictionary<string, int> GetAppointmentCountByServiceType();
        Dictionary<DateTime, int> GetAppointmentCountByDay(DateTime startDate, DateTime endDate);
    }
} 