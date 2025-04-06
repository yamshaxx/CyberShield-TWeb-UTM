using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CyberShield.Domain.Model.Blog
{
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Author { get; set; }
        
        [Required]
        public DateTime PostedDate { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Summary { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        [StringLength(255)]
        public string ImageUrl { get; set; }
        
        [StringLength(50)]
        public string Category { get; set; }
        
        // Navigation property
        public virtual ICollection<Comment> Comments { get; set; }
        
        public BlogPost()
        {
            Comments = new HashSet<Comment>();
        }
    }
} 