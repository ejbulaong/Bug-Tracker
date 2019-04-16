using Bug_Tracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class ActivityLogViewModel
    {
        public string UserName { get; set; }
        public List<UserLog> UserLogs { get; set; }
    }
}