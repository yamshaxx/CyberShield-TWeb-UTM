// This file is a placeholder for backward compatibility.
// We're using CyberShield.Domain.Model.Blog.Comment as the main Comment class in the application.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CyberShield.Domain.Model.User;

namespace CyberShield.Domain.Data
{
    // Redirect to the actual Model.Blog.Comment implementation
    [NotMapped]
    public class Comment
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime PostedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("BlogPostId")]
        public virtual BlogPost BlogPost { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        // This class is only here to maintain project structure.
        // The actual Comment class is in the Model/Blog directory.
    }
}