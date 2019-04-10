using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class AssignTicketViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public ApplicationUser AssignedDeveloper { get; set; }
        public string AssignedDeveloperId { get; set; }
        public List<ApplicationUser> Developers { get; set; }
    }
}