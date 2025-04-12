using System;
using System.Linq;
using System.Web.Helpers;
using CyberShield.Domain.Data;
using CyberShield.Domain.Model.User;

namespace CreateAdminUser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CyberShield Admin User Creation Utility");
            Console.WriteLine("---------------------------------------");
            
            try
            {
                using (var db = new CyberShieldContext())
                {
                    // Check if admin user already exists
                    bool adminExists = db.Users.Any(u => u.IsAdmin);
                    
                    if (adminExists)
                    {
                        Console.WriteLine("An admin user already exists in the database.");
                        Console.WriteLine("Existing admin users:");
                        
                        var admins = db.Users.Where(u => u.IsAdmin).ToList();
                        foreach (var admin in admins)
                        {
                            Console.WriteLine($"- Username: {admin.Username}, Email: {admin.Email}");
                        }
                        
                        Console.Write("Do you want to create another admin user? (y/n): ");
                        string answer = Console.ReadLine().ToLower();
                        
                        if (answer != "y" && answer != "yes")
                        {
                            Console.WriteLine("Operation cancelled. Press any key to exit.");
                            Console.ReadKey();
                            return;
                        }
                    }
                    
                    // Get admin user details
                    Console.Write("Enter admin username: ");
                    string username = Console.ReadLine();
                    
                    Console.Write("Enter admin email: ");
                    string email = Console.ReadLine();
                    
                    Console.Write("Enter admin password: ");
                    string password = Console.ReadLine();
                    
                    // Check if username or email already exists
                    if (db.Users.Any(u => u.Username == username))
                    {
                        Console.WriteLine("Error: Username already exists. Would you like to update this user to admin? (y/n): ");
                        string updateAnswer = Console.ReadLine().ToLower();
                        
                        if (updateAnswer == "y" || updateAnswer == "yes")
                        {
                            var existingUser = db.Users.First(u => u.Username == username);
                            existingUser.IsAdmin = true;
                            db.SaveChanges();
                            
                            Console.WriteLine($"User '{username}' has been updated to have admin rights.");
                            Console.WriteLine("Press any key to exit.");
                            Console.ReadKey();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Operation cancelled. Press any key to exit.");
                            Console.ReadKey();
                            return;
                        }
                    }
                    
                    if (db.Users.Any(u => u.Email == email))
                    {
                        Console.WriteLine("Error: Email already exists. Please use a different email.");
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey();
                        return;
                    }
                    
                    // Create a new admin user
                    var adminUser = new User
                    {
                        Username = username,
                        Email = email,
                        PasswordHash = Crypto.HashPassword(password),
                        IsAdmin = true
                    };
                    
                    // Save the admin user to the database
                    db.Users.Add(adminUser);
                    db.SaveChanges();
                    
                    Console.WriteLine($"Admin user '{username}' created successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
            }
            
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
} 