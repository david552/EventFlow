using EventFlow.Application.Users.Repositories;
using EventFlow.Domain.Users;
using EventFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Users
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }


        public async Task<User?> GetUserWithBookingsAsync(int id, CancellationToken token)
        {
            return await _dbSet
                .Include(u => u.Bookings)
                .ThenInclude(b => b.Event)
                .FirstOrDefaultAsync(u => u.Id == id, token);
        }

        public async Task<User?> GetUserWithCreatedEventsAsync(int id, CancellationToken token)
        {
            return await _dbSet
                .Include(u => u.CreatedEvents)
                .FirstOrDefaultAsync(u => u.Id == id, token);
        }

        

        public async Task<User?> GetByEmailAsync(string email, CancellationToken token)
        {
            return await GetFirstOrDefaultAsync(token, x => x.Email == email);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken token)
        {
            return await _dbSet.AsNoTracking().ToListAsync(token);
        }
    }
}