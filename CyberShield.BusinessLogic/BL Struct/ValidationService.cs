using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CyberShield.BusinessLogic.Interface;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class ValidationService : IValidationService
    {
        // Minimum password requirements
        private const int MinPasswordLength = 8;
        private const int MaxPasswordLength = 100;
        
        // File size limits (in bytes)
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private const int MaxImageWidth = 2000;
        private const int MaxImageHeight = 2000;
        
        // Content length limits
        private const int MaxTitleLength = 100;
        private const int MaxContentLength = 100000;
        private const int MaxCommentLength = 2000;
        
        public ValidationService()
        {
        }
        
        #region Password validation
        
        public bool ValidatePassword(string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(password))
            {
                errorMessage = "Password cannot be empty";
                return false;
            }
            
            if (password.Length < MinPasswordLength)
            {
                errorMessage = $"Password must be at least {MinPasswordLength} characters long";
                return false;
            }
            
            if (password.Length > MaxPasswordLength)
            {
                errorMessage = $"Password cannot be longer than {MaxPasswordLength} characters";
                return false;
            }
            
            // Check for complexity requirements
            return ValidatePasswordComplexity(password, out errorMessage);
        }
        
        public bool ValidatePasswordComplexity(string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Check for uppercase letters
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "Password must contain at least one uppercase letter";
                return false;
            }
            
            // Check for lowercase letters
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errorMessage = "Password must contain at least one lowercase letter";
                return false;
            }
            
            // Check for digits
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                errorMessage = "Password must contain at least one digit";
                return false;
            }
            
            // Check for special characters
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
            {
                errorMessage = "Password must contain at least one special character";
                return false;
            }
            
            return true;
        }
        
        public bool ValidatePasswordHistory(int userId, string newPassword, out string errorMessage)
        {
            // In a real implementation, this would check against a database of previous passwords
            // For now, just simulate success
            errorMessage = string.Empty;
            return true;
        }
        
        #endregion
        
        #region User validation
        
        public bool ValidateUsername(string username, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(username))
            {
                errorMessage = "Username cannot be empty";
                return false;
            }
            
            if (username.Length < 3)
            {
                errorMessage = "Username must be at least 3 characters long";
                return false;
            }
            
            if (username.Length > 50)
            {
                errorMessage = "Username cannot be longer than 50 characters";
                return false;
            }
            
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                errorMessage = "Username can only contain letters, numbers, and underscores";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateEmail(string email, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(email))
            {
                errorMessage = "Email cannot be empty";
                return false;
            }
            
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errorMessage = "Invalid email format";
                return false;
            }
            
            if (email.Length > 100)
            {
                errorMessage = "Email cannot be longer than 100 characters";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateUserData(string username, string email, out string errorMessage)
        {
            // Check username
            if (!ValidateUsername(username, out errorMessage))
            {
                return false;
            }
            
            // Check email
            if (!ValidateEmail(email, out errorMessage))
            {
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Content validation
        
        public bool ValidateBlogPostTitle(string title, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(title))
            {
                errorMessage = "Title cannot be empty";
                return false;
            }
            
            if (title.Length > MaxTitleLength)
            {
                errorMessage = $"Title cannot be longer than {MaxTitleLength} characters";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateBlogPostContent(string content, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(content))
            {
                errorMessage = "Content cannot be empty";
                return false;
            }
            
            if (content.Length > MaxContentLength)
            {
                errorMessage = $"Content cannot be longer than {MaxContentLength} characters";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateComment(string comment, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(comment))
            {
                errorMessage = "Comment cannot be empty";
                return false;
            }
            
            if (comment.Length > MaxCommentLength)
            {
                errorMessage = $"Comment cannot be longer than {MaxCommentLength} characters";
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region File validation
        
        public bool ValidateFileSize(long fileSizeBytes, long maxSizeBytes, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (fileSizeBytes <= 0)
            {
                errorMessage = "File size must be greater than 0";
                return false;
            }
            
            if (fileSizeBytes > maxSizeBytes)
            {
                var maxSizeMB = maxSizeBytes / (1024 * 1024);
                errorMessage = $"File size must not exceed {maxSizeMB} MB";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateFileExtension(string filename, string[] allowedExtensions, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(filename))
            {
                errorMessage = "Filename cannot be empty";
                return false;
            }
            
            var extension = System.IO.Path.GetExtension(filename).ToLower();
            if (string.IsNullOrEmpty(extension))
            {
                errorMessage = "File must have an extension";
                return false;
            }
            
            if (!allowedExtensions.Any(ext => extension.Equals(ext, StringComparison.OrdinalIgnoreCase)))
            {
                errorMessage = $"File extension must be one of: {string.Join(", ", allowedExtensions)}";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateImageDimensions(int width, int height, int maxWidth, int maxHeight, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (width <= 0 || height <= 0)
            {
                errorMessage = "Image dimensions must be greater than 0";
                return false;
            }
            
            if (width > maxWidth)
            {
                errorMessage = $"Image width must not exceed {maxWidth} pixels";
                return false;
            }
            
            if (height > maxHeight)
            {
                errorMessage = $"Image height must not exceed {maxHeight} pixels";
                return false;
            }
            
            return true;
        }
        
        public bool ValidatePdfFile(byte[] fileContent, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (fileContent == null || fileContent.Length < 4)
            {
                errorMessage = "Invalid or empty PDF file";
                return false;
            }
            
            // Check for PDF file signature (%PDF-)
            if (fileContent[0] != 0x25 || fileContent[1] != 0x50 || fileContent[2] != 0x44 || fileContent[3] != 0x46)
            {
                errorMessage = "File is not a valid PDF";
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Date validation
        
        public bool ValidateDateRange(DateTime date, DateTime minDate, DateTime maxDate, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (date < minDate)
            {
                errorMessage = $"Date must be on or after {minDate.ToShortDateString()}";
                return false;
            }
            
            if (date > maxDate)
            {
                errorMessage = $"Date must be on or before {maxDate.ToShortDateString()}";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateAppointmentTime(DateTime appointmentTime, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Validate that appointment is in the future
            if (appointmentTime <= DateTime.Now)
            {
                errorMessage = "Appointment time must be in the future";
                return false;
            }
            
            // Validate that appointment is within working hours (e.g., 9 AM to 5 PM)
            if (appointmentTime.Hour < 9 || appointmentTime.Hour >= 17)
            {
                errorMessage = "Appointment time must be between 9 AM and 5 PM";
                return false;
            }
            
            // Validate that appointment is on a weekday
            if (appointmentTime.DayOfWeek == DayOfWeek.Saturday || appointmentTime.DayOfWeek == DayOfWeek.Sunday)
            {
                errorMessage = "Appointments are not available on weekends";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateFutureDate(DateTime date, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (date <= DateTime.Now)
            {
                errorMessage = "Date must be in the future";
                return false;
            }
            
            return true;
        }
        
        #endregion
        
        #region Input sanitization
        
        public string SanitizeHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }
            
            // Remove script tags and their contents
            html = Regex.Replace(html, @"<script.*?>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Remove iframe tags
            html = Regex.Replace(html, @"<iframe.*?>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // Remove event handlers
            html = Regex.Replace(html, @"\s+on\w+\s*=\s*""[^""]*""", " ", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            html = Regex.Replace(html, @"\s+on\w+\s*=\s*'[^']*'", " ", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            
            // Remove javascript: URLs
            html = Regex.Replace(html, @"javascript:", "blocked:", RegexOptions.IgnoreCase);
            
            return html;
        }
        
        public string SanitizeUserInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            
            // Replace potentially dangerous characters
            input = input.Replace("<", "&lt;").Replace(">", "&gt;");
            
            return input;
        }
        
        public string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }
            
            // Remove invalid characters
            string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            
            return Regex.Replace(fileName, invalidRegStr, "_");
        }
        
        #endregion
        
        #region Domain-specific validation
        
        public bool ValidatePhoneNumber(string phoneNumber, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(phoneNumber))
            {
                errorMessage = "Phone number cannot be empty";
                return false;
            }
            
            // Remove non-numeric characters for validation
            string digitsOnly = Regex.Replace(phoneNumber, @"[^\d]", "");
            
            if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
            {
                errorMessage = "Phone number must have between 10 and 15 digits";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateServiceType(string serviceType, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(serviceType))
            {
                errorMessage = "Service type cannot be empty";
                return false;
            }
            
            // Check against valid service types (example list)
            string[] validServiceTypes = new[]
            {
                "Consultanta",
                "Testarea penetrarii",
                "Inginerie sociala",
                "Conformitate GDPR",
                "Analiza de risc"
            };
            
            if (!validServiceTypes.Contains(serviceType))
            {
                errorMessage = "Invalid service type";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateCompanyName(string companyName, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(companyName))
            {
                // Company name is optional in some cases
                return true;
            }
            
            if (companyName.Length > 100)
            {
                errorMessage = "Company name cannot be longer than 100 characters";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateCategoryName(string categoryName, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (string.IsNullOrEmpty(categoryName))
            {
                errorMessage = "Category name cannot be empty";
                return false;
            }
            
            // Check against valid service types (example list)
            string[] validCategoryNames = new[]
            {
                "Securitate Cibernetică",
                "Testare Penetrare",
                "GDPR",
                "Inginerie Socială",
                "Actualizări",
                "Announcement"
            };
            
            if (!validCategoryNames.Contains(categoryName))
            {
                errorMessage = "Invalid category name";
                return false;
            }
            
            return true;
        }
        
        #endregion
    }
} 