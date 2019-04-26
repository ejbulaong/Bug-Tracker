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
                var bugType = new TicketType();
                bugType.Name = nameof(EnumTicketTypes.Bug);
                context.TicketTypes.Add(bugType);

            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(EnumTicketTypes.Feature)))
            {
                var featureType = new TicketType();
                featureType.Name = nameof(EnumTicketTypes.Feature);
                context.TicketTypes.Add(featureType);
            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(EnumTicketTypes.Database)))
            {
                var databaseType = new TicketType();
                databaseType.Name = nameof(EnumTicketTypes.Database);
                context.TicketTypes.Add(databaseType);
            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(EnumTicketTypes.Support)))
            {
                var supportType = new TicketType();
                supportType.Name = nameof(EnumTicketTypes.Support);
                context.TicketTypes.Add(supportType);
            }

            //Creating Ticket Priorities
            if (!context.TicketPriorities.Any(p => p.Name == nameof(EnumTicketPriorities.Low)))
            {
                var low = new TicketPriority();
                low.Name = nameof(EnumTicketPriorities.Low);
                context.TicketPriorities.Add(low);
            }

            if (!context.TicketPriorities.Any(p => p.Name == nameof(EnumTicketPriorities.Medium)))
            {
                var medium = new TicketPriority();
                medium.Name = nameof(EnumTicketPriorities.Medium);
                context.TicketPriorities.Add(medium);
            }

            if (!context.TicketPriorities.Any(p => p.Name == nameof(EnumTicketPriorities.High)))
            {
                var high = new TicketPriority();
                high.Name = nameof(EnumTicketPriorities.High);
                context.TicketPriorities.Add(high);
            }

            //Creating Ticket Status
            if (!context.TicketStatuses.Any(p => p.Name == nameof(EnumTicketStatuses.Open)))
            {
                var open = new TicketStatus();
                open.Name = nameof(EnumTicketStatuses.Open);
                context.TicketStatuses.Add(open);
            }

            if (!context.TicketStatuses.Any(p => p.Name == nameof(EnumTicketStatuses.Resolved)))
            {
                var resolved = new TicketStatus();
                resolved.Name = nameof(EnumTicketStatuses.Resolved);
                context.TicketStatuses.Add(resolved);
            }

            if (!context.TicketStatuses.Any(p => p.Name == nameof(EnumTicketStatuses.Rejected)))
            {
                var rejected = new TicketStatus();
                rejected.Name = nameof(EnumTicketStatuses.Rejected);
                context.TicketStatuses.Add(rejected);
            }

            //Creating Demo Log-Ins
            ApplicationUser demoSubmitter;
            ApplicationUser demoDeveloper;
            ApplicationUser demoProjectManager;
            ApplicationUser demoAdmin;

            if (!context.Users.Any(
                p => p.UserName == "demosubmitter@mybugtracker.com"))
            {
                demoSubmitter = new ApplicationUser();
                demoSubmitter.Name = "DemoSubmitter";
                demoSubmitter.UserName = "demosubmitter@mybugtracker.com";
                demoSubmitter.Email = "demosubmitter@mybugtracker.com";
                userManager.Create(demoSubmitter, "Password-1");
            }
            else
            {
                demoSubmitter = context
                    .Users
                    .First(p => p.UserName == "demosubmitter@mybugtracker.com");
            }

            if (!context.Users.Any(
                p => p.UserName == "demodeveloper@mybugtracker.com"))
            {
                demoDeveloper = new ApplicationUser();
                demoDeveloper.Name = "DemoDeveloper";
                demoDeveloper.UserName = "demodeveloper@mybugtracker.com";
                demoDeveloper.Email = "demodeveloper@mybugtracker.com";
                userManager.Create(demoDeveloper,"Password-1");
            }
            else
            {
                demoDeveloper = context
                    .Users
                    .First(p => p.UserName == "demodeveloper@mybugtracker.com");
            }

            if (!context.Users.Any(
                p => p.UserName == "demoprojectmanager@mybugtracker.com"))
            {
                demoProjectManager = new ApplicationUser();
                demoProjectManager.Name = "DemoProjectManager";
                demoProjectManager.UserName = "demoprojectmanager@mybugtracker.com";
                demoProjectManager.Email = "demoprojectmanager@mybugtracker.com";
                userManager.Create(demoProjectManager, "Password-1");
            }
            else
            {
                demoProjectManager = context
                    .Users
                    .First(p => p.UserName == "demoprojectmanager@mybugtracker.com");
            }

            if (!context.Users.Any(
                p => p.UserName == "demoadmin@mybugtracker.com"))
            {
                demoAdmin = new ApplicationUser();
                demoAdmin.Name = "DemoAdmin";
                demoAdmin.UserName = "demoadmin@mybugtracker.com";
                demoAdmin.Email = "demoadmin@mybugtracker.com";
                userManager.Create(demoAdmin, "Password-1");
            }
            else
            {
                demoAdmin = context
                    .Users
                    .First(p => p.UserName == "demoadmin@mybugtracker.com");
            }

            //create roles for demo logins
            if (!userManager.IsInRole(demoSubmitter.Id, nameof(UserRoles.Submitter)))
            {
                userManager.AddToRole(demoSubmitter.Id, nameof(UserRoles.Submitter));
            }

            if (!userManager.IsInRole(demoDeveloper.Id, nameof(UserRoles.Developer)))
            {
                userManager.AddToRole(demoDeveloper.Id, nameof(UserRoles.Developer));
            }

            if (!userManager.IsInRole(demoProjectManager.Id, nameof(UserRoles.ProjectManager)))
            {
                userManager.AddToRole(demoProjectManager.Id, nameof(UserRoles.ProjectManager));
            }

            if (!userManager.IsInRole(demoAdmin.Id, nameof(UserRoles.Admin)))
            {
                userManager.AddToRole(demoAdmin.Id, nameof(UserRoles.Admin));
            }

            context.SaveChanges();
        }
    }
}
