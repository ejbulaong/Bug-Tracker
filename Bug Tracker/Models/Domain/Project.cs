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

        public virtual List<ApplicationUser> Users { get; set; }

        public Project()
        {
            Users = new List<ApplicationUser>();
        }

    }
}