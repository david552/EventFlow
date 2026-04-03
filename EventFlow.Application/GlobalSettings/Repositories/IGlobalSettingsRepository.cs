using EventFlow.Domain.GlobalSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalSettingEntity = EventFlow.Domain.GlobalSettings.GlobalSettings;
namespace EventFlow.Application.GlobalSettings.Repositories
{

    public interface IGlobalSettingsRepository : IBaseRepository<GlobalSettingEntity>
    {
        Task<GlobalSettingEntity?> GetByKeyAsync(string key, CancellationToken token);
    }
}
