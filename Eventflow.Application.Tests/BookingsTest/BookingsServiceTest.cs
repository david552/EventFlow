using Eventflow.Application.Tests.BookingsServiceTest.BookingsTest;
using EventFlow.Application.Bookings;
using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Bookings.Responses;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Localization;
using EventFlow.Domain.Bookings;
using EventFlow.Domain.Constansts;
using EventFlow.Domain.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
namespace Eventflow.Application.Tests.BookingsTest
{
    public class BookingsServiceTest 
    {

        readonly BookingServiceFixture _fixture;

        public BookingsServiceTest()
        {
           _fixture = new BookingServiceFixture(); 
            
        }
        #region CancleAcync Tests
        [Fact]
        public async Task CancelAsync_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int currentUserId = 5;

            _fixture.BookingRepoMock
                 .Setup(x => x.GetAsync(token, bookingId))
                 .ReturnsAsync((Booking?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _fixture.Service.CancelAsync(bookingId, currentUserId, token));
        }
        [Fact]
        public async Task CancelAsync_ShouldThrowForbiddenException_WhenUserDoesNotOwnBooking()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int currentUserId = 5;

            var booking = new Booking
            {
                Id = bookingId,
                UserId = 10,
                IsPurchased = false
            };

            _fixture.BookingRepoMock
                .Setup(x => x.GetAsync(token, bookingId))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ForbiddenException>(() =>
                _fixture.Service.CancelAsync(bookingId, currentUserId, token));
        }
        [Fact]
        public async Task CancelAsync_ShouldThrowBadRequestException_WhenBookingIsAlreadyPurchased()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int currentUserId = 5;

            var booking = new Booking
            {
                Id = bookingId,
                UserId = currentUserId,
                IsPurchased = true
            };

