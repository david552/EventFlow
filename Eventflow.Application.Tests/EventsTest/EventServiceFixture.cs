using EventFlow.Application;
using EventFlow.Application.Bookings;
using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Events;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.Events.Requests;
using EventFlow.Application.GlobalSettings;
using EventFlow.Domain.Events;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventflow.Application.Tests.EventsTest
{
    public class EventServiceFixture : IDisposable
    {
        public EventService Service { get; private set; }
        public Mock<IEventRepository> EventRepoMock { get; private set; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }
        public Mock<IGlobalSettingsService> SettingsMock { get; private set; }
        public Mock<ILogger<EventService>> LoggerMock { get; private set; }
        public Mock<IValidator<EventRequestCreateModel>> CreateModelValidatorMock { get; private set; }
        public Mock<IValidator<EventRequestUpdateModel>> UpdateModelValidatorMock { get; private set; }


        public EventServiceFixture()
        {
            EventRepoMock = new Mock<IEventRepository>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            SettingsMock = new Mock<IGlobalSettingsService>();
            LoggerMock = new Mock<ILogger<EventService>>();
            CreateModelValidatorMock = new Mock<IValidator<EventRequestCreateModel>>();
            UpdateModelValidatorMock = new Mock<IValidator<EventRequestUpdateModel>>();

            Service = new EventService(
                UnitOfWorkMock.Object,
                EventRepoMock.Object,
                SettingsMock.Object,
                LoggerMock.Object,
                CreateModelValidatorMock.Object,
                UpdateModelValidatorMock.Object
            );

        }


        public void Dispose()
        {
        }
    }
}
