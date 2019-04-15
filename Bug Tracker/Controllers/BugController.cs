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
using System.Data.Entity;
using System.IO;
using Bug_Tracker.Models.Filters;

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
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public ActionResult UnAuthorizeAccess()
        {

            return View();
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin))]
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
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin))]
        public ActionResult EditRoles(string userId)
        {
            if (userId == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(DbContext));

            var userRoles = userManager.GetRoles(userId);


            var model = (from u in DbContext.Users
                         where u.Id == userId
                         select new EditRolesViewModel
                         {
                             Id = u.Id,
                             Name = u.Name,
                             Roles = userRoles
                         }).FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin))]
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

            return RedirectToAction(nameof(BugController.ManageUsers), "Bug", model);
        }


        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
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
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult CreateProject(CreateProjectViewModel model, List<string> userIds)
        {
            var users = (from u in DbContext.Users
                         where u != null
                         select u).ToList();
            var members = new List<ApplicationUser>();

            if (userIds == null)
            {
                userIds = new List<string>();
            }

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
                Details = model.Details,
                Users = members
            };

            DbContext.Projects.Add(newProject);
            DbContext.SaveChanges();

            return RedirectToAction(nameof(BugController.ViewAllProjects), "Bug");
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult ViewAllProjects()
        {
            var model = (from p in DbContext.Projects
                         where p != null
                         select new ViewAllProjectsViewModel
                         {
                             Id = p.Id,
                             Name = p.Name,
                             Users = p.Users,
                             Tickets = p.Tickets,
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
                             Tickets = p.Tickets,
                             DateCreated = p.DateCreated,
                             DateUpdated = p.DateUpdated
                         }).ToList();

            return View(model);
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditProject(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var projectToEdit = (from p in DbContext.Projects
                                 where p.Id == Id
                                 select p).FirstOrDefault();

            var model = new EditProjectViewModel()
            {
                Id = projectToEdit.Id,
                Name = projectToEdit.Name,
                Details = projectToEdit.Details,
                DateCreated = projectToEdit.DateCreated,
                DateUpdated = projectToEdit.DateUpdated
            };

            return View(model);
        }

        [HttpPost]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
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
            projectToEdit.Details = model.Details;
            projectToEdit.DateUpdated = DateTime.Now;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(BugController.ViewAllProjects), "Bug");
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditMembers(int? Id)
        {
            if (Id == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
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

        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
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
                Id = project.Id,
                Name = project.Name,
                DateCreated = project.DateCreated,
                DateUpdated = project.DateUpdated,
                MemberUsers = memberUsers,
                NonMemberUsers = nonMemberUsers
            };

            return RedirectToAction(nameof(BugController.EditMembers), "Bug", model);
        }

        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
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

            return RedirectToAction(nameof(BugController.EditMembers), "Bug", model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProjectDetails(int? projectId)
        {
            if (projectId == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var project = (from p in DbContext.Projects
                           where p.Id == projectId
                           select p).FirstOrDefault();

            var model = new ProjectDetailsViewModel()
            {
                Id = project.Id,
                Name = project.Name,
                Details = project.Details,
                Tickets = project.Tickets,
                DateCreated = project.DateCreated,
                DateUpdated = project.DateUpdated,
                Users = project.Users
            };
            return View(model);
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Submitter))]
        public ActionResult CreateTicket()
        {
            var userId = User.Identity.GetUserId();
            var userProjects = (from u in DbContext.Users
                                where u.Id == userId
                                select u.Projects).FirstOrDefault();

            var types = (from t in DbContext.TicketTypes
                         where t != null
                         select t).ToList();
            var priorities = (from p in DbContext.TicketPriorities
                              where p != null
                              select p).ToList();
            var statuses = (from s in DbContext.TicketStatuses
                            where s != null
                            select s).ToList();

            var model = new CreateTicketViewModel()
            {
                Projects = userProjects,
                Priorities = priorities,
                Types = types,
                Statuses = statuses
            };

            return View(model);
        }

        [HttpPost]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Submitter))]
        public ActionResult CreateTicket(CreateTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity.GetUserId();
            var user = DbContext.Users.FirstOrDefault(u => u.Id == userId);
            var type = DbContext.TicketTypes.FirstOrDefault(t => t.Id == model.TypeId);
            var status = DbContext.TicketStatuses.FirstOrDefault(s => s.Name == nameof(EnumTicketStatuses.Open).ToString());
            var priority = DbContext.TicketPriorities.FirstOrDefault(p => p.Id == model.PriorityId);
            var project = DbContext.Projects.FirstOrDefault(p => p.Id == model.ProjectId);

            var newTicket = new Ticket();
            newTicket.Title = model.Title;
            newTicket.Description = model.Description;
            newTicket.DateCreated = DateTime.Now;
            newTicket.DateUpdated = null;
            newTicket.AssignedDeveloper = null;
            newTicket.Creator = user;
            newTicket.Priority = priority;
            newTicket.Status = status;
            newTicket.Type = type;
            newTicket.Project = project;
            DbContext.Tickets.Add(newTicket);
            DbContext.SaveChanges();

            return RedirectToAction(nameof(BugController.ViewMyCreatedTickets), "Bug");
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Submitter))]
        public ActionResult ViewMyCreatedTickets()
        {
            var userId = User.Identity.GetUserId();

            var myTickets = (from u in DbContext.Users
                             where u.Id == userId
                             select u.CreatedTickets).FirstOrDefault();

            var model = MakeTicketViewModel(myTickets);

            return View(model);
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult AssignTicket(int? Id)
        {
            if (!Id.HasValue)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(DbContext));

            var devRoleId = (from r in DbContext.Roles
                             where r.Name == nameof(UserRoles.Developer)
                             select r.Id).FirstOrDefault();

            var developers = (from u in DbContext.Users
                              where u.Roles.Any(r => r.RoleId == devRoleId)
                              select u).ToList();

            var ticket = (from t in DbContext.Tickets
                          where t.Id == Id
                          select t).FirstOrDefault();

            var model = new AssignTicketViewModel()
            {
                Id = ticket.Id,
                Title = ticket.Title,
                AssignedDeveloper = ticket.AssignedDeveloper,
                Developers = developers
            };

            return View(model);
        }

        [HttpPost]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult AssignTicket(AssignTicketViewModel model)
        {
            var ticket = (from t in DbContext.Tickets
                          where t.Id == model.Id
                          select t).FirstOrDefault();

            if (model.AssignedDeveloperId != null)
            {
                var developer = (from d in DbContext.Users
                                 where d.Id == model.AssignedDeveloperId
                                 select d).FirstOrDefault();

                ticket.AssignedDeveloper = developer;
                DbContext.SaveChanges();
            }

            return RedirectToAction(nameof(BugController.ViewAllTickets), "Bug");
        }

        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult UnAssignTicket(int Id)
        {
            var ticket = (from t in DbContext.Tickets
                          where t.Id == Id
                          select t).FirstOrDefault();

            ticket.AssignedDeveloper = null;
            ticket.AssignedDeveloperId = null;
            DbContext.SaveChanges();

            return RedirectToAction(nameof(BugController.ViewAllTickets), "Bug");
        }
        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Submitter) + "," + nameof(UserRoles.Developer))]
        public ActionResult ViewMyProjectsTickets()
        {
            var userId = User.Identity.GetUserId();

            var myProjects = (from u in DbContext.Users
                              where u.Id == userId
                              select u.Projects).FirstOrDefault();

            var listOfTickets = new List<Ticket>();
            foreach (var p in myProjects)
            {
                foreach (var t in p.Tickets)
                {
                    listOfTickets.Add(t);
                }
            }

            var model = MakeTicketViewModel(listOfTickets);

            return View(model);
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Developer))]
        public ActionResult ViewMyAssignedTickets()
        {
            var userId = User.Identity.GetUserId();

            var assignedTickets = (from u in DbContext.Users
                                   where u.Id == userId
                                   select u.AssignedTickets).FirstOrDefault();

            var model = MakeTicketViewModel(assignedTickets);

            return View(model);
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult ViewAllTickets()
        {
            var AllTickets = (from t in DbContext.Tickets
                              where t != null
                              select t).ToList();

            var model = MakeTicketViewModel(AllTickets);

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult EditTicket(int? Id)
        {
            if (!Id.HasValue)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var userId = User.Identity.GetUserId();

            var user = (from u in DbContext.Users
                        where u.Id == userId
                        select u).FirstOrDefault();

            var ticketToEdit = (from t in DbContext.Tickets
                                where t.Id == Id
                                select t).FirstOrDefault();

            if (ticketToEdit == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            if (User.IsInRole(nameof(UserRoles.Submitter)) && (ticketToEdit.CreatorId != userId))
            {
                return RedirectToAction(nameof(BugController.Index), "Home");
            }

            if (User.IsInRole(nameof(UserRoles.Developer)) && (ticketToEdit.AssignedDeveloperId != userId))
            {
                return RedirectToAction(nameof(BugController.Index), "Home");
            }

            var types = (from t in DbContext.TicketTypes
                         where t != null
                         select t).ToList();

            var priorities = (from p in DbContext.TicketPriorities
                              where p != null
                              select p).ToList();

            var statuses = (from s in DbContext.TicketStatuses
                            where s != null
                            select s).ToList();

            var allProjects = (from p in DbContext.Projects
                               where p != null
                               select p).ToList();

            var model = new EditTicketViewModel()
            {
                Id = ticketToEdit.Id,
                Title = ticketToEdit.Title,
                Description = ticketToEdit.Description,
                ProjectId = ticketToEdit.ProjectId,
                TypeId = ticketToEdit.TypeId,
                PriorityId = ticketToEdit.PriorityId,
                StatusId = ticketToEdit.StatusId,
                Creator = ticketToEdit.Creator,
                Projects = allProjects,
                Types = types,
                Priorities = priorities
            };

            if (User.IsInRole(nameof(UserRoles.Admin)) || User.IsInRole(nameof(UserRoles.ProjectManager)))
            {
                model.Statuses = statuses;
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditTicket(EditTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var type = DbContext.TicketTypes.FirstOrDefault(t => t.Id == model.TypeId);
            var priority = DbContext.TicketPriorities.FirstOrDefault(p => p.Id == model.PriorityId);
            var project = DbContext.Projects.FirstOrDefault(p => p.Id == model.ProjectId);
            var ticketToEdit = (from t in DbContext.Tickets
                                where t.Id == model.Id
                                select t).FirstOrDefault();

            ticketToEdit.Title = model.Title;
            ticketToEdit.Description = model.Description;
            ticketToEdit.DateUpdated = DateTime.Now;
            ticketToEdit.Priority = priority;
            ticketToEdit.Type = type;
            ticketToEdit.Project = project;

            if (User.IsInRole(nameof(UserRoles.Admin)) || User.IsInRole(nameof(UserRoles.ProjectManager)))
            {
                var status = DbContext.TicketStatuses.FirstOrDefault(t => t.Id == model.StatusId);
                ticketToEdit.Status = status;
            }

            DbContext.SaveChanges();

            if (User.IsInRole(nameof(UserRoles.Admin)) || User.IsInRole(nameof(UserRoles.ProjectManager)))
            {
                return RedirectToAction(nameof(BugController.ViewAllTickets), "Bug");
            }
            else if (User.IsInRole(nameof(UserRoles.Submitter)))
            {
                return RedirectToAction(nameof(BugController.ViewMyCreatedTickets), "Bug");
            }
            else if (User.IsInRole(nameof(UserRoles.Developer)))
            {
                return RedirectToAction(nameof(BugController.ViewMyAssignedTickets), "Bug");
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult ViewTicketDetails(int? Id)
        {
            if (!Id.HasValue)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var userId = User.Identity.GetUserId();

            var user = (from u in DbContext.Users
                        where u.Id == userId
                        select u).FirstOrDefault();

            var ticket = (from t in DbContext.Tickets
                          where t.Id == Id
                          select t).FirstOrDefault();

            if (ticket == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var model = new TicketsViewModel()
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                DateCreated = ticket.DateCreated,
                DateUpdated = ticket.DateUpdated,
                Project = ticket.Project,
                Type = ticket.Type,
                Priority = ticket.Priority,
                Status = ticket.Status,
                Creator = ticket.Creator,
                AssignedDeveloper = ticket.AssignedDeveloper,
                Comments = ticket.Comments,
                Attachments = ticket.Attachments
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult ViewTicketDetails(int? Id, HttpPostedFileBase file)
        {
            if (!Id.HasValue)
            {
                return RedirectToAction(nameof(BugController.Index), "Bug");
            }

            var ticket = (from t in DbContext.Tickets
                          where t.Id == Id
                          select t).FirstOrDefault();

            var userId = User.Identity.GetUserId();

            var user = (from u in DbContext.Users
                        where u.Id == userId
                        select u).FirstOrDefault();

            if (User.IsInRole(nameof(UserRoles.Submitter)) && !user.CreatedTickets.Contains(ticket))
            {
                return RedirectToAction(nameof(BugController.Index), "Bug");
            }

            if (User.IsInRole(nameof(UserRoles.Developer)) && !user.AssignedTickets.Contains(ticket))
            {
                return RedirectToAction(nameof(BugController.Index), "Bug");
            }

            if (file != null)
            {
                var uploadFolder = "~/Uploads/";
                var mappedFolder = Server.MapPath(uploadFolder);

                if (!Directory.Exists(mappedFolder))
                {
                    Directory.CreateDirectory(mappedFolder);
                }

                file.SaveAs(mappedFolder + file.FileName);

                var newAttachment = new TicketAttachment()
                {
                    FileName = file.FileName,
                    FilePath = uploadFolder + file.FileName,
                    DateCreated = DateTime.Now,
                    Ticket = ticket,
                    User = user
                };
                ticket.Attachments.Add(newAttachment);
                DbContext.SaveChanges();
            }

            var model = new TicketsViewModel()
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                DateCreated = ticket.DateCreated,
                DateUpdated = ticket.DateUpdated,
                Project = ticket.Project,
                Type = ticket.Type,
                Priority = ticket.Priority,
                Status = ticket.Status,
                Creator = ticket.Creator,
                AssignedDeveloper = ticket.AssignedDeveloper,
                Comments = ticket.Comments,
                Attachments = ticket.Attachments
            };

            return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", model);
        }

        [Authorize]
        public ActionResult AddComment(string comment, int? Id)
        {
            if (!Id.HasValue)
            {
                return RedirectToAction(nameof(BugController.Index), "Bug");
            }

            var ticket = (from t in DbContext.Tickets
                          where t.Id == Id
                          select t).FirstOrDefault();

            var userId = User.Identity.GetUserId();

            var user = (from u in DbContext.Users
                        where u.Id == userId
                        select u).FirstOrDefault();

            if (User.IsInRole(nameof(UserRoles.Submitter)) && !user.CreatedTickets.Contains(ticket))
            {
                return RedirectToAction(nameof(BugController.Index), "Bug");
            }

            if (User.IsInRole(nameof(UserRoles.Developer)) && !user.AssignedTickets.Contains(ticket))
            {
                return RedirectToAction(nameof(BugController.Index), "Bug");
            }

            var newComment = new TicketComment()
            {
                Comment = comment,
                DateCreated = DateTime.Now,
                User = user
            };
            ticket.Comments.Add(newComment);
            DbContext.SaveChanges();

            var model = new TicketsViewModel()
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                DateCreated = ticket.DateCreated,
                DateUpdated = ticket.DateUpdated,
                Project = ticket.Project,
                Type = ticket.Type,
                Priority = ticket.Priority,
                Status = ticket.Status,
                Creator = ticket.Creator,
                AssignedDeveloper = ticket.AssignedDeveloper,
                Comments = ticket.Comments
            };

            return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", model);
        }

        private List<TicketsViewModel> MakeTicketViewModel(List<Ticket> ticketList)
        {
            var model = new List<TicketsViewModel>();

            foreach (var t in ticketList)
            {
                var project = (from p in DbContext.Projects
                               where p.Id == t.ProjectId
                               select p).FirstOrDefault();

                var type = (from p in DbContext.TicketTypes
                            where p.Id == t.TypeId
                            select p).FirstOrDefault();

                var status = (from p in DbContext.TicketStatuses
                              where p.Id == t.StatusId
                              select p).FirstOrDefault();

                var priority = (from p in DbContext.TicketPriorities
                                where p.Id == t.PriorityId
                                select p).FirstOrDefault();

                var creator = (from p in DbContext.Users
                               where p.Id == t.CreatorId
                               select p).FirstOrDefault();

                var assignedDeveloper = (from p in DbContext.Users
                                         where p.Id == t.AssignedDeveloperId
                                         select p).FirstOrDefault();

                model.Add(new TicketsViewModel()
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DateCreated = t.DateCreated,
                    DateUpdated = t.DateUpdated,
                    Project = project,
                    Type = type,
                    Priority = priority,
                    Status = status,
                    Creator = creator,
                    AssignedDeveloper = assignedDeveloper
                });
            }

            return model;
        }

        private string UploadFile(HttpPostedFileBase file)
        {
            if (file != null)
            {
                var uploadFolder = "~/Upload/";
                var mappedFolder = Server.MapPath(uploadFolder);

                if (!Directory.Exists(mappedFolder))
                {
                    Directory.CreateDirectory(mappedFolder);
                }

                file.SaveAs(mappedFolder + file.FileName);

                return uploadFolder + file.FileName;
            }

            return null;
        }
    }
}