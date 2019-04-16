using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.Domain
{
    public class UserLog
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public DateTime Time { get; set; }
    }
}