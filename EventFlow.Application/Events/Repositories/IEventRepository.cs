using EventFlow.Domain.Bookings;
using EventFlow.Domain.Events;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Events.Repositories
{
    public interface IEventRepository : IBaseRepository<Event>
    {
        Task<(List<Event> Items, int TotalCount)> GetVisiblePagedAsync(int pageNumber, int pageSize, CancellationToken token);
        Task<List<Event>> GetExpiredEventsAsync(CancellationToken token);

        Task<List<Event>> GetPendingEventsAsync(CancellationToken token);
        Task<List<Event>> GetByUserIdAsync(int userId, CancellationToken token);

    }
}
