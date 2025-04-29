using System;
using System.Collections.Generic;
using System.Linq;
using CyberShield.BusinessLogic.Interface;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly List<ErrorLogEntry> _errorLog = new List<ErrorLogEntry>();
        private readonly int _maxLogEntries = 1000;
        
        public ErrorHandlingService()
        {
        }
        
        public void LogError(Exception ex, string source)
        {
            try
            {
                var entry = new ErrorLogEntry
                {
                    Id = _errorLog.Count > 0 ? _errorLog.Max(e => e.Id) + 1 : 1,
                    Timestamp = DateTime.Now,
                    Source = source,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    AdditionalData = ex.InnerException != null ? ex.InnerException.Message : null
                };
                
                // Add to in-memory log
                _errorLog.Add(entry);
                
                // Trim log if it gets too large
                if (_errorLog.Count > _maxLogEntries)
                {
                    _errorLog.RemoveRange(0, _errorLog.Count - _maxLogEntries);
                }
                
                // Also log to debug output
                System.Diagnostics.Debug.WriteLine($"ERROR [{entry.Timestamp}] in {source}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
            catch
            {
                // Fallback if error logging itself fails
                System.Diagnostics.Debug.WriteLine($"ERROR logging failed for {source}: {ex.Message}");
            }
        }
        
        public void LogError(string errorMessage, string source)
        {
            try
            {
                var entry = new ErrorLogEntry
                {
                    Id = _errorLog.Count > 0 ? _errorLog.Max(e => e.Id) + 1 : 1,
                    Timestamp = DateTime.Now,
                    Source = source,
                    ErrorMessage = errorMessage
                };
                
                // Add to in-memory log
                _errorLog.Add(entry);
                
                // Trim log if it gets too large
                if (_errorLog.Count > _maxLogEntries)
                {
                    _errorLog.RemoveRange(0, _errorLog.Count - _maxLogEntries);
                }
                
                // Also log to debug output
                System.Diagnostics.Debug.WriteLine($"LOG [{entry.Timestamp}] in {source}: {errorMessage}");
            }
            catch
            {
                // Fallback if error logging itself fails
                System.Diagnostics.Debug.WriteLine($"ERROR logging failed for {source}: {errorMessage}");
            }
        }
        
        public void LogError(string errorMessage, string source, Dictionary<string, object> additionalData)
        {
            try
            {
                // Convert additional data to string representation
                var additionalDataString = string.Join("; ", additionalData.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                
                var entry = new ErrorLogEntry
                {
                    Id = _errorLog.Count > 0 ? _errorLog.Max(e => e.Id) + 1 : 1,
                    Timestamp = DateTime.Now,
                    Source = source,
                    ErrorMessage = errorMessage,
                    AdditionalData = additionalDataString
                };
                
                // Add to in-memory log
                _errorLog.Add(entry);
                
                // Trim log if it gets too large
                if (_errorLog.Count > _maxLogEntries)
                {
                    _errorLog.RemoveRange(0, _errorLog.Count - _maxLogEntries);
                }
                
                // Also log to debug output
                System.Diagnostics.Debug.WriteLine($"LOG [{entry.Timestamp}] in {source}: {errorMessage}");
                System.Diagnostics.Debug.WriteLine($"Additional data: {additionalDataString}");
            }
            catch
            {
                // Fallback if error logging itself fails
                System.Diagnostics.Debug.WriteLine($"ERROR logging failed for {source}: {errorMessage}");
            }
        }
        
        public IEnumerable<ErrorLogEntry> GetRecentErrors(int count)
        {
            return _errorLog.OrderByDescending(e => e.Timestamp).Take(count).ToList();
        }
        
        public IEnumerable<ErrorLogEntry> GetErrorsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _errorLog
                .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }
        
        public IEnumerable<ErrorLogEntry> GetErrorsBySource(string source)
        {
            return _errorLog
                .Where(e => e.Source.Contains(source))
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }
        
        public ErrorLogEntry GetErrorById(int id)
        {
            return _errorLog.FirstOrDefault(e => e.Id == id);
        }
        
        public Dictionary<string, int> GetErrorCountBySource()
        {
            return _errorLog
                .GroupBy(e => e.Source)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        
        public Dictionary<DateTime, int> GetErrorCountByDay(DateTime startDate, DateTime endDate)
        {
            return _errorLog
                .Where(e => e.Timestamp.Date >= startDate.Date && e.Timestamp.Date <= endDate.Date)
                .GroupBy(e => e.Timestamp.Date)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        
        public IEnumerable<string> GetMostFrequentErrors(int count)
        {
            return _errorLog
                .GroupBy(e => e.ErrorMessage)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToList();
        }
        
        public void HandleDatabaseException(Exception ex, out string userFriendlyMessage)
        {
            LogError(ex, "Database");
            userFriendlyMessage = "A database error occurred. Please try again later.";
        }
        
        public void HandleValidationException(Exception ex, out string userFriendlyMessage)
        {
            LogError(ex, "Validation");
            userFriendlyMessage = "The submitted data is invalid. Please check your inputs and try again.";
        }
        
        public void HandleAuthorizationException(Exception ex, out string userFriendlyMessage)
        {
            LogError(ex, "Authorization");
            userFriendlyMessage = "You do not have permission to perform this action.";
        }
        
        public void HandleFileSystemException(Exception ex, out string userFriendlyMessage)
        {
            LogError(ex, "FileSystem");
            userFriendlyMessage = "An error occurred while processing files. Please try again later.";
        }
        
        public bool NotifyAdminOfCriticalError(Exception ex, string source)
        {
            try
            {
                // In a real system, this would send an email or other notification
                // For now, just log it with a special prefix
                LogError($"CRITICAL: {ex.Message}", source);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public bool NotifyAdminOfCriticalError(string errorMessage, string source)
        {
            try
            {
                // In a real system, this would send an email or other notification
                // For now, just log it with a special prefix
                LogError($"CRITICAL: {errorMessage}", source);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 