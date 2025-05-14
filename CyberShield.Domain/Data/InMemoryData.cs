using BlogModel = CyberShield.Domain.Model.Blog;
using UserModel = CyberShield.Domain.Model.User;
using System.Collections.Generic;
using System.Web.Helpers;
using CyberShield.Domain.Model;
using System;

namespace CyberShield.Domain.Data
{
    /// <summary>
    /// Static class to hold in-memory data when database connection is not available
    /// </summary>
    public static class InMemoryData
    {
        static InMemoryData()
        {
            // Initialize with default admin user
            Users.Add(new UserModel.User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@cybershield.com",
                PasswordHash = "AQAAAAEAACcQAAAAEKX9R+G+HjJ6sNBEVxMBrVeX6bTXyoTFLvYZO8vXDKnHhAaXZJM8+LcVv8K0bzRPjg==", // Hashed "Admin123!"
                IsAdmin = true,
                IsSpecialist = true,
            });
            
            // Initialize with a welcome blog post
            BlogPosts.Add(new BlogModel.BlogPost
            {
                Id = 1,
                Title = "Welcome to CyberShield",
                Author = "System",
                PostedDate = System.DateTime.Now,
                Summary = "This is a sample blog post created automatically.",
                Content = "<p>Welcome to the CyberShield cybersecurity platform.</p>",
                ImageUrl = "/Content/img/blog/welcome.jpg",
                Category = "Announcement"
            });
        }

        public static List<UserModel.User> Users { get; } = new List<UserModel.User>();
        public static List<BlogModel.BlogPost> BlogPosts { get; } = new List<BlogModel.BlogPost>();
        public static List<BlogModel.Comment> Comments { get; } = new List<BlogModel.Comment>();
        public static List<BlogModel.Appointment> Appointments { get; } = new List<BlogModel.Appointment>();
        public static List<CyberShield.Domain.Model.ContactMessage> ContactMessages { get; } = new List<CyberShield.Domain.Model.ContactMessage>();
    }
} 