using Bug_Tracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class TicketViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public Project Project { get; set; }
        public TicketType Type { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public ApplicationUser Creator { get; set; }
        public ApplicationUser AssignedDeveloper { get; set; }
        public List<TicketComment> Comments { get; set; }
        public List<TicketAttachment> Attachments { get; set; }
        public List<TicketHistory> Histories { get; set; }
        public List<ApplicationUser> NotificationReceiver { get; set; }
        public bool Notification { get; set; }
    }
}