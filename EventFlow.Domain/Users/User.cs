using EventFlow.Domain.Bookings;
using EventFlow.Domain.Events;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Domain.Users
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;


        public  ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
        public  ICollection<Booking> Bookings { get; set; } = new List<Booking>();



    }
}
