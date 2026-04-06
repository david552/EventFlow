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
        Task<List<Booking>> GetUserBookingsWithEventAsync(int userId, CancellationToken token);
        Task<List<Booking>> GetExpiredBookingsAsync(CancellationToken token);
        public void DeleteRange(IEnumerable<Booking> bookings, CancellationToken token);
    }
}
