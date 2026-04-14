using Asp.Versioning;
using EventFlow.API.infrastructures.JWT;
using EventFlow.Application.Users;
using EventFlow.Application.Users.Requests;
using EventFlow.Application.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
namespace EventFlow.API.Controllers
{
    [ApiVersion("1.0")] 
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOptions<JWTConfiguration> _options;

        public AccountController(IUserService userService, IOptions<JWTConfiguration> options)
        {
            _userService = userService;
            _options = options;
        }

        [HttpPost("register")]
        public async Task<string> Register(UserRegisterRequestModel model, CancellationToken token)
        {
            await _userService.RegisterAsync(model, token);
            return "User registered successfully!";
        }

        [HttpPost("login")]
        public async Task<string> Login(UserLoginRequestModel model, CancellationToken token)
        {
            var userModel = await _userService.AuthenticationAsync(model, token);

            return JWTHelper.GenerateSecurityToken(userModel, _options); 
        }



        [HttpGet("users")]
        [Authorize(Roles = "Admin")] 
        public async Task<List<UserResponseModel>> GetAllUsers(CancellationToken token)
        {
            var users = await _userService.GetAllUsersAsync(token);
            return users;
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<UserResponseModel> GetUserById(int id, CancellationToken token)
        {
            var user = await _userService.GetByIdAsync(id, token);
            return user;
        }

        [HttpPut("users/{id}/moderator")]
        [Authorize(Roles = "Admin")] 
        public async Task AssignModeratorRole(int id,  CancellationToken token)
        {
            await _userService.AssignModeratorRoleAsync(id, token);
        }


    }
}

