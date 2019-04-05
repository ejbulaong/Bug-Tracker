using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class EditProjectViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public List<ApplicationUser> MemberUsers { get; set; }
        public List<ApplicationUser> NonMemberUsers { get; set; }
    }
}