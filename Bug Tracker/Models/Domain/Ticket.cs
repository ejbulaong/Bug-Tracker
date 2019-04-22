using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.Domain
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual ApplicationUser AssignedDeveloper { get; set; }
        public string AssignedDeveloperId { get; set; }

        public virtual ApplicationUser Creator { get; set; }
        public string CreatorId { get; set; }

        public virtual Project Project { get; set; }
        public int ProjectId { get; set; }

        public virtual TicketPriority Priority { get; set; }
        public int PriorityId { get; set; }

        public virtual TicketStatus Status { get; set; }
        public int StatusId { get; set; }

        public virtual TicketType Type { get; set; }
        public int TypeId { get; set; }

        public virtual List<TicketComment> Comments { get; set; }

        public virtual List<TicketAttachment> Attachments { get; set; }

        public virtual List<TicketHistory> Histories { get; set; }
    }
}