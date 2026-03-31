using EventFlow.Domain.Bookings;
using EventFlow.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Domain.Events
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public int TotalTickets { get; set; } 
        public int AvailableTickets { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; } = null!; 
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
