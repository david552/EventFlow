using EventFlow.Domain.Events;
using EventFlow.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Bookings.Requests
{
    public class BookingRequestCreateModel
    {
       
        public int BookedTicketsCount { get; set; }
        public int EventId { get; set; }
    }
}
