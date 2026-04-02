using EventFlow.Application.Events.Repositories;
using EventFlow.Application.Events.Requests;
using EventFlow.Application.Events.Responses;
using EventFlow.Domain.Events;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Events
{
    public class EventService : IEventService
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IEventRepository _repository;
        public EventService(IUnitOfWork unitOfWork, IEventRepository repository)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;

        }

        public async Task<List<EventResponseModel>> GetAllVisibleAsync(CancellationToken token)
        {
            var visiblEvents = await _repository.GetVisibleEventsAsync(token);

            return visiblEvents.Adapt<List<EventResponseModel>>();
        }

        public async Task<EventResponseModel?> GetByIdAsync(int id, CancellationToken token)
        {
            var @event = await _repository.GetAsync(token, id);

            if (@event != null)
                return @event.Adapt<EventResponseModel>();
            throw new Exception(@"Event Not Found");
        }
        public async Task UpdateAsync(int id, EventRequestUpdateModel model, int currentUserId, CancellationToken token)
        {
            var existingEvent = await _repository.GetAsync(token, id);


            if (existingEvent == null )
                throw new Exception("Event not found to update");
            if (existingEvent.UserId != currentUserId)
            {
                throw new Exception("You don't have permission to update this event.");
            }

            int allowedDaysForUpdate = 3;

            if (DateTime.Now > existingEvent.CreatedAt.AddDays(allowedDaysForUpdate))
            {
                throw new Exception($"The update period ({allowedDaysForUpdate} days) has expired.");
            }

            model.Adapt(existingEvent);


            _repository.Update(existingEvent);

            await _unitOfWork.SaveChanges(token);
        }

        public async Task<int> CreateAsync(EventRequestCreateModel model, int userId, CancellationToken token)
        {
            var @event = model.Adapt<Event>();
            @event.UserId = userId;
            @event.IsActive = false;
            @event.CreatedAt = DateTime.Now;
            @event.AvailableTickets = @event.TotalTickets;

            await _repository.AddAsync(token, @event);
            await _unitOfWork.SaveChanges(token);
            return @event.Id;
        }

        public async  Task DeleteAsync(int id, CancellationToken token)
        {
            await _repository.RemoveAsync(token, id);
            await _unitOfWork.SaveChanges(token);
        }

    }
}
