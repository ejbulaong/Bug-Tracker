using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class ManageUsersViewModel
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public IList<string> Role { get; set; }
    }
}