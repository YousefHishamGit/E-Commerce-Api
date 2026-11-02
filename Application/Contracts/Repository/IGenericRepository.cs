using Application.Specifications;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contracts.Repository
{
    public interface IGenericRepository<TEntity,TKey> where TEntity : BaseEntity<TKey> where TKey : notnull
    {
        public Task<IEnumerable<TEntity>> GetAllAsync(ISpecification<TEntity>? spec=null);
        public Task<TEntity?> GetByIdAsync(TKey id, ISpecification<TEntity>? spec=null);
        Task<int> CountAsync(ISpecification<TEntity> spec);
        Task<bool> AnyAsync(ISpecification<TEntity> spec);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
