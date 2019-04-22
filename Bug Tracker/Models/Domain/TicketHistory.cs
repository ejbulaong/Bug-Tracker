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
        public ApplicationUser User { get; set; }
        public string PropertyChanged { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public virtual Ticket Ticket { get; set; }
        public int TicketId { get; set; }
    }
}