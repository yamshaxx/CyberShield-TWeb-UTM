using System;
using System.ComponentModel.DataAnnotations;

namespace CyberShieldWeb.Models.Blog
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        
        public int BlogPostId { get; set; }
        
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Comentariul nu poate fi gol")]
        [StringLength(2000, ErrorMessage = "Comentariul nu poate depăși 2000 de caractere")]
        [Display(Name = "Comentariu")]
        public string Content { get; set; }
        
        public DateTime PostedAt { get; set; }
    }
    
    public class CreateCommentViewModel
    {
        public int BlogPostId { get; set; }
        
        [Required(ErrorMessage = "Comentariul nu poate fi gol")]
        [StringLength(2000, ErrorMessage = "Comentariul nu poate depăși 2000 de caractere")]
        [Display(Name = "Comentariu")]
        public string Content { get; set; }
    }
} 