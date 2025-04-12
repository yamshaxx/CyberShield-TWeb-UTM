using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using CyberShieldWeb.App_Start;

namespace CyberShieldWeb
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
           AreaRegistration.RegisterAllAreas();
           RouteConfig.RegisterRoutes(RouteTable.Routes);
           BundleConfig.RegisterBundle(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            // Get FormsAuthentication ticket from the cookie
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            
            if (authCookie != null)
            {
                try
                {
                    // Decrypt the cookie to get the FormsAuthenticationTicket
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                    // Create a custom principal with roles
                    string[] roles = authTicket.UserData.Split(',');
                    var userIdentity = new System.Security.Principal.GenericIdentity(authTicket.Name);
                    var userPrincipal = new System.Security.Principal.GenericPrincipal(userIdentity, roles);

                    // Set the context user
                    Context.User = userPrincipal;
                }
                catch
                {
                    // In case of any error, don't set the principal
                }
            }
        }
    }
}