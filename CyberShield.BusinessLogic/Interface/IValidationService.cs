using System;
using System.Collections.Generic;

namespace CyberShield.BusinessLogic.Interface
{
    public interface IValidationService
    {
        // Password validation
        bool ValidatePassword(string password, out string errorMessage);
        bool ValidatePasswordComplexity(string password, out string errorMessage);
        bool ValidatePasswordHistory(int userId, string newPassword, out string errorMessage);
        
        // User validation
        bool ValidateUsername(string username, out string errorMessage);
        bool ValidateEmail(string email, out string errorMessage);
        bool ValidateUserData(string username, string email, out string errorMessage);
        
        // Content validation
        bool ValidateBlogPostTitle(string title, out string errorMessage);
        bool ValidateBlogPostContent(string content, out string errorMessage);
        bool ValidateComment(string comment, out string errorMessage);
        
        // File validation
        bool ValidateFileSize(long fileSizeBytes, long maxSizeBytes, out string errorMessage);
        bool ValidateFileExtension(string filename, string[] allowedExtensions, out string errorMessage);
        bool ValidateImageDimensions(int width, int height, int maxWidth, int maxHeight, out string errorMessage);
        bool ValidatePdfFile(byte[] fileContent, out string errorMessage);
        
        // Date validation
        bool ValidateDateRange(DateTime date, DateTime minDate, DateTime maxDate, out string errorMessage);
        bool ValidateAppointmentTime(DateTime appointmentTime, out string errorMessage);
        bool ValidateFutureDate(DateTime date, out string errorMessage);
        
        // Input sanitization
        string SanitizeHtml(string html);
        string SanitizeUserInput(string input);
        string SanitizeFileName(string fileName);
        
        // Domain-specific validation
        bool ValidatePhoneNumber(string phoneNumber, out string errorMessage);
        bool ValidateServiceType(string serviceType, out string errorMessage);
        bool ValidateCompanyName(string companyName, out string errorMessage);
        bool ValidateCategoryName(string categoryName, out string errorMessage);
    }
} 