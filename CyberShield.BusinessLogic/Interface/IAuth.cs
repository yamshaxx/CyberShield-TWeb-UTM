using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyberShield.Domain.Model.User;

namespace CyberShield.BusinessLogic.Interface
{
    public interface IAuth
    {
        // Authentication methods
        string UserAuthLogic(UserLoginDTO data);
        bool VerifyPassword(string hashedPassword, string providedPassword);
        string HashPassword(string password);
        bool ValidatePassword(string password, out string errorMessage);
        
        // User management
        bool RegisterUser(User user, string password, out string errorMessage);
        bool UpdateUser(User user, out string errorMessage);
        bool DeleteUser(int userId, out string errorMessage);
        User GetUserByUsername(string username);
        User GetUserByEmail(string email);
        bool IsUsernameAvailable(string username);
        bool IsEmailAvailable(string email);
        
        // Authorization
        bool IsUserAdmin(string username);
        bool IsUserSpecialist(string username);
        bool AssignAdminRole(string username, out string errorMessage);
        bool AssignSpecialistRole(string username, out string errorMessage);
        bool RemoveAdminRole(string username, out string errorMessage);
        bool RemoveSpecialistRole(string username, out string errorMessage);
        
        // Password management
        bool ChangePassword(string username, string oldPassword, string newPassword, out string errorMessage);
        bool ResetPassword(string username, string newPassword, out string errorMessage);
    }
}
