using System.Collections.Generic;

namespace CyberShield.BusinessLogic.Interface
{
    /// <summary>
    /// Interface for managing help and support functionality
    /// </summary>
    public interface IHelpService
    {
        /// <summary>
        /// Gets help content for the index page
        /// </summary>
        /// <returns>Help content as string</returns>
        string GetHelpContent();
        
        /// <summary>
        /// Gets frequently asked questions
        /// </summary>
        /// <returns>Dictionary of questions and answers</returns>
        Dictionary<string, string> GetFAQs();
        
        /// <summary>
        /// Gets troubleshooting guides
        /// </summary>
        /// <returns>List of troubleshooting guides</returns>
        IEnumerable<string> GetTroubleshootingGuides();
        
        /// <summary>
        /// Logs help request for analytics
        /// </summary>
        /// <param name="section">Help section accessed</param>
        /// <param name="username">Username if authenticated</param>
        void LogHelpRequest(string section, string username = null);
    }
} 