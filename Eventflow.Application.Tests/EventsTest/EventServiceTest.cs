using EventFlow.Application.Events.Requests;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Localization;
using EventFlow.Domain.Bookings;
using EventFlow.Domain.Constansts;
using EventFlow.Domain.Events;
using EventFlow.Domain.GlobalSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventflow.Application.Tests.EventsTest
{
    public class EventServiceTest
    {
        readonly EventServiceFixture _fixture;
        public EventServiceTest()
        {
            _fixture = new EventServiceFixture();
        }

        #region GetPagedVisibleAsync Tests
        [Fact]
        public async Task GetPagedVisibleAsync_ShouldReturnPagedList_WhenCalled()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int pageNumber = 2;
            int pageSize = 10;
            int totalCount = 25; 

            var eventsFromRepo = new List<Event>
            {
                new Event { Id = 1, Title = "Event 1", IsActive = true },
                new Event { Id = 2, Title = "Event 2", IsActive = true }
            };


            _fixture.EventRepoMock
                .Setup(x => x.GetVisiblePagedAsync(pageNumber, pageSize, token))
                .ReturnsAsync((eventsFromRepo, totalCount));

            var result = await _fixture.Service.GetPagedVisibleAsync(pageNumber, pageSize, token);

            Assert.NotNull(result);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(totalCount, result.TotalItemCount);

            Assert.Equal(eventsFromRepo.Count, result.Count);

            _fixture.EventRepoMock.Verify(x => x.GetVisiblePagedAsync(pageNumber, pageSize, token), Times.Once);
        }

        #endregion

        #region GetByIdAsync Tests
        [Fact]
         public async  Task GetByIdAsync_ShouldThrowNotFoundException_WhenEventDoesNotExists()
         {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int eventId = 1;

            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync((Event?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.GetByIdAsync(eventId, token));

            var expectedMessage = ErrorMessages.EventNotFound;

            Assert.Equal(expectedMessage, ex.Message);
         }


        #endregion


        #region UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFoundException_WhenEventDoesNotExists()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int eventId = 1;
            int currentUserId = 1;
            var model = new EventRequestUpdateModel();


            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync((Event?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.UpdateAsync(eventId,model,currentUserId, token));

            var expectedMessage = ErrorMessages.EventNotFound;

            Assert.Equal(expectedMessage, ex.Message);
        }


        [Fact]
        public async Task UpdateAsync_ShouldThrowForbiddenException_WhenUserIsNotEventCreator()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int eventId = 1;
            int currentUserId = 1;
            var model = new EventRequestUpdateModel();
            var @event = new Event()
            {
                UserId = 2
            };


            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync(@event);

            var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _fixture.Service.UpdateAsync(eventId, model, currentUserId, token));

            var expectedMessage = ErrorMessages.EventUpdateForbidden;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsBadRequestException_WhenEditPeriodExpired()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int eventId = 1;
            int currentUserId = 1;
            var model = new EventRequestUpdateModel();

            int allowedDaysForUpdate = 1;

            var @event = new Event()
            {
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UserId = 1
            };

            _fixture.SettingsMock.Setup(x => x.GetByKeyAsync(GlobalSettingsKeys.EventEditAllowedDays, token)).ReturnsAsync(allowedDaysForUpdate);
            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync(@event);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.UpdateAsync(eventId, model, currentUserId, token));

            var expectedMessage = string.Format(ErrorMessages.EventUpdatePeriodExpired, allowedDaysForUpdate); ;

            Assert.Equal(expectedMessage, ex.Message);
        }


        [Fact]
        public async Task UpdateAsync_UpdatesEvent_WhenAllDataIsCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            int eventId = 1;
            int currentUserId = 1;
            var model = new EventRequestUpdateModel()
            {
                AvailableTickets = 40
            };

            int allowedDaysForUpdate = 1;

            var @event = new Event()
            {
                TotalTickets = 100,
                CreatedAt = DateTime.UtcNow.AddDays(2),
                UserId = 1,
                AvailableTickets = 60
            };

            _fixture.SettingsMock.Setup(x => x.GetByKeyAsync(GlobalSettingsKeys.EventEditAllowedDays, token)).ReturnsAsync(allowedDaysForUpdate);
            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync(@event);

            int expectedEventTotalTicketsCount = @event.TotalTickets + (model.AvailableTickets - @event.AvailableTickets);

            await _fixture.Service.UpdateAsync(eventId, model, currentUserId, token);

            Assert.Equal(expectedEventTotalTicketsCount, @event.TotalTickets);

            _fixture.EventRepoMock.Verify(x => x.GetAsync(token,eventId), Times.Once);
            _fixture.SettingsMock.Verify(x => x.GetByKeyAsync(GlobalSettingsKeys.EventEditAllowedDays, token), Times.Once);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);

        }

        #endregion



        #region CreateAsync Tests
        [Fact]
        public async Task CreateAsync_CreateEvent_WhenAllDataIsCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var @event = (Event?)null;
            int userId = 1;
            var model = new EventRequestCreateModel()
            {
                Title = "Title",
                Description = "Description",
                TotalTickets = 10,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),

            };

            _fixture.EventRepoMock
              .Setup(x => x.AddAsync(token, It.IsAny<Event>()))
              .Callback<CancellationToken, Event>((_, b) => @event = b)
              .Returns(Task.CompletedTask);

            await _fixture.Service.CreateAsync(model, userId, token);

            Assert.NotNull(@event);
            Assert.Equal(userId, @event.UserId);
            Assert.False(@event.IsActive);
            Assert.Equal(model.TotalTickets, @event.AvailableTickets);

            _fixture.EventRepoMock.Verify(x => x.AddAsync(token, @event), Times.Once);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);

        }

        #endregion


        #region DeleteAsync Tests
        [Fact]
        public async Task DeleteAsync_ShouldThrowNotFoundException_WhenEventDoesNotExists()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int eventId = 1;

            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync((Event?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.DeleteAsync(eventId, token));

            var expectedMessage = ErrorMessages.EventNotFound;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteEvent_WhenAllDataIsCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int eventId = 1;

            var @event = new Event()
            {
                Id = eventId
            };

            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync(@event);

            await _fixture.Service.DeleteAsync(eventId, token);

            _fixture.EventRepoMock.Verify(x => x.Remove(@event), Times.Once);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);
        }

        #endregion


        #region DeactivateEndedEventsAsync Tests

        [Fact]
        public async Task DeactivateEndedEventsAsync_ShouldDoNothing_WhenNoExpiredEventsExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var emptyEventsList = new List<Event>();

            _fixture.EventRepoMock
                .Setup(x => x.GetExpiredEventsAsync(token))
                .ReturnsAsync(emptyEventsList);

            await _fixture.Service.DeactivateEndedEventsAsync(token);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Never);
        }

        [Fact]
        public async Task DeactivateEndedEventsAsync_ShouldDeactivateEventsAndSaveChanges_WhenExpiredEventsExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var expiredEvents = new List<Event>
            {
                new Event { Id = 1, IsActive = true },
                new Event { Id = 2, IsActive = true }
            };

            _fixture.EventRepoMock
                .Setup(x => x.GetExpiredEventsAsync(token))
                .ReturnsAsync(expiredEvents);

            await _fixture.Service.DeactivateEndedEventsAsync(token);

            Assert.False(expiredEvents[0].IsActive);
            Assert.False(expiredEvents[1].IsActive);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);
        }

        #endregion



        #region ActivateEvent Tests

        [Fact]
        public async Task ActivateEvent_ShouldThrowNotFoundException_WhenEventDoesNotExists()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int eventId = 1;

            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync((Event?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.ActivateEvent(eventId, token));

            var expectedMessage = ErrorMessages.EventNotFound;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task ActivateEvent_ShouldDoNothing_WhenEventIsAlreadyActive()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int eventId = 1;

            var @event = new Event
            {
                Id = eventId,
                IsActive = true 
            };

            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync(@event);

            await _fixture.Service.ActivateEvent(eventId, token);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Never);
        }

        [Fact]
        public async Task ActivateEvent_ShouldThrowBadRequestException_WhenEventIsExpired()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int eventId = 1;

            var @event = new Event
            {
                Id = eventId,
                IsActive = false,
                EndTime = DateTime.UtcNow.AddDays(-1) 
            };

            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync(@event);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.ActivateEvent(eventId, token));

            var expectedMessage = ErrorMessages.CannotActivateExpiredEvent;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task ActivateEvent_ShouldActivateEvent_WhenAllDataIsCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int eventId = 1;

            var @event = new Event
            {
                Id = eventId,
                IsActive = false,
                EndTime = DateTime.UtcNow.AddDays(5) 
            };

            _fixture.EventRepoMock.Setup(x => x.GetAsync(token, eventId)).ReturnsAsync(@event);

            await _fixture.Service.ActivateEvent(eventId, token);

            Assert.True(@event.IsActive);

            _fixture.UnitOfWorkMock.Verify(x => x.SaveChanges(token), Times.Once);
        }

        #endregion



        #region GetPendingEvents Tests

        [Fact]
        public async Task GetPendingEvents_ShouldReturnPendingEvents_WhenCalled()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var pendingEventsFromRepo = new List<Event>
            {
                new Event { Id = 1, Title = "Event 1", IsActive = false },
                new Event { Id = 2, Title = "Event 2", IsActive = false }
            };

            _fixture.EventRepoMock
                .Setup(x => x.GetPendingEventsAsync(token))
                .ReturnsAsync(pendingEventsFromRepo);

            var result = await _fixture.Service.GetPendingEvents(token);

            Assert.NotNull(result);
            Assert.Equal(pendingEventsFromRepo.Count, result.Count);

            _fixture.EventRepoMock.Verify(x => x.GetPendingEventsAsync(token), Times.Once);
        }

        #endregion

        #region GetEventsByUserIdAsync Tests

        [Fact]
        public async Task GetEventsByUserIdAsync_ShouldReturnUserEvents_WhenUserHasEvents()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var userEventsFromRepo = new List<Event>
            {
                new Event { Id = 1, UserId = userId, Title = "Event 1" },
                new Event { Id = 2, UserId = userId, Title = "Event 2" }
            };

            _fixture.EventRepoMock
                .Setup(x => x.GetByUserIdAsync(userId, token))
                .ReturnsAsync(userEventsFromRepo);

            var result = await _fixture.Service.GetEventsByUserIdAsync(userId, token);

            Assert.NotNull(result);
            Assert.Equal(userEventsFromRepo.Count, result.Count);

            _fixture.EventRepoMock.Verify(x => x.GetByUserIdAsync(userId, token), Times.Once);
        }

        #endregion
    }
}
