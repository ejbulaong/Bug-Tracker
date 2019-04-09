using Bug_Tracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Bug_Tracker.Models.ViewModels
{
    public class CreateTicketViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        [Required(ErrorMessage = "The Project field is required.")]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "The Type field is required.")]
        public int TypeId { get; set; }
        [Required(ErrorMessage ="The Priority field is required.")]
        public int PriorityId { get; set; }
        public ApplicationUser Creator { get; set; }
        public ApplicationUser AssignedDeveloper { get; set; }

        public List<Project> Projects { get; set; }
        public List<TicketType> Types { get; set; }
        public List<TicketPriority> Priorities { get; set; }
        public List<TicketStatus> Statuses { get; set; }                        
    }   
}