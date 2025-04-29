using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Data;
using CyberShield.Domain.Model.User;

namespace CyberShield.BusinessLogic
{
    public class AuthBL : IAuth
    {
        private readonly CyberShieldContext _db;
        private readonly IErrorHandlingService _errorHandler;
        
        public AuthBL()
        {
            _db = new CyberShieldContext();
        }
        
        public AuthBL(IErrorHandlingService errorHandler)
        {
            _db = new CyberShieldContext();
            _errorHandler = errorHandler;
        }

        public string UserAuthLogic(UserLoginDTO userData)
        {
            try
            {
                if (string.IsNullOrEmpty(userData.UserName) || string.IsNullOrEmpty(userData.Password))
                {
                    return "Invalid credentials";
                }

                var user = _db.Users.FirstOrDefault(u => u.Username == userData.UserName);
                if (user == null)
                {
                    return "User not found";
                }
                
                if (!VerifyPassword(user.PasswordHash, userData.Password))
                {
                    return "Invalid password";
                }
                
                // Log successful login
                if (_errorHandler != null)
                {
                    _errorHandler.LogError($"User {userData.UserName} logged in successfully from {userData.UserIp}", "Authentication");
                }
                
                return "Authentication successful";
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.UserAuthLogic");
                }
                return "An error occurred during authentication";
            }
        }
        
        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            try
            {
                return Crypto.VerifyHashedPassword(hashedPassword, providedPassword);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.VerifyPassword");
                }
                return false;
            }
        }
        
        public string HashPassword(string password)
        {
            try
            {
                return Crypto.HashPassword(password);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.HashPassword");
                }
                return null;
            }
        }
        
        public bool ValidatePassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(password))
            {
                errorMessage = "Password cannot be empty";
                return false;
            }
            
            if (password.Length < 8)
            {
                errorMessage = "Password must be at least 8 characters long";
                return false;
            }
            
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "Password must contain at least one uppercase letter";
                return false;
            }
            
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errorMessage = "Password must contain at least one lowercase letter";
                return false;
            }
            
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                errorMessage = "Password must contain at least one digit";
                return false;
            }
            
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
            {
                errorMessage = "Password must contain at least one special character";
                return false;
            }
            
            return true;
        }
        
        public bool RegisterUser(User user, string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // Validate user data
                if (string.IsNullOrEmpty(user.Username))
                {
                    errorMessage = "Username cannot be empty";
                    return false;
                }
                
                if (string.IsNullOrEmpty(user.Email))
                {
                    errorMessage = "Email cannot be empty";
                    return false;
                }
                
                if (!Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    errorMessage = "Invalid email format";
                    return false;
                }
                
                // Check if username or email already exists
                if (_db.Users.Any(u => u.Username == user.Username))
                {
                    errorMessage = "Username already exists";
                    return false;
                }
                
                if (_db.Users.Any(u => u.Email == user.Email))
                {
                    errorMessage = "Email already exists";
                    return false;
                }
                
                // Validate password
                if (!ValidatePassword(password, out errorMessage))
                {
                    return false;
                }
                
                // Hash password
                user.PasswordHash = HashPassword(password);
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    errorMessage = "Error creating password hash";
                    return false;
                }
                
                // Save user to database
                _db.Users.Add(user);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.RegisterUser");
                }
                errorMessage = "An error occurred during registration";
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
                
                // Check if username is changed and already exists
                if (existingUser.Username != user.Username && _db.Users.Any(u => u.Username == user.Username))
                {
                    errorMessage = "Username already exists";
                    return false;
                }
                
                // Check if email is changed and already exists
                if (existingUser.Email != user.Email && _db.Users.Any(u => u.Email == user.Email))
                {
                    errorMessage = "Email already exists";
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
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.UpdateUser");
                }
                errorMessage = "An error occurred during user update";
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
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.DeleteUser");
                }
                errorMessage = "An error occurred during user deletion";
                return false;
            }
        }
        
        public User GetUserByUsername(string username)
        {
            try
            {
                return _db.Users.FirstOrDefault(u => u.Username == username);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.GetUserByUsername");
                }
                return null;
            }
        }
        
        public User GetUserByEmail(string email)
        {
            try
            {
                return _db.Users.FirstOrDefault(u => u.Email == email);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.GetUserByEmail");
                }
                return null;
            }
        }
        
        public bool IsUsernameAvailable(string username)
        {
            try
            {
                return !_db.Users.Any(u => u.Username == username);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.IsUsernameAvailable");
                }
                return false;
            }
        }
        
        public bool IsEmailAvailable(string email)
        {
            try
            {
                return !_db.Users.Any(u => u.Email == email);
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.IsEmailAvailable");
                }
                return false;
            }
        }
        
        public bool IsUserAdmin(string username)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                return user != null && user.IsAdmin;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.IsUserAdmin");
                }
                return false;
            }
        }
        
        public bool IsUserSpecialist(string username)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                return user != null && user.IsSpecialist;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.IsUserSpecialist");
                }
                return false;
            }
        }
        
        public bool AssignAdminRole(string username, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                user.IsAdmin = true;
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.AssignAdminRole");
                }
                errorMessage = "An error occurred while assigning admin role";
                return false;
            }
        }
        
        public bool AssignSpecialistRole(string username, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                user.IsSpecialist = true;
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.AssignSpecialistRole");
                }
                errorMessage = "An error occurred while assigning specialist role";
                return false;
            }
        }
        
        public bool RemoveAdminRole(string username, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                user.IsAdmin = false;
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.RemoveAdminRole");
                }
                errorMessage = "An error occurred while removing admin role";
                return false;
            }
        }
        
        public bool RemoveSpecialistRole(string username, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                user.IsSpecialist = false;
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.RemoveSpecialistRole");
                }
                errorMessage = "An error occurred while removing specialist role";
                return false;
            }
        }
        
        public bool ChangePassword(string username, string oldPassword, string newPassword, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                if (!VerifyPassword(user.PasswordHash, oldPassword))
                {
                    errorMessage = "Current password is incorrect";
                    return false;
                }
                
                if (!ValidatePassword(newPassword, out errorMessage))
                {
                    return false;
                }
                
                user.PasswordHash = HashPassword(newPassword);
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    errorMessage = "Error creating password hash";
                    return false;
                }
                
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.ChangePassword");
                }
                errorMessage = "An error occurred during password change";
                return false;
            }
        }
        
        public bool ResetPassword(string username, string newPassword, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    errorMessage = "User not found";
                    return false;
                }
                
                if (!ValidatePassword(newPassword, out errorMessage))
                {
                    return false;
                }
                
                user.PasswordHash = HashPassword(newPassword);
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    errorMessage = "Error creating password hash";
                    return false;
                }
                
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                if (_errorHandler != null)
                {
                    _errorHandler.LogError(ex, "AuthBL.ResetPassword");
                }
                errorMessage = "An error occurred during password reset";
                return false;
            }
        }
        
        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }
    }
} 
