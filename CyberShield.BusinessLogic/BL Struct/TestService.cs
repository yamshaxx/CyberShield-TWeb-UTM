using System;
using CyberShield.BusinessLogic.Interface;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class TestService : ITestService
    {
        private readonly IErrorHandlingService _errorHandler;

        public TestService(IErrorHandlingService errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public string GetTestContent()
        {
            return "Test controller is working. The routing system is functioning.";
        }

        public string GetTestContactContent()
        {
            return "This is a test contact action. If you can see this, routing to /Test/Contact works.";
        }

        public bool PerformHealthCheck()
        {
            try
            {
                // Basic system health checks
                var memoryUsage = GC.GetTotalMemory(false);
                var currentTime = DateTime.Now;
                
                // Simple checks - in a real system, you'd check database connectivity,
                // external services, disk space, etc.
                bool systemHealthy = memoryUsage > 0 && currentTime.Year > 2020;
                
                LogTestAccess($"HealthCheck - Result: {systemHealthy}, Memory: {memoryUsage}, Time: {currentTime}");
                
                return systemHealthy;
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "TestService.PerformHealthCheck");
                return false;
            }
        }

        public void LogTestAccess(string action, string username = null)
        {
            try
            {
                var logMessage = $"Test action: {action}";
                if (!string.IsNullOrEmpty(username))
                {
                    logMessage += $" by user: {username}";
                }
                
                _errorHandler?.LogError(logMessage, "TestService.LogTestAccess");
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "TestService.LogTestAccess");
            }
        }
    }
} 