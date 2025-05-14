using BlogModel = CyberShield.Domain.Model.Blog;
using UserModel = CyberShield.Domain.Model.User;
using System.Data.Entity;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using System.Linq;
using CyberShield.Domain.Model;

namespace CyberShield.Domain.Data
{
    public class CyberShieldInitializer : CreateDatabaseIfNotExists<CyberShieldContext>
    {
        protected override void Seed(CyberShieldContext context)
        {
            // Add a default admin user
            var adminUser = new UserModel.User
            {
                Username = "admin",
                Email = "admin@cybershield.com",
                PasswordHash = "AQAAAAEAACcQAAAAEKX9R+G+HjJ6sNBEVxMBrVeX6bTXyoTFLvYZO8vXDKnHhAaXZJM8+LcVv8K0bzRPjg==", // Hashed "Admin123!"
                IsAdmin = true
            };
            context.Users.Add(adminUser);
            
            // Add a sample blog post
            var samplePost = new BlogModel.BlogPost
            {
                Title = "Welcome to CyberShield",
                Author = "System",
                PostedDate = DateTime.Now,
                Summary = "This is a sample blog post created automatically when the database is initialized.",
                Content = "<p>Welcome to the CyberShield cybersecurity platform. This is a sample blog post created when the database was first initialized.</p>",
                ImageUrl = "/Content/img/blog/welcome.jpg",
                Category = "Announcement"
            };
            context.BlogPosts.Add(samplePost);
            
            context.SaveChanges();
            
            base.Seed(context);
        }
    }

    public class CyberShieldContext : DbContext
    {
        private static bool _initialized = false;
        private static readonly object _initLock = new object();
        
        static CyberShieldContext()
        {
            // Disable database initialization completely for performance
            Database.SetInitializer<CyberShieldContext>(null);
        }
        
        public CyberShieldContext() : base("name=CyberShieldConnection")
        {
            // Enable change tracking to ensure updates are detected
            this.Configuration.ValidateOnSaveEnabled = true;
            this.Configuration.AutoDetectChangesEnabled = true;
            
            // Set a higher command timeout for schema operations
            Database.CommandTimeout = 120;
            
            // Create database if it doesn't exist
            if (!Database.Exists())
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Database does not exist, creating it");
                    Database.Create();
                    System.Diagnostics.Debug.WriteLine("Database created successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating database: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Database already exists");
            }
        }

