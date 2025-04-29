using System;
using System.Collections.Generic;

namespace CyberShield.BusinessLogic.Interface
{
    public interface IErrorHandlingService
    {
        // Error logging
        void LogError(Exception ex, string source);
        void LogError(string errorMessage, string source);
        void LogError(string errorMessage, string source, Dictionary<string, object> additionalData);
        
        // Error retrieval
        IEnumerable<ErrorLogEntry> GetRecentErrors(int count);
        IEnumerable<ErrorLogEntry> GetErrorsByDateRange(DateTime startDate, DateTime endDate);
        IEnumerable<ErrorLogEntry> GetErrorsBySource(string source);
        ErrorLogEntry GetErrorById(int id);
        
        // Error analysis
        Dictionary<string, int> GetErrorCountBySource();
        Dictionary<DateTime, int> GetErrorCountByDay(DateTime startDate, DateTime endDate);
        IEnumerable<string> GetMostFrequentErrors(int count);
        
        // Error handling
        void HandleDatabaseException(Exception ex, out string userFriendlyMessage);
        void HandleValidationException(Exception ex, out string userFriendlyMessage);
        void HandleAuthorizationException(Exception ex, out string userFriendlyMessage);
        void HandleFileSystemException(Exception ex, out string userFriendlyMessage);
        
        // Notification
        bool NotifyAdminOfCriticalError(Exception ex, string source);
        bool NotifyAdminOfCriticalError(string errorMessage, string source);
    }
    
    public class ErrorLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string AdditionalData { get; set; }
        public string UserName { get; set; }
        public string UserIpAddress { get; set; }
        public string RequestUrl { get; set; }
    }
} 