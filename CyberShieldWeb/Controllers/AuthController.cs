using System;
using System.Web.Mvc;
using System.Web.Security;
using CyberShieldWeb.Models.Auth;
using CyberShield.Domain.Model.User;
using CyberShield.BusinessLogic.Interface;
using BL = CyberShield.BusinessLogic;

namespace CyberShieldWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuth _auth;
        private readonly IErrorHandlingService _errorHandler;

        public AuthController()
        {
            var bl = new BL.BusinessLogic();
            _auth = bl.GetAuthBL();
            _errorHandler = bl.GetErrorHandlingService();
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
                    // Convert view model to DTO
                    var userDto = new UserRegistrationDTO
                    {
                        Username = model.Username,
                        Email = model.Email,
                        Password = model.Password
                    };
                    
                    // Use the authentication service to register the user
                    bool success = _auth.RegisterUser(userDto, out string errorMessage);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Înregistrarea a fost realizată cu succes! Vă puteți autentifica acum.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError("", errorMessage);
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler?.LogError(ex, "AuthController.Register");
                    ModelState.AddModelError("", "A apărut o eroare în timpul înregistrării. Vă rugăm să încercați din nou.");
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
                    // Convert view model to DTO
                    var loginDto = new UserLoginDTO
                    {
                        UserName = model.Username,
                        Password = model.Password,
                        RememberMe = model.RememberMe,
                        UserIp = Request.UserHostAddress ?? "localhost"
                    };
                    
                    // Use the authentication service to login the user
                    bool success = _auth.LoginUser(loginDto, out string errorMessage, out User user);
                    
                    if (success && user != null)
                    {
                        // Set authentication cookie
                        FormsAuthentication.SetAuthCookie(user.Username, false);
                        
                        // Check user role and redirect appropriately
                        if (user.IsSpecialist)
                        {
                            return RedirectToAction("Dashboard", "Specialist");
                        }
                        else if (user.IsAdmin)
                        {
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Dashboard", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", errorMessage ?? "Nume de utilizator sau parolă incorectă.");
                    }
                }
                catch (Exception ex)
                {
                    _errorHandler?.LogError(ex, "AuthController.Login");
                    ModelState.AddModelError("", "A apărut o eroare în timpul autentificării. Vă rugăm să încercați din nou.");
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

        // Only accessible directly through URL, for testing purposes
        public ActionResult CreateAdmin()
        {
            try
            {
                bool success = _auth.CreateAdminUser(out string errorMessage);
                
                if (success)
                {
                    return Content("Admin account created successfully. Username: admin, Password: Admin123!");
                }
                else
                {
                    return Content("Error creating admin account: " + errorMessage);
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AuthController.CreateAdmin");
                return Content("Error creating admin account: " + ex.Message);
            }
        }

        // Only accessible directly through URL, for testing purposes
        public ActionResult CreateSpecialist()
        {
            try
            {
                bool success = _auth.CreateSpecialistUser(out string errorMessage);
                
                if (success)
                {
                    return Content("Specialist account created successfully. Username: specialist, Password: Admin123!");
                }
                else
                {
                    return Content("Error creating specialist account: " + errorMessage);
                }
            }
            catch (Exception ex)
            {
                _errorHandler?.LogError(ex, "AuthController.CreateSpecialist");
                return Content("Error creating specialist account: " + ex.Message);
            }
        }
    }
}
