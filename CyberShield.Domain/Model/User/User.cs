using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BlogComment = CyberShield.Domain.Model.Blog.Comment;

namespace CyberShield.Domain.Model.User
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public bool IsAdmin { get; set; }
        
        // Navigation properties
        public virtual ICollection<BlogComment> Comments { get; set; }
        
        public User()
        {
            IsAdmin = false;
            Comments = new HashSet<BlogComment>();
        }
    }
} 