using EventFlow.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application.Users.Repositories
{
    public interface IUserRepository : IBaseRepository<User>
    {

        Task<User?> GetByEmailAsync(string email, CancellationToken token);

        Task<User?> GetUserWithBookingsAsync(int id, CancellationToken token);

        Task<User?> GetUserWithCreatedEventsAsync(int id, CancellationToken token);

        Task<List<User>> GetAllAsync(CancellationToken token);



    }
}
