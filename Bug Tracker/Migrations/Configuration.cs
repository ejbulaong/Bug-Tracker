namespace Bug_Tracker.Migrations
{
    using Bug_Tracker.Enums;
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
            if (!context.Roles.Any(p => p.Name == nameof(UserRoles.Admin)))
            {
                var adminRole = new IdentityRole(nameof(UserRoles.Admin));
                roleManager.Create(adminRole);
            }

            //Adding project manager role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == nameof(UserRoles.ProjectManager)))
            {
                var moderatorRole = new IdentityRole(nameof(UserRoles.ProjectManager));
                roleManager.Create(moderatorRole);
            }

            //Adding developer role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == nameof(UserRoles.Developer)))
            {
                var moderatorRole = new IdentityRole(nameof(UserRoles.Developer));
                roleManager.Create(moderatorRole);
            }

            //Adding submitter  role if it doesn't exist.
            if (!context.Roles.Any(p => p.Name == nameof(UserRoles.Submitter)))
            {
                var moderatorRole = new IdentityRole(nameof(UserRoles.Submitter));
                roleManager.Create(moderatorRole);
            }

            //Creating the adminuser
            ApplicationUser adminUser;

            if (!context.Users.Any(
                p => p.UserName == "admin@mybugtracker.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.Name = "Admin";
                adminUser.UserName = "admin@mybugtracker.com";
                adminUser.Email = "admin@mybugtracker.com";

                userManager.Create(adminUser, "Password-1");
            }
            else
            {
                adminUser = context
                    .Users
                    .First(p => p.UserName == "admin@mybugtracker.com");
            }

            //Make sure the user is on the admin role
            if (!userManager.IsInRole(adminUser.Id, nameof(UserRoles.Admin)))
            {
                userManager.AddToRole(adminUser.Id, nameof(UserRoles.Admin));
            }
        }
    }
}
