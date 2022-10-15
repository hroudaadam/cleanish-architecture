﻿using Microsoft.EntityFrameworkCore;
using Onion.App.Data.Cache;
using Onion.App.Data.Database.Entities;
using Onion.App.Data.Database.Repositories;
using Onion.Impl.App.Data.Database.Specifications;
using Onion.Shared.Exceptions;
using Onion.Shared.Extensions;
using Onion.Shared.Helpers;
using Onion.Shared.Structures;
using System.Runtime.CompilerServices;

namespace Onion.Impl.App.Data.Database.Repositories;

public abstract class DatabaseRepository<TEntity> : IDatabaseRepository<TEntity> where TEntity : BaseEntity
{
    private readonly SqlDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly DbSet<TEntity> _dbSet;
    public CacheStrategy CacheStrategy { get; init; }

    public DatabaseRepository(SqlDbContext dbContext, ICacheService cacheService, CacheStrategy cacheStrategy)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        CacheStrategy = cacheStrategy;
        _dbSet = _dbContext.Set<TEntity>();
    }

    public async Task<TEntity> CreateAsync(TEntity newEntity, bool commitAfter = true)
    {
        Guard.NotNull(newEntity, nameof(newEntity));

        TEntity createdEntity = _dbSet.Add(newEntity).Entity;
        await CommitIfTrueAsync(commitAfter);
        return createdEntity;
    }

    public async Task<TEntity> UpdateAsync(TEntity updatedEntity, bool commitAfter = true)
    {
        Guard.NotNull(updatedEntity, nameof(updatedEntity));

        await CommitIfTrueAsync(commitAfter);
        return updatedEntity;
    }

    public async Task<TEntity> DeleteAsync(TEntity entityToDelete, bool commitAfter = true)
    {
        Guard.NotNull(entityToDelete, nameof(entityToDelete));

        TEntity deletedEntity = _dbSet.Remove(entityToDelete).Entity;
        await CommitIfTrueAsync(commitAfter);
        return deletedEntity;
    }

    private async Task<int> CommitIfTrueAsync(bool shouldCommit)
    {
        if (shouldCommit) return await _dbContext.SaveChangesAsync();
        return 0;
    }

    protected SpecificationBuilder<TEntity> Specification() => new();

    protected async Task<PaginableList<TEntity>> ReadPaginatedData(
        ISpecification<TEntity> specification, 
        int pageSize, 
        int page,
        [CallerMemberName] string callerMethodName = "")
    {
        Guard.Min(pageSize, 1, nameof(pageSize));
        Guard.Min(page, 1, nameof(page));

        int countOfItems = await ReadDataAsync(specification, q => q.CountAsync(), CacheStrategy.Bypass);
        int countOfPages = (int)Math.Ceiling((double)countOfItems / pageSize);

        if (page > countOfPages) throw new ValidationException("Page is out of bounds");

        specification.Skip = pageSize * (page - 1);
        specification.Take = pageSize;

        var data = await ReadDataAsync(specification, q => q.ToListAsync(), callerMethodName);

        return new PaginableList<TEntity>(data, countOfItems, pageSize, page, countOfPages);
    }

    protected async Task<TResult> ReadDataAsync<TResult>(
        ISpecification<TEntity> specification,
        Func<IQueryable<TEntity>, Task<TResult>> queryOperation, 
        [CallerMemberName] string callerMethodName = "")
    {
        return await ReadDataAsync(specification, queryOperation, CacheStrategy, callerMethodName);
    }

    private async Task<TResult> ReadDataAsync<TResult>(
        ISpecification<TEntity> specification, 
        Func<IQueryable<TEntity>, Task<TResult>> queryOperation, 
        CacheStrategy cacheStrategy,
        [CallerMemberName] string callerMethodName = "")
    {
        Guard.NotNull(specification, nameof(specification));
        Guard.NotNull(queryOperation, nameof(queryOperation));

        bool useCache = cacheStrategy == CacheStrategy.Use;
        var valueProvider = () => queryOperation(SpecificationEvaluator.Evaluate(_dbSet, specification));

        if (!useCache) return await valueProvider();

        var cacheKey = new CacheKey(
            typeof(TEntity).Name,
            callerMethodName,
            specification.Filter?.ToEvaluatedString(),
            specification.OrderBy?.ToEvaluatedString(),
            specification.OrderByDesc?.ToEvaluatedString()
        );

        return await _cacheService.UseCacheAsync(cacheKey, valueProvider);
    }
}