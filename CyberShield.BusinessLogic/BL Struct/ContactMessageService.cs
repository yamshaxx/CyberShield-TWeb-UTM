using System;
using System.Collections.Generic;
using System.Linq;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Data;
using CyberShield.Domain.Model;

namespace CyberShield.BusinessLogic.BL_Struct
{
    /// <summary>
    /// Service for managing contact messages
    /// </summary>
    public class ContactMessageService : IContactMessageService
    {
        private readonly CyberShieldContext _db;

        /// <summary>
        /// Constructor with database context
        /// </summary>
        /// <param name="db">The database context</param>
        public ContactMessageService(CyberShieldContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Default constructor - creates a new database context
        /// </summary>
        public ContactMessageService()
        {
            _db = new CyberShieldContext();
        }

        /// <summary>
        /// Gets a message by its ID
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <returns>The contact message or null if not found</returns>
        public ContactMessage GetMessageById(int id)
        {
            try
            {
                return _db.ContactMessages.Find(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting message by ID {id}: {ex.Message}");
                // Try from in-memory data
                return InMemoryData.ContactMessages.FirstOrDefault(m => m.Id == id);
            }
        }

        /// <summary>
        /// Gets all contact messages
        /// </summary>
        /// <returns>List of all contact messages</returns>
        public IEnumerable<ContactMessage> GetAllMessages()
        {
            try
            {
                return _db.ContactMessages.OrderByDescending(m => m.SentDate).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all messages: {ex.Message}");
                // Fall back to in-memory data
                return InMemoryData.ContactMessages.OrderByDescending(m => m.SentDate).ToList();
            }
        }

        /// <summary>
        /// Gets contact messages by user email
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <returns>List of contact messages from that email</returns>
        public IEnumerable<ContactMessage> GetMessagesByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return new List<ContactMessage>();
            }

            try
            {
                return _db.ContactMessages
                    .Where(m => m.Email.ToLower() == email.ToLower())
                    .OrderByDescending(m => m.SentDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting messages by email {email}: {ex.Message}");
                // Fall back to in-memory data
                return InMemoryData.ContactMessages
                    .Where(m => m.Email.ToLower() == email.ToLower())
                    .OrderByDescending(m => m.SentDate)
                    .ToList();
            }
        }

        /// <summary>
        /// Creates a new contact message
        /// </summary>
        /// <param name="message">The message to create</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool CreateMessage(ContactMessage message)
        {
            if (message == null)
            {
                return false;
            }

            try
            {
                message.SentDate = DateTime.Now;
                message.IsRead = false;
                
                _db.ContactMessages.Add(message);
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating message: {ex.Message}");
                
                // Try to add to in-memory data
                try
                {
                    message.Id = InMemoryData.ContactMessages.Any() 
                        ? InMemoryData.ContactMessages.Max(m => m.Id) + 1 
                        : 1;
                    message.SentDate = DateTime.Now;
                    message.IsRead = false;
                    
                    InMemoryData.ContactMessages.Add(message);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Marks a message as read
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool MarkAsRead(int id)
        {
            try
            {
                var message = _db.ContactMessages.Find(id);
                if (message == null)
                {
                    return false;
                }

                message.IsRead = true;
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking message as read: {ex.Message}");
                
                // Try in-memory
                var memoryMessage = InMemoryData.ContactMessages.FirstOrDefault(m => m.Id == id);
                if (memoryMessage != null)
                {
                    memoryMessage.IsRead = true;
                    return true;
                }
                
                return false;
            }
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool DeleteMessage(int id)
        {
            try
            {
                var message = _db.ContactMessages.Find(id);
                if (message == null)
                {
                    return false;
                }

                _db.ContactMessages.Remove(message);
                _db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting message: {ex.Message}");
                
                // Try in-memory
                var memoryMessage = InMemoryData.ContactMessages.FirstOrDefault(m => m.Id == id);
                if (memoryMessage != null)
                {
                    InMemoryData.ContactMessages.Remove(memoryMessage);
                    return true;
                }
                
                return false;
            }
        }
    }
} 