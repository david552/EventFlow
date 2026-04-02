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
        Task<List<Event>> GetVisibleEventsAsync(CancellationToken token);

    }
}
