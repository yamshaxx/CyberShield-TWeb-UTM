using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberShield.Domain.Model.Blog
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Phone { get; set; }
        
        [StringLength(100)]
        public string Company { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ServiceType { get; set; }
        
        [Required]
        public DateTime PreferredDate { get; set; }
        
        [StringLength(2000)]
        public string Message { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } // Pending, Confirmed, Completed, Cancelled
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User.User User { get; set; }
    }
} 