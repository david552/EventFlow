using EventFlow.Application.Bookings;
using EventFlow.Application.Bookings.Requests;
using EventFlow.Application.Events;
using EventFlow.Application.Events.Requests;
using EventFlow.MVC.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventFlow.MVC.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public EventController(IEventService eventService, IBookingService bookingService)
        {
            _eventService = eventService;
            _bookingService = bookingService;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventCreateViewModel model, CancellationToken token)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var request = model.Adapt<EventRequestCreateModel>();

                await _eventService.CreateAsync(request, CurrentUserId, token);

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (FluentValidation.ValidationException ex) 
            {
                foreach (var error in ex.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken token)
        {
            var @event = await _eventService.GetByIdAsync(id, token);
            if (@event == null) return NotFound();

            return View(@event.Adapt<EventUpdateViewModel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventUpdateViewModel model, CancellationToken token)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var request = model.Adapt<EventRequestUpdateModel>();
                await _eventService.UpdateAsync(id, request, CurrentUserId, token);
                return RedirectToAction("Details", "Home", new { id });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return BadRequest(ex.Message);

                TempData["ErrorMessage"] = ex.Message;
                string returnUrl = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);

                return RedirectToAction("Details", "Home", new { id = id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int eventId, int quantity, CancellationToken token)
        {
            try
            {
                var bookingId = await _bookingService.CreateAsync(new BookingRequestCreateModel
                {
                    EventId = eventId,
                    BookedTicketsCount = quantity
                }, CurrentUserId, token);

                await _bookingService.BuyAsync(bookingId, CurrentUserId, token);
                TempData["SuccessMessage"] = "Tickets purchased successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok();

            string returnUrl = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("MyBookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int eventId, int quantity, CancellationToken token)
        {
            try
            {
                await _bookingService.CreateAsync(new BookingRequestCreateModel
                {
                    EventId = eventId,
                    BookedTicketsCount = quantity
                }, CurrentUserId, token);

                TempData["SuccessMessage"] = "Ticket booked successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok();

            string returnUrl = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("MyBookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId, CancellationToken token)
        {
            try
            {
                await _bookingService.CancelAsync(bookingId, CurrentUserId, token);
                TempData["SuccessMessage"] = "Booking canceled and tickets returned to the database.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings(CancellationToken token)
        {
            var allBookings = await _bookingService.GetUserBookingsAsync(CurrentUserId, token);
            var pendingOnly = allBookings.Where(x => !x.IsPurchased).ToList();
            return View(pendingOnly);
        }

        [HttpGet]
        public async Task<IActionResult> Orders(CancellationToken token)
        {
            var allBookings = await _bookingService.GetUserBookingsAsync(CurrentUserId, token);
            var purchasedOnly = allBookings.Where(x => x.IsPurchased).ToList();
            return View(purchasedOnly);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPurchase(int bookingId, CancellationToken token)
        {
            try
            {
                await _bookingService.BuyAsync(bookingId, CurrentUserId, token);

                TempData["SuccessMessage"] = "Ticket purchased successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> MyEvents(CancellationToken token)
        {
            var myEvents = await _eventService.GetEventsByUserIdAsync(CurrentUserId, token);

            return View(myEvents);
        }
    }
}