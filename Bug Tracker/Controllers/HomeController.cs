using Bug_Tracker.Enums;
using Bug_Tracker.Models;
using Bug_Tracker.Models.Domain;
using Bug_Tracker.Models.Filters;
using Bug_Tracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bug_Tracker.Controllers
{
    [UserLogFilters]
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            var DbContext = new ApplicationDbContext();

            var allProjects = DbContext.Projects.ToList();
            var allUsers = DbContext.Users.ToList();
            var allTickets = DbContext.Tickets.ToList();
            var openTickets = DbContext.Tickets.Where(t => t.Status.Name == nameof(EnumTicketStatuses.Open)).ToList();
            var resolvedTickets = DbContext.Tickets.Where(t => t.Status.Name == nameof(EnumTicketStatuses.Resolved)).ToList();
            var rejectedTickets = DbContext.Tickets.Where(t => t.Status.Name == nameof(EnumTicketStatuses.Rejected)).ToList();

            var userId = User.Identity.GetUserId();
            var user = DbContext.Users.FirstOrDefault(u => u.Id == userId);

            var myProjectsTickets = new List<Ticket>();
            foreach(var p in user.Projects)
            {
                foreach(var t in p.Tickets)
                {
                    myProjectsTickets.Add(t);
                }
            }

            var model = new HomeIndexViewModel();
            model.AllProjects = allProjects;
            model.AllTickets = allTickets;
            model.OpenTickets = openTickets;
            model.ResolvedTickets = resolvedTickets;
            model.RejectedTickets = rejectedTickets;
            model.AllUsers = allUsers;
            model.CurrentUser = user;
            model.MyProjectsTickets = myProjectsTickets;

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}