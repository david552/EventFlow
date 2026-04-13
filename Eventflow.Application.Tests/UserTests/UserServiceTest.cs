using EventFlow.Application;
using EventFlow.Application.Exceptions;
using EventFlow.Application.Localization;
using EventFlow.Application.Users;
using EventFlow.Application.Users.Repositories;
using EventFlow.Application.Users.Requests;
using EventFlow.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Eventflow.Application.Tests.UserTests
{
    public class UserServiceTest 
    {
        readonly UserServiceFixture _fixture;

        public UserServiceTest()
        {
            _fixture = new UserServiceFixture();
        }


        #region AssignModeratorRoleAsync Tests

        [Fact]
        public async Task AssignModeratorRoleAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            _fixture.UserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.AssignModeratorRoleAsync(userId, token));

            var expectedMessage = ErrorMessages.UserNotFound;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task AssignModeratorRoleAsync_ShouldDoNothing_WhenUserIsAlreadyModerator()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var user = new User { Id = userId };

            _fixture.UserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.IsInRoleAsync(user, "Moderator")).ReturnsAsync(true);

            await _fixture.Service.AssignModeratorRoleAsync(userId, token);

            _fixture.UserManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignModeratorRoleAsync_ShouldThrowBadRequestException_WhenRoleAssignmentFails()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var user = new User { Id = userId };

            _fixture.UserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.IsInRoleAsync(user, "Moderator")).ReturnsAsync(false);

            _fixture.UserManager.Setup(x => x.AddToRoleAsync(user, "Moderator")).ReturnsAsync(IdentityResult.Failed());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.AssignModeratorRoleAsync(userId, token));

            var expectedMessage = ErrorMessages.RoleAssignmentFailed;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task AssignModeratorRoleAsync_ShouldAssignRole_WhenAllDataIsCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var user = new User { Id = userId };

            _fixture.UserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.IsInRoleAsync(user, "Moderator")).ReturnsAsync(false);

            _fixture.UserManager.Setup(x => x.AddToRoleAsync(user, "Moderator")).ReturnsAsync(IdentityResult.Success);

            await _fixture.Service.AssignModeratorRoleAsync(userId, token);

            _fixture.UserManager.Verify(x => x.AddToRoleAsync(user, "Moderator"), Times.Once);
        }

        #endregion

        #region RemoveModeratorRoleAsync Tests

        [Fact]
        public async Task RemoveModeratorRoleAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            _fixture.UserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.RemoveModeratorRoleAsync(userId, token));

            var expectedMessage = ErrorMessages.UserNotFound;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task RemoveModeratorRoleAsync_ShouldDoNothing_WhenUserIsNotModerator()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var user = new User { Id = userId };

            _fixture.UserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.IsInRoleAsync(user, "Moderator")).ReturnsAsync(false); 

            await _fixture.Service.RemoveModeratorRoleAsync(userId, token);

            _fixture.UserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RemoveModeratorRoleAsync_ShouldThrowBadRequestException_WhenRoleRemovalFails()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var user = new User { Id = userId };

            _fixture.UserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.IsInRoleAsync(user, "Moderator")).ReturnsAsync(true); 

            _fixture.UserManager.Setup(x => x.RemoveFromRoleAsync(user, "Moderator")).ReturnsAsync(IdentityResult.Failed());

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.RemoveModeratorRoleAsync(userId, token));

            var expectedMessage = ErrorMessages.RoleRemovalFailed;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task RemoveModeratorRoleAsync_ShouldRemoveRole_WhenAllDataIsCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var user = new User { Id = userId };

            _fixture.UserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.IsInRoleAsync(user, "Moderator")).ReturnsAsync(true);

            _fixture.UserManager.Setup(x => x.RemoveFromRoleAsync(user, "Moderator")).ReturnsAsync(IdentityResult.Success);

            await _fixture.Service.RemoveModeratorRoleAsync(userId, token);

            _fixture.UserManager.Verify(x => x.RemoveFromRoleAsync(user, "Moderator"), Times.Once);
        }

        #endregion

        #region GetAllUsersAsync Tests

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var emptyUsersList = new List<User>();

            _fixture.UserRepoMock.Setup(x => x.GetAllAsync(token)).ReturnsAsync(emptyUsersList);

            var result = await _fixture.Service.GetAllUsersAsync(token);

            Assert.Empty(result);
            _fixture.UserRepoMock.Verify(x => x.GetAllAsync(token), Times.Once);
            _fixture.UserManager.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnMappedUsersWithRoles_WhenUsersExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var user1 = new User { Id = 1, FirstName = "name 1", Email = "name1@gmail.com" };
            var user2 = new User { Id = 2, FirstName = "name 2", Email = "name2@gmail.com" };
            var usersFromRepo = new List<User> { user1, user2 };

            var user1Roles = new List<string> { "Admin", "User" };
            var user2Roles = new List<string> { "Moderator" };

            _fixture.UserRepoMock.Setup(x => x.GetAllAsync(token)).ReturnsAsync(usersFromRepo);
            _fixture.UserManager.Setup(x => x.GetRolesAsync(user1)).ReturnsAsync(user1Roles);
            _fixture.UserManager.Setup(x => x.GetRolesAsync(user2)).ReturnsAsync(user2Roles);

            var result = await _fixture.Service.GetAllUsersAsync(token);


            Assert.Equal(usersFromRepo.Count, result.Count);
            Assert.Equal(user1Roles, result.First(x => x.Id == user1.Id).Roles);
            Assert.Equal(user2Roles, result.First(x => x.Id == user2.Id).Roles);

            _fixture.UserRepoMock.Verify(x => x.GetAllAsync(token), Times.Once);
            _fixture.UserManager.Verify(x => x.GetRolesAsync(user1), Times.Once);
            _fixture.UserManager.Verify(x => x.GetRolesAsync(user2), Times.Once);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            _fixture.UserRepoMock.Setup(x => x.GetAsync(token, userId)).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _fixture.Service.GetByIdAsync(userId, token));

            var expectedMessage = ErrorMessages.UserNotFound;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMappedUserWithRoles_WhenUserExists()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;
            int userId = 1;

            var user = new User { Id = userId, FirstName = "name 1", Email = "name1@gmail.com" };
            var expectedRoles = new List<string> { "Admin", "User" };

            _fixture.UserRepoMock.Setup(x => x.GetAsync(token, userId)).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(expectedRoles);

            var result = await _fixture.Service.GetByIdAsync(userId, token);

            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(expectedRoles, result.Roles);

            _fixture.UserRepoMock.Verify(x => x.GetAsync(token, userId), Times.Once);
            _fixture.UserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
        }

        #endregion

        #region AuthenticationAsync Tests
        [Fact]
        public async Task AuthenticationAsync_ShouldThrowBadRequestException_WhenEmailIsNotCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var model = new UserLoginRequestModel { Email = "user@gmail.com", Password = "Password" };
            var user = new User { Id = 1, Email = model.Email };

            _fixture.UserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.AuthenticationAsync(model, token));

            var expectedMessage = ErrorMessages.InvalidCredentials;

            Assert.Equal(expectedMessage, ex.Message);
        }
        [Fact]
        public async Task AuthenticationAsync_ShouldThrowBadRequestException_WhenPasswordIsNotCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var model = new UserLoginRequestModel { Email = "user@gmail.com", Password = "Password" };
            var user = new User { Id = 1, Email = model.Email };

            _fixture.UserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.CheckPasswordAsync(user, model.Password)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.AuthenticationAsync(model, token));

            var expectedMessage = ErrorMessages.InvalidCredentials;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task AuthenticationAsync_ShouldReturnUserResponseModel_WhenCredentialsAreValid()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var model = new UserLoginRequestModel { Email = "user@gmail.com", Password = "Password" };
            var user = new User { Id = 1, Email = model.Email, FirstName = "FirstName", LastName = "LastName" };
            var expectedRoles = new List<string> { "User" };

            _fixture.UserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _fixture.UserManager.Setup(x => x.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
            _fixture.UserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(expectedRoles);

            var result = await _fixture.Service.AuthenticationAsync(model, token);

            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.FirstName, result.FirstName);
            Assert.Equal(user.LastName, result.LastName);
            Assert.Equal(expectedRoles, result.Roles);

            _fixture.UserManager.Verify(x => x.FindByEmailAsync(model.Email), Times.Once);
            _fixture.UserManager.Verify(x => x.CheckPasswordAsync(user, model.Password), Times.Once);
            _fixture.UserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_ShouldThrowBadRequestException_WhenEmailAlreadyExists()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var model = new UserRegisterRequestModel { Email = "user@gmail.com", Username = "Username", Password = "Password" };
            var existingUser = new User { Id = 1, Email = model.Email };

            _fixture.RegisterValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<UserRegisterRequestModel>>(), token))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _fixture.UserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(existingUser);

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.RegisterAsync(model, token));

            var expectedMessage = ErrorMessages.EmailAlreadyExists;

            Assert.Equal(expectedMessage, ex.Message);
            _fixture.UserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowBadRequestException_WhenUsernameAlreadyExists()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var model = new UserRegisterRequestModel { Email = "user@gmail.com", Username = "Username", Password = "Password" };



            _fixture.UserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);

            var identityError = new IdentityError { Code = "DuplicateUserName", Description = "Username is already taken." };

            _fixture.UserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), model.Password)).ReturnsAsync(IdentityResult.Failed(identityError));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.RegisterAsync(model, token));

            var expectedMessage = ErrorMessages.UsernameAlreadyExists;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowBadRequestException_WhenRegistrationFails()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var model = new UserRegisterRequestModel { Email = "user@gmail.com", Username = "Username", Password = "Password" };

            _fixture.RegisterValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<UserRegisterRequestModel>>(), token))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _fixture.UserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);

            var identityError = new IdentityError { Code = "Error", Description = "Error" };

            _fixture.UserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), model.Password)).ReturnsAsync(IdentityResult.Failed(identityError));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _fixture.Service.RegisterAsync(model, token));

            var expectedMessage = ErrorMessages.RegistrationFailed;

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldRegisterUserAndAssignRole_WhenAllDataIsCorrect()
        {
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var model = new UserRegisterRequestModel { Email = "user@gmail.com", Username = "Username", Password = "Password" };
            var createdUser = (User?)null;

            _fixture.UserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);

            _fixture.UserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), model.Password))
                .Callback<User, string>((u, p) => createdUser = u)
                .ReturnsAsync(IdentityResult.Success);

            _fixture.UserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            await _fixture.Service.RegisterAsync(model, token);

            Assert.Equal(model.Email, createdUser.Email);
            Assert.Equal(model.Username, createdUser.UserName);

            _fixture.UserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), model.Password), Times.Once);
            _fixture.UserManager.Verify(x => x.AddToRoleAsync(createdUser, "User"), Times.Once);
        }

        #endregion
    }
}
