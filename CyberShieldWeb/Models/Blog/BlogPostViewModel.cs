using System;
using System.Collections.Generic;

namespace CyberShieldWeb.Models.Blog
{
    public class BlogPostViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PostedDate { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public int CommentCount { get; set; }
    }

    public class BlogPostDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PostedDate { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public List<CommentViewModel> Comments { get; set; }
        public CreateCommentViewModel NewComment { get; set; }
    }
} 