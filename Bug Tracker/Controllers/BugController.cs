using Bug_Tracker.Enums;
using Bug_Tracker.Models;
using Bug_Tracker.Models.Domain;
using Bug_Tracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Bug_Tracker.Controllers
{
    public class BugController : Controller
    {
        private ApplicationDbContext DbContext;

        public BugController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin))]
        public ActionResult ManageUsers()
        {
            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(DbContext));

            var model = new List<ManageUsersViewModel>();

            foreach (var user in DbContext.Users)
            {
                model.Add(
                    new ManageUsersViewModel
                    {
                        Id = user.Id,
                        Name = user.Name,
                        UserName = user.Email
                    });
            }

            foreach (var m in model)
            {
                var userId = (from u in DbContext.Users
                              where u.UserName == m.UserName
                              select u.Id).FirstOrDefault();

                m.Role = userManager.GetRoles(userId);
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin))]
        public ActionResult ManageUsers(string id, string newRole)
        {
            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(DbContext));

            var user = (from u in DbContext.Users
                        where u.Id == id
                        select u).FirstOrDefault();
            if (newRole != "none")
            {
                var currentRole = userManager.GetRoles(user.Id).FirstOrDefault();
                userManager.RemoveFromRole(user.Id, currentRole);
            }

            if (newRole == nameof(UserRoles.Admin))
            {
                userManager.AddToRoles(user.Id, nameof(UserRoles.Admin));
            }
            else if (newRole == nameof(UserRoles.ProjectManager))
            {
                userManager.AddToRoles(user.Id, nameof(UserRoles.ProjectManager));
            }
            else if (newRole == nameof(UserRoles.Developer))
            {
                userManager.AddToRoles(user.Id, nameof(UserRoles.Developer));
            }
            else if (newRole == nameof(UserRoles.Submitter))
            {
                userManager.AddToRoles(user.Id, nameof(UserRoles.Submitter));
            }

            var model = new List<ManageUsersViewModel>();

            foreach (var u in DbContext.Users)
            {
                model.Add(
                    new ManageUsersViewModel
                    {
                        Id = u.Id,
                        Name = u.Name,
                        UserName = u.Email
                    });
            }

            foreach (var m in model)
            {
                var userId = (from u in DbContext.Users
                              where u.UserName == m.UserName
                              select u.Id).FirstOrDefault();

                m.Role = userManager.GetRoles(userId);
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) +","+ nameof(UserRoles.ProjectManager))]
        public ActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult CreateProject(string projectName)
        {
            var newProject = new Project()
            {
                Name = projectName
            };

            DbContext.Projects.Add(newProject);
            DbContext.SaveChanges();

            return RedirectToAction("ViewAllProjects","Bug");
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult ViewAllProjects()
        {
            var model = (from p in DbContext.Projects
                               where p != null
                               select new ViewAllProjectsViewModel {
                                   Id = p.Id,
                                   Name = p.Name,
                                   Users = p.Users,
                                   DateCreated = p.DateCreated,
                                   DateUpdated = p.DateUpdated
                               }).ToList();

            return View(model);
        }
    }
}