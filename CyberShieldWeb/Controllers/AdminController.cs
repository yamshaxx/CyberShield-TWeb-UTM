using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CyberShield.Domain.Data;
using DomainBlog = CyberShield.Domain.Model.Blog;
using DomainUser = CyberShield.Domain.Model.User;
using CyberShieldWeb.Models.Admin;

namespace CyberShieldWeb.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly CyberShieldContext _db = new CyberShieldContext();

        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var adminDashboard = new AdminDashboardViewModel
            {
                UserCount = _db.Users.Count(),
                BlogPostCount = _db.BlogPosts.Count(),
                CommentCount = _db.Comments.Count(),
                LatestUsers = _db.Users.OrderByDescending(u => u.Id).Take(5).ToList(),
                LatestBlogPosts = _db.BlogPosts.OrderByDescending(b => b.PostedDate).Take(5).ToList()
            };

            return View(adminDashboard);
        }

        // GET: Admin/Users
        [Authorize(Roles = "Admin")]
        public ActionResult Users()
        {
            var users = _db.Users.ToList();
            return View(users);
        }

        // GET: Admin/UserDetails/5
        [Authorize(Roles = "Admin")]
        public ActionResult UserDetails(int id)
        {
            var user = _db.Users.Find(id);
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
            var user = _db.Users.Find(id);
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
                var user = _db.Users.Find(model.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.Username = model.Username;
                user.Email = model.Email;
                user.IsAdmin = model.IsAdmin;

                _db.Entry(user).State = EntityState.Modified;
                _db.SaveChanges();

                return RedirectToAction("Users");
            }

            return View(model);
        }

        // GET: Admin/DeleteUser/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteUser(int id)
        {
            var user = _db.Users.Find(id);
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
            var user = _db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            _db.Users.Remove(user);
            _db.SaveChanges();

            return RedirectToAction("Users");
        }

        // GET: Admin/BlogPosts
        [Authorize(Roles = "Admin")]
        public ActionResult BlogPosts()
        {
            var blogPosts = _db.BlogPosts.OrderByDescending(p => p.PostedDate).ToList();
            return View(blogPosts);
        }

        // GET: Admin/EditBlogPost/5
        [Authorize(Roles = "Admin")]
        public ActionResult EditBlogPost(int id)
        {
            var blogPost = _db.BlogPosts.Find(id);
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
                var blogPost = _db.BlogPosts.Find(model.Id);
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

                _db.Entry(blogPost).State = EntityState.Modified;
                _db.SaveChanges();

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
                var blogPost = new DomainBlog.BlogPost
                {
                    Title = model.Title,
                    Author = model.Author,
                    PostedDate = model.PostedDate,
                    Summary = model.Summary,
                    Content = model.Content,
                    Category = model.Category,
                    ImageUrl = model.ImageUrl
                };

                _db.BlogPosts.Add((BlogPost)blogPost);
                _db.SaveChanges();

                return RedirectToAction("BlogPosts");
            }

            return View(model);
        }

        // GET: Admin/DeleteBlogPost/5
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteBlogPost(int id)
        {
            var blogPost = _db.BlogPosts.Find(id);
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
            var blogPost = _db.BlogPosts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }

            _db.BlogPosts.Remove(blogPost);
            _db.SaveChanges();

            return RedirectToAction("BlogPosts");
        }

        // GET: Admin/Comments
        [Authorize(Roles = "Admin")]
        public ActionResult Comments()
        {
            var comments = _db.Comments
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
            var comment = _db.Comments
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
            var comment = _db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }

            _db.Comments.Remove(comment);
            _db.SaveChanges();

            return RedirectToAction("Comments");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 