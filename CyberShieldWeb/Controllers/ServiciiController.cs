using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CyberShieldWeb.Controllers
{
    public class ServiciiController : Controller
    {
        // GET: Servicii
        public ActionResult Index()
        {
            return View();
        }

        // GET: Servicii/TestareaPenetrarii
        public ActionResult TestareaPenetrarii()
        {
            return View();
        }

        // GET: Servicii/Consultanta
        public ActionResult Consultanta()
        {
            return View();
        }

        // GET: Servicii/InginerieSociala
        public ActionResult InginerieSociala()
        {
            return View();
        }

        // GET: Servicii/ConformitateGDPR
        public ActionResult ConformitateGDPR()
        {
            return View();
        }
    }
} 
