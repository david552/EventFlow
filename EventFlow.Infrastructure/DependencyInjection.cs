using EventFlow.Application;
using EventFlow.Application.Bookings;
using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Events;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.GlobalSettings.Repositories;
using EventFlow.Application.Users;
using EventFlow.Application.Users.Repositories;
using EventFlow.Infrastructure.Bookings;
using EventFlow.Infrastructure.Events;
using EventFlow.Infrastructure.GlobalSettings;
using EventFlow.Infrastructure.Users;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGlobalSettingsRepository, GlobalSettingsRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            return services;
        }
    }
}
