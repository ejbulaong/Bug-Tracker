using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.Domain
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual List<ApplicationUser> Users { get; set; }
        public virtual List<Ticket> Tickets { get; set; }

        public bool Active { get; set; } 
 
        public Project()
        {
            Users = new List<ApplicationUser>();
            Tickets = new List<Ticket>();
            DateCreated = DateTime.Now;
            DateUpdated = null;
        }
    }
}