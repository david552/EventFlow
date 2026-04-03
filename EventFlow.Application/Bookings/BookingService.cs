using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Bookings.Responses;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.GlobalSettings;
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
                throw new Exception("Book does not exists");
            if(booking.UserId != currentUserId)
                throw new Exception("You do not have permission to buy ticket");
            if (booking.ExpirationTime < DateTime.Now)
                throw new Exception("Booking has already expired");
            if (booking.IsPurchased)
                throw new Exception("Booking is already purchased.");

            booking.IsPurchased = true;
            booking.ExpirationTime = DateTime.MaxValue;
            _bookingRepository.Update(booking);
            await _unitOfWork.SaveChanges(token);
        }

        public async Task CancelAsync(int bookingId, int currentUserId, CancellationToken token)
        {
            var booking = await _bookingRepository.GetAsync(token, bookingId);
            if (booking == null)
                throw new Exception("Book does not exists");
            if (booking.UserId != currentUserId)
                throw new Exception("You do not have permission to cancel this booking");
            if (booking.IsPurchased)
                throw new Exception("Booking is already purchased, you cannot cancel it");

            var @event = await _eventRepository.GetAsync(token,booking.EventId);
            @event.AvailableTickets += booking.BookedTicketsCount;
            _eventRepository.Update(@event);

            await _bookingRepository.RemoveAsync(token, bookingId);

            await _unitOfWork.SaveChanges(token);

        }

        public async Task<int> CreateAsync(BookingRequestModel model, int currentUserId, CancellationToken token)
        {
            var @event = await _eventRepository.GetAsync(token, model.EventId);
            if (@event == null)
                throw new Exception("Event not found.");
            if (@event.AvailableTickets < model.BookedTicketsCount)
                throw new Exception($"Not enough tickets available. Only {@event.AvailableTickets} left.");

            int maxTicketPerUser = 5;
            int bookingExpiratioinHours = await _globalSettingsService.GetByKeyAsync(GlobalSettingsKeys.BookingExpirationHours, token);

            var alreadyBookedTickets = (await GetUserBookingsAsync(currentUserId, token))
                .Where(b => b.EventId == model.EventId)
                .Sum(b => b.BookedTicketsCount);

            var maxTicketUserCouldBook = maxTicketPerUser - alreadyBookedTickets;

            if (model.BookedTicketsCount > maxTicketUserCouldBook)
                throw new Exception($"You have reached the limit. You can only book {maxTicketUserCouldBook} more tickets.");
            var booking = model.Adapt<Booking>();
            
            if(model.IsPurchased == true)
            {
                booking.ExpirationTime = DateTime.MaxValue;
                booking.IsPurchased = true;
            }
            else
            {
                booking.IsPurchased = false;
                booking.ExpirationTime = DateTime.Now.AddHours(bookingExpiratioinHours);
            }
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