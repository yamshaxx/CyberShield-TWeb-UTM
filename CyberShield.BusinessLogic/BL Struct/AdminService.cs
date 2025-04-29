using System;
using System.Collections.Generic;
using System.Linq;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Data;
using BlogModel = CyberShield.Domain.Model.Blog;
using UserModel = CyberShield.Domain.Model.User;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class AdminService : IAdminService
    {
        private readonly CyberShieldContext _db;
        private readonly IErrorHandlingService _errorHandler;
        private readonly IAuth _authService;
        
        public AdminService()
        {
            _db = new CyberShieldContext();
        }
        
        public AdminService(IErrorHandlingService errorHandler, IAuth authService = null)
        {
            _db = new CyberShieldContext();
            _errorHandler = errorHandler;
            _authService = authService;
        }

        #region User Management
        
        public IEnumerable<UserModel.User> GetAllUsers()
        {
            try
            {
                return _db.Users.ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAllUsers));
                return new List<UserModel.User>();
            }
        }

        public bool DeleteUser(int userId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                _db.Users.Remove(user);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(DeleteUser));
                errorMessage = "An error occurred while deleting the user";
                return false;
            }
        }

        public bool ResetUserPassword(int userId, string newPassword, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                if (_authService != null)
                {
                    if (!_authService.ValidatePassword(newPassword, out errorMessage))
                    {
                        return false;
                    }
                    
                    user.PasswordHash = _authService.HashPassword(newPassword);
                }
                else
                {
                    // If auth service is not available, use a placeholder password hash
                    errorMessage = "Auth service not available";
                    return false;
                }
                
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(ResetUserPassword));
                errorMessage = "An error occurred while resetting the user password";
                return false;
            }
        }

        public bool ToggleUserAdmin(int userId, bool isAdmin, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                user.IsAdmin = isAdmin;
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(ToggleUserAdmin));
                errorMessage = "An error occurred while toggling admin status";
                return false;
            }
        }

        public bool ToggleUserSpecialist(int userId, bool isSpecialist, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                user.IsSpecialist = isSpecialist;
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(ToggleUserSpecialist));
                errorMessage = "An error occurred while toggling specialist status";
                return false;
            }
        }
        
        #endregion
        
        #region Content Management
        
        public IEnumerable<BlogModel.BlogPost> GetAllBlogPosts()
        {
            try
            {
                return _db.BlogPosts.OrderByDescending(p => p.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAllBlogPosts));
                return new List<BlogModel.BlogPost>();
            }
        }

        public bool DeleteBlogPost(int postId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var post = _db.BlogPosts.FirstOrDefault(p => p.Id == postId);
                if (post == null)
                {
                    errorMessage = "Blog post not found";
                    return false;
                }
                
                // Remove associated comments first
                var comments = _db.Comments.Where(c => c.BlogPostId == postId).ToList();
                foreach (var comment in comments)
                {
                    _db.Comments.Remove(comment);
                }
                
                // Remove blog post
                _db.BlogPosts.Remove(post);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(DeleteBlogPost));
                errorMessage = "An error occurred while deleting the blog post";
                return false;
            }
        }

        public bool ModerateComment(int commentId, bool approved, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);
                if (comment == null)
                {
                    errorMessage = "Comment not found";
                    return false;
                }
                
                // For now, we're just deleting disapproved comments
                // In a more advanced system, we could have an IsApproved flag
                if (!approved)
                {
                    _db.Comments.Remove(comment);
                    _db.SaveChanges();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(ModerateComment));
                errorMessage = "An error occurred while moderating the comment";
                return false;
            }
        }

        public IEnumerable<BlogModel.Comment> GetFlaggedComments()
        {
            try
            {
                // In a more advanced system, we would have a flag for flagged comments
                // For now, just return all comments for moderation
                return _db.Comments.OrderByDescending(c => c.PostedAt).ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetFlaggedComments));
                return new List<BlogModel.Comment>();
            }
        }
        
        #endregion
        
        #region System Management
        
        public bool BackupDatabase(string location, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // This would typically use SQL Server's backup functionality
                // For now, just log it
                LogError($"Database backup requested to {location}", "SystemManagement");
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(BackupDatabase));
                errorMessage = "An error occurred while backing up the database";
                return false;
            }
        }

        public bool RestoreDatabase(string backupFile, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // This would typically use SQL Server's restore functionality
                // For now, just log it
                LogError($"Database restore requested from {backupFile}", "SystemManagement");
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(RestoreDatabase));
                errorMessage = "An error occurred while restoring the database";
                return false;
            }
        }

        public Dictionary<string, string> GetSystemConfiguration()
        {
            try
            {
                // In a real system, this would load from a configuration table
                // For now, just return a placeholder
                return new Dictionary<string, string>
                {
                    ["ApplicationName"] = "CyberShield",
                    ["Version"] = "1.0.0",
                    ["DatabaseConnection"] = "Connected"
                };
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetSystemConfiguration));
                return new Dictionary<string, string>();
            }
        }

        public bool UpdateSystemConfiguration(Dictionary<string, string> config, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // In a real system, this would save to a configuration table
                // For now, just log it
                foreach (var kvp in config)
                {
                    LogError($"Configuration update: {kvp.Key} = {kvp.Value}", "SystemManagement");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(UpdateSystemConfiguration));
                errorMessage = "An error occurred while updating the system configuration";
                return false;
            }
        }
        
        #endregion
        
        #region Security Management
        
        public IEnumerable<string> GetSecurityAuditLog(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // In a real system, this would query a security audit log table
                // For now, just return a placeholder
                return new List<string>
                {
                    "Security audit logs would be shown here"
                };
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetSecurityAuditLog));
                return new List<string>();
            }
        }

        public bool LockUserAccount(int userId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                // In a more advanced system, we would have an IsLocked flag
                // For now, just log it
                LogError($"User account locked: {user.Username}", "SecurityManagement");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(LockUserAccount));
                errorMessage = "An error occurred while locking the user account";
                return false;
            }
        }

        public bool UnlockUserAccount(int userId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                // In a more advanced system, we would have an IsLocked flag
                // For now, just log it
                LogError($"User account unlocked: {user.Username}", "SecurityManagement");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(UnlockUserAccount));
                errorMessage = "An error occurred while unlocking the user account";
                return false;
            }
        }

        public bool ForcePasswordChange(int userId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                // In a more advanced system, we would have a force password change flag
                // For now, just log it
                LogError($"Forced password change for user: {user.Username}", "SecurityManagement");
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(ForcePasswordChange));
                errorMessage = "An error occurred while forcing password change";
                return false;
            }
        }
        
        #endregion
        
        #region Analytics
        
        public Dictionary<string, int> GetUserRegistrationsByMonth(int months)
        {
            try
            {
                // In a real system, this would query user registration dates
                // For now, just return a placeholder
                var result = new Dictionary<string, int>();
                
                var currentDate = DateTime.Now;
                for (int i = 0; i < months; i++)
                {
                    var date = currentDate.AddMonths(-i);
                    var month = date.ToString("MMMM yyyy");
                    result[month] = new Random().Next(5, 20); // Placeholder random values
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetUserRegistrationsByMonth));
                return new Dictionary<string, int>();
            }
        }

        public Dictionary<string, int> GetBlogPostsByMonth(int months)
        {
            try
            {
                var result = new Dictionary<string, int>();
                var currentDate = DateTime.Now;
                
                for (int i = 0; i < months; i++)
                {
                    var date = currentDate.AddMonths(-i);
                    var month = date.ToString("MMMM yyyy");
                    
                    int count = _db.BlogPosts.Count(p => 
                        p.PostedDate.Year == date.Year && 
                        p.PostedDate.Month == date.Month);
                    
                    result[month] = count;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetBlogPostsByMonth));
                return new Dictionary<string, int>();
            }
        }

        public Dictionary<string, int> GetCommentsByMonth(int months)
        {
            try
            {
                var result = new Dictionary<string, int>();
                var currentDate = DateTime.Now;
                
                for (int i = 0; i < months; i++)
                {
                    var date = currentDate.AddMonths(-i);
                    var month = date.ToString("MMMM yyyy");
                    
                    int count = _db.Comments.Count(c => 
                        c.PostedAt.Year == date.Year && 
                        c.PostedAt.Month == date.Month);
                    
                    result[month] = count;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetCommentsByMonth));
                return new Dictionary<string, int>();
            }
        }

        public Dictionary<string, int> GetActiveUsersByDay(int days)
        {
            try
            {
                // In a real system, this would query user activity logs
                // For now, just return a placeholder
                var result = new Dictionary<string, int>();
                
                var currentDate = DateTime.Now;
                for (int i = 0; i < days; i++)
                {
                    var date = currentDate.AddDays(-i);
                    var day = date.ToString("MMM dd");
                    result[day] = new Random().Next(10, 50); // Placeholder random values
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetActiveUsersByDay));
                return new Dictionary<string, int>();
            }
        }
        
        #endregion
        
        #region Appointment Management
        
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
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(CancelAppointment));
                errorMessage = "An error occurred while cancelling the appointment";
                return false;
            }
        }

        public Dictionary<string, int> GetAppointmentsByStatus()
        {
            try
            {
                return _db.Appointments
                    .GroupBy(a => a.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAppointmentsByStatus));
                return new Dictionary<string, int>();
            }
        }

        public Dictionary<string, int> GetAppointmentsByServiceType()
        {
            try
            {
                return _db.Appointments
                    .GroupBy(a => a.ServiceType)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAppointmentsByServiceType));
                return new Dictionary<string, int>();
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LogError(Exception ex, string methodName)
        {
            if (_errorHandler != null)
            {
                _errorHandler.LogError(ex, $"AdminService.{methodName}");
            }
            else
            {
                // Fallback logging
                System.Diagnostics.Debug.WriteLine($"Error in AdminService.{methodName}: {ex.Message}");
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