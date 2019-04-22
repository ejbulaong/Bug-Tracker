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
    [UserLogFilters]
    public class BugController : Controller
    {
        private ApplicationDbContext DbContext;
        private UserManager<ApplicationUser> UserManager;

        public BugController()
        {
            DbContext = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(DbContext));
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

                m.Roles = UserManager.GetRoles(userId);
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

            var userRoles = UserManager.GetRoles(userId);


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

            var userRoles = UserManager.GetRoles(userId);

            foreach (var role in userRoles)
            {
                UserManager.RemoveFromRole(userId, role);
            }

            foreach (var role in roles)
            {
                UserManager.AddToRoles(userId, role);
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
            var users = GetAllUsers();

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
            var users = GetAllUsers();

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
            var allUsers = GetAllUsers();

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

            var user = GetUserById(userId);

            project.Users.Add(user);
            DbContext.SaveChanges();

            var memberUsers = project.Users;

            var nonMemberUsers = new List<ApplicationUser>();
            var allUsers = GetAllUsers();

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

            var user = GetUserById(userId);

            project.Users.Remove(user);
            DbContext.SaveChanges();

            var memberUsers = project.Users;

            var nonMemberUsers = new List<ApplicationUser>();
            var allUsers = GetAllUsers();

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
            var user = GetUserById(userId);
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

            var model = new List<TicketViewModel>();

            foreach (var t in myTickets)
            {
                model.Add(MakeTicketViewModel(t));
            }

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

            var userid = User.Identity.GetUserId();
            var user = GetUserById(userid);

            if (model.AssignedDeveloperId != null)
            {
                var developer = (from d in DbContext.Users
                                 where d.Id == model.AssignedDeveloperId
                                 select d).FirstOrDefault();

                if (ticket.AssignedDeveloper != developer)
                {
                    var developerHistory = new TicketHistory()
                    {
                        DateTime = DateTime.Now,
                        User = user,
                        PropertyChanged = nameof(ticket.AssignedDeveloper),
                        OldValue = "none",
                        NewValue = developer.Name
                    };
                    ticket.Histories.Add(developerHistory);
                    ticket.AssignedDeveloper = developer;
                }

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
            var userid = User.Identity.GetUserId();
            var user = GetUserById(userid);

            var developerHistory = new TicketHistory()
            {
                DateTime = DateTime.Now,
                User = user,
                PropertyChanged = nameof(ticket.AssignedDeveloper),
                OldValue = ticket.AssignedDeveloper.Name,
                NewValue = "none"
            };
            ticket.Histories.Add(developerHistory);

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

            var model = new List<TicketViewModel>();

            foreach (var t in listOfTickets)
            {
                model.Add(MakeTicketViewModel(t));
            }

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

            var model = new List<TicketViewModel>();

            foreach (var t in assignedTickets)
            {
                model.Add(MakeTicketViewModel(t));
            }

            return View(model);
        }

        [HttpGet]
        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult ViewAllTickets()
        {
            var AllTickets = (from t in DbContext.Tickets
                              where t != null
                              select t).ToList();

            var model = new List<TicketViewModel>();

            foreach (var t in AllTickets)
            {
                model.Add(MakeTicketViewModel(t));
            }

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

            var user = GetUserById(userId);

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

            var userId = User.Identity.GetUserId();
            var user = GetUserById(userId);

            if (ticketToEdit.Title != model.Title)
            {
                var titleHistory = new TicketHistory()
                {
                    DateTime = DateTime.Now,
                    User = user,
                    PropertyChanged = nameof(ticketToEdit.Title),
                    OldValue = ticketToEdit.Title,
                    NewValue = model.Title
                };
                ticketToEdit.Histories.Add(titleHistory);
                ticketToEdit.Title = model.Title;
            }

            if (ticketToEdit.Project != project)
            {
                var projectHistory = new TicketHistory()
                {
                    DateTime = DateTime.Now,
                    User = user,
                    PropertyChanged = nameof(ticketToEdit.Project),
                    OldValue = ticketToEdit.Project.Name,
                    NewValue = project.Name
                };
                ticketToEdit.Histories.Add(projectHistory);
                ticketToEdit.Project = project;
            }

            if (ticketToEdit.Description != model.Description)
            {
                var descriptionHistory = new TicketHistory()
                {
                    DateTime = DateTime.Now,
                    User = user,
                    PropertyChanged = nameof(ticketToEdit.Description),
                    OldValue = ticketToEdit.Description,
                    NewValue = model.Description
                };
                ticketToEdit.Histories.Add(descriptionHistory);
                ticketToEdit.Description = model.Description;
            }

            if (ticketToEdit.Type != type)
            {
                var typeHistory = new TicketHistory()
                {
                    DateTime = DateTime.Now,
                    User = user,
                    PropertyChanged = nameof(ticketToEdit.Type),
                    OldValue = ticketToEdit.Type.Name,
                    NewValue = type.Name
                };
                ticketToEdit.Histories.Add(typeHistory);
                ticketToEdit.Type = type;
            }

            if (ticketToEdit.Priority != priority)
            {
                var priorityHistory = new TicketHistory()
                {
                    DateTime = DateTime.Now,
                    User = user,
                    PropertyChanged = nameof(ticketToEdit.Priority),
                    OldValue = ticketToEdit.Priority.Name,
                    NewValue = priority.Name
                };
                ticketToEdit.Histories.Add(priorityHistory);
                ticketToEdit.Priority = priority;
            }



            if (User.IsInRole(nameof(UserRoles.Admin)) || User.IsInRole(nameof(UserRoles.ProjectManager)))
            {
                var status = DbContext.TicketStatuses.FirstOrDefault(t => t.Id == model.StatusId);
                if (ticketToEdit.Status != status)
                {
                    var statusHistory = new TicketHistory()
                    {
                        DateTime = DateTime.Now,
                        User = user,
                        PropertyChanged = nameof(ticketToEdit.Status),
                        OldValue = ticketToEdit.Status.Name,
                        NewValue = status.Name
                    };
                    ticketToEdit.Histories.Add(statusHistory);
                    ticketToEdit.Status = status;
                }
            }

            ticketToEdit.DateUpdated = DateTime.Now;
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

            var user = GetUserById(userId);

            var ticket = (from t in DbContext.Tickets
                          where t.Id == Id
                          select t).FirstOrDefault();

            if (ticket == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var model = MakeTicketViewModel(ticket);

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

            var user = GetUserById(userId);

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
                var newFileName = Guid.NewGuid().ToString();

                if (!Directory.Exists(mappedFolder))
                {
                    Directory.CreateDirectory(mappedFolder);
                }

                var newAttachment = new TicketAttachment()
                {
                    FileName = newFileName + file.FileName,
                    FilePath = uploadFolder + newFileName + file.FileName,
                    DateCreated = DateTime.Now,
                    Ticket = ticket,
                    User = user
                };

                file.SaveAs(mappedFolder + newFileName + file.FileName);
                ticket.Attachments.Add(newAttachment);
                DbContext.SaveChanges();
            }

            var model = MakeTicketViewModel(ticket);

            return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", model);
        }

        [Authorize]
        public ActionResult RemoveAttachment(int? Id, int? ticketId)
        {
            var ticket = (from t in DbContext.Tickets
                          where t.Id == ticketId
                          select t).FirstOrDefault();

            var attachment = (from a in DbContext.TicketAttachments
                              where a.Id == Id
                              select a).FirstOrDefault();


            string fullPath = Request.MapPath("~/Uploads/" + attachment.FileName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            DbContext.TicketAttachments.Remove(attachment);
            DbContext.SaveChanges();

            var model = MakeTicketViewModel(ticket);

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

            var user = GetUserById(userId);

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

            var model = MakeTicketViewModel(ticket);

            return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", model);
        }

        [Authorize]
        public ActionResult ActivityLog()
        {
            var userId = User.Identity.GetUserId();
            var user = GetUserById(userId);

            var activityLogs = (from a in DbContext.UserLogs
                                where a.UserName == user.UserName
                                select a).ToList();

            var model = new ActivityLogViewModel()
            {
                UserName = user.UserName,
                UserLogs = activityLogs
            };

            return View(model);
        }

        private TicketViewModel MakeTicketViewModel(Ticket ticket)
        {
            var model = new TicketViewModel()
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
                Attachments = ticket.Attachments,
                Histories = ticket.Histories
            };

            return model;
        }

        private List<ApplicationUser> GetAllUsers()
        {
            var allUsers = (from u in DbContext.Users
                            where u != null
                            select u).ToList();

            return allUsers;
        }

        private ApplicationUser GetUserById(string Id)
        {
            var user = (from u in DbContext.Users
                        where u.Id == Id
                        select u).FirstOrDefault();

            return user;
        }
    }
}