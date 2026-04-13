using EventFlow.Application.Exceptions;
using EventFlow.Application.Localization;
using EventFlow.Application.Users.Repositories;
using EventFlow.Application.Users.Requests;
using EventFlow.Application.Users.Responses;
using EventFlow.Domain.Users;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
        readonly ILogger<UserService> _logger;
        readonly IValidator<UserRegisterRequestModel> _registerModelValidator;
        readonly IValidator<UserLoginRequestModel> _loginModelValidator;
        public UserService(UserManager<User> userManager, IUserRepository repository, ILogger<UserService> logger, IValidator<UserRegisterRequestModel> registerModelValidator, IValidator<UserLoginRequestModel> loginModelValidator)
        {
            _repository = repository;
            _userManager = userManager;
            _logger = logger;
            _registerModelValidator = registerModelValidator;
            _loginModelValidator = loginModelValidator;
        }
        public async Task AssignModeratorRoleAsync(int userId, CancellationToken token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException(ErrorMessages.UserNotFound, "UserNotFound");

            if(!await _userManager.IsInRoleAsync(user, "Moderator"))
            {

                var result = await _userManager.AddToRoleAsync(user, "Moderator");
                if (!result.Succeeded)
                    throw new BadRequestException(ErrorMessages.RoleAssignmentFailed, "RoleAssignmentFailed");
                _logger.LogInformation("Assigned Moderator role to user {UserId}", userId); 
            }
            
        }
        public async Task RemoveModeratorRoleAsync(int userId, CancellationToken token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException(ErrorMessages.UserNotFound, "UserNotFound");

            if (await _userManager.IsInRoleAsync(user, "Moderator"))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, "Moderator");
                if (!result.Succeeded)
                    throw new BadRequestException(ErrorMessages.RoleRemovalFailed, "RoleRemovalFailed");

                _logger.LogInformation("Removed Moderator role from user {UserId}", userId);
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
                throw new NotFoundException(ErrorMessages.UserNotFound, "UserNotFound");

            var response = user.Adapt<UserResponseModel>();
            response.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return response;
        }

        public async Task<UserResponseModel> AuthenticationAsync(UserLoginRequestModel model, CancellationToken token)
        {
            await _loginModelValidator.ValidateAndThrowAsync(model, token);
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                throw new BadRequestException(ErrorMessages.InvalidCredentials, "InvalidCredentials"); 

            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation("User {UserId} successfully logged in", user.Id);
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
            await _registerModelValidator.ValidateAndThrowAsync(model, token);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                throw new BadRequestException(ErrorMessages.EmailAlreadyExists, "EmailAlreadyExists");
            }
            var user = model.Adapt<User>();
            user.UserName = model.Username;

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {

                if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                    throw new BadRequestException(ErrorMessages.UsernameAlreadyExists, "UsernameAlreadyExists");

                throw new BadRequestException(ErrorMessages.RegistrationFailed, "RegistrationFailed");
            }

            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("New user registered successfully with ID {UserId} and email {Email}", user.Id, user.Email);
        }

    }
}
