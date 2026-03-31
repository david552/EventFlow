using EventFlow.Domain.Events;
using EventFlow.Domain.GlobalSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Persistence.Configuration
{
    public class GlobalSettingsConfiguration : IEntityTypeConfiguration<GlobalSettings>
    {
        public void Configure(EntityTypeBuilder<GlobalSettings> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Value).IsRequired();
            builder.HasIndex(x => x.Key).IsUnique();
        }
    }
}
