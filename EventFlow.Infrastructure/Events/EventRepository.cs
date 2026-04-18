using EventFlow.Application;
using EventFlow.Application.Events.Repositories;
using EventFlow.Domain.Events;
using EventFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure.Events
{
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {

        public EventRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Event>> GetExpiredEventsAsync(CancellationToken token)
        {
            var currentTime = DateTime.UtcNow;
            return await _dbSet.Where(x => x.EndTime < currentTime && x.IsActive).ToListAsync();
        }

        public async Task<List<Event>> GetPendingEventsAsync(CancellationToken token)
        {
            var currentTime = DateTime.UtcNow;
            return await _dbSet.Where(x => x.EndTime > currentTime && !x.IsActive).ToListAsync();
        }

        public async Task<List<Event>> GetByUserIdAsync(int userId, CancellationToken token)
        {
            return await _context.Events
                  .Where(e => e.UserId == userId)
                  .ToListAsync(token);
        }

        public async Task<(List<Event> Items, int TotalCount)> GetVisiblePagedAsync(int pageNumber, int pageSize, CancellationToken token)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedAt); 

            var totalCount = await query.CountAsync(token);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token);

            return (items, totalCount);
        }
    }
}
