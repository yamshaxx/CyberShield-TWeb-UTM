using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DomainBlog = CyberShield.Domain.Model.Blog;
using DomainUser = CyberShield.Domain.Model.User;

namespace CyberShieldWeb.Models.Admin
{
    public class AdminDashboardViewModel
    {
        public int UserCount { get; set; }
        public int BlogPostCount { get; set; }
        public int CommentCount { get; set; }
        public IEnumerable<DomainUser.User> LatestUsers { get; set; }
        public IEnumerable<DomainBlog.BlogPost> LatestBlogPosts { get; set; }
    }

    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Is Admin")]
        public bool IsAdmin { get; set; }
    }

    public class EditBlogPostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(50, ErrorMessage = "Author cannot be longer than 50 characters")]
        [Display(Name = "Author")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Summary is required")]
        [StringLength(500, ErrorMessage = "Summary cannot be longer than 500 characters")]
        [Display(Name = "Summary")]
        public string Summary { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [StringLength(255, ErrorMessage = "Image URL cannot be longer than 255 characters")]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot be longer than 50 characters")]
        [Display(Name = "Category")]
        public string Category { get; set; }
    }

    public class CreateBlogPostViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(50, ErrorMessage = "Author cannot be longer than 50 characters")]
        [Display(Name = "Author")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Posted Date is required")]
        [Display(Name = "Posted Date")]
        [DataType(DataType.DateTime)]
        public DateTime PostedDate { get; set; }

        [Required(ErrorMessage = "Summary is required")]
        [StringLength(500, ErrorMessage = "Summary cannot be longer than 500 characters")]
        [Display(Name = "Summary")]
        public string Summary { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [StringLength(255, ErrorMessage = "Image URL cannot be longer than 255 characters")]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot be longer than 50 characters")]
        [Display(Name = "Category")]
        public string Category { get; set; }
    }
} 