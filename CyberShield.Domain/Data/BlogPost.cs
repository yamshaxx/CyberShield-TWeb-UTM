// This file is a placeholder for backward compatibility.
// We're using CyberShield.Domain.Model.Blog.BlogPost as the main BlogPost class in the application.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberShield.Domain.Data
{
    // Redirect to the actual Model.Blog.BlogPost implementation
    [NotMapped]
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PostedDate { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        // This class is only here to maintain project structure.
        // The actual BlogPost class is in the Model/Blog directory.
    }
}