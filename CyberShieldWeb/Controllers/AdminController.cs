using System;
using System.Linq;
using System.Web.Mvc;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;
using CyberShieldWeb.Models.Admin;
using BlogModel = CyberShield.Domain.Model.Blog;
using UserModel = CyberShield.Domain.Model.User;

namespace CyberShieldWeb.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IBlogService _blogService;
        private readonly IUserService _userService;
        private readonly IAuth _authService;
        private readonly IErrorHandlingService _errorHandler;

        public AdminController()
        {
            var bl = new BL.BusinessLogic();
            _adminService = bl.GetAdminService();
            _blogService = bl.GetBlogService();
            _userService = bl.GetUserService();
            _authService = bl.GetAuthBL();
            _errorHandler = bl.GetErrorHandlingService();
        }

        [NonAction]
        private bool IsAdmin()
        {
            if (!User.Identity.IsAuthenticated)
                return false;
                
            string username = User.Identity.Name;
            return _authService.IsUserAdmin(username);
        }

        [NonAction]
        private ActionResult CheckAdminAccess()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        // GET: Admin
        public ActionResult Index()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var users = _adminService.GetAllUsers().ToList();
                var blogPosts = _adminService.GetAllBlogPosts().ToList();
                var appointments = _adminService.GetAllAppointments().ToList();

                var adminDashboard = new AdminDashboardViewModel
                {
                    UserCount = users.Count,
                    BlogPostCount = blogPosts.Count,
                    CommentCount = 0, // Will be calculated from blog posts
                    LatestUsers = users.OrderByDescending(u => u.Id).Take(5).ToList(),
                    LatestBlogPosts = blogPosts.OrderByDescending(b => b.PostedDate).Take(5).ToList()
                };

                return View(adminDashboard);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.Index");
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea dashboard-ului.";
                return View(new AdminDashboardViewModel());
            }
        }

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var users = _adminService.GetAllUsers().ToList();
                var blogPosts = _adminService.GetAllBlogPosts().ToList();
                var appointments = _adminService.GetAllAppointments().ToList();

                var adminDashboard = new AdminDashboardViewModel
                {
                    UserCount = users.Count,
                    BlogPostCount = blogPosts.Count,
                    CommentCount = 0, // Will be calculated from blog posts
                    LatestUsers = users.OrderByDescending(u => u.Id).Take(5).ToList(),
                    LatestBlogPosts = blogPosts.OrderByDescending(b => b.PostedDate).Take(5).ToList()
                };

                return View("Index", adminDashboard);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.Dashboard");
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea dashboard-ului.";
                return View("Index", new AdminDashboardViewModel());
            }
        }

        // GET: Admin/Users
        public ActionResult Users()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var users = _adminService.GetAllUsers().ToList();
                return View(users);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.Users");
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea utilizatorilor.";
                return View(new System.Collections.Generic.List<UserModel.User>());
            }
        }

        // GET: Admin/UserDetails/5
        public ActionResult UserDetails(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var user = _userService.GetUserById(id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.UserDetails");
                return HttpNotFound();
            }
        }

        // GET: Admin/EditUser/5
        public ActionResult EditUser(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var user = _userService.GetUserById(id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new EditUserViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.EditUser");
                return HttpNotFound();
            }
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(EditUserViewModel model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _userService.GetUserById(model.Id);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.IsAdmin = model.IsAdmin;

                    if (_userService.UpdateUser(user, out string errorMessage))
                    {
                        TempData["SuccessMessage"] = "Utilizatorul a fost actualizat cu succes.";
                        return RedirectToAction("Users");
                    }
                    else
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler?.LogError(ex, "AdminController.EditUser POST");
                    ModelState.AddModelError("", "A apărut o eroare la actualizarea utilizatorului.");
                }
            }

            return View(model);
        }

        // GET: Admin/DeleteUser/5
        public ActionResult DeleteUser(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var user = _userService.GetUserById(id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.DeleteUser");
                return HttpNotFound();
            }
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUserConfirmed(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                if (_adminService.DeleteUser(id, out string errorMessage))
                {
                    TempData["SuccessMessage"] = "Utilizatorul a fost șters cu succes.";
                }
                else
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.DeleteUserConfirmed");
                TempData["ErrorMessage"] = "A apărut o eroare la ștergerea utilizatorului.";
            }

            return RedirectToAction("Users");
        }

        // GET: Admin/BlogPosts
        public ActionResult BlogPosts()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var blogPosts = _adminService.GetAllBlogPosts().OrderByDescending(p => p.PostedDate).ToList();
                return View(blogPosts);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.BlogPosts");
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea postărilor.";
                return View(new System.Collections.Generic.List<BlogModel.BlogPost>());
            }
        }

        // GET: Admin/EditBlogPost/5
        public ActionResult EditBlogPost(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var blogPost = _blogService.GetBlogPostById(id);
                if (blogPost == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new EditBlogPostViewModel
                {
                    Id = blogPost.Id,
                    Title = blogPost.Title,
                    Author = blogPost.Author,
                    Summary = blogPost.Summary,
                    Content = blogPost.Content,
                    Category = blogPost.Category,
                    ImageUrl = blogPost.ImageUrl
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.EditBlogPost");
                return HttpNotFound();
            }
        }

        // POST: Admin/EditBlogPost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML content
        public ActionResult EditBlogPost(EditBlogPostViewModel model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            if (ModelState.IsValid)
            {
                try
                {
                    var blogPost = _blogService.GetBlogPostById(model.Id);
                    if (blogPost == null)
                    {
                        return HttpNotFound();
                    }

                    blogPost.Title = model.Title;
                    blogPost.Author = model.Author;
                    blogPost.Summary = model.Summary;
                    blogPost.Content = model.Content;
                    blogPost.Category = model.Category;
                    blogPost.ImageUrl = model.ImageUrl;

                    if (_blogService.UpdateBlogPost(blogPost, out string errorMessage))
                    {
                        TempData["SuccessMessage"] = "Postarea a fost actualizată cu succes.";
                        return RedirectToAction("BlogPosts");
                    }
                    else
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler?.LogError(ex, "AdminController.EditBlogPost POST");
                    ModelState.AddModelError("", "A apărut o eroare la actualizarea postării.");
                }
            }

            return View(model);
        }

        // GET: Admin/CreateBlogPost
        public ActionResult CreateBlogPost()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            return View(new CreateBlogPostViewModel());
        }

        // POST: Admin/CreateBlogPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML content
        public ActionResult CreateBlogPost(CreateBlogPostViewModel model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            if (ModelState.IsValid)
            {
                try
                {
                    var blogPost = new BlogModel.BlogPost
                    {
                        Title = model.Title,
                        Author = model.Author,
                        Summary = model.Summary,
                        Content = model.Content,
                        Category = model.Category,
                        ImageUrl = model.ImageUrl,
                        PostedDate = DateTime.Now
                    };

                    if (_blogService.CreateBlogPost(blogPost, out string errorMessage))
                    {
                        TempData["SuccessMessage"] = "Postarea a fost creată cu succes.";
                        return RedirectToAction("BlogPosts");
                    }
                    else
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler?.LogError(ex, "AdminController.CreateBlogPost POST");
                    ModelState.AddModelError("", "A apărut o eroare la crearea postării.");
                }
            }

            return View(model);
        }

        // GET: Admin/DeleteBlogPost/5
        public ActionResult DeleteBlogPost(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var blogPost = _blogService.GetBlogPostById(id);
                if (blogPost == null)
                {
                    return HttpNotFound();
                }

                return View(blogPost);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.DeleteBlogPost");
                return HttpNotFound();
            }
        }

        // POST: Admin/DeleteBlogPost/5
        [HttpPost, ActionName("DeleteBlogPost")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBlogPostConfirmed(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                if (_adminService.DeleteBlogPost(id, out string errorMessage))
                {
                    TempData["SuccessMessage"] = "Postarea a fost ștearsă cu succes.";
                }
                else
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.DeleteBlogPostConfirmed");
                TempData["ErrorMessage"] = "A apărut o eroare la ștergerea postării.";
            }

            return RedirectToAction("BlogPosts");
        }

        // GET: Admin/Comments
        public ActionResult Comments()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var comments = _adminService.GetFlaggedComments().ToList();
                return View(comments);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.Comments");
                TempData["ErrorMessage"] = "A apărut o eroare la încărcarea comentariilor.";
                return View(new System.Collections.Generic.List<BlogModel.Comment>());
            }
        }

        // GET: Admin/DeleteComment/5
        public ActionResult DeleteComment(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                var comment = _blogService.GetCommentById(id);
                if (comment == null)
                {
                    return HttpNotFound();
                }

                return View(comment);
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.DeleteComment");
                return HttpNotFound();
            }
        }

        // POST: Admin/DeleteComment/5
        [HttpPost, ActionName("DeleteComment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCommentConfirmed(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;
            
            try
            {
                if (_blogService.DeleteComment(id, out string errorMessage))
                {
                    TempData["SuccessMessage"] = "Comentariul a fost șters cu succes.";
                }
                else
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AdminController.DeleteCommentConfirmed");
                TempData["ErrorMessage"] = "A apărut o eroare la ștergerea comentariului.";
            }

            return RedirectToAction("Comments");
        }
    }
} 