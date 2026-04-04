using EventFlow.Application.Exceptions;
using EventFlow.Application.Users.Repositories;
using EventFlow.Application.Users.Requests;
using EventFlow.Application.Users.Responses;
using EventFlow.Domain.Users;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EventFlow.Application.Users
{
    public class UserService : IUserService
    {
        readonly UserManager<User> _userManager;
        readonly IUserRepository _repository;

        public UserService(UserManager<User> userManager, IUserRepository repository)
        {
            _repository = repository;
            _userManager = userManager;
        }
        public async Task AssignModeratorRoleAsync(int userId, CancellationToken token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException("User not found", "UserNotFound");

            if(!await _userManager.IsInRoleAsync(user, "Moderator"))
            {
                var result = await _userManager.AddToRoleAsync(user, "Moderator");
                if (!result.Succeeded)
                    throw new BadRequestException("Failed to assign Moderator role", "RoleAssignmentFailed");
            }
            
        }

        public async Task DeleteUserAsync(int id, CancellationToken token)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new NotFoundException("User not found", "UserNotFound");
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new BadRequestException($"Failed to delete user", "UserDeletionFailed");
            }

        }

        public async Task<List<UserResponseModel>> GetAllUsersAsync(CancellationToken token)
        {
            var users = await _repository.GetAllAsync(token);

            var usersToReturn = new List<UserResponseModel>();
            foreach(var user in users)
            {
                var userToReturn = user.Adapt<UserResponseModel>();
                userToReturn.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                usersToReturn.Add(userToReturn);
            }
            return usersToReturn;
        }

        public async Task<UserResponseModel> GetByIdAsync(int userId, CancellationToken token)
        {
            var user = await _repository.GetAsync(token, userId);

            if (user == null)
                throw new NotFoundException("User Not Found", "UserNotFound");

            var response = user.Adapt<UserResponseModel>();
            response.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return response;
        }

        public async Task<UserResponseModel> AuthenticationAsync(UserLoginRequestModel model, CancellationToken token)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                throw new BadRequestException("Invalid email or password", "InvalidCredentials"); 

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponseModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList()
            };
        }

        public async Task RegisterAsync(UserRegisterRequestModel model, CancellationToken token)
        {

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                throw new BadRequestException("User with this email already exists", "EmailAlreadyExists");

            var user = model.Adapt<User>();
            user.UserName = model.Username;

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            { 
                throw new BadRequestException($"Registration failed", "RegistrationFailed");
            }

          
            await _userManager.AddToRoleAsync(user, "User");
        }
    }
}
