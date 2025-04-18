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