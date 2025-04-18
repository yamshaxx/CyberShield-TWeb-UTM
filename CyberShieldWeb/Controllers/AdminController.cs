using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CyberShield.Domain.Data;
using BlogModel = CyberShield.Domain.Model.Blog;
using UserModel = CyberShield.Domain.Model.User;
using CyberShieldWeb.Models.Admin;

namespace CyberShieldWeb.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private CyberShieldContext _db;
        
        // Lazy-load the database context to avoid initialization during controller construction
        private CyberShieldContext Db
        {
            get
            {
                if (_db == null)
                {
                    _db = new CyberShieldContext();
                }
                return _db;
            }
        }

        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var adminDashboard = new AdminDashboardViewModel
            {
                UserCount = Db.Users.Count(),
                BlogPostCount = Db.BlogPosts.Count(),
                CommentCount = Db.Comments.Count(),
                LatestUsers = Db.Users.OrderByDescending(u => u.Id).Take(5).ToList(),
                LatestBlogPosts = Db.BlogPosts.OrderByDescending(b => b.PostedDate).Take(5).ToList()
            };

            return View(adminDashboard);
        }

        // GET: Admin/Users
        [Authorize(Roles = "Admin")]
        public ActionResult Users()
        {
            var users = Db.Users.ToList();
            return View(users);
        }

        // GET: Admin/UserDetails/5
        [Authorize(Roles = "Admin")]
        public ActionResult UserDetails(int id)
        {
            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // GET: Admin/EditUser/5
        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(int id)
        {
            var user = Db.Users.Find(id);
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

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Db.Users.Find(model.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.Username = model.Username;
                user.Email = model.Email;
                user.IsAdmin = model.IsAdmin;

                Db.Entry(user).State = EntityState.Modified;
                Db.SaveChanges();

                return RedirectToAction("Users");
            }

            return View(model);
        }

        // GET: Admin/DeleteUser/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteUser(int id)
        {
            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteUserConfirmed(int id)
        {
            var user = Db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            Db.Users.Remove(user);
            Db.SaveChanges();

            return RedirectToAction("Users");
        }

        // GET: Admin/BlogPosts
        [Authorize(Roles = "Admin")]
        public ActionResult BlogPosts()
        {
            var blogPosts = Db.BlogPosts.OrderByDescending(p => p.PostedDate).ToList();
            return View(blogPosts);
        }

        // GET: Admin/EditBlogPost/5
        [Authorize(Roles = "Admin")]
        public ActionResult EditBlogPost(int id)
        {
            var blogPost = Db.BlogPosts.Find(id);
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

        // POST: Admin/EditBlogPost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML content
        [Authorize(Roles = "Admin")]
        public ActionResult EditBlogPost(EditBlogPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var blogPost = Db.BlogPosts.Find(model.Id);
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

                Db.Entry(blogPost).State = EntityState.Modified;
                Db.SaveChanges();

                return RedirectToAction("BlogPosts");
            }

            return View(model);
        }

        // GET: Admin/CreateBlogPost
        [Authorize(Roles = "Admin")]
        public ActionResult CreateBlogPost()
        {
            return View(new CreateBlogPostViewModel
            {
                PostedDate = DateTime.Now
            });
        }

        // POST: Admin/CreateBlogPost
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML content
        [Authorize(Roles = "Admin")]
        public ActionResult CreateBlogPost(CreateBlogPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var blogPost = new BlogModel.BlogPost
                {
                    Title = model.Title,
                    Author = model.Author,
                    PostedDate = model.PostedDate,
                    Summary = model.Summary,
                    Content = model.Content,
                    Category = model.Category,
                    ImageUrl = model.ImageUrl
                };

                Db.BlogPosts.Add(blogPost);
                Db.SaveChanges();

                return RedirectToAction("BlogPosts");
            }

            return View(model);
        }

        // GET: Admin/DeleteBlogPost/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteBlogPost(int id)
        {
            var blogPost = Db.BlogPosts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }

            return View(blogPost);
        }

        // POST: Admin/DeleteBlogPost/5
        [HttpPost, ActionName("DeleteBlogPost")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteBlogPostConfirmed(int id)
        {
            var blogPost = Db.BlogPosts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }

            Db.BlogPosts.Remove(blogPost);
            Db.SaveChanges();

            return RedirectToAction("BlogPosts");
        }

        // GET: Admin/Comments
        [Authorize(Roles = "Admin")]
        public ActionResult Comments()
        {
            var comments = Db.Comments
                .Include(c => c.User)
                .Include(c => c.BlogPost)
                .OrderByDescending(c => c.PostedAt)
                .ToList();
            
            return View(comments);
        }

        // GET: Admin/DeleteComment/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteComment(int id)
        {
            var comment = Db.Comments
                .Include(c => c.User)
                .Include(c => c.BlogPost)
                .FirstOrDefault(c => c.Id == id);
                
            if (comment == null)
            {
                return HttpNotFound();
            }

            return View(comment);
        }

        // POST: Admin/DeleteComment/5
        [HttpPost, ActionName("DeleteComment")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCommentConfirmed(int id)
        {
            var comment = Db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }

            Db.Comments.Remove(comment);
            Db.SaveChanges();

            return RedirectToAction("Comments");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _db != null)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 