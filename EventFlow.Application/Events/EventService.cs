using EventFlow.Application.Bookings;
using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.Events.Requests;
using EventFlow.Application.Events.Responses;
using EventFlow.Application.Exceptions;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.Localization;
using EventFlow.Domain.Constansts;
using EventFlow.Domain.Events;
using Mapster;
using Microsoft.Extensions.Logging;
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
        readonly ILogger<EventService> _logger;


        public EventService(IUnitOfWork unitOfWork, IEventRepository eventRepository, IGlobalSettingsService globalSettingsService, ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _globalSettingsService = globalSettingsService;
            _logger = logger;
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
            throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");
        }

        public async Task UpdateAsync(int id, EventRequestUpdateModel model, int currentUserId, CancellationToken token)
        {
            var existingEvent = await _eventRepository.GetAsync(token, id);


            if (existingEvent == null )
                throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");
            if (existingEvent.UserId != currentUserId)
            {
                _logger.LogWarning("Security: User {UserId} tried to update event {EventId} belonging to another user", currentUserId, id);
                throw new ForbiddenException(ErrorMessages.EventUpdateForbidden, "EventUpdateForbidden");
            }

            int allowedDaysForUpdate = await _globalSettingsService.GetByKeyAsync(GlobalSettingsKeys.EventEditAllowedDays, token);

            if (DateTime.Now > existingEvent.CreatedAt.AddDays(allowedDaysForUpdate))
            {
                var errorMessage = string.Format(ErrorMessages.EventUpdatePeriodExpired, allowedDaysForUpdate);
                throw new BadRequestException(errorMessage, "EventUpdatePeriodExpired");
            }

            model.Adapt(existingEvent);



            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Event {EventId} updated by user {UserId}", id, currentUserId);
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
            _logger.LogInformation("Event {EventId} created by user {UserId}", @event.Id, userId);
            return @event.Id;
        }

        public async Task DeleteAsync(int id, CancellationToken token)
        {
            var @event = await _eventRepository.GetAsync(token,id);
            if (@event == null)
                throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");

             _eventRepository.Remove(@event);
            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Event {EventId} deleted successfully}", id);
        }

        public async Task DeactivateEndedEventsAsync(CancellationToken token)
        {
            var events = await _eventRepository.GetExpiredEventsAsync(token);
            if (!events.Any())
            {
                _logger.LogDebug("There are no expired events to deactivate right now.");
                return; 
            }
            foreach (var @event in events)
            {
                @event.IsActive = false;
                _logger.LogDebug("Event {EventId} deactivated", @event.Id);

            }
            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Deactivated {Count} expired events", events.Count);
        }

        public async Task ActivateEvent(int id, CancellationToken token)
        {
            var @event = await _eventRepository.GetAsync(token, id);
            if(@event == null)
                throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");
            if (@event.IsActive)
                return;
            if(@event.EndTime < DateTime.Now)
                throw new BadRequestException(ErrorMessages.CannotActivateExpiredEvent, "CannotActivateExpiredEvent");

            @event.IsActive = true;
            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Event {EventId} activated", id);


        }
    }
}
