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

namespace Eventflow.Application.Tests.UserTests
{
    public class UserServiceFixture : IDisposable
    {
        public UserService Service { get; private set; }
        public Mock<UserManager<User>> UserManager { get; private set; }
        public Mock<IUserRepository> UserRepoMock { get; private set; }
        public Mock<ILogger<UserService>> LoggerMock { get; private set; }
        public Mock<IValidator<UserRegisterRequestModel>> RegisterValidatorMock { get; private set; }
        public Mock<IValidator<UserLoginRequestModel>> LogInModelValidatorMock { get; private set; }

        public UserServiceFixture()
        {
            var storeMock = new Mock<IUserStore<User>>();
            UserRepoMock = new Mock<IUserRepository>();
            LoggerMock = new Mock<ILogger<UserService>>();
            UserManager = new Mock<UserManager<User>>(storeMock.Object,null, null, null, null, null, null, null, null); RegisterValidatorMock = new Mock<IValidator<UserRegisterRequestModel>>();
            LogInModelValidatorMock = new Mock<IValidator<UserLoginRequestModel>>();

            Service = new UserService(
                UserManager.Object,
                UserRepoMock.Object,
                LoggerMock.Object,
                RegisterValidatorMock.Object,
                LogInModelValidatorMock.Object

                );

        }

        public void Dispose()
        {
        }
    }
}
