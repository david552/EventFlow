using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Bookings.Responses;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.Exceptions;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.Localization;
using EventFlow.Domain.Bookings;
using EventFlow.Domain.Constansts;
using Mapster;
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

        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository, IUnitOfWork unitOdWork, IGlobalSettingsService globalSettingsService)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
            _unitOfWork = unitOdWork;
            _globalSettingsService = globalSettingsService;
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
        }

        public async Task CancelAsync(int bookingId, int currentUserId, CancellationToken token)
        {
            var booking = await _bookingRepository.GetAsync(token, bookingId);
            if (booking == null)
                throw new NotFoundException(ErrorMessages.BookingNotFound, "BookingNotFound");
            if (booking.UserId != currentUserId)
                throw new ForbiddenException(ErrorMessages.BookingCancelForbidden, "BookingCancelForbidden");
            if (booking.IsPurchased)
                throw new BadRequestException(ErrorMessages.CannotCancelPurchasedBooking, "CannotCancelPurchasedBooking");

            var @event = await _eventRepository.GetAsync(token,booking.EventId);
            @event.AvailableTickets += booking.BookedTicketsCount;
            _eventRepository.Update(@event);

            await _bookingRepository.RemoveAsync(token, bookingId);

            await _unitOfWork.SaveChanges(token);

        }

        public async Task<int> CreateAsync(BookingRequestCreateModel model, int currentUserId, CancellationToken token)
        {
            var @event = await _eventRepository.GetAsync(token, model.EventId);
            if (@event == null)
                throw new NotFoundException(ErrorMessages.EventNotFound, "EventNotFound");
            if (@event.AvailableTickets < model.BookedTicketsCount)
            {
                var errorMessage = string.Format(ErrorMessages.NotEnoughTickets, @event.AvailableTickets);
                throw new BadRequestException(errorMessage, "NotEnoughTickets");
            }

            int maxTicketPerUser = 5;
            int bookingExpiratioinHours = await _globalSettingsService.GetByKeyAsync(GlobalSettingsKeys.BookingExpirationHours, token);

            var alreadyBookedTickets = (await GetUserBookingsAsync(currentUserId, token))
                .Where(b => b.EventId == model.EventId)
                .Sum(b => b.BookedTicketsCount);

            var maxTicketUserCouldBook = maxTicketPerUser - alreadyBookedTickets;

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
            return booking.Id;


        }

        public async Task<List<BookingResponseModel>> GetUserBookingsAsync(int userId, CancellationToken token)
        {
            var userBookings = await _bookingRepository.GetUserBookingsWithEventAsync(userId, token);
            if (userBookings == null)
                return new List<BookingResponseModel>();

            return userBookings.Adapt<List<BookingResponseModel>>();
  
        }
    }
}