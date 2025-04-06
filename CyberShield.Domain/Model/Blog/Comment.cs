using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberShield.Domain.Model.Blog
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int BlogPostId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
        
        [Required]
        public DateTime PostedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("BlogPostId")]
        public virtual BlogPost BlogPost { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User.User User { get; set; }
    }
} 