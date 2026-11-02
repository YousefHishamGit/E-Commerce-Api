using Application.Contracts.Repository;
using Application.Specifications;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<TEntity,Tkey> : IGenericRepository<TEntity,Tkey> where TEntity : BaseEntity<Tkey> where Tkey :notnull
    {
        private readonly AppDbContext _Context;
        public GenericRepository(AppDbContext context) {
            _Context=context;
        }

        public async Task AddAsync(TEntity entity)
        {

            await _Context.Set<TEntity>().AddAsync(entity);
        }

        public async Task<bool> AnyAsync(ISpecification<TEntity> spec)
        {
            return await SpecificationEvaluator.GetQuery(_Context.Set<TEntity>(),spec).AnyAsync();
        }

        public async Task<int> CountAsync(ISpecification<TEntity> spec)
        {
            return await SpecificationEvaluator.GetCountQuery(_Context.Set<TEntity>(),spec).CountAsync();
        }

        public void Delete(TEntity entity)
        {
            _Context.Set<TEntity>().Remove(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(ISpecification<TEntity>? spec = null)
        {

            var q = spec != null ? SpecificationEvaluator.GetQuery<TEntity>(_Context.Set<TEntity>(), spec, e => e.Id) : _Context.Set<TEntity>();
            return await q.AsNoTracking().ToListAsync();

            //AsNoTracking() => tracing Entity!! 
            // then to use it,function should return Entity
        }

        public async Task<TEntity?> GetByIdAsync(Tkey id, ISpecification<TEntity>? spec = null)
        {
            IQueryable<TEntity> query = _Context.Set<TEntity>();
            if (spec is not null)
            {
                query = SpecificationEvaluator.GetQuery(query, spec);
            }
            return await query.FirstOrDefaultAsync(e =>EF.Property<Tkey>(e, nameof(BaseEntity<Tkey>.Id)).Equals(id));
        }

        public void Update(TEntity entity)
        {
             _Context.Set<TEntity>().Update(entity);
        }
    }
}
