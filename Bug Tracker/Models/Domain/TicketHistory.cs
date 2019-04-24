using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.Domain
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public int TicketId { get; set; }

        public virtual List<PropertyChange> PropertyChanges { get; set; }

        public TicketHistory()
        {
            PropertyChanges = new List<PropertyChange>();
        }
    }
}