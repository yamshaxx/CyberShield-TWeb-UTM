using System.Collections.Generic;
using CyberShield.Domain.Model;

namespace CyberShield.BusinessLogic.Interface
{
    /// <summary>
    /// Interface for managing contact messages
    /// </summary>
    public interface IContactMessageService
    {
        /// <summary>
        /// Gets a message by its ID
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <returns>The contact message or null if not found</returns>
        ContactMessage GetMessageById(int id);
        
        /// <summary>
        /// Gets all contact messages
        /// </summary>
        /// <returns>List of all contact messages</returns>
        IEnumerable<ContactMessage> GetAllMessages();
        
        /// <summary>
        /// Gets contact messages by user email
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <returns>List of contact messages from that email</returns>
        IEnumerable<ContactMessage> GetMessagesByEmail(string email);
        
        /// <summary>
        /// Creates a new contact message
        /// </summary>
        /// <param name="message">The message to create</param>
        /// <returns>True if successful, false otherwise</returns>
        bool CreateMessage(ContactMessage message);
        
        /// <summary>
        /// Marks a message as read
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <returns>True if successful, false otherwise</returns>
        bool MarkAsRead(int id);
        
        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <returns>True if successful, false otherwise</returns>
        bool DeleteMessage(int id);
    }
} 