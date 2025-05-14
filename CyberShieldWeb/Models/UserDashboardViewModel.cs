using System;
using System.Collections.Generic;
using CyberShield.Domain.Model.Blog;

namespace CyberShieldWeb.Models
{
    public class UserDashboardViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public IList<UserCommentViewModel> Comments { get; set; }
        public IList<UserAppointmentViewModel> Appointments { get; set; }
        public IList<UserContactMessageViewModel> SentMessages { get; set; }
        
        public UserDashboardViewModel()
        {
            Comments = new List<UserCommentViewModel>();
            Appointments = new List<UserAppointmentViewModel>();
            SentMessages = new List<UserContactMessageViewModel>();
        }
    }

    public class UserContactMessageViewModel
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
    }

    public class UserCommentViewModel
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }
        public string BlogPostTitle { get; set; }
        public string Content { get; set; }
        public DateTime PostedAt { get; set; }
    }

    public class UserAppointmentViewModel
    {
        public int Id { get; set; }
        public string ServiceType { get; set; }
        public DateTime PreferredDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 