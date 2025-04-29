using System;
using System.Collections.Generic;
using BlogModel = CyberShield.Domain.Model.Blog;
using UserModel = CyberShield.Domain.Model.User;

namespace CyberShield.BusinessLogic.Interface
{
    public interface IAdminService
    {
        // User management
        IEnumerable<UserModel.User> GetAllUsers();
        bool DeleteUser(int userId, out string errorMessage);
        bool ResetUserPassword(int userId, string newPassword, out string errorMessage);
        bool ToggleUserAdmin(int userId, bool isAdmin, out string errorMessage);
        bool ToggleUserSpecialist(int userId, bool isSpecialist, out string errorMessage);
        
        // Content management
        IEnumerable<BlogModel.BlogPost> GetAllBlogPosts();
        bool DeleteBlogPost(int postId, out string errorMessage);
        bool ModerateComment(int commentId, bool approved, out string errorMessage);
        IEnumerable<BlogModel.Comment> GetFlaggedComments();
        
        // System management
        bool BackupDatabase(string location, out string errorMessage);
        bool RestoreDatabase(string backupFile, out string errorMessage);
        Dictionary<string, string> GetSystemConfiguration();
        bool UpdateSystemConfiguration(Dictionary<string, string> config, out string errorMessage);
        
        // Security management
        IEnumerable<string> GetSecurityAuditLog(DateTime? fromDate = null, DateTime? toDate = null);
        bool LockUserAccount(int userId, out string errorMessage);
        bool UnlockUserAccount(int userId, out string errorMessage);
        bool ForcePasswordChange(int userId, out string errorMessage);
        
        // Analytics
        Dictionary<string, int> GetUserRegistrationsByMonth(int months);
        Dictionary<string, int> GetBlogPostsByMonth(int months);
        Dictionary<string, int> GetCommentsByMonth(int months);
        Dictionary<string, int> GetActiveUsersByDay(int days);
        
        // Appointment management
        IEnumerable<BlogModel.Appointment> GetAllAppointments();
        bool CancelAppointment(int appointmentId, out string errorMessage);
        Dictionary<string, int> GetAppointmentsByStatus();
        Dictionary<string, int> GetAppointmentsByServiceType();
    }
} 