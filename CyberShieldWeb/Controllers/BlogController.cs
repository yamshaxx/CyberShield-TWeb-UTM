using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CyberShield.Domain.Data;
using DomainBlog = CyberShield.Domain.Model.Blog;
using CyberShield.Domain.Model.User;
using CyberShieldWeb.Models.Blog;

namespace CyberShieldWeb.Controllers
{
    public class BlogController : Controller
    {
        private readonly CyberShieldContext _db = new CyberShieldContext();

        // GET: Blog
        public ActionResult Index()
        {
            var blogPosts = _db.BlogPosts
                .OrderByDescending(p => p.PostedDate)
                .ToList();

            // If no blog posts exist yet, create sample posts
            if (blogPosts.Count == 0)
            {
                CreateSampleBlogPosts();
                blogPosts = _db.BlogPosts
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

        // GET: Blog/Post/5
        public ActionResult Post(int id)
        {
            var post = _db.BlogPosts
                .Include(p => p.Comments.Select(c => c.User))
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var comments = post.Comments
                .OrderByDescending(c => c.PostedAt)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    BlogPostId = c.BlogPostId,
                    Username = c.User.Username,
                    Content = c.Content,
                    PostedAt = c.PostedAt
                })
                .ToList();

            var viewModel = new BlogPostDetailViewModel
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

            return View(viewModel);
        }

        // POST: Blog/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AddComment(CreateCommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                var user = _db.Users.FirstOrDefault(u => u.Username == username);

                if (user != null)
                {
                    var comment = new DomainBlog.Comment
                    {
                        BlogPostId = model.BlogPostId,
                        UserId = user.Id,
                        Content = model.Content,
                        PostedAt = DateTime.Now
                    };

                    _db.Comments.Add((Comment)comment);
                    _db.SaveChanges();
                }

                return RedirectToAction("Post", new { id = model.BlogPostId });
            }

            // If we got this far, something failed, redisplay form
            var post = _db.BlogPosts
                .Include(p => p.Comments.Select(c => c.User))
                .FirstOrDefault(p => p.Id == model.BlogPostId);

            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var comments = post.Comments
                .OrderByDescending(c => c.PostedAt)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    BlogPostId = c.BlogPostId,
                    Username = c.User.Username,
                    Content = c.Content,
                    PostedAt = c.PostedAt
                })
                .ToList();

            var viewModel = new BlogPostDetailViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Author = post.Author,
                PostedDate = post.PostedDate,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                Category = post.Category,
                Comments = comments,
                NewComment = model
            };

            return View("Post", viewModel);
        }

        // Helper method to create sample blog posts
        private void CreateSampleBlogPosts()
        {
            var samplePosts = new List<DomainBlog.BlogPost>
            {
                new DomainBlog.BlogPost
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
                new DomainBlog.BlogPost
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
                new DomainBlog.BlogPost
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
                new DomainBlog.BlogPost
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

            _db.BlogPosts.AddRange((IEnumerable<BlogPost>)samplePosts);
            _db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 
