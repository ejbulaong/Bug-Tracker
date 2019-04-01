namespace Bug_Tracker.Migrations
{
    using Bug_Tracker.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Bug_Tracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Bug_Tracker.Models.ApplicationDbContext context)
        {
            //RoleManager, used to manage roles
            var roleManager =
                new RoleManager<IdentityRole>(
                    new RoleStore<Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(context));

            //UserManager, used to manage users
            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(context));

            //Adding admin role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == "Admin"))
            {
                var adminRole = new IdentityRole("Admin");
                roleManager.Create(adminRole);
            }

            //Adding project manager role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == "Project Manager"))
            {
                var moderatorRole = new IdentityRole("Project Manager");
                roleManager.Create(moderatorRole);
            }

            //Adding developer role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == "Developer"))
            {
                var moderatorRole = new IdentityRole("Developer");
                roleManager.Create(moderatorRole);
            }

            //Adding submitter  role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == "Submitter "))
            {
                var moderatorRole = new IdentityRole("Submitter ");
                roleManager.Create(moderatorRole);
            }
        }
    }
}
