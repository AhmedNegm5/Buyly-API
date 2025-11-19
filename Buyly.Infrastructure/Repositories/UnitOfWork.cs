using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Buyly.Infrastructure.Data;

namespace Buyly.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ConcurrentDictionary<string, object> _repositories = new();

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var typeName = typeof(T).Name;

            var repo = _repositories.GetOrAdd(typeName, _ => new GenericRepository<T>(_context));
            return (IGenericRepository<T>)repo;
        }

        public async Task<int> CommitAsync()
        {
                return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}