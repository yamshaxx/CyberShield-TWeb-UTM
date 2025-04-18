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
using CyberShield.Domain.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;

namespace CyberShieldWeb
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Display startup message
            System.Diagnostics.Debug.WriteLine("Application starting - initializing components...");
           
            // Set |DataDirectory| for LocalDB files and ensure the directory exists
            string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(dataDirectory))
            {
                dataDirectory = Server.MapPath("~/App_Data");
                
                // Ensure the App_Data directory exists
                if (!Directory.Exists(dataDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(dataDirectory);
                        System.Diagnostics.Debug.WriteLine($"Created App_Data directory: {dataDirectory}");
                    }
                    catch (Exception dirEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating App_Data directory: {dirEx.Message}");
                    }
                }
                
                // Ensure the App_Data directory has proper permissions
                try
                {
                    var dirInfo = new DirectoryInfo(dataDirectory);
                    var dirSecurity = dirInfo.GetAccessControl();
                    // Verify the directory exists and is accessible
                    System.Diagnostics.Debug.WriteLine($"App_Data directory exists and is accessible: {dataDirectory}");
                    
                    // Create an empty MDF file if needed (just to ensure the location is writeable)
                    string mdfPath = Path.Combine(dataDirectory, "CyberShield.mdf");
                    if (!File.Exists(mdfPath))
                    {
                        try
                        {
                            // Touch the file to make sure we can write there
                            using (var fs = File.Create(mdfPath + ".empty"))
                            {
                                fs.Close();
                            }
                            File.Delete(mdfPath + ".empty");
                            System.Diagnostics.Debug.WriteLine($"Verified write access to MDF location: {mdfPath}");
                        }
                        catch (Exception fileEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Cannot write to MDF location: {fileEx.Message}");
                        }
                    }
                }
                catch (Exception secEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error checking App_Data permissions: {secEx.Message}");
                }
                
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
                System.Diagnostics.Debug.WriteLine($"DataDirectory set to: {dataDirectory}");
            }
            
            // Initialize the database first, before any controllers are created
            try
            {
                // Verify connection string first
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CyberShieldConnection"].ConnectionString;
                System.Diagnostics.Debug.WriteLine($"Connection string: {connectionString}");
                
                // Test direct SQL connection
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        System.Diagnostics.Debug.WriteLine("Direct SQL connection test successful");
                        connection.Close();
                    }
                    catch (Exception sqlEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Direct SQL connection failed: {sqlEx.Message}");
                        // Continue to try EF initialization anyway
                    }
                }
                
                // Force database initialization
                System.Diagnostics.Debug.WriteLine("Starting database initialization...");
                CyberShieldContext.EnsureDbAndTablesCreated();
                
                // Log success
                System.Diagnostics.Debug.WriteLine("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                // Log database initialization error with full details
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Log inner exception if available
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                
                // Continue with application startup even if database initialization fails
                System.Diagnostics.Debug.WriteLine("Continuing application startup despite database initialization failure");
            }
           
            // Register MVC components after database initialization
            try
            {
                System.Diagnostics.Debug.WriteLine("Registering MVC areas...");
                AreaRegistration.RegisterAllAreas();
                
                System.Diagnostics.Debug.WriteLine("Registering routes...");
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                
                System.Diagnostics.Debug.WriteLine("Registering bundles...");
                BundleConfig.RegisterBundle(BundleTable.Bundles);
                
                System.Diagnostics.Debug.WriteLine("MVC components registered successfully");
            }
            catch (Exception mvcEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering MVC components: {mvcEx.Message}");
                if (mvcEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {mvcEx.InnerException.Message}");
                }
                // Re-throw MVC initialization errors as they are critical
                throw;
            }
            
            System.Diagnostics.Debug.WriteLine("Application started successfully");
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