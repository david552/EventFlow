using EventFlow.Application.Events.Repositories;
using EventFlow.Application.Events.Requests;
using EventFlow.Application.Events.Responses;
using EventFlow.Application.Exceptions;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.Localization;
using EventFlow.Domain.Constansts;
using EventFlow.Domain.Events;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace EventFlow.Application.Events
{
    public class EventService : IEventService
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IEventRepository _eventRepository;
        readonly IGlobalSettingsService _globalSettingsService;
        readonly ILogger<EventService> _logger;
        private readonly IValidator<EventRequestCreateModel> _createModelValidator;
        private readonly IValidator<EventRequestUpdateModel> _updateModelValidator;



        public EventService(IUnitOfWork unitOfWork, IEventRepository eventRepository, IGlobalSettingsService globalSettingsService, ILogger<EventService> logger, IValidator<EventRequestCreateModel> createModelValidator, IValidator<EventRequestUpdateModel> updateModelValidator)
        {
            _eventRepository = eventRepository;
            _unitOfWork = unitOfWork;
            _globalSettingsService = globalSettingsService;
            _logger = logger;
            _createModelValidator = createModelValidator;
            _updateModelValidator = updateModelValidator;
        }

        public async Task<IPagedList<EventResponseModel>> GetPagedVisibleAsync(int pageNumber, int pageSize, CancellationToken token)
        {
            var (items, totalCount) = await _eventRepository.GetVisiblePagedAsync(pageNumber, pageSize, token);

            var mappedItems = items.Adapt<List<EventResponseModel>>();

            return new StaticPagedList<EventResponseModel>(mappedItems, pageNumber, pageSize, totalCount);
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
            await _updateModelValidator.ValidateAndThrowAsync(model, token);

            var existingEvent = await _eventRepository.GetAsync(token, id);


            if (existingEvent == null)
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

            int ticketsToAddInTotalCount = model.AvailableTickets - existingEvent.AvailableTickets;
            model.Adapt(existingEvent);
            existingEvent.TotalTickets = existingEvent.TotalTickets + ticketsToAddInTotalCount;

            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Event {EventId} updated by user {UserId}", id, currentUserId);
        }

        public async Task<int> CreateAsync(EventRequestCreateModel model, int userId, CancellationToken token)
        {
            await _createModelValidator.ValidateAndThrowAsync(model, token);

            var @event = model.Adapt<Event>();
            @event.UserId = userId;
            @event.IsActive = false;
            @event.CreatedAt = DateTime.UtcNow;
            @event.StartTime = DateTime.SpecifyKind(model.StartTime, DateTimeKind.Utc);
            @event.EndTime = DateTime.SpecifyKind(model.EndTime, DateTimeKind.Utc);
            @event.AvailableTickets = @event.TotalTickets;

            await _eventRepository.AddAsync(token, @event);
            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Event {EventId} created by user {UserId}", @event.Id, userId);
            return @event.Id;
        }

        public async Task DeleteAsync(int id, CancellationToken token)
        {
            var @event = await _eventRepository.GetAsync(token, id);
            if (@event == null)
                throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");

            _eventRepository.Remove(@event);
            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Event {EventId} deleted successfully", id);
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
            if (@event == null)
                throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");
            if (@event.IsActive)
                return;
            if (@event.EndTime < DateTime.UtcNow)
                throw new BadRequestException(ErrorMessages.CannotActivateExpiredEvent, "CannotActivateExpiredEvent");

            @event.IsActive = true;
            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Event {EventId} activated", id);


        }

        public async Task<List<EventResponseModel>> GetPendingEvents(CancellationToken token)
        {
            var pendingEvents = await _eventRepository.GetPendingEventsAsync(token);

            return pendingEvents.Adapt<List<EventResponseModel>>();
        }

        public async Task<List<EventResponseModel>> GetEventsByUserIdAsync(int userId, CancellationToken token)
        {
            var userEvents = await _eventRepository.GetByUserIdAsync(userId, token);

            return userEvents.Adapt<List<EventResponseModel>>();
        }
    }
}
