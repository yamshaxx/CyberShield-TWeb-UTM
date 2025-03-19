using System;
using System.Web.Mvc;
using CyberShieldWeb.Models.Blog;

namespace CyberShieldWeb.Controllers
{
    public class BlogController : Controller
    {
        // GET: Blog
        public ActionResult Index()
        {
            var blogPosts = new[]
            {
                new BlogPost
                {
                    Id = 1,
                    Title = "Securitatea în era digitală: Ce trebuie să știți",
                    Author = "Ion Popescu",
                    PostedDate = DateTime.Now.AddDays(-2),
                    Summary = "În lumea digitală de astăzi, amenințările cibernetice evoluează constant. Aflați care sunt cele mai recente tendințe în securitatea cibernetică și cum vă puteți proteja datele sensibile.",
                    ImageUrl = "/Content/images/blog/cybersecurity-post.jpg",
                    Category = "Securitate"
                },
                // Add more sample posts as needed
            };

            return View(blogPosts);
        }

        public ActionResult ArticleView(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            // Sample post data (replace with database fetch later)
            var post = new BlogPost
            {
                Id = id.Value,
                Title = "Securitatea în era digitală: Ce trebuie să știți",
                Author = "Ion Popescu",
                PostedDate = DateTime.Now.AddDays(-2),
                Summary = "În lumea digitală de astăzi, amenințările cibernetice evoluează constant. Aflați care sunt cele mai recente tendințe în securitatea cibernetică și cum vă puteți proteja datele sensibile.",
                Content = @"<p>În era digitală actuală, securitatea cibernetică a devenit o preocupare majoră pentru organizații și utilizatori individuali deopotrivă. Cu amenințări în continuă evoluție și atacuri din ce în ce mai sofisticate, este esențial să înțelegem și să implementăm măsuri de protecție adecvate.</p>

                <h2>Principalele amenințări cibernetice în 2024</h2>
                <p>Atacurile de tip ransomware, phishing-ul și breșele de date continuă să reprezinte amenințări semnificative. În plus, noile tehnologii precum inteligența artificială sunt folosite atât pentru apărare, cât și pentru atacuri.</p>

                <h2>Cum să vă protejați datele</h2>
                <ul>
                    <li>Utilizați parole puternice și unice pentru fiecare cont</li>
                    <li>Activați autentificarea în doi factori</li>
                    <li>Mențineți software-ul și sistemele de operare actualizate</li>
                    <li>Folosiți soluții de securitate de încredere</li>
                </ul>",
                ImageUrl = "/Content/images/blog/cybersecurity-post.jpg",
                Category = "Securitate"
            };

            return View(post);
        }
    }
} 
