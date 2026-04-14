using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Bookings.Responses;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.Exceptions;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.Localization;
using EventFlow.Domain.Bookings;
using EventFlow.Domain.Constansts;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Bookings
{
    public class BookingService : IBookingService
    {
        readonly IBookingRepository _bookingRepository;
        readonly IEventRepository _eventRepository;
        readonly IUnitOfWork _unitOfWork;
        readonly IGlobalSettingsService _globalSettingsService;
        readonly ILogger<BookingService> _logger;
        readonly IValidator<BookingRequestCreateModel> _createModelValidator;

        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository, IUnitOfWork unitOdWork, IGlobalSettingsService globalSettingsService, ILogger<BookingService> logger, IValidator<BookingRequestCreateModel> createModelValidator)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
            _unitOfWork = unitOdWork;
            _globalSettingsService = globalSettingsService;
            _logger = logger;
            _createModelValidator = createModelValidator;


        }

        public async Task BuyAsync(int bookingId, int currentUserId, CancellationToken token)
        {
            var booking = await _bookingRepository.GetAsync(token, bookingId);
            if (booking == null)
                throw new NotFoundException(ErrorMessages.BookingNotFound, "BookingNotFound");
            if(booking.UserId != currentUserId)
                throw new ForbiddenException(ErrorMessages.BookingPurchaseForbidden, "BookingPurchaseForbidden");
            if (booking.ExpirationTime < DateTime.Now)
                throw new BadRequestException(ErrorMessages.BookingExpired, "BookingExpired");
            if (booking.IsPurchased)
                throw new BadRequestException(ErrorMessages.BookingAlreadyPurchased, "BookingAlreadyPurchased");

            booking.IsPurchased = true;
            booking.ExpirationTime = DateTime.MaxValue;
            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Booking {BookingId} purchased by user {UserId}", bookingId, currentUserId);

        }

        public async Task CancelAsync(int bookingId, int currentUserId, CancellationToken token)
        {
            var booking = await _bookingRepository.GetAsync(token, bookingId);
            if (booking == null)
                throw new NotFoundException(ErrorMessages.BookingNotFound, "BookingNotFound");
            if (booking.UserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} attempted to cancel booking {BookingId} which belongs to another user!", currentUserId, bookingId);
                throw new ForbiddenException(ErrorMessages.BookingCancelForbidden, "BookingCancelForbidden");
            }
            if (booking.IsPurchased)
                throw new BadRequestException(ErrorMessages.CannotCancelPurchasedBooking, "CannotCancelPurchasedBooking");

            var @event = await _eventRepository.GetAsync(token,booking.EventId);

            if (@event == null)
                throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");

            @event.AvailableTickets += booking.BookedTicketsCount;
          
            _eventRepository.Update(@event);

            await _bookingRepository.RemoveAsync(token, bookingId);

            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Booking {BookingId} cancelled by user {UserId}", bookingId, currentUserId);


        }

        public async Task<int> CreateAsync(BookingRequestCreateModel model, int currentUserId, CancellationToken token)
        {
            await _createModelValidator.ValidateAndThrowAsync(model, token);
            var @event = await _eventRepository.GetAsync(token, model.EventId);
            if (@event == null)
                throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");
            if (!@event.IsActive)
                throw new BadRequestException(ErrorMessages.EventIsInactive, "EventNotFound");

            if (@event.AvailableTickets < model.BookedTicketsCount)
            {
                var errorMessage = string.Format(ErrorMessages.NotEnoughTickets, @event.AvailableTickets);
                throw new BadRequestException(errorMessage, "NotEnoughTickets");
            }
            
            int maxTicketPerUser = await _globalSettingsService.GetByKeyAsync(GlobalSettingsKeys.MaxTicketPerUser, token); ;
            int bookingExpiratioinHours = await _globalSettingsService.GetByKeyAsync(GlobalSettingsKeys.BookingExpirationHours, token);

            var alreadyBookedTickets = (await GetUserBookingsAsync(currentUserId, token))
                .Where(b => b.EventId == model.EventId)
                .Sum(b => b.BookedTicketsCount);

            var maxTicketUserCouldBook = Math.Max(0, maxTicketPerUser - alreadyBookedTickets);

            if (model.BookedTicketsCount > maxTicketUserCouldBook)
            {
                var errorMessage = string.Format(ErrorMessages.UserTicketLimitReached, maxTicketUserCouldBook);
                throw new BadRequestException(errorMessage, "UserTicketLimitReached");
            }
                var booking = model.Adapt<Booking>();
          

            booking.IsPurchased = false;
            booking.ExpirationTime = DateTime.Now.AddHours(bookingExpiratioinHours);
            booking.UserId = currentUserId;
            booking.CreatedAt = DateTime.Now;

            @event.AvailableTickets -= model.BookedTicketsCount;
            _eventRepository.Update(@event);

            await _bookingRepository.AddAsync(token, booking);

            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Booking {BookingId} created for event {EventId} by user {UserId}", booking.Id, model.EventId, currentUserId);
            return booking.Id;


        }

        public async Task<List<BookingResponseModel>> GetUserBookingsAsync(int userId, CancellationToken token)
        {
            var userBookings = await _bookingRepository.GetUserBookingsWithEventAsync(userId, token);
            if (userBookings == null)
                return new List<BookingResponseModel>();

            return userBookings.Adapt<List<BookingResponseModel>>();
        }


        public async Task CleanupExpiredBookingsAsync(CancellationToken token)
        {
            var expiredBookings = await _bookingRepository.GetExpiredBookingsAsync(token);
            if (!expiredBookings.Any())
            {
                return; 
            }

            foreach (var booking in expiredBookings)
            {
                _logger.LogInformation("Expired booking {BookingId} for event {EventId} removed",booking.Id, booking.EventId);
                booking.Event.AvailableTickets += booking.BookedTicketsCount;
            }

             _bookingRepository.DeleteRange(expiredBookings, token);

            await _unitOfWork.SaveChanges(token);
            _logger.LogInformation("Deleted {Count} expired bookings", expiredBookings.Count);

        }
    }
}