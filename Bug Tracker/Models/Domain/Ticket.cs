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

        public virtual int ProjectId { get; set; }
        public virtual int TicketTypeId { get; set; }
        public virtual int TicketPriorityId { get; set; }
        public virtual int TicketStatusId { get; set; }
        public virtual int CreatorId { get; set; }
        public virtual int DeveloperAssignedId { get; set; }
    }
}