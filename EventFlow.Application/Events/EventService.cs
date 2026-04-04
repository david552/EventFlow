using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.Events.Requests;
using EventFlow.Application.Events.Responses;
using EventFlow.Application.Exceptions;
using EventFlow.Application.GlobalSettings;
using EventFlow.Domain.Constansts;
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
        readonly IEventRepository _eventRepository;
        readonly IGlobalSettingsService _globalSettingsService;

        public EventService(IUnitOfWork unitOfWork, IEventRepository eventRepository, IGlobalSettingsService globalSettingsService)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _globalSettingsService = globalSettingsService;

        }

        public async Task<List<EventResponseModel>> GetAllVisibleAsync(CancellationToken token)
        {
            var visiblEvents = await _eventRepository.GetVisibleEventsAsync(token);

            return visiblEvents.Adapt<List<EventResponseModel>>();
        }

        public async Task<EventResponseModel?> GetByIdAsync(int id, CancellationToken token)
        {
            var @event = await _eventRepository.GetAsync(token, id);

            if (@event != null)
                return @event.Adapt<EventResponseModel>();
            throw new NotFoundException(@"Event Not Found", "EventNotFound");
        }
        public async Task UpdateAsync(int id, EventRequestUpdateModel model, int currentUserId, CancellationToken token)
        {
            var existingEvent = await _eventRepository.GetAsync(token, id);


            if (existingEvent == null )
                throw new NotFoundException("Event not found to update", "EventNotFound");
            if (existingEvent.UserId != currentUserId)
            {
                throw new ForbiddenException("You don't have permission to update this event.", "EventUpdateForbidden");
            }

            int allowedDaysForUpdate = await _globalSettingsService.GetByKeyAsync(GlobalSettingsKeys.EventEditAllowedDays, token);

            if (DateTime.Now > existingEvent.CreatedAt.AddDays(allowedDaysForUpdate))
            {
                throw new BadRequestException($"The update period ({allowedDaysForUpdate} days) has expired.", "EventUpdatePeriodExpired");
            }

            model.Adapt(existingEvent);


            _eventRepository.Update(existingEvent);

            await _unitOfWork.SaveChanges(token);
        }

        public async Task<int> CreateAsync(EventRequestCreateModel model, int userId, CancellationToken token)
        {
            var @event = model.Adapt<Event>();
            @event.UserId = userId;
            @event.IsActive = false;
            @event.CreatedAt = DateTime.Now;
            @event.AvailableTickets = @event.TotalTickets;

            await _eventRepository.AddAsync(token, @event);
            await _unitOfWork.SaveChanges(token);
            return @event.Id;
        }

        public async  Task DeleteAsync(int id, CancellationToken token)
        {
            await _eventRepository.RemoveAsync(token, id);
            await _unitOfWork.SaveChanges(token);
        }

    }
}
