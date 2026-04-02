using EventFlow.Application.Users.Requests;
using EventFlow.Application.Users.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Users
{
    public interface IUserService
    {
        Task RegisterAsync(UserRegisterRequestModel model, CancellationToken token);
        Task<UserResponseModel> AuthenticationAsync(UserLoginRequestModel model, CancellationToken token);
        Task<UserResponseModel> GetByIdAsync(int id, CancellationToken token);
        Task<List<UserResponseModel>> GetAllUsersAsync(CancellationToken token);
        Task AssignModeratorRoleAsync(int userId, CancellationToken token);
        Task DeleteUserAsync(int id, CancellationToken token);
    }
}
