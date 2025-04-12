// This file is a placeholder for backward compatibility.
// We're using CyberShield.Domain.Model.Blog.BlogPost as the main BlogPost class in the application.
namespace CyberShield.Domain.Data
{
    // This class forwards to the domain model
    public class BlogPost : Model.Blog.BlogPost
    {
        // This is a wrapper/alias class around the actual BlogPost class
    }
}