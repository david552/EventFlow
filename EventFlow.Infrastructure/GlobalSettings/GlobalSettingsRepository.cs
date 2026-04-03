using EventFlow.Application;
using EventFlow.Application.GlobalSettings.Repositories;
using EventFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalSettingEntity = EventFlow.Domain.GlobalSettings.GlobalSettings;
namespace EventFlow.Infrastructure.GlobalSettings
{
    public class GlobalSettingsRepository : BaseRepository<GlobalSettingEntity>, IGlobalSettingsRepository
    {
        public GlobalSettingsRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async  Task<GlobalSettingEntity?> GetByKeyAsync(string key, CancellationToken token)
        {
            return await _dbSet.FirstOrDefaultAsync(x=>x.Key == key,token);
        }
    }
}
