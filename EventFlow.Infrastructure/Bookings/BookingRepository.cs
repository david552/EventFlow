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

        public async Task<List<Booking>> GetUserBookingsWithEventAsync(int userId, CancellationToken token)
        {
          return   await _dbSet.Include(b => b.Event)
                .Where(b => b.UserId == userId)
                .ToListAsync(token);
        }
    }
}