        // Method to create database and tables directly with SQL commands
        public static void EnsureDbAndTablesCreated()
        {
            // Only run initialization once per application lifecycle
            if (_initialized)
                return;
                
            lock (_initLock)
            {
                // Check again after acquiring lock
                if (_initialized)
                    return;
                    
                try
                {
                    System.Diagnostics.Debug.WriteLine("Attempting database initialization...");
                    
                    // Get the App_Data path
                    string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                    if (string.IsNullOrEmpty(dataDirectory))
                    {
                        throw new InvalidOperationException("DataDirectory is not set");
                    }
                    
                    // Ensure we have a database file
                    string dbFilePath = Path.Combine(dataDirectory, "CyberShield.mdf");
                    System.Diagnostics.Debug.WriteLine($"Database file path: {dbFilePath}");
                    
                    if (!File.Exists(dbFilePath))
                    {
                        System.Diagnostics.Debug.WriteLine("Database file doesn't exist, will be created by Entity Framework");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Database file already exists");
                    }
                    
                    // Get connection string and ensure it uses the correct DataDirectory
                    string connectionString = ConfigurationManager.ConnectionStrings["CyberShieldConnection"].ConnectionString;
                    
                    // Try different SQL Server instances
                    var connectionStrings = new[] {
                        connectionString,
                        connectionString.Replace("Data Source=.\\SQLEXPRESS", "Data Source=(LocalDb)\\MSSQLLocalDB"),
                        "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=CyberShield;Integrated Security=True;MultipleActiveResultSets=True", 
                        "Data Source=(LocalDb)\\v11.0;Initial Catalog=CyberShield;Integrated Security=True",
                        "Data Source=localhost\\SQLEXPRESS;Initial Catalog=CyberShield;Integrated Security=True",
                        "Data Source=.;Initial Catalog=CyberShield;Integrated Security=True"
                    };
                    
                    bool success = false;
                    
                    foreach (var connStr in connectionStrings)
                    {
                        System.Diagnostics.Debug.WriteLine($"Trying connection string: {connStr}");
                        
                        if (TryInitializeSqlServer(connStr))
                        {
                            success = true;
                            System.Diagnostics.Debug.WriteLine("Successfully connected using: " + connStr);
                            break;
                        }
                    }
                    
                    if (!success)
                    {
                        System.Diagnostics.Debug.WriteLine("All connection attempts failed. Falling back to in-memory database");
                        CreateInMemoryDatabase();
                    }
                    
                    // Mark as initialized regardless of success/failure
                    _initialized = true;
                    System.Diagnostics.Debug.WriteLine("Database initialization completed");
                }
                catch (Exception ex)
                {
                    // Log detailed error
                    System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        System.Diagnostics.Debug.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                    }
                    
                    // Still mark as initialized to prevent repeated failures
                    _initialized = true;
                    
                    // Rethrow for upper layers to handle
                    throw;
                }
            }
        }
        
        private static bool TryInitializeSqlServer(string connectionString)
        {
            try
            {
                // Test connection first with direct SQL connection
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        System.Diagnostics.Debug.WriteLine("Database connection test successful");
                        
                        // Create tables using the open connection
                        CreateTablesWithSqlConnection(connection);
                        
                        connection.Close();
                        return true;
                    }
                    catch (Exception connEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Database connection failed: {connEx.Message}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Server initialization error: {ex.Message}");
                return false;
            }
        }
        
        private static void CreateTablesWithSqlConnection(SqlConnection connection)
        {
            var connectionState = connection.State;
            bool connectionOpened = false;
            
            if (connectionState != System.Data.ConnectionState.Open)
            {
                connection.Open();
                connectionOpened = true;
                System.Diagnostics.Debug.WriteLine("Database connection opened");
            }
            
            try
            {
                // Create Users table if it doesn't exist
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                        CREATE TABLE Users(
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Username NVARCHAR(50) NOT NULL,
                            Email NVARCHAR(100) NOT NULL,
                            PasswordHash NVARCHAR(MAX) NOT NULL,
                            IsAdmin BIT NOT NULL DEFAULT 0,
                            IsSpecialist BIT NOT NULL DEFAULT 0
                        )";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Create Users table result: {result}");
                }
                
                // If the table exists but IsAdmin column is missing, add it
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (
                            SELECT * FROM sys.columns 
                            WHERE Name = N'IsAdmin' AND Object_ID = Object_ID(N'Users')
                        )
                        BEGIN
                            ALTER TABLE Users ADD IsAdmin BIT NOT NULL DEFAULT 0
                            PRINT 'Added missing IsAdmin column'
                        END";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Add IsAdmin column result: {result}");
                }
                
                // If the table exists but IsSpecialist column is missing, add it
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (
                            SELECT * FROM sys.columns 
                            WHERE Name = N'IsSpecialist' AND Object_ID = Object_ID(N'Users')
                        )
                        BEGIN
                            ALTER TABLE Users ADD IsSpecialist BIT NOT NULL DEFAULT 0
                            PRINT 'Added missing IsSpecialist column'
                        END";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Add IsSpecialist column result: {result}");
                }
                
                // Add uniqueness constraints if they don't exist
                try 
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Username' AND object_id = OBJECT_ID('Users'))
                            CREATE UNIQUE INDEX IX_Username ON Users(Username)";
                        int result = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Create Username index result: {result}");
                    }
                    
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Email' AND object_id = OBJECT_ID('Users'))
                            CREATE UNIQUE INDEX IX_Email ON Users(Email)";
                        int result = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Create Email index result: {result}");
                    }
                }
                catch (Exception indexEx)
                {
                    // Log but continue if index creation fails
                    System.Diagnostics.Debug.WriteLine($"Index creation error: {indexEx.Message}");
                }
                
                // Create BlogPosts table if it doesn't exist
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BlogPosts')
                        CREATE TABLE BlogPosts(
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Title NVARCHAR(100) NOT NULL,
                            Author NVARCHAR(50) NOT NULL,
                            PostedDate DATETIME NOT NULL,
                            Summary NVARCHAR(500) NOT NULL,
                            Content NVARCHAR(MAX) NOT NULL,
                            ImageUrl NVARCHAR(255) NULL,
                            Category NVARCHAR(50) NULL
                        )";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Create BlogPosts table result: {result}");
                }
                
                // Create Comments table if it doesn't exist
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments')
                        CREATE TABLE Comments(
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            BlogPostId INT NOT NULL,
                            UserId INT NOT NULL,
                            Content NVARCHAR(2000) NOT NULL,
                            PostedAt DATETIME NOT NULL
                        )";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Create Comments table result: {result}");
                }
                
                // Create Appointments table if it doesn't exist
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Appointments')
                        CREATE TABLE Appointments(
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            UserId INT NOT NULL,
                            Name NVARCHAR(100) NOT NULL,
                            Email NVARCHAR(100) NOT NULL,
                            Phone NVARCHAR(20) NOT NULL,
                            Company NVARCHAR(100) NULL,
                            ServiceType NVARCHAR(50) NOT NULL,
                            PreferredDate DATETIME NOT NULL,
                            Message NVARCHAR(2000) NULL,
                            CreatedAt DATETIME NOT NULL,
                            Status NVARCHAR(50) NOT NULL
                        )";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Create Appointments table result: {result}");
                }
                
                // Create ContactMessages table if it doesn't exist
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContactMessages')
                        CREATE TABLE ContactMessages(
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Name NVARCHAR(100) NOT NULL,
                            Email NVARCHAR(100) NOT NULL,
                            Subject NVARCHAR(200) NOT NULL,
                            Message NVARCHAR(2000) NOT NULL,
                            SentDate DATETIME NOT NULL,
                            IsRead BIT NOT NULL DEFAULT 0
                        )";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Create ContactMessages table result: {result}");
                }
                
                // Add foreign keys for Comments table after making sure both parent tables exist
                try 
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_BlogPosts' AND parent_object_id = OBJECT_ID('Comments'))
                            ALTER TABLE Comments ADD CONSTRAINT FK_Comments_BlogPosts FOREIGN KEY (BlogPostId) REFERENCES BlogPosts(Id)";
                        int result = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Create BlogPosts FK result: {result}");
                    }
                    
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Comments_Users' AND parent_object_id = OBJECT_ID('Comments'))
                            ALTER TABLE Comments ADD CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId) REFERENCES Users(Id)";
                        int result = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Create Users FK result: {result}");
                    }
                    
                    // Add foreign key for Appointments
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Appointments_Users' AND parent_object_id = OBJECT_ID('Appointments'))
                            ALTER TABLE Appointments ADD CONSTRAINT FK_Appointments_Users FOREIGN KEY (UserId) REFERENCES Users(Id)";
                        int result = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Create Appointments FK result: {result}");
                    }
                }
                catch (Exception fkEx)
                {
                    // Log but continue if foreign key creation fails
                    System.Diagnostics.Debug.WriteLine($"Foreign key creation error: {fkEx.Message}");
                }
                
                // Check if admin user exists and add if not
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
                        BEGIN
                            INSERT INTO Users (Username, Email, PasswordHash, IsAdmin)
                            VALUES ('admin', 'admin@cybershield.com', 'AQAAAAEAACcQAAAAEKX9R+G+HjJ6sNBEVxMBrVeX6bTXyoTFLvYZO8vXDKnHhAaXZJM8+LcVv8K0bzRPjg==', 1)
                            PRINT 'Admin user created'
                        END";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Add admin user result: {result}");
                }
                
                // Check if sample blog post exists and add if not
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF NOT EXISTS (SELECT * FROM BlogPosts)
                        INSERT INTO BlogPosts (Title, Author, PostedDate, Summary, Content, ImageUrl, Category)
                        VALUES ('Welcome to CyberShield', 'System', GETDATE(), 
                        'This is a sample blog post created automatically when the database is initialized.', 
                        '<p>Welcome to the CyberShield cybersecurity platform. This is a sample blog post created when the database was first initialized.</p>', 
                        '/Content/img/blog/welcome.jpg', 'Announcement')";
                    int result = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Add sample post result: {result}");
                }
            }
            finally
            {
                // Close the connection if we opened it
                if (connectionOpened)
                {
                    connection.Close();
                    System.Diagnostics.Debug.WriteLine("Database connection closed");
                }
            }
        }
        
        private static void CreateInMemoryDatabase()
        {
            System.Diagnostics.Debug.WriteLine("Initializing in-memory database (fallback)");
            
            // No actual initialization needed - we'll just use in-memory collections
            // Code will naturally fall back to memory as Entity Framework operations fail
            System.Diagnostics.Debug.WriteLine("In-memory fallback mode activated");
            
            // Static collection for holding Users when database is not available
            if (!InMemoryData.Users.Any(u => u.Username == "admin"))  // Only add if not already there
            {
                InMemoryData.Users.Add(new UserModel.User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@cybershield.com",
                    // This exact hash corresponds to "Admin123!" 
                    PasswordHash = "AQAAAAEAACcQAAAAEKX9R+G+HjJ6sNBEVxMBrVeX6bTXyoTFLvYZO8vXDKnHhAaXZJM8+LcVv8K0bzRPjg==",
                    IsAdmin = true
                });
                
                System.Diagnostics.Debug.WriteLine("Added admin user to in-memory database");
            }
            
            // Add a default blog post
            if (!InMemoryData.BlogPosts.Any())  // Only add if not already there
            {
                InMemoryData.BlogPosts.Add(new BlogModel.BlogPost
                {
                    Id = 1,
                    Title = "Welcome to CyberShield",
                    Author = "System",
                    PostedDate = DateTime.Now,
                    Summary = "This is a sample blog post created automatically when the database is initialized.",
                    Content = "<p>Welcome to the CyberShield cybersecurity platform. This is a sample blog post created when the database was first initialized.</p>",
                    ImageUrl = "/Content/img/blog/welcome.jpg",
                    Category = "Announcement"
                });
                
                System.Diagnostics.Debug.WriteLine("Added sample blog post to in-memory database");
            }
            
            System.Diagnostics.Debug.WriteLine("In-memory database initialized with sample data");
        }

        public DbSet<UserModel.User> Users { get; set; }
        public DbSet<BlogModel.BlogPost> BlogPosts { get; set; }
        public DbSet<BlogModel.Comment> Comments { get; set; }
        public DbSet<BlogModel.Appointment> Appointments { get; set; }
        public DbSet<CyberShield.Domain.Model.ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<UserModel.User>()
                .HasIndex(u => u.Username)
                .IsUnique(true);

            modelBuilder.Entity<UserModel.User>()
                .HasIndex(u => u.Email)
                .IsUnique(true);
                
            // Configure table names explicitly
            modelBuilder.Entity<UserModel.User>().ToTable("Users");
            modelBuilder.Entity<BlogModel.BlogPost>().ToTable("BlogPosts");
            modelBuilder.Entity<BlogModel.Comment>().ToTable("Comments");
            modelBuilder.Entity<CyberShield.Domain.Model.ContactMessage>().ToTable("ContactMessages");
                
            // Explicitly ignore the Data namespace versions
            modelBuilder.Ignore<BlogPost>();
            modelBuilder.Ignore<Comment>();
        }
    }
} 