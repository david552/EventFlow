using Asp.Versioning;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.GlobalSettings.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class GlobalSettingsController : ControllerBase
    {
        readonly IGlobalSettingsService _globalSettingaService;

        public GlobalSettingsController(IGlobalSettingsService globalSettingsService)
        {
            _globalSettingaService = globalSettingsService;
        }

        [HttpPost]
        public async Task<int> CreateAsync(GlobalSettingsRequestCreateModel model, CancellationToken token)
        {
            int id = await _globalSettingaService.CreateAsync(model, token);
            return id;
        }


        [HttpPut("{id}")]
        public async Task UpdateAsync(int id, GlobalSettingsRequestUpdateModel model, CancellationToken token)
        {
            await _globalSettingaService.UpdateAsync(id,model, token);
        
        }
    }
}
