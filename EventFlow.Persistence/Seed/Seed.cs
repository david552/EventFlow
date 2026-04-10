using EventFlow.Domain.Constansts;
using EventFlow.Domain.Events;
using EventFlow.Domain.GlobalSettings;
using EventFlow.Domain.Users;
using EventFlow.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedGlobalSettingsAsync(context);
            await SeedEventsAsync(context, userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
        {
            string[] roleNames = { "Admin", "Moderator", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<User> userManager)
        {
            var usersToSeed = new List<(User User, string Password, string Role)>
            {
                (new User { UserName = "admin@eventflow.ge", Email = "admin@eventflow.ge", FirstName = "System", LastName = "Admin", EmailConfirmed = true }, "AdminPass123!", "Admin"),
                (new User { UserName = "david.moderator@gmail.com", Email = "david.moderator@gmail.com", FirstName = "David", LastName = "Piranishvili", EmailConfirmed = true }, "ModPass123!", "Moderator"),
                (new User { UserName = "nino.diasamidze@gmail.com", Email = "nino.diasamidze@gmail.com", FirstName = "Nino", LastName = "Diasamidze", EmailConfirmed = true }, "UserPass123!", "User"),
                (new User { UserName = "sandro.tech@gmail.com", Email = "sandro.tech@gmail.com", FirstName = "Sandro", LastName = "Beridze", EmailConfirmed = true }, "UserPass123!", "User")
            };

            foreach (var item in usersToSeed)
            {
                var existingUser = await userManager.FindByEmailAsync(item.User.Email);
                if (existingUser == null)
                {
                    var result = await userManager.CreateAsync(item.User, item.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(item.User, item.Role);
                    }
                }
            }
        }

        private static async Task SeedGlobalSettingsAsync(ApplicationDbContext context)
        {
            if (!await context.GlobalSettings.AnyAsync())
            {
                var settings = new List<GlobalSettings>
                {
                    new GlobalSettings { Key = GlobalSettingsKeys.EventEditAllowedDays, Value = 3 },
                    new GlobalSettings { Key = GlobalSettingsKeys.BookingExpirationHours, Value = 1 },
                    new GlobalSettings { Key = GlobalSettingsKeys.MaxTicketPerUser, Value = 5 },
                };

                await context.GlobalSettings.AddRangeAsync(settings);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedEventsAsync(ApplicationDbContext context, UserManager<User> userManager)
        {
            if (!await context.Events.AnyAsync())
            {
                var nino = await userManager.FindByEmailAsync("nino.diasamidze@gmail.com");
                var sandro = await userManager.FindByEmailAsync("sandro.tech@gmail.com");

                if (nino == null || sandro == null) return;

                var events = new List<Event>
                {
                    new Event
                    {
                        Title = "September 24 - Jazz Evening at Mushtaidi Park",
                        Description = "Get ready for an unforgettable autumn evening! Mushtaidi Park is hosting the finest jazz bands in Tbilisi. Enjoy wine tasting, live music, and a fantastic atmosphere.",
                        TotalTickets = 150,
                        AvailableTickets = 150,
                        StartTime = DateTime.Now.AddDays(10),
                        EndTime = DateTime.Now.AddDays(10).AddHours(4),
                        CreatedAt = DateTime.Now,
                        IsActive = true, 
                        UserId = nino.Id
                    },
                    new Event
                    {
                        Title = ".NET Architecture Masterclass",
                        Description = "An exclusive technical workshop for senior developers. We will dive deep into Clean Architecture, CQRS, and Microservices patterns. Seats are strictly limited.",
                        TotalTickets = 50,
                        AvailableTickets = 50,
                        StartTime = DateTime.Now.AddDays(20),
                        EndTime = DateTime.Now.AddDays(20).AddHours(6),
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        UserId = sandro.Id
                    },
                    new Event
                    {
                        Title = "Bakuriani Winter Festival 2026",
                        Description = "Celebrate the opening of the winter season in Bakuriani! Join us for electronic music, thrilling snowboard competitions, and a massive open-air party at Didveli.",
                        TotalTickets = 500,
                        AvailableTickets = 500,
                        StartTime = DateTime.Now.AddMonths(3),
                        EndTime = DateTime.Now.AddMonths(3).AddDays(2),
                        CreatedAt = DateTime.Now,
                        IsActive = false, 
                        UserId = nino.Id
                    }
                };

                await context.Events.AddRangeAsync(events);
                await context.SaveChangesAsync();
            }
        }
    }
}
