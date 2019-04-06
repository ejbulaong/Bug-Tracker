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
            return RedirectToAction("Index", "Home");
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

                m.Roles = userManager.GetRoles(userId);
            }
            return View(model);
        }    

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditRoles(string userId)
        {
            if(userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(DbContext));

            var userRoles = userManager.GetRoles(userId);


            var model = (from u in DbContext.Users
                        where u.Id == userId
                        select new EditRolesViewModel {
                            Id = u.Id,
                            Name = u.Name,
                            Roles = userRoles
                        }).FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditRoles(string userId, List<string> roles)
        {
            if (roles == null)
            {
                roles = new List<string>();
                roles.Add(nameof(UserRoles.Submitter)); // setting Submitter as the default role for user if no role is chosen
            }

            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(DbContext));

            var userRoles = userManager.GetRoles(userId);

            foreach (var role in userRoles)
            {
                userManager.RemoveFromRole(userId, role);
            }

            foreach (var role in roles)
            {
                userManager.AddToRoles(userId, role);
            }

            DbContext.SaveChanges();

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

            return RedirectToAction("ManageUsers", "Bug", model);
        }


        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult CreateProject()
        {
            var users = (from u in DbContext.Users
                         where u != null
                         select u).ToList();

            var model = new CreateProjectViewModel()
            {
                Users = users
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult CreateProject(CreateProjectViewModel model, List<string>userIds)
        {
            var users = (from u in DbContext.Users
                         where u != null
                         select u).ToList();
            var members = new List<ApplicationUser>();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            foreach (var id in userIds)
            {
                members.Add(users.FirstOrDefault(u => u.Id == id));
            }

            var newProject = new Project()
            {
                Name = model.Name,
                Users = members
            };

            DbContext.Projects.Add(newProject);
            DbContext.SaveChanges();

            return RedirectToAction("ViewAllProjects", "Bug");
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult ViewAllProjects()
        {
            var model = (from p in DbContext.Projects
                         where p != null
                         select new ViewAllProjectsViewModel
                         {
                             Id = p.Id,
                             Name = p.Name,
                             Users = p.Users,
                             DateCreated = p.DateCreated,
                             DateUpdated = p.DateUpdated
                         }).ToList();

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult ViewMyProjects()
        {
            var userId = User.Identity.GetUserId();
            var userProjects = (from u in DbContext.Users
                                where u.Id == userId
                                select u.Projects).FirstOrDefault();

            var model = (from p in userProjects
                         where p != null
                         select new ViewMyProjectsViewModel
                         {
                             Id = p.Id,
                             Name = p.Name,
                             Users = p.Users,
                             DateCreated = p.DateCreated,
                             DateUpdated = p.DateUpdated
                         }).ToList();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditProject(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var projectToEdit = (from p in DbContext.Projects
                                 where p.Id == Id
                                 select p).FirstOrDefault();
          
            var model = new EditProjectViewModel()
            {
                Id = projectToEdit.Id,
                Name = projectToEdit.Name,
                DateCreated = projectToEdit.DateCreated,
                DateUpdated = projectToEdit.DateUpdated
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditProject(int Id, EditProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var projectToEdit = (from p in DbContext.Projects
                                 where p.Id == Id
                                 select p).FirstOrDefault();

            projectToEdit.Name = model.Name;
            projectToEdit.DateUpdated = DateTime.Now;

            DbContext.SaveChanges();

            return RedirectToAction("ViewMyProjects", "Bug");
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditMembers(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var project = (from p in DbContext.Projects
                                 where p.Id == Id
                                 select p).FirstOrDefault();

            var memberUsers = project.Users;
            var nonMemberUsers = new List<ApplicationUser>();
            var allUsers = (from u in DbContext.Users
                            where u != null
                            select u).ToList();

            foreach (var u in allUsers)
            {
                if (!memberUsers.Contains(u))
                {
                    nonMemberUsers.Add(u);
                }
            }

            var model = new EditMembersViewModel()
            {
                Id = project.Id,
                Name = project.Name,
                DateCreated = project.DateCreated,
                DateUpdated = project.DateUpdated,
                MemberUsers = memberUsers,
                NonMemberUsers = nonMemberUsers
            };

            return View(model);
        }

        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult Assign(int projectId, string userId)
        {
            var project = (from p in DbContext.Projects
                           where p.Id == projectId
                           select p).FirstOrDefault();

            var user = (from u in DbContext.Users
                           where u.Id == userId
                        select u).FirstOrDefault();

            project.Users.Add(user);
            DbContext.SaveChanges();

            var memberUsers = project.Users;

            var nonMemberUsers = new List<ApplicationUser>();
            var allUsers = (from u in DbContext.Users
                            where u != null
                            select u).ToList();

            foreach (var u in allUsers)
            {
                if (!memberUsers.Contains(u))
                {
                    nonMemberUsers.Add(u);
                }
            }

            var model = new EditMembersViewModel()
            {
                Id= project.Id,
                Name = project.Name,
                DateCreated = project.DateCreated,
                DateUpdated = project.DateUpdated,
                MemberUsers = memberUsers,
                NonMemberUsers = nonMemberUsers
            };

            return RedirectToAction("EditMembers", "Bug", model);
        }

        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult Unassign(int projectId, string userId)
        {
            var project = (from p in DbContext.Projects
                           where p.Id == projectId
                           select p).FirstOrDefault();

            var user = (from u in DbContext.Users
                        where u.Id == userId
                        select u).FirstOrDefault();

            project.Users.Remove(user);
            DbContext.SaveChanges();

            var memberUsers = project.Users;

            var nonMemberUsers = new List<ApplicationUser>();
            var allUsers = (from u in DbContext.Users
                            where u != null
                            select u).ToList();

            foreach (var u in allUsers)
            {
                if (!memberUsers.Contains(u))
                {
                    nonMemberUsers.Add(u);
                }
            }

            var model = new EditMembersViewModel()
            {
                Id = project.Id,
                Name = project.Name,
                DateCreated = project.DateCreated,
                DateUpdated = project.DateUpdated,
                MemberUsers = memberUsers,
                NonMemberUsers = nonMemberUsers
            };

            return RedirectToAction("EditMembers", "Bug", model);
        }

        //[HttpGet]
        //[Authorize(Roles = nameof(UserRoles.Admin))]
        //public ActionResult ManageUsers()
        //{
        //    var userManager =
        //        new UserManager<ApplicationUser>(
        //                new UserStore<ApplicationUser>(DbContext));

        //    var model = new List<ManageUsersViewModel>();

        //    foreach (var user in DbContext.Users)
        //    {
        //        model.Add(
        //            new ManageUsersViewModel
        //            {
        //                Id = user.Id,
        //                Name = user.Name,
        //                UserName = user.Email
        //            });
        //    }

        //    foreach (var m in model)
        //    {
        //        var userId = (from u in DbContext.Users
        //                      where u.UserName == m.UserName
        //                      select u.Id).FirstOrDefault();

        //        m.Role = userManager.GetRoles(userId);
        //    }
        //    return View(model);
        //}

        //[HttpPost]
        //[Authorize(Roles = nameof(UserRoles.Admin))]
        //public ActionResult ManageUsers(string id, string newRole)
        //{
        //    if (id == null)
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var userManager =
        //        new UserManager<ApplicationUser>(
        //                new UserStore<ApplicationUser>(DbContext));

        //    var user = (from u in DbContext.Users
        //                where u.Id == id
        //                select u).FirstOrDefault();
        //    if (newRole != "none")
        //    {
        //        var currentRole = userManager.GetRoles(user.Id).FirstOrDefault();
        //        userManager.RemoveFromRole(user.Id, currentRole);
        //    }

        //    if (newRole == nameof(UserRoles.Admin))
        //    {
        //        userManager.AddToRoles(user.Id, nameof(UserRoles.Admin));
        //    }
        //    else if (newRole == nameof(UserRoles.ProjectManager))
        //    {
        //        userManager.AddToRoles(user.Id, nameof(UserRoles.ProjectManager));
        //    }
        //    else if (newRole == nameof(UserRoles.Developer))
        //    {
        //        userManager.AddToRoles(user.Id, nameof(UserRoles.Developer));
        //    }
        //    else if (newRole == nameof(UserRoles.Submitter))
        //    {
        //        userManager.AddToRoles(user.Id, nameof(UserRoles.Submitter));
        //    }

        //    var model = new List<ManageUsersViewModel>();

        //    foreach (var u in DbContext.Users)
        //    {
        //        model.Add(
        //            new ManageUsersViewModel
        //            {
        //                Id = u.Id,
        //                Name = u.Name,
        //                UserName = u.Email
        //            });
        //    }

        //    foreach (var m in model)
        //    {
        //        var userId = (from u in DbContext.Users
        //                      where u.UserName == m.UserName
        //                      select u.Id).FirstOrDefault();

        //        m.Role = userManager.GetRoles(userId);
        //    }
        //    return View(model);
        //}
    }
}