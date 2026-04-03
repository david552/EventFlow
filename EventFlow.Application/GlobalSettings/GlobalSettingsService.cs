using EventFlow.Application.GlobalSettings.Repositories;
using EventFlow.Application.GlobalSettings.Requests;
using Mapster;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using GlobalSettingEntity = EventFlow.Domain.GlobalSettings.GlobalSettings;
namespace EventFlow.Application.GlobalSettings
{
    public class GlobalSettingsService : IGlobalSettingsService
    {
         readonly IGlobalSettingsRepository _repository;
         readonly IMemoryCache _memoryCache;
         readonly IUnitOfWork _unitOfWork;

        public GlobalSettingsService(IGlobalSettingsRepository repository, IMemoryCache memoryCache, IUnitOfWork unitOfWork)
        {
            _memoryCache = memoryCache;
            _repository = repository;
            _unitOfWork = unitOfWork;
        }
        public async Task<int> GetByKeyAsync(string key, CancellationToken token)
        {
            if(_memoryCache.TryGetValue(key,out int cachedValue))
            {
                return cachedValue;
            }
            var setting = await _repository.GetByKeyAsync(key,token);
            var value = setting?.Value ?? 0;
            _memoryCache.Set(key, value, TimeSpan.FromHours(1));
            return value;
        }

        public async Task UpdateAsync(int id, GlobalSettingsRequestUpdateModel model, CancellationToken token)
        {
            var setting = await _repository.GetAsync(token,id);

            if (setting == null)
                throw new Exception("Setting not found");

            setting.Value = model.Value;

            _repository.Update(setting);
            await _unitOfWork.SaveChanges(token);

            _memoryCache.Remove(setting.Key);

        }
        public async Task<int> CreateAsync(GlobalSettingsRequestCreateModel model, CancellationToken token)
        {
            var globalSettings = model.Adapt<GlobalSettingEntity>();
            await  _repository.AddAsync(token, globalSettings);
            await _unitOfWork.SaveChanges(token);
            return globalSettings.Id;

        }


    }
}
