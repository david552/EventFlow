using EventFlow.Application.Events;
using EventFlow.Application.GlobalSettings;
using EventFlow.Application.GlobalSettings.Requests;
using EventFlow.Application.Users;
using EventFlow.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.MVC.Controllers
{
    [Authorize(Roles = "Admin, Moderator")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEventService _eventService;
        private readonly IGlobalSettingsService _settingsService;

        public AdminController(IUserService userService, IEventService eventService, IGlobalSettingsService settingsService)
        {
            _userService = userService;
            _eventService = eventService;
            _settingsService = settingsService;
        }

    
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users(CancellationToken token)
        {
            var users = await _userService.GetAllUsersAsync(token);
            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MakeModerator(int id, CancellationToken token)
        {
            await _userService.AssignModeratorRoleAsync(id, token);
            TempData["SuccessMessage"] = "User successfully promoted to Moderator!";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> PendingEvents(CancellationToken token)
        {
            var allEvents = await _eventService.GetPendingEvents(token);
            return View(allEvents);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveEvent(int id, CancellationToken token)
        {
            await _eventService.ActivateEvent(id, token);
            TempData["SuccessMessage"] = "Event approved successfully!";
            return RedirectToAction(nameof(PendingEvents));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Settings(CancellationToken token)
        {
            var settings = await _settingsService.GetAllAsync(token);
            return View(new SettingsViewModel { Settings = settings });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Settings(SettingsViewModel model, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                foreach (var setting in model.Settings)
                {
                    var request = new GlobalSettingsRequestUpdateModel { Value = setting.Value };
                    await _settingsService.UpdateAsync(setting.Id, request, token);
                }

                TempData["SuccessMessage"] = "All settings have been successfully updated!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RevokeModerator(int id, CancellationToken token)
        {
            try
            {
                await _userService.RemoveModeratorRoleAsync(id, token);
                TempData["SuccessMessage"] = "Moderator role has been revoked!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Users));
        }
    }
}