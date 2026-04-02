using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Application
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetAsync(CancellationToken token, params object[] key);
        Task AddAsync(CancellationToken token, T entity);
        void Update(T entity);
        void Remove(T entity);
        Task RemoveAsync(CancellationToken token, params object[] key);
        Task<T?> GetFirstOrDefaultAsync(CancellationToken token, Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(CancellationToken token, Expression<Func<T, bool>> predicate);
    }
}
