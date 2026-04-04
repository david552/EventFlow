using EventFlow.Application.Bookings;
using EventFlow.Application.Bookings.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;

namespace EventFlow.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<int> CreateAsync(BookingRequestCreateModel model,CancellationToken token)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserId = int.Parse(userIdClaim);
            var bookingId = await _bookingService.CreateAsync(model,currentUserId, token);
            return bookingId;
        }

        [HttpPut("{id}/buy")]
        public async Task BuyAsync(int id, CancellationToken token)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserId = int.Parse(userIdClaim);
            await _bookingService.BuyAsync(id,currentUserId, token);
        }

        [HttpDelete("{id}/cancel")]
        public async Task CancelAsync(int id, CancellationToken token)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserId = int.Parse(userIdClaim);

            await _bookingService.CancelAsync(id, currentUserId, token);

        }

    }
}
