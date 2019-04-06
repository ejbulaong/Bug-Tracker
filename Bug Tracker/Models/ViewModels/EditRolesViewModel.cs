using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class EditRolesViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IList<string> Roles { get; set; }
    }
}