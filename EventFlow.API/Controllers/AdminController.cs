using EventFlow.Application.GlobalSettings;
using EventFlow.Application.GlobalSettings.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        readonly IGlobalSettingsService _globalSettingaService;

        public AdminController(IGlobalSettingsService globalSettingsService)
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