            _fixture.BookingRepoMock
                .Setup(x => x.GetAsync(token, bookingId))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _fixture.Service.CancelAsync(bookingId, currentUserId, token));
        }
        [Fact]
        public async Task CancelAsync_ShouldThrowNotFoundException_WhenEventDoesNotExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;

            int currentUserId = 5;


            var booking = new Booking
            {
                Id = bookingId,
                UserId = currentUserId,
                IsPurchased = false,
                EventId = 1

            };



            _fixture.BookingRepoMock
                .Setup(x => x.GetAsync(token, bookingId))
                .ReturnsAsync(booking);
            _fixture.EventRepoMock
                .Setup(x => x.GetAsync(token, booking.EventId))
                .ReturnsAsync((Event?)null);


            await Assert.ThrowsAsync<NotFoundException>(() =>
                 _fixture.Service.CancelAsync(bookingId, currentUserId, token));
        }

        [Fact]
        public async Task CancelAsync_ShouldRestoreTicketsAndRemoveBooking_WhenValid()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int currentUserId = 5;
            int eventId = 100;
            int avaliableEventTickets = 10;
            int bookedTicketsCount = 3;

            var booking = new Booking
            {
                Id = bookingId,
                UserId = currentUserId,
                IsPurchased = false,
                EventId = eventId,
                BookedTicketsCount = bookedTicketsCount
            };

            var @event = new Event
            {
                Id = eventId,
                AvailableTickets = avaliableEventTickets
            };

            _fixture.BookingRepoMock
                .Setup(x => x.GetAsync(token, bookingId))
                .ReturnsAsync(booking);
            _fixture.EventRepoMock
                .Setup(x => x.GetAsync(token, eventId)).ReturnsAsync(@event);

            await _fixture.Service.CancelAsync(bookingId, currentUserId, token);

            Assert.Equal(13, @event.AvailableTickets);
            _fixture.EventRepoMock.Verify(x => x.Update(@event), Times.Once);
            _fixture.BookingRepoMock.Verify(x => x.RemoveAsync(token, bookingId), Times.Once);
            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);
        }
        #endregion

        #region CreateAsync Tests
        [Fact]
        public async Task CreateAsync_ShouldThrowNotFoundException_WhenEventDoesNotExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int currentUserId = 1;

            var booking = new BookingRequestCreateModel
            {
                BookedTicketsCount = 1,
                EventId = 1
            };



            _fixture.EventRepoMock
                .Setup(x => x.GetAsync(token, booking.EventId))
                .ReturnsAsync((Event?)null);


            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                 _fixture.Service.CreateAsync(booking, currentUserId, token));

            var expectedMessage = ErrorMessages.EventNotFound;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowBadRequestException_WhenEventIsInNotActive()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int currentUserId = 1;

            var booking = new BookingRequestCreateModel
            {
                BookedTicketsCount = 1,
                EventId = 1
            };

            var @event = new Event
            {
                IsActive = false
            };

            _fixture.EventRepoMock
                .Setup(x => x.GetAsync(token, 1))
                .ReturnsAsync(@event);

             var ex = await Assert.ThrowsAsync<BadRequestException>(() =>  _fixture.Service.CreateAsync(booking, currentUserId, token));

            var expectedMessage = ErrorMessages.EventIsInactive;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowBadRequestException_WhenNotEnoughTicketsAvailable()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int currentUserId = 1;

            var booking = new BookingRequestCreateModel
            {
                BookedTicketsCount = 2,
                EventId = 1
            };

            var @event = new Event
            {
                IsActive = true,
                AvailableTickets = 1
            };



            _fixture.EventRepoMock
                .Setup(x => x.GetAsync(token, booking.EventId))
                .ReturnsAsync(@event);



            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.CreateAsync(booking, currentUserId, token));

            var expectedMessage = string.Format(ErrorMessages.NotEnoughTickets, @event.AvailableTickets);

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task CreateAsync_ShouldDecreaseEventTicketsAndAddBooking_WhenAllInputsAreValid()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int currentUserId = 1;

            var model = new BookingRequestCreateModel
            {
                BookedTicketsCount = 2,
                EventId = 1
            };

            var @event = new Event
            {
                Id = 1,
                IsActive = true,
                AvailableTickets = 5
            };

            var addedBooking = (Booking?)null;

            _fixture.EventRepoMock
                .Setup(x => x.GetAsync(token, model.EventId))
                .ReturnsAsync(@event);

            _fixture.SettingsMock
                .Setup(x => x.GetByKeyAsync(GlobalSettingsKeys.MaxTicketPerUser, token))
                .ReturnsAsync(10);

            _fixture.SettingsMock
                .Setup(x => x.GetByKeyAsync(GlobalSettingsKeys.BookingExpirationHours, token))
                .ReturnsAsync(1);

            _fixture.BookingRepoMock
                .Setup(x => x.AddAsync(token, It.IsAny<Booking>()))
                .Callback<CancellationToken, Booking>((_, b) => addedBooking = b)
                .Returns(Task.CompletedTask);


            _fixture.BookingRepoMock
              .Setup(x => x.GetUserBookingsWithEventAsync(currentUserId, token))
              .ReturnsAsync(new List<Booking>());

            var result = await _fixture.Service.CreateAsync(model, currentUserId, token);


            Assert.Equal(3, @event.AvailableTickets);

            Assert.NotNull(addedBooking);

            Assert.Equal(currentUserId, addedBooking.UserId);
            Assert.False(addedBooking.IsPurchased);
            Assert.Equal(model.EventId, addedBooking.EventId);
            Assert.Equal(model.BookedTicketsCount, addedBooking.BookedTicketsCount);

            Assert.True(addedBooking.CreatedAt <= DateTime.Now);
            Assert.True(addedBooking.ExpirationTime > DateTime.Now);

            _fixture.EventRepoMock.Verify(x => x.Update(@event), Times.Once);
            _fixture.BookingRepoMock.Verify(x => x.AddAsync(token, It.IsAny<Booking>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);
        }
        
        [Fact]
        public async Task CreateAsync_ShouldThrowBadRequestException_WhenUserTicketLimitReached()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int currentUserId = 1;
            int maxLimit = 5;

            var model = new BookingRequestCreateModel { BookedTicketsCount = 3, EventId = 1 };
            var @event = new Event { Id = 1, IsActive = true, AvailableTickets = 10 };

            var existingBookings = new List<Booking>
            {
               new Booking { EventId = 1, BookedTicketsCount = 3 }
            };
            int maxTicketUserCouldBook = Math.Max(0, maxLimit - existingBookings.Sum(x => x.BookedTicketsCount)); 

            _fixture.EventRepoMock
                .Setup(x => x.GetAsync(token, model.EventId))
                .ReturnsAsync(@event);

            _fixture.SettingsMock
                .Setup(x => x.GetByKeyAsync(GlobalSettingsKeys.MaxTicketPerUser, token))
                .ReturnsAsync(maxLimit);
            _fixture.SettingsMock
                .Setup(x => x.GetByKeyAsync(GlobalSettingsKeys.BookingExpirationHours, token))
                .ReturnsAsync(24);


            _fixture.BookingRepoMock.Setup(x => x.GetUserBookingsWithEventAsync(currentUserId, token)).ReturnsAsync(existingBookings);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.CreateAsync(model, currentUserId, token));

            var expectedMessage = string.Format(ErrorMessages.UserTicketLimitReached, maxTicketUserCouldBook);

            Assert.Equal(expectedMessage, ex.Message);
        }
        #endregion

        #region CleanupExpiredBookingsAsync Tests

        [Fact]
        public async Task CleanupExpiredBookingsAsync_ShouldRestoreTicketsAndDeleteBookings_WhenExpiredBookingsExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            var @event = new Event { AvailableTickets = 10 };
            var bookings = new List<Booking>
            {
                new Booking
                {
                    Id = 1,
                    ExpirationTime = DateTime.Now.AddMinutes(-1),
                    BookedTicketsCount = 3,
                    Event = @event
                },
                  new Booking
                {
                    Id = 2,
                    ExpirationTime = DateTime.Now.AddMinutes(-10),
                    BookedTicketsCount = 2,
                    Event = @event
                },
            };

            _fixture.BookingRepoMock
                .Setup(x => x.GetExpiredBookingsAsync(token)).ReturnsAsync(bookings);

            await _fixture.Service.CleanupExpiredBookingsAsync(token);

            Assert.Equal(15, @event.AvailableTickets);
            _fixture.BookingRepoMock.Verify(x => x.DeleteRange(bookings, token), Times.Once);
            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);

        }


        [Fact]
        public async Task CleanupExpiredBookingsAsync_ShouldDoNothing_WhenNoExpiredBookingsExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var emptyBookingsList = new List<Booking>();

            _fixture.BookingRepoMock
                .Setup(x => x.GetExpiredBookingsAsync(token))
                .ReturnsAsync(emptyBookingsList);

            await _fixture.Service.CleanupExpiredBookingsAsync(token);

            _fixture.BookingRepoMock.Verify(x => x.DeleteRange(It.IsAny<IEnumerable<Booking>>(), token), Times.Never);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Never);
        }
        #endregion

        #region GetUserBookingsAsync Tests

        [Fact]
        public async Task GetUserBookingsAsync_ShouldReturnEmptyList_WhenUserHasNoBookings()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var bookings = new List<Booking>();

            _fixture.BookingRepoMock
                .Setup(x => x.GetUserBookingsWithEventAsync(userId, token)).ReturnsAsync(bookings);

            var result = await _fixture.Service.GetUserBookingsAsync(userId, token);

            Assert.NotNull(result); 
            Assert.Empty(result);   

        }
        #endregion

        #region BuyAsync Tests
        [Fact]
        public async Task BuyAsync_ShouldThrowNotFoundException_WhenBookingDoesNotExists()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int userId = 1;

            _fixture.BookingRepoMock.Setup(x => x.GetAsync(token, bookingId)).ReturnsAsync((Booking?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.BuyAsync(bookingId,userId,token));

            var expectedMessage = ErrorMessages.BookingNotFound;

            Assert.Equal(expectedMessage, ex.Message);

            
        }

        [Fact]
        public async Task BuyAsync_ForbiddenException_WhenUserDoesNotOwnBooking()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int currentUserId = 1;


            var booking = new Booking { Id = bookingId, UserId = 2 };

            _fixture.BookingRepoMock.Setup(x => x.GetAsync(token, bookingId)).ReturnsAsync(booking);


            var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _fixture.Service.BuyAsync(bookingId, currentUserId, token));

            var expectedMessage = ErrorMessages.BookingPurchaseForbidden;
            Assert.Equal(expectedMessage, ex.Message);


        }

        [Fact]
        public async Task BuyAsync_BadRequestException_WhenBookingIsAlreadyPurchased()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int currentUserId = 5;

            var booking = new Booking
            {
                Id = bookingId,
                UserId = currentUserId,
                ExpirationTime = DateTime.Now.AddDays(1),
                IsPurchased = true
            };

            _fixture.BookingRepoMock
                .Setup(x => x.GetAsync(token, bookingId))
                .ReturnsAsync(booking);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.BuyAsync(bookingId, currentUserId, token));

            var expectedMessage = ErrorMessages.BookingAlreadyPurchased;

            Assert.Equal(expectedMessage, ex.Message);

        }

        [Fact]
        public async Task BuyAsync_BadRequestException_WhenBookingIsAlreadyExpired()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int currentUserId = 5;

            var booking = new Booking
            {
                Id = bookingId,
                ExpirationTime = DateTime.Now.AddMinutes(-1),
                UserId = currentUserId,
                IsPurchased = true
            };

            _fixture.BookingRepoMock
                .Setup(x => x.GetAsync(token, bookingId))
                .ReturnsAsync(booking);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.BuyAsync(bookingId, currentUserId, token));

            var expectedMessage = ErrorMessages.BookingExpired;

            Assert.Equal(expectedMessage, ex.Message);

        }

        [Fact]
        public async Task BuyAsync_ShouldSetBookingAsBought_WhenAllDataIsValid()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int bookingId = 1;
            int currentUserId = 5;

            var booking = new Booking
            {
                Id = bookingId,
                ExpirationTime = DateTime.Now.AddMinutes(1),
                UserId = currentUserId,
                IsPurchased = false,
            };

            _fixture.BookingRepoMock
                .Setup(x => x.GetAsync(token, bookingId))
                .ReturnsAsync(booking);

             await _fixture.Service.BuyAsync(bookingId, currentUserId, token);

            Assert.True(booking.IsPurchased);
            Assert.Equal(DateTime.MaxValue, booking.ExpirationTime);

            _fixture.BookingRepoMock.Verify(x => x.Update(booking), Times.Once);
            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);

        }




        #endregion
    }
}