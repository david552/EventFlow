using EventFlow.Application;
using EventFlow.Application.Bookings.Repositories;
using EventFlow.Domain.Bookings;
using EventFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Bookings
{
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void  DeleteRange(IEnumerable<Booking> bookings, CancellationToken token)
        {
            if (bookings == null || !bookings.Any())
            {
                return;
            }
            _dbSet.RemoveRange(bookings);
        }

        public async Task<List<Booking>> GetExpiredBookingsAsync( CancellationToken token)
        {
            var currentTime = DateTime.UtcNow;

            return await _dbSet
                .Where(x => x.ExpirationTime <= currentTime)
                .Include(x=>x.Event)
                .ToListAsync(token);
        }

        public async Task<List<Booking>> GetUserBookingsWithEventAsync(int userId, CancellationToken token)
        {
          return   await _dbSet.Include(b => b.Event)
                .Where(b => b.UserId == userId)
                .ToListAsync(token);
        }
    }
}
