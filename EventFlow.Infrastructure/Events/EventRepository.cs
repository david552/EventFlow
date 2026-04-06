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


        public async Task<List<Event>> GetVisibleEventsAsync(CancellationToken token)
        {

            return await _dbSet.AsNoTracking()
                  .Where(x => x.IsActive)
                  .ToListAsync(token);

        }

        public async Task<List<Event>> GetExpiredEventsAsync(CancellationToken token)
        {
            return await _dbSet.Where(x => x.EndTime < DateTime.Now && x.IsActive).ToListAsync();
        }

      
    }
}
