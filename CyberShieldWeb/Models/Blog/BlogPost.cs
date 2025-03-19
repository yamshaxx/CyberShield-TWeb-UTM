using System;

namespace CyberShieldWeb.Models.Blog
{
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
    }
} 
