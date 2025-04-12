using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CyberShieldWeb.Models.Auth;
using CyberShield.Domain.Model.User;
using CyberShield.BusinessLogic.Interface;
using CyberShield.BusinessLogic;
using CyberShield.Domain.Data;
using System.Web.Helpers;
using System.Web.Security;

namespace CyberShieldWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuth _auth;
        private readonly CyberShieldContext _db = new CyberShieldContext();

        public AuthController()
        {
            var bl = new BusinessLogic();
            _auth = bl.GetAuthBL();
        }

        // GET: Auth
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Auth(UserDataLogin login)
        {
            var data = new UserLoginDTO()
            {
                Password = login.Password,
                UserName = login.UserName,
                UserIp = "localhost"
            };
            string taken = _auth.UserAuthLogic(data);
            return View();
        }

        // GET: Auth/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if username or email already exists
                    if (_db.Users.Any(u => u.Username == model.Username))
                    {
                        ModelState.AddModelError("Username", "Username already exists.");
                        return View(model);
                    }

                    if (_db.Users.Any(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                        return View(model);
                    }

                    // Create a new user
                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = Crypto.HashPassword(model.Password)
                    };

                    // Save the user to the database
                    _db.Users.Add(user);
                    _db.SaveChanges();

                    // Auto-login the user after registration
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    // Redirect to home page
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Log the error (you could add real logging)
                    ModelState.AddModelError("", "An error occurred while registering: " + ex.Message);
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        // GET: Auth/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Find the user by username
                    var user = _db.Users.SingleOrDefault(u => u.Username == model.Username);

                    if (user != null && Crypto.VerifyHashedPassword(user.PasswordHash, model.Password))
                    {
                        // Create custom authentication ticket with roles
                        var ticket = new FormsAuthenticationTicket(
                            1,                              // ticket version
                            user.Username,                  // username
                            DateTime.Now,                   // issue time
                            DateTime.Now.AddMinutes(30),    // expiration time
                            model.RememberMe,               // persistent
                            user.IsAdmin ? "Admin" : "User" // user data/roles
                        );

                        // Encrypt the ticket
                        var encryptedTicket = FormsAuthentication.Encrypt(ticket);

                        // Create the cookie
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                        Response.Cookies.Add(cookie);

                        // Redirect the user to the return URL if provided
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid username or password.");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    ModelState.AddModelError("", "An error occurred while logging in: " + ex.Message);
                }
            }

            // If we got this far, something failed; redisplay form
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: Auth/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
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
