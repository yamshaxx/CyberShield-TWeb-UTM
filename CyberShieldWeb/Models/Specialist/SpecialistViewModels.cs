using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CyberShieldWeb.Models
{
    public class SpecialistDashboardViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int TotalAppointments { get; set; }
        public IList<SpecialistBlogViewModel> BlogPosts { get; set; }
        public IList<AppointmentViewModel> Appointments { get; set; }
        public IList<AppointmentViewModel> RecentAppointments { get; set; }
        public IList<AppointmentViewModel> PendingAppointments { get; set; }
        public IList<AppointmentViewModel> ConfirmedAppointments { get; set; }
        public IList<AppointmentViewModel> CancelledAppointments { get; set; }
        public IList<ContactMessageViewModel> ContactMessages { get; set; }
        
        public SpecialistDashboardViewModel()
        {
            BlogPosts = new List<SpecialistBlogViewModel>();
            Appointments = new List<AppointmentViewModel>();
            RecentAppointments = new List<AppointmentViewModel>();
            PendingAppointments = new List<AppointmentViewModel>();
            ConfirmedAppointments = new List<AppointmentViewModel>();
            CancelledAppointments = new List<AppointmentViewModel>();
            ContactMessages = new List<ContactMessageViewModel>();
        }
    }

    public class SpecialistBlogViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime PostedDate { get; set; }
        public string Category { get; set; }
        public int CommentCount { get; set; }
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Company { get; set; }
        public string ServiceType { get; set; }
        public DateTime PreferredDate { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ContactMessageViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
    }

    public class ContactMessagesViewModel
    {
        public IList<ContactMessageViewModel> Messages { get; set; }
        
        public ContactMessagesViewModel()
        {
            Messages = new List<ContactMessageViewModel>();
        }
    }

    public class CreateBlogPostViewModel
    {
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate depăși 100 de caractere")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Rezumatul este obligatoriu")]
        [StringLength(500, ErrorMessage = "Rezumatul nu poate depăși 500 de caractere")]
        public string Summary { get; set; }
        
        [Required(ErrorMessage = "Conținutul este obligatoriu")]
        public string Content { get; set; }
        
        [Required(ErrorMessage = "Categoria este obligatorie")]
        public string Category { get; set; }
        
        public string ImageUrl { get; set; }
    }
} 