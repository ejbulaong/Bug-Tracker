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

        public ApplicationUser AssignedDeveloper { get; set; }
        public string AssignedDeveloperId { get; set; }

        public ApplicationUser CreatedBy { get; set; }
        public string CreatedById { get; set; }

        public Project Project { get; set; }
        public int ProjectId { get; set; }

        public TicketPriority Priority { get; set; }
        public int PriorityId { get; set; }

        public TicketStatus Status { get; set; }
        public int StatusId { get; set; }

        public TicketType Type { get; set; }
        public int TypeId { get; set; }
    }
}