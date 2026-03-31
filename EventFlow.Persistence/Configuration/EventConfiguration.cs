using EventFlow.Domain.Bookings;
using EventFlow.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Persistence.Configuration
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TotalTickets).IsRequired();
            builder.Property(x => x.AvailableTickets).IsRequired();
            builder.Property(x => x.StartTime).IsRequired();
            builder.Property(x => x.EndTime).IsRequired();

            builder.Property(x => x.Description).IsRequired();
            builder.Property(x => x.Title).IsRequired();
            builder.Property(x => x.Title).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasMany(x => x.Bookings)
                .WithOne(x => x.Event)
                .OnDelete(DeleteBehavior.Cascade); 


        }
    }
}
