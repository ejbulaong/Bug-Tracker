﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class EditMembersViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public List<ApplicationUser> MemberUsers { get; set; }
        public List<ApplicationUser> NonMemberUsers { get; set; }
    }
}