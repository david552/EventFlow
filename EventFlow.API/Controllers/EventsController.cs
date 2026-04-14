using Asp.Versioning;
using EventFlow.Application.Events;
using EventFlow.Application.Events.Requests;
using EventFlow.Application.Events.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventFlow.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IEnumerable<EventResponseModel>> GetEvents([FromQuery] int page = 1,[FromQuery] int pageSize = 6,CancellationToken token = default)
        {
            var pagedResult = await _eventService.GetPagedVisibleAsync(page, pageSize, token);

         
            return pagedResult;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<EventResponseModel> GetById(int id, CancellationToken token)
        {
                var @event = await _eventService.GetByIdAsync(id, token);
                return @event;
        }

        [HttpPost]
        [Authorize]
        public async Task<int> Create(EventRequestCreateModel model, CancellationToken token)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdString);
            var eventId = await _eventService.CreateAsync(model, userId, token);
            return eventId;
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task Update(int id,  EventRequestUpdateModel model, CancellationToken token)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUserId = int.Parse(userIdClaim);
            await _eventService.UpdateAsync(id, model, CurrentUserId, token);

        }

        [HttpPut("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task SetEventAsActive(int id, CancellationToken token)
        {

            await _eventService.ActivateEvent(id, token);

        }





        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task Delete(int id, CancellationToken token)
        {
            await _eventService.DeleteAsync(id, token);

        }
    }
}
