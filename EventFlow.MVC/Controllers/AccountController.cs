using EventFlow.Application.Users;
using EventFlow.Application.Users.Requests;
using EventFlow.Domain.Users;
using EventFlow.MVC.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(IUserService userService, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, CancellationToken token)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var request = model.Adapt<UserLoginRequestModel>();
                var userResponse = await _userService.AuthenticationAsync(request, token);

                var user = await _userManager.FindByIdAsync(userResponse.Id.ToString());
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken token)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var request = model.Adapt<UserRegisterRequestModel>();
                await _userService.RegisterAsync(request, token);

                var user = await _userManager.FindByEmailAsync(model.Email);
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}