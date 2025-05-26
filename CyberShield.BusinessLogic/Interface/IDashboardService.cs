using System.Collections.Generic;
using UserModel = CyberShield.Domain.Model.User;
using BlogModel = CyberShield.Domain.Model.Blog;
using CyberShield.Domain.Model;

namespace CyberShield.BusinessLogic.Interface
{
    /// <summary>
    /// Interface for managing dashboard functionality
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Gets user dashboard data including comments, appointments, and messages
        /// </summary>
        /// <param name="username">Username of the authenticated user</param>
        /// <returns>Dashboard data object</returns>
        object GetUserDashboardData(string username);
        
        /// <summary>
        /// Gets specialist dashboard data including appointments and contact messages
        /// </summary>
        /// <param name="username">Username of the authenticated specialist</param>
        /// <returns>Specialist dashboard data object</returns>
        object GetSpecialistDashboardData(string username);
        
        /// <summary>
        /// Checks if user is a specialist
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if user is specialist, false otherwise</returns>
        bool IsUserSpecialist(string username);
        
        /// <summary>
        /// Gets user comments with blog post information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of comments with blog post details</returns>
        IEnumerable<object> GetUserComments(int userId);
        
        /// <summary>
        /// Gets user appointments
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user appointments</returns>
        IEnumerable<BlogModel.Appointment> GetUserAppointments(int userId);
        
        /// <summary>
        /// Gets user contact messages
        /// </summary>
        /// <param name="userEmail">User email</param>
        /// <returns>List of contact messages</returns>
        IEnumerable<ContactMessage> GetUserContactMessages(string userEmail);
        
        /// <summary>
        /// Gets all contact messages for specialist dashboard
        /// </summary>
        /// <returns>List of all contact messages</returns>
        IEnumerable<ContactMessage> GetAllContactMessages();
    }
} 