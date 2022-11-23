﻿using Onion.App.Data.Database.Entities;

namespace Onion.Impl.App.Data.Database.Specifications;

internal static class SpecificationEvaluator
{
    public static IQueryable<TEntity> Evaluate<TEntity>(IQueryable<TEntity> query, ISpecification<TEntity> specification) where TEntity : BaseEntity
    {
        if (specification == null) return query;

        if (specification.Filter != null)
        {
            query = query.Where(specification.Filter);
        }

        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDesc != null)
        {
            query = query.OrderByDescending(specification.OrderByDesc);
        }

        if (specification.Skip != null)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take != null)
        {
            query = query.Take(specification.Skip.Value);
        }

        query = specification.Includes.Aggregate(query, (current, include) => include(current));

        return query;
    }
}
