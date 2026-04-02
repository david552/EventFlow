using EventFlow.Application;
using EventFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

      
       
        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

      

        public async Task<T?> GetAsync(CancellationToken token, params object[] key)
        {
            return await _dbSet.FindAsync(key,token);
        }

        public async Task AddAsync(CancellationToken token, T entity)
        {
            await _dbSet.AddAsync(entity, token);
        }

        public async Task<bool> AnyAsync(CancellationToken token, Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate, token);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task RemoveAsync(CancellationToken token, params object[] key)
        {
            var entity = await GetAsync(token, key);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public void Update(T entity)
        { 
            _dbSet.Update(entity);
        }

        public async Task<T?> GetFirstOrDefaultAsync(CancellationToken token, Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, token);
        }
    }
}
