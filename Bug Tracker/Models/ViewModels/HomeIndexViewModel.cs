using Bug_Tracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Project> AllProjects { get; set; }
        public List<Ticket> AllTickets { get; set; }
        public List<Ticket> OpenTickets { get; set; }
        public List<Ticket> ResolvedTickets { get; set; }
        public List<Ticket> RejectedTickets { get; set; }
        public List<Ticket> MyProjectsTickets { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public List<ApplicationUser> AllUsers { get; set; }
    }
}