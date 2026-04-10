using EventFlow.Application.Events.Requests;
using EventFlow.Application.Events.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace EventFlow.Application.Events
{
    public interface IEventService
    {
        Task<IPagedList<EventResponseModel>> GetPagedVisibleAsync(int pageNumber, int pageSize, CancellationToken token);

        Task<EventResponseModel?> GetByIdAsync(int id, CancellationToken token);

        Task<int> CreateAsync(EventRequestCreateModel model, int userId, CancellationToken token);

        Task UpdateAsync(int id, EventRequestUpdateModel model, int currentUserId, CancellationToken token);

        Task DeleteAsync(int id, CancellationToken token);

        Task DeactivateEndedEventsAsync(CancellationToken token);
        Task ActivateEvent(int id, CancellationToken token);
        Task<List<EventResponseModel>> GetPendingEvents(CancellationToken token);
        Task<List<EventResponseModel>> GetEventsByUserIdAsync(int userId, CancellationToken token);

    }
}
