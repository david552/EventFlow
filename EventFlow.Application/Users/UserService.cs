using EventFlow.Application.Users.Requests;
using EventFlow.Application.Users.Responses;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventFlow.Domain.Users;
using EventFlow.Application.Users.Repositories;
using Mapster;
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
                throw new Exception("User does not Exists");

            if(!await _userManager.IsInRoleAsync(user, "Moderator"))
            {
                var result = await _userManager.AddToRoleAsync(user, "Moderator");
                if (!result.Succeeded)
                    throw new Exception("Failed to assign Moderator role.");
            }
            
        }

        public async Task DeleteUserAsync(int id, CancellationToken token)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new Exception("Such User Does Not Exists");
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception($"Failed to delete user");
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
                throw new Exception("User Not Found");

            var response = user.Adapt<UserResponseModel>();
            response.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return response;
        }

        public async Task<UserResponseModel> AuthenticationAsync(UserLoginRequestModel model, CancellationToken token)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                throw new Exception("Invalid email or password"); 

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
            if (model.Password != model.ConfirmPassword)
                throw new Exception("Passwords do not match");

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                throw new Exception("User with this email already exists");

            var user = model.Adapt<User>();
            user.UserName = model.Username;

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            { 
                throw new Exception($"Registration failed");
            }

          
            await _userManager.AddToRoleAsync(user, "User");
        }
    }
}
