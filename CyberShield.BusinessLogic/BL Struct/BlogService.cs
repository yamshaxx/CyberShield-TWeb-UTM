using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CyberShield.BusinessLogic.Interface;
using CyberShield.Domain.Data;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class BlogService : IBlogService
    {
        private readonly CyberShieldContext _db;
        private readonly IErrorHandlingService _errorHandler;
        private readonly IValidationService _validationService;
        
        public BlogService()
        {
            _db = new CyberShieldContext();
        }
        
        public BlogService(IErrorHandlingService errorHandler, IValidationService validationService = null)
        {
            _db = new CyberShieldContext();
            _errorHandler = errorHandler;
            _validationService = validationService;
        }
        
        #region Blog post CRUD operations
        
        public BlogModel.BlogPost GetBlogPostById(int id)
        {
            try
            {
                return _db.BlogPosts.FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetBlogPostById");
                return null;
            }
        }
        
        public IEnumerable<BlogModel.BlogPost> GetAllBlogPosts()
        {
            try
            {
                return _db.BlogPosts.OrderByDescending(p => p.PostedDate).ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetAllBlogPosts");
                return new List<BlogModel.BlogPost>();
            }
        }
        
        public IEnumerable<BlogModel.BlogPost> GetBlogPostsByAuthor(string author)
        {
            try
            {
                return _db.BlogPosts
                    .Where(p => p.Author == author)
                    .OrderByDescending(p => p.PostedDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetBlogPostsByAuthor");
                return new List<BlogModel.BlogPost>();
            }
        }
        
        public IEnumerable<BlogModel.BlogPost> GetBlogPostsByCategory(string category)
        {
            try
            {
                return _db.BlogPosts
                    .Where(p => p.Category == category)
                    .OrderByDescending(p => p.PostedDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetBlogPostsByCategory");
                return new List<BlogModel.BlogPost>();
            }
        }
        
        public bool CreateBlogPost(BlogModel.BlogPost post, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // Validate post
                if (!ValidateBlogPost(post, out errorMessage))
                {
                    return false;
                }
                
                // Set posted date if not set
                if (post.PostedDate == default)
                {
                    post.PostedDate = DateTime.Now;
                }
                
                // Sanitize content if validation service is available
                if (_validationService != null)
                {
                    post.Content = _validationService.SanitizeHtml(post.Content);
                }
                
                // Add post to database
                _db.BlogPosts.Add(post);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.CreateBlogPost");
                errorMessage = "An error occurred while creating the blog post";
                return false;
            }
        }
        
        public bool UpdateBlogPost(BlogModel.BlogPost post, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var existingPost = _db.BlogPosts.FirstOrDefault(p => p.Id == post.Id);
                if (existingPost == null)
                {
                    errorMessage = "Blog post not found";
                    return false;
                }
                
                // Validate post
                if (!ValidateBlogPost(post, out errorMessage))
                {
                    return false;
                }
                
                // Check if title is changed and is unique
                if (existingPost.Title != post.Title && !IsTitleUnique(post.Title, post.Id))
                {
                    errorMessage = "Blog post with this title already exists";
                    return false;
                }
                
                // Sanitize content if validation service is available
                if (_validationService != null)
                {
                    post.Content = _validationService.SanitizeHtml(post.Content);
                }
                
                // Update post properties
                existingPost.Title = post.Title;
                existingPost.Summary = post.Summary;
                existingPost.Content = post.Content;
                existingPost.Category = post.Category;
                existingPost.ImageUrl = post.ImageUrl;
                
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.UpdateBlogPost");
                errorMessage = "An error occurred while updating the blog post";
                return false;
            }
        }
        
        public bool DeleteBlogPost(int postId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var post = _db.BlogPosts.FirstOrDefault(p => p.Id == postId);
                if (post == null)
                {
                    errorMessage = "Blog post not found";
                    return false;
                }
                
                // Remove associated comments first
                var comments = _db.Comments.Where(c => c.BlogPostId == postId).ToList();
                foreach (var comment in comments)
                {
                    _db.Comments.Remove(comment);
                }
                
                // Remove blog post
                _db.BlogPosts.Remove(post);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.DeleteBlogPost");
                errorMessage = "An error occurred while deleting the blog post";
                return false;
            }
        }
        
        public bool IsTitleUnique(string title, int? excludePostId = null)
        {
            try
            {
                if (excludePostId.HasValue)
                {
                    return !_db.BlogPosts.Any(p => p.Title == title && p.Id != excludePostId.Value);
                }
                else
                {
                    return !_db.BlogPosts.Any(p => p.Title == title);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.IsTitleUnique");
                return false;
            }
        }
        
        #endregion
        
        #region Blog post search and filtering
        
        public IEnumerable<BlogModel.BlogPost> SearchBlogPosts(string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return GetAllBlogPosts();
                }
                
                return _db.BlogPosts
                    .Where(p => p.Title.Contains(searchTerm) || 
                                p.Content.Contains(searchTerm) ||
                                p.Summary.Contains(searchTerm) ||
                                p.Author.Contains(searchTerm) ||
                                p.Category.Contains(searchTerm))
                    .OrderByDescending(p => p.PostedDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.SearchBlogPosts");
                return new List<BlogModel.BlogPost>();
            }
        }
        
        public IEnumerable<BlogModel.BlogPost> GetBlogPostsSortedBy(string sortField, bool ascending = true)
        {
            try
            {
                IQueryable<BlogModel.BlogPost> query = _db.BlogPosts;
                
                switch (sortField.ToLower())
                {
                    case "title":
                        query = ascending ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title);
                        break;
                    case "author":
                        query = ascending ? query.OrderBy(p => p.Author) : query.OrderByDescending(p => p.Author);
                        break;
                    case "category":
                        query = ascending ? query.OrderBy(p => p.Category) : query.OrderByDescending(p => p.Category);
                        break;
                    case "date":
                    default:
                        query = ascending ? query.OrderBy(p => p.PostedDate) : query.OrderByDescending(p => p.PostedDate);
                        break;
                }
                
                return query.ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetBlogPostsSortedBy");
                return new List<BlogModel.BlogPost>();
            }
        }
        
        public IEnumerable<BlogModel.BlogPost> GetPaginatedBlogPosts(int pageNumber, int pageSize, out int totalCount)
        {
            totalCount = 0;
            
            try
            {
                totalCount = _db.BlogPosts.Count();
                
                return _db.BlogPosts
                    .OrderByDescending(p => p.PostedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetPaginatedBlogPosts");
                return new List<BlogModel.BlogPost>();
            }
        }
        
        public IEnumerable<BlogModel.BlogPost> GetRecentBlogPosts(int count)
        {
            try
            {
                return _db.BlogPosts
                    .OrderByDescending(p => p.PostedDate)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetRecentBlogPosts");
                return new List<BlogModel.BlogPost>();
            }
        }
        
        public IEnumerable<string> GetAllCategories()
        {
            try
            {
                return _db.BlogPosts
                    .Select(p => p.Category)
                    .Distinct()
                    .Where(c => !string.IsNullOrEmpty(c))
                    .OrderBy(c => c)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetAllCategories");
                return new List<string>();
            }
        }
        
        #endregion
        
        #region Blog comments
        
        public BlogModel.Comment GetCommentById(int id)
        {
            try
            {
                return _db.Comments.FirstOrDefault(c => c.Id == id);
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetCommentById");
                return null;
            }
        }
        
        public IEnumerable<BlogModel.Comment> GetCommentsByBlogPost(int blogPostId)
        {
            try
            {
                return _db.Comments
                    .Where(c => c.BlogPostId == blogPostId)
                    .OrderByDescending(c => c.PostedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetCommentsByBlogPost");
                return new List<BlogModel.Comment>();
            }
        }
        
        public IEnumerable<BlogModel.Comment> GetCommentsByUser(int userId)
        {
            try
            {
                return _db.Comments
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.PostedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetCommentsByUser");
                return new List<BlogModel.Comment>();
            }
        }
        
        public bool AddComment(BlogModel.Comment comment, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                // Validate comment
                if (!ValidateComment(comment, out errorMessage))
                {
                    return false;
                }
                
                // Set posted date if not set
                if (comment.PostedAt == default)
                {
                    comment.PostedAt = DateTime.Now;
                }
                
                // Sanitize content if validation service is available
                if (_validationService != null)
                {
                    comment.Content = _validationService.SanitizeUserInput(comment.Content);
                }
                
                // Add comment to database
                _db.Comments.Add(comment);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.AddComment");
                errorMessage = "An error occurred while adding the comment";
                return false;
            }
        }
        
        public bool UpdateComment(BlogModel.Comment comment, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var existingComment = _db.Comments.FirstOrDefault(c => c.Id == comment.Id);
                if (existingComment == null)
                {
                    errorMessage = "Comment not found";
                    return false;
                }
                
                // Validate comment
                if (!ValidateComment(comment, out errorMessage))
                {
                    return false;
                }
                
                // Sanitize content if validation service is available
                if (_validationService != null)
                {
                    comment.Content = _validationService.SanitizeUserInput(comment.Content);
                }
                
                // Update comment properties
                existingComment.Content = comment.Content;
                
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.UpdateComment");
                errorMessage = "An error occurred while updating the comment";
                return false;
            }
        }
        
        public bool DeleteComment(int commentId, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);
                if (comment == null)
                {
                    errorMessage = "Comment not found";
                    return false;
                }
                
                _db.Comments.Remove(comment);
                _db.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.DeleteComment");
                errorMessage = "An error occurred while deleting the comment";
                return false;
            }
        }
        
        public bool ModerateComment(int commentId, bool approved, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                var comment = _db.Comments.FirstOrDefault(c => c.Id == commentId);
                if (comment == null)
                {
                    errorMessage = "Comment not found";
                    return false;
                }
                
                // For now, we're just deleting disapproved comments
                // In a more advanced system, we could have an IsApproved flag
                if (!approved)
                {
                    _db.Comments.Remove(comment);
                    _db.SaveChanges();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.ModerateComment");
                errorMessage = "An error occurred while moderating the comment";
                return false;
            }
        }
        
        #endregion
        
        #region Blog statistics
        
        public int GetTotalBlogPostCount()
        {
            try
            {
                return _db.BlogPosts.Count();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetTotalBlogPostCount");
                return 0;
            }
        }
        
        public int GetTotalCommentCount()
        {
            try
            {
                return _db.Comments.Count();
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetTotalCommentCount");
                return 0;
            }
        }
        
        public Dictionary<string, int> GetPostCountByCategory()
        {
            try
            {
                return _db.BlogPosts
                    .Where(p => !string.IsNullOrEmpty(p.Category))
                    .GroupBy(p => p.Category)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetPostCountByCategory");
                return new Dictionary<string, int>();
            }
        }
        
        public Dictionary<string, int> GetPostCountByAuthor()
        {
            try
            {
                return _db.BlogPosts
                    .GroupBy(p => p.Author)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetPostCountByAuthor");
                return new Dictionary<string, int>();
            }
        }
        
        public BlogModel.BlogPost GetMostCommentedPost()
        {
            try
            {
                var postWithMostComments = _db.BlogPosts
                    .OrderByDescending(p => p.Comments.Count)
                    .FirstOrDefault();
                
                return postWithMostComments;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.GetMostCommentedPost");
                return null;
            }
        }
        
        #endregion
        
        #region Content validation
        
        public bool ValidateBlogPost(BlogModel.BlogPost post, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Validate using the validation service if available
            if (_validationService != null)
            {
                if (!_validationService.ValidateBlogPostTitle(post.Title, out errorMessage))
                {
                    return false;
                }
                
                if (!_validationService.ValidateBlogPostContent(post.Content, out errorMessage))
                {
                    return false;
                }
                
                if (!_validationService.ValidateCategoryName(post.Category, out errorMessage))
                {
                    return false;
                }
            }
            else
            {
                // Basic validation if validation service is not available
                if (string.IsNullOrEmpty(post.Title))
                {
                    errorMessage = "Title is required";
                    return false;
                }
                
                if (post.Title.Length > 100)
                {
                    errorMessage = "Title cannot be longer than 100 characters";
                    return false;
                }
                
                if (string.IsNullOrEmpty(post.Content))
                {
                    errorMessage = "Content is required";
                    return false;
                }
                
                if (string.IsNullOrEmpty(post.Author))
                {
                    errorMessage = "Author is required";
                    return false;
                }
            }
            
            // Check if title is unique (for new posts)
            if (post.Id == 0 && !IsTitleUnique(post.Title))
            {
                errorMessage = "A blog post with this title already exists";
                return false;
            }
            
            return true;
        }
        
        public bool ValidateComment(BlogModel.Comment comment, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Validate using the validation service if available
            if (_validationService != null)
            {
                if (!_validationService.ValidateComment(comment.Content, out errorMessage))
                {
                    return false;
                }
            }
            else
            {
                // Basic validation if validation service is not available
                if (string.IsNullOrEmpty(comment.Content))
                {
                    errorMessage = "Comment text is required";
                    return false;
                }
                
                if (comment.Content.Length > 2000)
                {
                    errorMessage = "Comment cannot be longer than 2000 characters";
                    return false;
                }
            }
            
            // Check if blog post exists
            var blogPost = _db.BlogPosts.FirstOrDefault(p => p.Id == comment.BlogPostId);
            if (blogPost == null)
            {
                errorMessage = "Blog post does not exist";
                return false;
            }
            
            // Check if user exists
            var user = _db.Users.FirstOrDefault(u => u.Id == comment.UserId);
            if (user == null)
            {
                errorMessage = "User does not exist";
                return false;
            }
            
            return true;
        }
        
        public bool SanitizeContent(ref string content, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                if (_validationService != null)
                {
                    content = _validationService.SanitizeHtml(content);
                }
                else
                {
                    // Basic sanitization if validation service is not available
                    // Remove potentially dangerous script tags
                    content = Regex.Replace(content, @"<script.*?>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    
                    // Remove iframe tags
                    content = Regex.Replace(content, @"<iframe.*?>.*?</iframe>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    
                    // Remove event handlers
                    content = Regex.Replace(content, @"\s+on\w+\s*=\s*""[^""]*""", " ", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    content = Regex.Replace(content, @"\s+on\w+\s*=\s*'[^']*'", " ", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "BlogService.SanitizeContent");
                errorMessage = "An error occurred while sanitizing content";
                return false;
            }
        }
        
        #endregion
        
        #region Helper methods
        
        private void LogError(Exception ex, string source)
        {
            if (_errorHandler != null)
            {
                _errorHandler.LogError(ex, source);
            }
            else
            {
                // If error handler is not available, log to debug output
                System.Diagnostics.Debug.WriteLine($"Error in {source}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }
        
        #endregion
        
        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }
    }
} 