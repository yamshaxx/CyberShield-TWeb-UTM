using CyberShield.Domain.Data;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace CyberShield.Domain.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<CyberShieldContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(CyberShieldContext context)
        {
            // Seed method will be called when migrations are run
            // Add any seed data here if needed
        }
    }
} 