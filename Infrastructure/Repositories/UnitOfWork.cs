using Application.Contracts.Repository;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UnitOfWork :IUnitOfWork
    {
        private readonly AppDbContext _Context;
        private readonly ConcurrentDictionary<string, object> _repositories = new ConcurrentDictionary<string, object>();

        public UnitOfWork(AppDbContext Context) { 
            _Context = Context;
        }
        
        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey> where TKey : notnull
        {
            return (IGenericRepository<TEntity, TKey>)_repositories.GetOrAdd(typeof(TEntity).Name, new GenericRepository<TEntity, TKey>(_Context));
        }

        public async Task<int> SaveChangesAsync()
        {
           return await _Context.SaveChangesAsync();
        }
    }
}
