namespace CyberShield.BusinessLogic.Interface
{
    /// <summary>
    /// Interface for managing test functionality
    /// </summary>
    public interface ITestService
    {
        /// <summary>
        /// Gets test content for index page
        /// </summary>
        /// <returns>Test content as string</returns>
        string GetTestContent();
        
        /// <summary>
        /// Gets test contact content
        /// </summary>
        /// <returns>Test contact content as string</returns>
        string GetTestContactContent();
        
        /// <summary>
        /// Performs system health check
        /// </summary>
        /// <returns>True if system is healthy, false otherwise</returns>
        bool PerformHealthCheck();
        
        /// <summary>
        /// Logs test access for monitoring
        /// </summary>
        /// <param name="action">Test action performed</param>
        /// <param name="username">Username if authenticated</param>
        void LogTestAccess(string action, string username = null);
    }
} 