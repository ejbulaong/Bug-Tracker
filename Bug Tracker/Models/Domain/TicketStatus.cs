using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.Domain
{
    public class TicketStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public TicketStatus(string name)
        {
            Name = name;
        }
        public virtual List<Ticket> Tickets { get; set; }
    }
}