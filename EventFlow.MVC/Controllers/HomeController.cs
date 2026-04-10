using EventFlow.Application.Events;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using X.PagedList.Extensions;

namespace EventFlow.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventService _eventService;

        public HomeController(IEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index(int? page, CancellationToken token)
        {
            int pageSize = 6;
            int pageNumber = page ?? 1;

            var pagedEvents = await _eventService.GetPagedVisibleAsync(pageNumber, pageSize, token);

            return View(pagedEvents);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Details(int id, CancellationToken token)
        {
            var @event = await _eventService.GetByIdAsync(id, token);
            return View(@event);
        }
    }
}