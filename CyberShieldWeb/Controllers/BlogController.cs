using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CyberShield.Domain.Data;
using BlogModel = CyberShield.Domain.Model.Blog;
using UserModel = CyberShield.Domain.Model.User;
using CyberShieldWeb.Models.Blog;

namespace CyberShieldWeb.Controllers
{
    public class BlogController : Controller
    {
        private CyberShieldContext _db;
        
        // Lazy-load the database context to avoid initialization during controller construction
        private CyberShieldContext Db
        {
            get
            {
                if (_db == null)
                {
                    _db = new CyberShieldContext();
                }
                return _db;
            }
        }

        // GET: Blog
        public ActionResult Index()
        {
            try
            {
                // Verify and create database tables directly if they don't exist
                using (var conn = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CyberShieldConnection"].ConnectionString))
                {
                    conn.Open();
                    
                    // Check if BlogPosts table exists
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BlogPosts') " +
                        "CREATE TABLE BlogPosts(" +
                        "Id INT PRIMARY KEY IDENTITY(1,1), " +
                        "Title NVARCHAR(100) NOT NULL, " +
                        "Author NVARCHAR(50) NOT NULL, " +
                        "PostedDate DATETIME NOT NULL, " +
                        "Summary NVARCHAR(500) NOT NULL, " +
                        "Content NVARCHAR(MAX) NOT NULL, " +
                        "ImageUrl NVARCHAR(255) NULL, " +
                        "Category NVARCHAR(50) NULL)", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Check if Comments table exists
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments') " +
                        "CREATE TABLE Comments(" +
                        "Id INT PRIMARY KEY IDENTITY(1,1), " +
                        "BlogPostId INT NOT NULL, " +
                        "UserId INT NOT NULL, " +
                        "Content NVARCHAR(2000) NOT NULL, " +
                        "PostedAt DATETIME NOT NULL)", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Check if Users table exists
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users') " +
                        "CREATE TABLE Users(" +
                        "Id INT PRIMARY KEY IDENTITY(1,1), " +
                        "Username NVARCHAR(50) NOT NULL UNIQUE, " +
                        "Email NVARCHAR(100) NOT NULL UNIQUE, " +
                        "PasswordHash NVARCHAR(MAX) NOT NULL, " +
                        "IsAdmin BIT NOT NULL DEFAULT 0)", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Add the admin user if it doesn't exist
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin') " +
                        "INSERT INTO Users (Username, Email, PasswordHash, IsAdmin) " +
                        "VALUES ('admin', 'admin@cybershield.com', 'AQAAAAEAACcQAAAAEKX9R+G+HjJ6sNBEVxMBrVeX6bTXyoTFLvYZO8vXDKnHhAaXZJM8+LcVv8K0bzRPjg==', 1)", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Add a sample blog post if none exist
                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "IF NOT EXISTS (SELECT * FROM BlogPosts) " +
                        "INSERT INTO BlogPosts (Title, Author, PostedDate, Summary, Content, ImageUrl, Category) " +
                        "VALUES ('Welcome to CyberShield', 'System', GETDATE(), " +
                        "'This is a sample blog post created automatically when the database is initialized.', " +
                        "'<p>Welcome to the CyberShield cybersecurity platform. This is a sample blog post created when the database was first initialized.</p>', " +
                        "'/Content/img/blog/welcome.jpg', 'Announcement')", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                
                var blogPosts = Db.BlogPosts
                    .OrderByDescending(p => p.PostedDate)
                    .ToList();

                // If no blog posts exist yet, create sample posts
                if (blogPosts.Count == 0)
                {
                    CreateSampleBlogPosts();
                    blogPosts = Db.BlogPosts
                        .OrderByDescending(p => p.PostedDate)
                        .ToList();
                }

                var viewModels = blogPosts.Select(p => new BlogPostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Author = p.Author,
                    PostedDate = p.PostedDate,
                    Summary = p.Summary,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category,
                    CommentCount = p.Comments.Count
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error in BlogController.Index: {ex.Message}");
                
                // Return an empty list as fallback
                return View(new List<BlogPostViewModel>());
            }
        }

        // GET: Blog/Post/5
        public ActionResult Post(int id)
        {
            try
            {
                // First try to get from database
                BlogPostDetailViewModel viewModel = null;
                bool foundInDb = false;
                
                try
                {
                    var post = Db.BlogPosts
                        .Include(p => p.Comments.Select(c => c.User))
                        .FirstOrDefault(p => p.Id == id);
                    
                    if (post != null)
                    {
                        var comments = post.Comments
                            .OrderByDescending(c => c.PostedAt)
                            .Select(c => new CommentViewModel
                            {
                                Id = c.Id,
                                BlogPostId = c.BlogPostId,
                                Username = c.User?.Username ?? "Unknown",
                                Content = c.Content,
                                PostedAt = c.PostedAt
                            })
                            .ToList();

                        viewModel = new BlogPostDetailViewModel
                        {
                            Id = post.Id,
                            Title = post.Title,
                            Author = post.Author,
                            PostedDate = post.PostedDate,
                            Content = post.Content,
                            ImageUrl = post.ImageUrl,
                            Category = post.Category,
                            Comments = comments,
                            NewComment = new CreateCommentViewModel { BlogPostId = post.Id }
                        };
                        
                        foundInDb = true;
                    }
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Database error in Post method: {dbEx.Message}");
                    // Continue to in-memory fallback
                }
                
                // If not found in database, try in-memory
                if (!foundInDb)
                {
                    var post = InMemoryData.BlogPosts.FirstOrDefault(p => p.Id == id);
                    if (post == null)
                    {
                        return RedirectToAction("Index");
                    }
                    
                    // Get comments from in-memory data
                    var comments = InMemoryData.Comments
                        .Where(c => c.BlogPostId == id)
                        .OrderByDescending(c => c.PostedAt)
                        .Select(c => 
                        {
                            var user = InMemoryData.Users.FirstOrDefault(u => u.Id == c.UserId);
                            return new CommentViewModel
                            {
                                Id = c.Id,
                                BlogPostId = c.BlogPostId,
                                Username = user?.Username ?? "Unknown",
                                Content = c.Content,
                                PostedAt = c.PostedAt
                            };
                        })
                        .ToList();
                    
                    viewModel = new BlogPostDetailViewModel
                    {
                        Id = post.Id,
                        Title = post.Title,
                        Author = post.Author,
                        PostedDate = post.PostedDate,
                        Content = post.Content,
                        ImageUrl = post.ImageUrl ?? "/Content/img/blog/default.jpg",
                        Category = post.Category,
                        Comments = comments,
                        NewComment = new CreateCommentViewModel { BlogPostId = post.Id }
                    };
                }
                
                // Check if viewModel was assigned before returning
                if (viewModel == null)
                {
                    return RedirectToAction("Index");
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unhandled error in Post method: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // POST: Blog/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AddComment(int BlogPostId, string Content)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validate inputs
                    if (string.IsNullOrEmpty(Content))
                    {
                        ModelState.AddModelError("", "Comentariul nu poate fi gol.");
                        return RedirectToAction("Post", new { id = BlogPostId });
                    }
                    
                    string username = User.Identity.Name;
                    var user = Db.Users.FirstOrDefault(u => u.Username == username);
                    
                    // If user not found in database, try in-memory
                    if (user == null)
                    {
                        user = InMemoryData.Users.FirstOrDefault(u => u.Username == username);
                        if (user == null)
                        {
                            // Create a minimal user object if not found anywhere
                            user = new UserModel.User
                            {
                                Id = InMemoryData.Users.Any() ? InMemoryData.Users.Max(u => u.Id) + 1 : 1,
                                Username = username,
                                Email = username + "@example.com",
                                IsAdmin = false
                            };
                            InMemoryData.Users.Add(user);
                            System.Diagnostics.Debug.WriteLine($"Created temporary user for comment: {username}");
                        }
                    }

                    var comment = new BlogModel.Comment
                    {
                        BlogPostId = BlogPostId,
                        UserId = user.Id,
                        Content = Content,
                        PostedAt = DateTime.Now,
                        User = user // Set the navigation property
                    };

                    // Try to save to database first
                    bool savedToDb = false;
                    try
                    {
                        Db.Comments.Add(comment);
                        Db.SaveChanges();
                        savedToDb = true;
                        System.Diagnostics.Debug.WriteLine($"Comment saved to database for post {BlogPostId}");
                    }
                    catch (Exception dbEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving comment to database: {dbEx.Message}");
                        // Continue to in-memory fallback
                    }

                    // If database save failed, add to in-memory
                    if (!savedToDb)
                    {
                        // Assign an ID for the in-memory comment
                        if (InMemoryData.Comments.Any())
                        {
                            comment.Id = InMemoryData.Comments.Max(c => c.Id) + 1;
                        }
                        else
                        {
                            comment.Id = 1;
                        }
                        
                        InMemoryData.Comments.Add(comment);
                        System.Diagnostics.Debug.WriteLine($"Comment saved to in-memory for post {BlogPostId}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in AddComment: {ex.Message}");
                    ModelState.AddModelError("", "A apărut o eroare la adăugarea comentariului.");
                    // Continue to render the form again
                }

                return RedirectToAction("Post", new { id = BlogPostId });
            }

            return RedirectToAction("Post", new { id = BlogPostId });
        }

        // Helper method to create sample blog posts
        private void CreateSampleBlogPosts()
        {
            var samplePosts = new List<BlogModel.BlogPost>
            {
                new BlogModel.BlogPost
                {
                    Title = "Securitatea în era digitală: Ce trebuie să știți",
                    Author = "Ion Popescu",
                    PostedDate = DateTime.Now.AddDays(-2),
                    Summary = "În lumea digitală de astăzi, amenințările cibernetice evoluează constant. Aflați care sunt cele mai recente tendințe în securitatea cibernetică și cum vă puteți proteja datele sensibile.",
                    Content = "<p>În era digitală actuală, securitatea cibernetică a devenit o preocupare majoră pentru organizații și utilizatori individuali deopotrivă. Cu amenințări în continuă evoluție și atacuri din ce în ce mai sofisticate, este esențial să înțelegem și să implementăm măsuri de protecție adecvate.</p>" +
                    "<h2>Principalele amenințări cibernetice în 2024</h2>" +
                    "<p>Atacurile de tip ransomware, phishing-ul și breșele de date continuă să reprezinte amenințări semnificative. În plus, noile tehnologii precum inteligența artificială sunt folosite atât pentru apărare, cât și pentru atacuri.</p>" +
                    "<h2>Cum să vă protejați datele</h2>" +
                    "<ul>" +
                    "    <li>Utilizați parole puternice și unice pentru fiecare cont</li>" +
                    "    <li>Activați autentificarea în doi factori</li>" +
                    "    <li>Mențineți software-ul și sistemele de operare actualizate</li>" +
                    "    <li>Folosiți soluții de securitate de încredere</li>" +
                    "</ul>",
                    ImageUrl = "/Content/img/blog/cybersecurity.jpg",
                    Category = "Securitate"
                },
                new BlogModel.BlogPost
                {
                    Title = "Ingineria socială: Tehnici de manipulare psihologică în atacurile cibernetice",
                    Author = "Maria Ionescu",
                    PostedDate = DateTime.Now.AddDays(-5),
                    Summary = "Ingineria socială reprezintă una dintre cele mai eficiente metode de compromitere a securității informatice. Descoperiți cum funcționează atacurile de inginerie socială și cum vă puteți apăra împotriva lor.",
                    Content = "<p>Ingineria socială reprezintă ansamblul tehnicilor de manipulare psihologică utilizate pentru a determina utilizatorii legitimi să divulge informații confidențiale sau să efectueze acțiuni care pot compromite securitatea sistemelor informatice. Spre deosebire de atacurile tehnice, ingineria socială exploatează vulnerabilitățile umane, precum încrederea, frica sau dorința de a ajuta.</p>" +
                    "<h2>Cele mai comune tipuri de atacuri de inginerie socială</h2>" +
                    "<h3>Phishing</h3>" +
                    "<p>Atacurile de tip phishing implică trimiterea de e-mailuri care par a proveni din surse legitime, cum ar fi bănci, platforme de social media sau departamente IT. Aceste e-mailuri solicită informații sensibile sau conțin link-uri către site-uri false concepute pentru a fura date de autentificare.</p>" +
                    "<h3>Pretexting</h3>" +
                    "<p>În cazul pretextingului, atacatorul creează un scenariu fals pentru a câștiga încrederea victimei și a obține acces la informații valoroase. De exemplu, un atacator se poate prezenta drept reprezentant IT care are nevoie de credențialele utilizatorului pentru a efectua o \"actualizare de sistem\".</p>" +
                    "<h3>Baiting</h3>" +
                    "<p>Baiting-ul implică tentația unei oferte atractive pentru a determina victimele să efectueze acțiuni riscante. Un exemplu clasic este plasarea de dispozitive USB infectate în locuri publice, mizând pe curiozitatea oamenilor de a le conecta la calculatoarele lor.</p>" +
                    "<h2>Cum să vă protejați împotriva ingineriei sociale</h2>" +
                    "<ul>" +
                    "    <li>Verificați întotdeauna identitatea persoanelor care solicită informații sensibile</li>" +
                    "    <li>Fiți sceptici față de ofertele care par prea bune pentru a fi adevărate</li>" +
                    "    <li>Nu deschideți atașamente sau link-uri din e-mailuri suspecte</li>" +
                    "    <li>Implementați programe de conștientizare și instruire pentru angajați</li>" +
                    "    <li>Folosiți autentificarea multi-factor pentru a limita daunele în cazul compromiterii credențialelor</li>" +
                    "</ul>",
                    ImageUrl = "/Content/img/blog/social-engineering.jpg",
                    Category = "Inginerie Socială"
                },
                new BlogModel.BlogPost
                {
                    Title = "GDPR și securitatea datelor: Ce trebuie să știe fiecare afacere",
                    Author = "Alexandru Munteanu",
                    PostedDate = DateTime.Now.AddDays(-10),
                    Summary = "Regulamentul General privind Protecția Datelor (GDPR) impune obligații stricte pentru organizațiile care procesează date personale. Aflați cum să vă conformați cu cerințele legale și să evitați sancțiuni costisitoare.",
                    Content = "<p>Regulamentul General privind Protecția Datelor (GDPR) a revoluționat modul în care organizațiile trebuie să gestioneze datele personale în Uniunea Europeană. La peste cinci ani de la intrarea sa în vigoare, GDPR continuă să reprezinte standardul de aur în materie de protecție a datelor la nivel global.</p>" +
                    "<h2>Principiile fundamentale ale GDPR</h2>" +
                    "<p>GDPR se bazează pe șapte principii fundamentale:</p>" +
                    "<ol>" +
                    "    <li><strong>Legalitate, echitate și transparență</strong> - procesarea datelor trebuie să fie legală, corectă și transparentă pentru persoanele vizate.</li>" +
                    "    <li><strong>Limitarea scopului</strong> - datele trebuie colectate pentru scopuri specificate, explicite și legitime.</li>" +
                    "    <li><strong>Minimizarea datelor</strong> - doar datele necesare pentru scopurile declarate ar trebui procesate.</li>" +
                    "    <li><strong>Exactitate</strong> - datele trebuie să fie exacte și actualizate.</li>" +
                    "    <li><strong>Limitarea stocării</strong> - datele nu ar trebui păstrate mai mult decât este necesar.</li>" +
                    "    <li><strong>Integritate și confidențialitate</strong> - procesarea trebuie să asigure securitatea adecvată a datelor.</li>" +
                    "    <li><strong>Responsabilitate</strong> - organizațiile trebuie să poată demonstra conformitatea cu principiile GDPR.</li>" +
                    "</ol>" +
                    "<h2>Măsuri tehnice și organizaționale pentru conformitate</h2>" +
                    "<p>Conformitatea cu GDPR necesită implementarea unor măsuri adecvate de securitate. Acestea includ:</p>" +
                    "<ul>" +
                    "    <li>Pseudonimizarea și criptarea datelor personale</li>" +
                    "    <li>Asigurarea confidențialității, integrității și disponibilității sistemelor de procesare</li>" +
                    "    <li>Capacitatea de a restabili accesul la date în caz de incident fizic sau tehnic</li>" +
                    "    <li>Procese regulare de testare și evaluare a eficacității măsurilor de securitate</li>" +
                    "</ul>" +
                    "<h2>Responsabilități ale controlorului de date</h2>" +
                    "<p>Organizațiile care determină scopurile și mijloacele de procesare a datelor personale (controlorii) au numeroase responsabilități, inclusiv:</p>" +
                    "<ul>" +
                    "    <li>Menținerea unui registru al activităților de procesare</li>" +
                    "    <li>Realizarea evaluărilor de impact privind protecția datelor</li>" +
                    "    <li>Numirea unui responsabil cu protecția datelor (DPO), dacă este necesar</li>" +
                    "    <li>Notificarea autorităților și persoanelor afectate în caz de breșă de securitate</li>" +
                    "</ul>" +
                    "<h2>Consecințele neconformității</h2>" +
                    "<p>Nerespectarea prevederilor GDPR poate duce la:</p>" +
                    "<ul>" +
                    "    <li>Amenzi administrative de până la 20 milioane EUR sau 4% din cifra de afaceri globală anuală</li>" +
                    "    <li>Daune reputaționale semnificative</li>" +
                    "    <li>Pierderea încrederii clienților și partenerilor</li>" +
                    "    <li>Litigii costisitoare</li>" +
                    "</ul>",
                    ImageUrl = "/Content/img/blog/gdpr.jpg",
                    Category = "Conformitate"
                },
                new BlogModel.BlogPost
                {
                    Title = "Zero Trust: Viitorul arhitecturii de securitate",
                    Author = "Andrei Dumitrescu",
                    PostedDate = DateTime.Now.AddDays(-15),
                    Summary = "Modelul de securitate Zero Trust câștigă tot mai multă popularitate în contextul actual al amenințărilor cibernetice. Descoperiți principiile acestui model și cum poate fi implementat în organizația dumneavoastră.",
                    Content = "<p>În contextul actual al amenințărilor cibernetice în continuă evoluție, vechiul model de securitate perimetrală de tip \"castel și șanț\" nu mai este suficient. Modelul Zero Trust reprezintă o schimbare de paradigmă în abordarea securității, bazându-se pe principiul \"nu încredere niciodată, verifică întotdeauna\".</p>" +
                    "<h2>Principiile fundamentale ale Zero Trust</h2>" +
                    "<p>Modelul Zero Trust se bazează pe următoarele principii cheie:</p>" +
                    "<ol>" +
                    "    <li><strong>Verificarea explicită</strong> - Autentificarea și autorizarea bazate pe toate punctele de date disponibile</li>" +
                    "    <li><strong>Acces cu privilegii minime</strong> - Limitarea accesului utilizatorilor la doar ceea ce au nevoie pentru a-și îndeplini sarcinile</li>" +
                    "    <li><strong>Presupunerea breșei</strong> - Operarea cu prezumția că breșele sunt inevitabile sau că s-au produs deja</li>" +
                    "</ol>" +
                    "<h2>Componentele esențiale ale arhitecturii Zero Trust</h2>" +
                    "<h3>1. Identitate</h3>" +
                    "<p>Verificarea identității utilizatorilor, dispozitivelor și serviciilor prin autentificare multi-factor și managementul identității și accesului (IAM).</p>" +
                    "<h3>2. Dispozitive</h3>" +
                    "<p>Monitorizarea stării de sănătate și conformității dispozitivelor înainte de a permite accesul la resurse.</p>" +
                    "<h3>3. Rețea</h3>" +
                    "<p>Segmentarea rețelei, criptarea traficului și implementarea controalelor granulare de acces la nivel de aplicație.</p>" +
                    "<h3>4. Aplicații</h3>" +
                    "<p>Verificarea comportamentului aplicațiilor și controlul accesului la date.</p>" +
                    "<h3>5. Date</h3>" +
                    "<p>Clasificarea, criptarea și protejarea datelor, indiferent de locație.</p>" +
                    "<h2>Implementarea Zero Trust în organizația dumneavoastră</h2>" +
                    "<p>Trecerea la modelul Zero Trust este un proces gradual care poate include:</p>" +
                    "<ol>" +
                    "    <li>Identificarea datelor sensibile și a fluxurilor de date</li>" +
                    "    <li>Inventarierea aplicațiilor și dispozitivelor</li>" +
                    "    <li>Implementarea IAM robust și autentificării multi-factor</li>" +
                    "    <li>Micro-segmentarea rețelei</li>" +
                    "    <li>Monitorizarea continuă și analiza comportamentului</li>" +
                    "    <li>Automatizarea răspunsului la incidente</li>" +
                    "</ol>" +
                    "<h2>Beneficiile adoptării modelului Zero Trust</h2>" +
                    "<ul>" +
                    "    <li>Reducerea semnificativă a suprafeței de atac</li>" +
                    "    <li>Limitarea mișcării laterale a atacatorilor în caz de breșă</li>" +
                    "    <li>Protecție îmbunătățită pentru munca la distanță și mediile cloud</li>" +
                    "    <li>Vizibilitate crescută asupra activității utilizatorilor și dispozitivelor</li>" +
                    "    <li>Conformitate mai bună cu reglementările de securitate</li>" +
                    "</ul>",
                    ImageUrl = "/Content/img/blog/zero-trust.jpg",
                    Category = "Arhitectură de Securitate"
                }
            };

            Db.BlogPosts.AddRange(samplePosts);
            Db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _db != null)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
        
        // Test method to add a comment directly
        public ActionResult AddTestComment(int id)
        {
            try
            {
                var testUsername = "TestUser";
                var testContent = "This is a test comment added at " + DateTime.Now.ToString();
                
                // Create a test user if it doesn't exist
                var user = InMemoryData.Users.FirstOrDefault(u => u.Username == testUsername);
                if (user == null)
                {
                    user = new UserModel.User
                    {
                        Id = InMemoryData.Users.Any() ? InMemoryData.Users.Max(u => u.Id) + 1 : 1,
                        Username = testUsername,
                        Email = testUsername + "@example.com",
                        PasswordHash = "test",
                        IsAdmin = false
                    };
                    InMemoryData.Users.Add(user);
                }
                
                // Create and add the comment
                var comment = new BlogModel.Comment
                {
                    BlogPostId = id,
                    UserId = user.Id,
                    Content = testContent,
                    PostedAt = DateTime.Now,
                    User = user
                };
                
                // Assign an ID for the in-memory comment
                if (InMemoryData.Comments.Any())
                {
                    comment.Id = InMemoryData.Comments.Max(c => c.Id) + 1;
                }
                else
                {
                    comment.Id = 1;
                }
                
                InMemoryData.Comments.Add(comment);
                System.Diagnostics.Debug.WriteLine($"Test comment added to post {id}: {testContent}");
                
                // Redirect to the post page
                return RedirectToAction("Post", new { id = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddTestComment: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
    }
} 
