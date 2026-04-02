using EventFlow.Application;
using EventFlow.Application.Bookings;
using EventFlow.Application.Bookings.Repositories;
using EventFlow.Application.Events;
using EventFlow.Application.Events.Repositories;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.GlobalSettings.Repositories;
using EventFlow.Application.Users;
using EventFlow.Application.Users.Repositories;
using EventFlow.Infrastructure;
using EventFlow.Infrastructure.Bookings;
using EventFlow.Infrastructure.Events;
using EventFlow.Infrastructure.GlobalSettings;
using EventFlow.Infrastructure.Users;

namespace EventFlow.API.infrastructures.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGlobalSettingsService, GlobalSettingsService>();
            services.AddScoped<IGlobalSettingsRepository, GlobalSettingsRepository>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();



            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        }
    }
}
