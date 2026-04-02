using EventFlow.Application;
using EventFlow.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly protected ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SaveChanges(CancellationToken token)
        {
            try
            {
                await _context.SaveChangesAsync(token);
            }
            catch(Exception ex)
            {
               
                throw;
            }
        }
    }
}
