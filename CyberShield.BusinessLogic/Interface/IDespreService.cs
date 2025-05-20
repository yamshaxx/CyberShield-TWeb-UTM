namespace CyberShield.BusinessLogic.Interface
{
    /// <summary>
    /// Interface for managing about/despre functionality
    /// </summary>
    public interface IDespreService
    {
        /// <summary>
        /// Gets company information content
        /// </summary>
        /// <returns>Company information as string</returns>
        string GetCompanyInfo();
        
        /// <summary>
        /// Gets team member information
        /// </summary>
        /// <returns>Team information</returns>
        object GetTeamInfo();
        
        /// <summary>
        /// Gets company history and mission
        /// </summary>
        /// <returns>Company history and mission</returns>
        string GetCompanyHistory();
        
        /// <summary>
        /// Logs page visit for analytics
        /// </summary>
        /// <param name="section">Section visited</param>
        /// <param name="username">Username if authenticated</param>
        void LogPageVisit(string section, string username = null);
    }
} 