using EventFlow.Application;
using EventFlow.Application.Bookings;
using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.GlobalSettings;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Eventflow.Application.Tests.BookingsServiceTest.BookingsTest
{
    public class BookingServiceFixture : IDisposable
    {
        public BookingService Service { get; private set; }
        public Mock<IBookingRepository> BookingRepoMock { get; private set; }
        public Mock<IEventRepository> EventRepoMock { get; private set; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }
        public Mock<IGlobalSettingsService> SettingsMock { get; private set; }
        public Mock<ILogger<BookingService>> LoggerMock { get; private set; }
        public Mock<IValidator<BookingRequestCreateModel>> ValidatorMock { get; private set; }

        public BookingServiceFixture()
        {
            BookingRepoMock = new Mock<IBookingRepository>();
            EventRepoMock = new Mock<IEventRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            SettingsMock = new Mock<IGlobalSettingsService>();
            LoggerMock = new Mock<ILogger<BookingService>>();
            ValidatorMock = new Mock<IValidator<BookingRequestCreateModel>>();

            Service = new BookingService(
                BookingRepoMock.Object,
                EventRepoMock.Object,
                UnitOfWorkMock.Object,
                SettingsMock.Object,
                LoggerMock.Object,
                ValidatorMock.Object
            );

        }


        public void Dispose()
        {
        }
    }
}

