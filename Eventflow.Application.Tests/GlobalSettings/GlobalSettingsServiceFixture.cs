using EventFlow.Application;
using EventFlow.Application.Events;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.Events.Requests;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.GlobalSettings.Repositories;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventflow.Application.Tests.GlobalSettings
{
    public class GlobalSettingsServiceFixture : IDisposable
    {
        public GlobalSettingsService Service { get; private set; }
        public Mock<IGlobalSettingsRepository> SettingsRepoMock { get; private set; }
        public Mock<IMemoryCache> MemoryCacheMock { get; private set; }
        public Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }

        public GlobalSettingsServiceFixture()
        {
            SettingsRepoMock = new Mock<IGlobalSettingsRepository>();
            MemoryCacheMock = new Mock<IMemoryCache>();
            UnitOfWorkMock = new Mock<IUnitOfWork>();
            Service = new GlobalSettingsService(
                SettingsRepoMock.Object,
                MemoryCacheMock.Object,
                UnitOfWorkMock.Object
            );

        }
        public void Dispose()
        {
        }
    }
}
