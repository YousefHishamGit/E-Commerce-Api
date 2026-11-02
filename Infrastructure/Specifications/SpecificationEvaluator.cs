using Application.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Specifications
{
    public static class SpecificationEvaluator
    {
        public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> spec, Expression<Func<T, object>>? defaultOrderBy = null) where T : class
        {
            var query = inputQuery;

            if (spec is null) return query;

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

            query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            if (spec.OrderBy != null)
            {
                query = query.OrderBy(spec.OrderBy);
            }
            else if (spec.OrderByDescending != null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }
            else if (spec.IsPagingEnabled && defaultOrderBy != null)
            {
                query = query.OrderBy(defaultOrderBy);
            }

            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            return query;
        }

        public static IQueryable<T> GetCountQuery<T>(
          IQueryable<T> inputQuery,
          ISpecification<T>? spec) where T : class
        {
            return spec?.Criteria != null
                ? inputQuery.Where(spec.Criteria)
                : inputQuery;
        }
    }
}
