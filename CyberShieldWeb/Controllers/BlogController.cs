using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using CyberShield.BusinessLogic;
using CyberShieldWeb.Models.Blog;
using BlogModel = CyberShield.Domain.Model.Blog;

namespace CyberShieldWeb.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly IUserService _userService;
        private readonly IErrorHandlingService _errorHandler;

        public BlogController()
        {
            var bl = new BusinessLogic.BusinessLogic();
            _blogService = bl.GetBlogService();
            _userService = bl.GetUserService();
            _errorHandler = bl.GetErrorHandlingService();
        }

        // GET: Blog
        public ActionResult Index()
        {
            try
            {
                var blogPosts = _blogService.GetAllBlogPosts().ToList();

                var viewModels = blogPosts.Select(p => new BlogPostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Author = p.Author,
                    PostedDate = p.PostedDate,
                    Summary = p.Summary,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category,
                    CommentCount = p.Comments?.Count ?? 0
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "BlogController.Index");
                // Return an empty list as fallback
                return View(new List<BlogPostViewModel>());
            }
        }

        // GET: Blog/Post/5
        public ActionResult Post(int id)
        {
            try
            {
                var post = _blogService.GetBlogPostById(id);
                if (post == null)
                {
                    return RedirectToAction("Index");
                }

                var comments = _blogService.GetCommentsByBlogPostId(id).ToList();

                var commentViewModels = comments
                    .OrderByDescending(c => c.PostedAt)
                    .Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        BlogPostId = c.BlogPostId,
                        Username = c.User?.Username ?? "Unknown",
                        Content = c.Content,
                        PostedAt = c.PostedAt
                    })
                    .ToList();

                var viewModel = new BlogPostDetailViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Author = post.Author,
                    PostedDate = post.PostedDate,
                    Content = post.Content,
                    ImageUrl = post.ImageUrl,
                    Category = post.Category,
                    Comments = commentViewModels,
                    NewComment = new CreateCommentViewModel { BlogPostId = post.Id }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "BlogController.Post");
                return RedirectToAction("Index");
            }
        }

        // POST: Blog/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AddComment(int BlogPostId, string Content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Content))
                {
                    TempData["ErrorMessage"] = "Conținutul comentariului nu poate fi gol.";
                    return RedirectToAction("Post", new { id = BlogPostId });
                }

                string username = User.Identity.Name;
                var user = _userService.GetUserByUsername(username);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                    return RedirectToAction("Post", new { id = BlogPostId });
                }

                var comment = new BlogModel.Comment
                {
                    BlogPostId = BlogPostId,
                    UserId = user.Id,
                    Content = Content,
                    PostedAt = DateTime.Now
                };

                if (_blogService.CreateComment(comment, out string errorMessage))
                {
                    TempData["SuccessMessage"] = "Comentariul a fost adăugat cu succes.";
                }
                else
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "BlogController.AddComment");
                TempData["ErrorMessage"] = "A apărut o eroare la adăugarea comentariului.";
            }

            return RedirectToAction("Post", new { id = BlogPostId });
        }

        // GET: Blog/AddTestComment/5 (for testing purposes)
        public ActionResult AddTestComment(int id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["ErrorMessage"] = "Trebuie să fiți autentificat pentru a adăuga comentarii.";
                    return RedirectToAction("Post", new { id = id });
                }

                // Create a test comment
                string username = User.Identity.Name;
                var user = _userService.GetUserByUsername(username);
                
                if (user != null)
                {
                    var testComment = new BlogModel.Comment
                    {
                        BlogPostId = id,
                        UserId = user.Id,
                        Content = $"Test comment added by {username} at {DateTime.Now}",
                        PostedAt = DateTime.Now
                    };

                    if (_blogService.CreateComment(testComment, out string errorMessage))
                    {
                        TempData["SuccessMessage"] = "Comentariul de test a fost adăugat cu succes.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = errorMessage;
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "BlogController.AddTestComment");
                TempData["ErrorMessage"] = "A apărut o eroare la adăugarea comentariului de test.";
            }

            return RedirectToAction("Post", new { id = id });
        }
    }
} 
