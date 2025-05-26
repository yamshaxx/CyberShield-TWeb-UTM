using System;
using System.Collections.Generic;
using CyberShield.Domain.Model.User;

namespace CyberShield.BusinessLogic.Interface
{
    public interface IUserService
    {
        // User CRUD operations
        User GetUserById(int id);
        User GetUserByUsername(string username);
        IEnumerable<User> GetAllUsers();
        IEnumerable<User> GetUsersByRole(string role);
        bool CreateUser(User user, out string errorMessage);
        bool UpdateUser(User user, out string errorMessage);
        bool DeleteUser(int userId, out string errorMessage);
        
        // User search and filtering
        IEnumerable<User> SearchUsers(string searchTerm);
        IEnumerable<User> GetUsersSortedBy(string sortField, bool ascending = true);
        IEnumerable<User> GetPaginatedUsers(int pageNumber, int pageSize, out int totalCount);
        
        // User profile management
        bool UpdateUserProfile(int userId, string email, string displayName, out string errorMessage);
        bool UploadUserAvatar(int userId, byte[] imageData, string contentType, out string errorMessage);
        byte[] GetUserAvatar(int userId);
        
        // User session management
        bool LogUserActivity(int userId, string activity);
        DateTime? GetLastLoginTime(int userId);
        IEnumerable<string> GetUserActivityLog(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        
        // User statistics
        int GetTotalUserCount();
        int GetActiveUserCount();
        Dictionary<string, int> GetUserCountByRole();
    }
} 