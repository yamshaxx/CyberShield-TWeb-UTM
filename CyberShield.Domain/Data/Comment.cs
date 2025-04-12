// This file is a placeholder for backward compatibility.
// We're using CyberShield.Domain.Model.Blog.Comment as the main Comment class in the application.
namespace CyberShield.Domain.Data
{
    // This class forwards to the domain model
    public class Comment : Model.Blog.Comment
    {
        // This is a wrapper/alias class around the actual Comment class
    }
}