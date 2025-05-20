using System;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShield.BusinessLogic.Interface
{
    /// <summary>
    /// Interface for managing services and appointments
    /// </summary>
    public interface IServiciiService
    {
        /// <summary>
        /// Submits a new appointment request
        /// </summary>
        /// <param name="name">Client name</param>
        /// <param name="email">Client email</param>
        /// <param name="phone">Client phone</param>
        /// <param name="company">Client company</param>
        /// <param name="serviceType">Type of service requested</param>
        /// <param name="preferredDate">Preferred appointment date</param>
        /// <param name="message">Additional message</param>
        /// <param name="username">Authenticated username (if any)</param>
        /// <returns>True if successful, false otherwise</returns>
        bool SubmitAppointment(string name, string email, string phone, string company, 
            string serviceType, string preferredDate, string message, string username = null);
        
        /// <summary>
        /// Gets available service types
        /// </summary>
        /// <returns>List of service types</returns>
        string[] GetServiceTypes();
        
        /// <summary>
        /// Validates appointment data
        /// </summary>
        /// <param name="name">Client name</param>
        /// <param name="email">Client email</param>
        /// <param name="phone">Client phone</param>
        /// <param name="serviceType">Service type</param>
        /// <param name="preferredDate">Preferred date</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateAppointmentData(string name, string email, string phone, 
            string serviceType, string preferredDate, out string errorMessage);
    }
} 