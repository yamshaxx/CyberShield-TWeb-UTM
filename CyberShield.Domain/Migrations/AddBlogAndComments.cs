namespace CyberShield.Domain.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBlogAndComments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogPosts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Author = c.String(nullable: false, maxLength: 50),
                        PostedDate = c.DateTime(nullable: false),
                        Summary = c.String(nullable: false, maxLength: 500),
                        Content = c.String(nullable: false),
                        ImageUrl = c.String(maxLength: 255),
                        Category = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlogPostId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Content = c.String(nullable: false, maxLength: 2000),
                        PostedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BlogPosts", t => t.BlogPostId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.BlogPostId)
                .Index(t => t.UserId);
                
            // Add IsAdmin column to Users table
            AddColumn("dbo.Users", "IsAdmin", c => c.Boolean(nullable: false, defaultValue: false));
            
            // Create a default admin user
            Sql("UPDATE dbo.Users SET IsAdmin = 1 WHERE Username = 'admin'");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Comments", "BlogPostId", "dbo.BlogPosts");
            DropIndex("dbo.Comments", new[] { "UserId" });
            DropIndex("dbo.Comments", new[] { "BlogPostId" });
            DropColumn("dbo.Users", "IsAdmin");
            DropTable("dbo.Comments");
            DropTable("dbo.BlogPosts");
        }
    }
} 