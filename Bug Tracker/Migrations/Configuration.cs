namespace Bug_Tracker.Migrations
{
    using Bug_Tracker.Enums;
    using Bug_Tracker.Models;
    using Bug_Tracker.Models.Domain;
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

            //Creating Ticket Types
            if (!context.TicketTypes.Any(p => p.Name == nameof(EnumTicketTypes.Bug)))
            {
                var bugType = new TicketType(nameof(EnumTicketTypes.Bug));
                context.TicketTypes.Add(bugType);

            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(EnumTicketTypes.Feature)))
            {
                var featureType = new TicketType(nameof(EnumTicketTypes.Feature));
                context.TicketTypes.Add(featureType);
            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(EnumTicketTypes.Database)))
            {
                var databaseType = new TicketType(nameof(EnumTicketTypes.Database));
                context.TicketTypes.Add(databaseType);
            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(EnumTicketTypes.Support)))
            {
                var supportType = new TicketType(nameof(EnumTicketTypes.Support));
                context.TicketTypes.Add(supportType);
            }

            //Creating Ticket Priorities
            if (!context.TicketPriorities.Any(p => p.Name == nameof(EnumTicketPriorities.Low)))
            {
                var low = new TicketPriority(nameof(EnumTicketPriorities.Low));
                context.TicketPriorities.Add(low);
            }

            if (!context.TicketPriorities.Any(p => p.Name == nameof(EnumTicketPriorities.Medium)))
            {
                var medium = new TicketPriority(nameof(EnumTicketPriorities.Medium));
                context.TicketPriorities.Add(medium);
            }

            if (!context.TicketPriorities.Any(p => p.Name == nameof(EnumTicketPriorities.High)))
            {
                var high = new TicketPriority(nameof(EnumTicketPriorities.High));
                context.TicketPriorities.Add(high);
            }

            //Creating Ticket Status
            if (!context.TicketStatuses.Any(p => p.Name == nameof(EnumTicketStatuses.Open)))
            {
                var open = new TicketStatus(nameof(EnumTicketStatuses.Open));
                context.TicketStatuses.Add(open);
            }

            if (!context.TicketStatuses.Any(p => p.Name == nameof(EnumTicketStatuses.Resolved)))
            {
                var resolved = new TicketStatus(nameof(EnumTicketStatuses.Resolved));
                context.TicketStatuses.Add(resolved);
            }

            if (!context.TicketStatuses.Any(p => p.Name == nameof(EnumTicketStatuses.Rejected)))
            {
                var rejected = new TicketStatus(nameof(EnumTicketStatuses.Rejected));
                context.TicketStatuses.Add(rejected);
            }

            context.SaveChanges();
        }
    }
}
