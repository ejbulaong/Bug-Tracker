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
using System.Net.Mail;

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
                Users = members,
                Active = true
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
                         where p.Active
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
                         where p.Active
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

        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult ArchiveProject(int? projectId)
        {
            if(projectId == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var projectToArchive = DbContext.Projects.FirstOrDefault(p=>p.Id == projectId);
            projectToArchive.Active = false;
            DbContext.SaveChanges();

            return RedirectToAction(nameof(BugController.ViewAllProjects), "Bug");
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

                ticket.AssignedDeveloper = developer;
                DbContext.SaveChanges();

                var newHistory = new TicketHistory();
                var propChange = new PropertyChange();
                propChange.PropertyName = nameof(ticket.AssignedDeveloper);
                propChange.OldValue = "none";
                propChange.NewValue = ticket.AssignedDeveloper.Name;
                newHistory.PropertyChanges.Add(propChange);
                newHistory.User = user;
                newHistory.DateTime = DateTime.Now;
                ticket.Histories.Add(newHistory);
                DbContext.SaveChanges();
                SendNotification(newHistory, user, ticket);
            }

            return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", new { Id = ticket.Id });
        }

        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult UnAssignTicket(int Id)
        {
            var ticket = (from t in DbContext.Tickets
                          where t.Id == Id
                          select t).FirstOrDefault();
            var userid = User.Identity.GetUserId();
            var user = GetUserById(userid);

            var newHistory = new TicketHistory();
            var propChange = new PropertyChange();
            propChange.PropertyName = nameof(ticket.AssignedDeveloper);
            propChange.OldValue = ticket.AssignedDeveloper.Name;
            propChange.NewValue = "none";
            newHistory.PropertyChanges.Add(propChange);
            newHistory.User = user;
            newHistory.DateTime = DateTime.Now;
            ticket.Histories.Add(newHistory);
            SendNotification(newHistory, user, ticket);

            ticket.AssignedDeveloper = null;
            ticket.AssignedDeveloperId = null;
            DbContext.SaveChanges();

            return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", new { Id = ticket.Id });
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
                               where p.Active
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
            var newHistory = new TicketHistory();

            if (ticketToEdit.Title != model.Title)
            {
                var propChange = new PropertyChange();
                propChange.PropertyName = nameof(ticketToEdit.Title);
                propChange.OldValue = ticketToEdit.Title;
                propChange.NewValue = model.Title;
                newHistory.PropertyChanges.Add(propChange);
                ticketToEdit.Title = model.Title;
            }

            if (ticketToEdit.Project != project)
            {
                var propChange = new PropertyChange();
                propChange.PropertyName = nameof(ticketToEdit.Project);
                propChange.OldValue = ticketToEdit.Project.Name;
                propChange.NewValue = project.Name;
                newHistory.PropertyChanges.Add(propChange);
                ticketToEdit.Project = project;
            }

            if (ticketToEdit.Description != model.Description)
            {
                var propChange = new PropertyChange();
                propChange.PropertyName = nameof(ticketToEdit.Description);
                propChange.OldValue = ticketToEdit.Description;
                propChange.NewValue = model.Description;
                newHistory.PropertyChanges.Add(propChange);
                ticketToEdit.Description = model.Description;
            }

            if (ticketToEdit.Type != type)
            {
                var propChange = new PropertyChange();
                propChange.PropertyName = nameof(ticketToEdit.Type);
                propChange.OldValue = ticketToEdit.Type.Name;
                propChange.NewValue = type.Name;
                newHistory.PropertyChanges.Add(propChange);
                ticketToEdit.Type = type;
            }

            if (ticketToEdit.Priority != priority)
            {
                var propChange = new PropertyChange();
                propChange.PropertyName = nameof(ticketToEdit.Priority);
                propChange.OldValue = ticketToEdit.Priority.Name;
                propChange.NewValue = priority.Name;
                newHistory.PropertyChanges.Add(propChange);
                ticketToEdit.Priority = priority;
            }

            if (User.IsInRole(nameof(UserRoles.Admin)) || User.IsInRole(nameof(UserRoles.ProjectManager)))
            {
                var status = DbContext.TicketStatuses.FirstOrDefault(t => t.Id == model.StatusId);
                if (ticketToEdit.Status != status)
                {
                    var propChange = new PropertyChange();
                    propChange.PropertyName = nameof(ticketToEdit.Status);
                    propChange.OldValue = ticketToEdit.Status.Name;
                    propChange.NewValue = status.Name;
                    newHistory.PropertyChanges.Add(propChange);
                    ticketToEdit.Status = status;
                }
            }

            if (newHistory.PropertyChanges.Any())
            {
                newHistory.DateTime = DateTime.Now;
                newHistory.User = user;
                ticketToEdit.Histories.Add(newHistory);
                SendNotification(newHistory, user, ticketToEdit);
            }

            ticketToEdit.DateUpdated = DateTime.Now;
            DbContext.SaveChanges();

            return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", new { Id = ticketToEdit.Id });

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
            if (ticket.NotificationReceiver.Contains(user))
            {
                model.Notification = true;
            }
            else
            {
                model.Notification = false;
            }

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
                SendNotification(null, user, ticket);
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

            var userId = User.Identity.GetUserId();
            var user = GetUserById(userId);

            if (User.IsInRole(nameof(UserRoles.Submitter)) || User.IsInRole(nameof(UserRoles.Developer)))
            {
                if (attachment.User != user)
                {
                    return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", new { Id = ticket.Id });
                }
            }

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
            SendNotification(null, user, ticket);

            var model = MakeTicketViewModel(ticket);

            return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", model);
        }

        [Authorize]
        public ActionResult RemoveComment(int? Id, int? ticketId)
        {
            var ticket = (from t in DbContext.Tickets
                          where t.Id == ticketId
                          select t).FirstOrDefault();

            var comment = (from c in DbContext.TicketComments
                              where c.Id == Id
                              select c).FirstOrDefault();

            var userId = User.Identity.GetUserId();
            var user = GetUserById(userId);

            if (User.IsInRole(nameof(UserRoles.Submitter)) || User.IsInRole(nameof(UserRoles.Developer)))
            {
                if (comment.User != user)
                {
                    return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", new { Id = ticket.Id });
                }
            }

            DbContext.TicketComments.Remove(comment);
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

        [BugTrackerFiltersAuthorization(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult ToggleNotification(int? Id)
        {
            if (Id != null)
            {
                var ticket = DbContext.Tickets.FirstOrDefault(t => t.Id == Id);
                var userId = User.Identity.GetUserId();
                var user = GetUserById(userId);

                if (ticket.NotificationReceiver.Contains(user))
                {
                    ticket.NotificationReceiver.Remove(user);
                }
                else
                {
                    ticket.NotificationReceiver.Add(user);
                }
                DbContext.SaveChanges();
                return RedirectToAction(nameof(BugController.ViewTicketDetails), "Bug", new { id = ticket.Id });
            }

            return RedirectToAction(nameof(BugController.Index), "Home");
        }

        private TicketViewModel MakeTicketViewModel(Ticket ticket)
        {
            var user = GetUserById(User.Identity.GetUserId());
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
                Histories = ticket.Histories,
                NotificationReceiver = ticket.NotificationReceiver,
                CurrentUser = user
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

        private void SendNotification(TicketHistory history, ApplicationUser changer, Ticket ticket)
        {
            if (ticket.AssignedDeveloper != null)
            {
                var subject = $"Ticket Update";
                var body = $"The following changes are made on ticket <strong>{ticket.Title}</strong> by <strong>{changer.Name}</strong>: <br><br>";
                var to = new List<string>();
                if (history != null)
                {
                    foreach (var c in history.PropertyChanges)
                    {
                        if (c.PropertyName == nameof(Ticket.AssignedDeveloper) && c.OldValue == "none")
                        {
                            body = $"{ticket.AssignedDeveloper.Name} is assigned to Ticket <strong>{ticket.Title}</strong> by <strong>{changer.Name}</strong>.";
                        }
                        else if (c.PropertyName == nameof(Ticket.AssignedDeveloper) && c.NewValue == "none")
                        {
                            body = $"{ticket.AssignedDeveloper.Name} is unassigned to Ticket <strong>{ticket.Title}</strong> by <strong>{changer.Name}</strong>.";
                        }
                        else
                        {
                            var newString = $" - <strong>{c.PropertyName}</strong> is changed from <strong>{c.OldValue }</strong> to <strong>{c.NewValue}</strong>. <br> ";
                            body += newString;
                        }
                    }
                }
                else
                {
                    var newString = $" - a <strong>comment/attachment</strong> has been added on this ticket.";
                    body += newString;
                }

                to.Add(ticket.AssignedDeveloper.Email);
                foreach (var u in ticket.NotificationReceiver)
                {
                    to.Add(u.Email);
                }

                var emailService = new EmailService();
                emailService.Send(to, body, subject);
            }
        }
    }
}