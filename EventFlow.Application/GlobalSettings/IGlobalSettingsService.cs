using EventFlow.Application.GlobalSettings.Requests;
using EventFlow.Application.GlobalSettings.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.GlobalSettings
{
    public interface IGlobalSettingsService
    {
        public Task UpdateAsync(int id, GlobalSettingsRequestUpdateModel model, CancellationToken token);
        public Task<int> GetByKeyAsync(string key, CancellationToken token);
        public Task<int> CreateAsync(GlobalSettingsRequestCreateModel model, CancellationToken token);
        public Task<List<GlobalSettingsResponseModel>> GetAllAsync(CancellationToken token);

    }
}
