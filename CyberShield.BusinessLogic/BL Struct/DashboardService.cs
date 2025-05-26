using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Data;
using CyberShield.Domain.Model;
using UserModel = CyberShield.Domain.Model.User;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class DashboardService : IDashboardService
    {
        private readonly CyberShieldContext _db;
        private readonly IErrorHandlingService _errorHandler;

        public DashboardService(IErrorHandlingService errorHandler = null)
        {
            _db = new CyberShieldContext();
            _errorHandler = errorHandler;
        }

        public object GetUserDashboardData(string username)
        {
            try
            {
                var dashboardData = new
                {
                    Username = username,
                    Email = "",
                    Comments = new List<object>(),
                    Appointments = new List<BlogModel.Appointment>(),
                    SentMessages = new List<ContactMessage>()
                };

                // Try to find user in the database
                UserModel.User user = null;
                bool userFoundInDb = false;

                try
                {
                    user = _db.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        userFoundInDb = true;
                        
                        // Set email
                        var emailProperty = dashboardData.GetType().GetProperty("Email");
                        
                        // Get user's comments from database
                        try
                        {
                            var dbComments = _db.Comments
                                .Include(c => c.BlogPost)
                                .Where(c => c.UserId == user.Id)
                                .OrderByDescending(c => c.PostedAt)
                                .ToList();

                            var commentsList = dbComments.Select(comment => new
                            {
                                Id = comment.Id,
                                BlogPostId = comment.BlogPostId,
                                BlogPostTitle = comment.BlogPost != null ? comment.BlogPost.Title : "Unknown Post",
                                Content = comment.Content,
                                PostedAt = comment.PostedAt
                            }).ToList();

                            return new
                            {
                                Username = username,
                                Email = user.Email,
                                Comments = commentsList,
                                Appointments = GetUserAppointments(user.Id).ToList(),
                                SentMessages = GetUserContactMessages(user.Email).ToList()
                            };
                        }
                        catch (Exception ex)
                        {
                            _errorHandler?.LogError(ex, "DashboardService.GetUserDashboardData - Comments");
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    _errorHandler?.LogError(dbEx, "DashboardService.GetUserDashboardData - Database");
                }

                // If user not found in database, try in-memory
                if (!userFoundInDb)
                {
                    var memoryUser = InMemoryData.Users.FirstOrDefault(u => u.Username == username);
                    if (memoryUser != null)
                    {
                        return new
                        {
                            Username = username,
                            Email = memoryUser.Email,
                            Comments = new List<object>(),
                            Appointments = GetUserAppointments(memoryUser.Id).ToList(),
                            SentMessages = GetUserContactMessages(memoryUser.Email).ToList()
                        };
                    }
                }

                // Return default dashboard data
                return new
                {
                    Username = username,
                    Email = "",
                    Comments = new List<object>(),
                    Appointments = new List<BlogModel.Appointment>(),
                    SentMessages = new List<ContactMessage>()
                };
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "DashboardService.GetUserDashboardData");
                return new
                {
                    Username = username,
                    Email = "",
                    Comments = new List<object>(),
                    Appointments = new List<BlogModel.Appointment>(),
                    SentMessages = new List<ContactMessage>()
                };
            }
        }

        public object GetSpecialistDashboardData(string username)
        {
            try
            {
                return new
                {
                    Username = username,
                    TotalAppointments = GetTotalAppointments(),
                    RecentAppointments = GetRecentAppointments().Take(10).ToList(),
                    ContactMessages = GetAllContactMessages().Take(10).ToList(),
                    PendingAppointments = GetPendingAppointments().ToList()
                };
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "DashboardService.GetSpecialistDashboardData");
                return new
                {
                    Username = username,
                    TotalAppointments = 0,
                    RecentAppointments = new List<BlogModel.Appointment>(),
                    ContactMessages = new List<ContactMessage>(),
                    PendingAppointments = new List<BlogModel.Appointment>()
                };
            }
        }

        public bool IsUserSpecialist(string username)
        {
            try
            {
                // Try database first
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user != null && user.IsSpecialist)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "DashboardService.IsUserSpecialist - Database");
            }

            // Try in-memory as fallback
            try
            {
                var user = InMemoryData.Users.FirstOrDefault(u => u.Username == username);
                if (user != null && user.IsSpecialist)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "DashboardService.IsUserSpecialist - Memory");
            }

            return false;
        }

        public IEnumerable<object> GetUserComments(int userId)
        {
            try
            {
                return _db.Comments
                    .Include(c => c.BlogPost)
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.PostedAt)
                    .Select(comment => new
                    {
                        Id = comment.Id,
                        BlogPostId = comment.BlogPostId,
                        BlogPostTitle = comment.BlogPost != null ? comment.BlogPost.Title : "Unknown Post",
                        Content = comment.Content,
                        PostedAt = comment.PostedAt
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "DashboardService.GetUserComments");
                return new List<object>();
            }
        }

        public IEnumerable<BlogModel.Appointment> GetUserAppointments(int userId)
        {
            try
            {
                // Try database first
                var dbAppointments = _db.Appointments
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();

                if (dbAppointments.Any())
                {
                    return dbAppointments;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "DashboardService.GetUserAppointments - Database");
            }

            // Try in-memory as fallback
            return InMemoryData.Appointments
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }

        public IEnumerable<ContactMessage> GetUserContactMessages(string userEmail)
        {
            try
            {
                // Try database first
                var dbMessages = _db.ContactMessages
                    .Where(m => m.Email == userEmail)
                    .OrderByDescending(m => m.SentDate)
                    .ToList();

                if (dbMessages.Any())
                {
                    return dbMessages;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "DashboardService.GetUserContactMessages - Database");
            }

            // Try in-memory as fallback
            return InMemoryData.ContactMessages
                .Where(m => m.Email == userEmail)
                .OrderByDescending(m => m.SentDate)
                .ToList();
        }

        public IEnumerable<ContactMessage> GetAllContactMessages()
        {
            try
            {
                // Try database first
                var dbMessages = _db.ContactMessages
                    .OrderByDescending(m => m.SentDate)
                    .ToList();

                if (dbMessages.Any())
                {
                    return dbMessages;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "DashboardService.GetAllContactMessages - Database");
            }

            // Try in-memory as fallback
            return InMemoryData.ContactMessages
                .OrderByDescending(m => m.SentDate)
                .ToList();
        }

        private int GetTotalAppointments()
        {
            try
            {
                return _db.Appointments.Count();
            }
            catch
            {
                return InMemoryData.Appointments.Count;
            }
        }

        private IEnumerable<BlogModel.Appointment> GetRecentAppointments()
        {
            try
            {
                return _db.Appointments
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();
            }
            catch
            {
                return InMemoryData.Appointments
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();
            }
        }

        private IEnumerable<BlogModel.Appointment> GetPendingAppointments()
        {
            try
            {
                return _db.Appointments
                    .Where(a => a.Status == "Pending")
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();
            }
            catch
            {
                return InMemoryData.Appointments
                    .Where(a => a.Status == "Pending")
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
} 