using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CyberShieldWeb.Models.Auth;
using CyberShield.Domain.Model.User;
using CyberShield.BusinessLogic.Interface;
using CyberShield.BusinessLogic;

namespace CyberShieldWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuth _auth;

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
    }
}
