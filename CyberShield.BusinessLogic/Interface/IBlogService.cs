using System;
using System.Collections.Generic;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShield.BusinessLogic.Interface
{
    public interface IBlogService
    {
        // Blog post CRUD operations
        BlogModel.BlogPost GetBlogPostById(int id);
        IEnumerable<BlogModel.BlogPost> GetAllBlogPosts();
        IEnumerable<BlogModel.BlogPost> GetBlogPostsByAuthor(string author);
        IEnumerable<BlogModel.BlogPost> GetBlogPostsByCategory(string category);
        bool CreateBlogPost(BlogModel.BlogPost post, out string errorMessage);
        bool UpdateBlogPost(BlogModel.BlogPost post, out string errorMessage);
        bool DeleteBlogPost(int postId, out string errorMessage);
        bool IsTitleUnique(string title, int? excludePostId = null);
        
        // Blog post search and filtering
        IEnumerable<BlogModel.BlogPost> SearchBlogPosts(string searchTerm);
        IEnumerable<BlogModel.BlogPost> GetBlogPostsSortedBy(string sortField, bool ascending = true);
        IEnumerable<BlogModel.BlogPost> GetPaginatedBlogPosts(int pageNumber, int pageSize, out int totalCount);
        IEnumerable<BlogModel.BlogPost> GetRecentBlogPosts(int count);
        IEnumerable<string> GetAllCategories();
        
        // Blog comments
        BlogModel.Comment GetCommentById(int id);
        IEnumerable<BlogModel.Comment> GetCommentsByBlogPost(int blogPostId);
        IEnumerable<BlogModel.Comment> GetCommentsByUser(int userId);
        bool AddComment(BlogModel.Comment comment, out string errorMessage);
        bool UpdateComment(BlogModel.Comment comment, out string errorMessage);
        bool DeleteComment(int commentId, out string errorMessage);
        bool ModerateComment(int commentId, bool approved, out string errorMessage);
        
        // Blog statistics
        int GetTotalBlogPostCount();
        int GetTotalCommentCount();
        Dictionary<string, int> GetPostCountByCategory();
        Dictionary<string, int> GetPostCountByAuthor();
        BlogModel.BlogPost GetMostCommentedPost();
        
        // Content validation
        bool ValidateBlogPost(BlogModel.BlogPost post, out string errorMessage);
        bool ValidateComment(BlogModel.Comment comment, out string errorMessage);
        bool SanitizeContent(ref string content, out string errorMessage);
    }
} 