using System;
using System.Collections.Generic;
using System.Linq;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Data;
using CyberShield.Domain.Model.User;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class UserService : IUserService
    {
        private readonly CyberShieldContext _db;
        private readonly IErrorHandlingService _errorHandler;
        
        public UserService()
        {
            _db = new CyberShieldContext();
        }
        
        public UserService(IErrorHandlingService errorHandler)
        {
            _db = new CyberShieldContext();
            _errorHandler = errorHandler;
        }

        public User GetUserById(int id)
        {
            try
            {
                return _db.Users.FirstOrDefault(u => u.Id == id);
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetUserById));
                return null;
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            try
            {
                return _db.Users.ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetAllUsers));
                return new List<User>();
            }
        }

        public IEnumerable<User> GetUsersByRole(string role)
        {
            try
            {
                if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    return _db.Users.Where(u => u.IsAdmin).ToList();
                }
                else if (role.Equals("specialist", StringComparison.OrdinalIgnoreCase))
                {
                    return _db.Users.Where(u => u.IsSpecialist).ToList();
                }
                else
                {
                    return _db.Users.Where(u => !u.IsAdmin && !u.IsSpecialist).ToList();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetUsersByRole));
                return new List<User>();
            }
        }

        public bool CreateUser(User user, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                _db.Users.Add(user);
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(CreateUser));
                errorMessage = "An error occurred while creating the user";
                return false;
            }
        }

        public bool UpdateUser(User user, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var existingUser = _db.Users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                // Update user properties
                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.IsAdmin = user.IsAdmin;
                existingUser.IsSpecialist = user.IsSpecialist;
                
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(UpdateUser));
                errorMessage = "An error occurred while updating the user";
                return false;
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
        
        public IEnumerable<User> SearchUsers(string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return GetAllUsers();
                }
                
                return _db.Users
                    .Where(u => u.Username.Contains(searchTerm) || 
                                u.Email.Contains(searchTerm))
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(SearchUsers));
                return new List<User>();
            }
        }

        public IEnumerable<User> GetUsersSortedBy(string sortField, bool ascending = true)
        {
            try
            {
                IQueryable<User> query = _db.Users;
                
                switch (sortField.ToLower())
                {
                    case "username":
                        query = ascending ? query.OrderBy(u => u.Username) : query.OrderByDescending(u => u.Username);
                        break;
                    case "email":
                        query = ascending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email);
                        break;
                    default:
                        query = ascending ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id);
                        break;
                }
                
                return query.ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetUsersSortedBy));
                return new List<User>();
            }
        }

        public IEnumerable<User> GetPaginatedUsers(int pageNumber, int pageSize, out int totalCount)
        {
            totalCount = 0;
            
            try
            {
                totalCount = _db.Users.Count();
                
                return _db.Users
                    .OrderBy(u => u.Username)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetPaginatedUsers));
                return new List<User>();
            }
        }

        public bool UpdateUserProfile(int userId, string email, string displayName, out string errorMessage)
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
                
                user.Email = email;
                // Currently there is no display name property in the User model
                // user.DisplayName = displayName;
                
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(UpdateUserProfile));
                errorMessage = "An error occurred while updating the user profile";
                return false;
            }
        }

        public bool UploadUserAvatar(int userId, byte[] imageData, string contentType, out string errorMessage)
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
                
                // Currently there is no avatar property in the User model
                // user.AvatarData = imageData;
                // user.AvatarContentType = contentType;
                
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(UploadUserAvatar));
                errorMessage = "An error occurred while uploading the user avatar";
                return false;
            }
        }

        public byte[] GetUserAvatar(int userId)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);
                
                // Currently there is no avatar property in the User model
                // return user?.AvatarData;
                
                return null;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetUserAvatar));
                return null;
            }
        }

        public bool LogUserActivity(int userId, string activity)
        {
            try
            {
                // This would typically save to an activity log table
                // But we'll just log it for now
                LogError($"User ID {userId}: {activity}", "UserActivity");
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(LogUserActivity));
                return false;
            }
        }

        public DateTime? GetLastLoginTime(int userId)
        {
            // This would typically come from a user activity or sessions table
            // But we don't have that implemented yet
            return null;
        }

        public IEnumerable<string> GetUserActivityLog(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // This would typically come from a user activity log table
            // But we don't have that implemented yet
            return new List<string>();
        }

        public int GetTotalUserCount()
        {
            try
            {
                return _db.Users.Count();
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetTotalUserCount));
                return 0;
            }
        }

        public int GetActiveUserCount()
        {
            // This would typically count users who have logged in recently
            // But we don't have that implemented yet, so just return total
            return GetTotalUserCount();
        }

        public Dictionary<string, int> GetUserCountByRole()
        {
            try
            {
                var result = new Dictionary<string, int>();
                
                result["Admin"] = _db.Users.Count(u => u.IsAdmin);
                result["Specialist"] = _db.Users.Count(u => u.IsSpecialist);
                result["Regular"] = _db.Users.Count(u => !u.IsAdmin && !u.IsSpecialist);
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, nameof(GetUserCountByRole));
                return new Dictionary<string, int>();
            }
        }
        
        private void LogError(Exception ex, string method)
        {
            if (_errorHandler != null)
            {
                _errorHandler.LogError(ex, $"UserService.{method}");
            }
            else
            {
                // Fallback logging
                System.Diagnostics.Debug.WriteLine($"Error in UserService.{method}: {ex.Message}");
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
    }
} 