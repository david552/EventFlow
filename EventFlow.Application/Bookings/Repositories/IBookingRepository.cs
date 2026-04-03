using EventFlow.Domain.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Bookings.Repositories
{
    public interface IBookingRepository : IBaseRepository<Booking>
    {
        public  Task<List<Booking>> GetUserBookingsWithEventAsync(int userId, CancellationToken token);
    }
}
