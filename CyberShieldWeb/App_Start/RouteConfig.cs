using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CyberShieldWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Ruta specifică pentru pagina principală - trebuie să fie prima
            routes.MapRoute(
                name: "Home",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );
            
            // Ruta specifică pentru exact /servicii 
            routes.MapRoute(
                name: "Servicii",
                url: "servicii",
                defaults: new { controller = "Servicii", action = "Index" }
            );
            
            // Rute pentru paginile specifice de servicii
            routes.MapRoute(
                name: "TestareaPenetrarii",
                url: "servicii/testarea-penetrarii",
                defaults: new { controller = "Servicii", action = "TestareaPenetrarii" }
            );

            routes.MapRoute(
                name: "Consultanta",
                url: "servicii/consultanta",
                defaults: new { controller = "Servicii", action = "Consultanta" }
            );

            routes.MapRoute(
                name: "InginerieSociala",
                url: "servicii/inginerie-sociala",
                defaults: new { controller = "Servicii", action = "InginerieSociala" }
            );

            routes.MapRoute(
                name: "ConformitateGDPR",
                url: "servicii/conformitate-gdpr",
                defaults: new { controller = "Servicii", action = "ConformitateGDPR" }
            );

            // Ruta implicită - trebuie să fie ultima
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
