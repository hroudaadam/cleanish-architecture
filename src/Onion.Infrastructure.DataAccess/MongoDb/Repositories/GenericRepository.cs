﻿using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Onion.Application.DataAccess.Entities;
using Onion.Application.DataAccess.Exceptions.Common;
using Onion.Application.DataAccess.Repositories;
using Onion.Core.Structures;
using System.Linq.Expressions;

namespace Onion.Infrastructure.DataAccess.MongoDb.Repositories;

public abstract class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _dbSet;
    protected readonly IMongoDbContext _mongoDbContext;

    public GenericRepository(IMongoDbContext mongoDbContext)
    {
        _dbSet = mongoDbContext.Collection<T>();
        _mongoDbContext = mongoDbContext;
    }

    public async Task<T> GetByIdAsync(Guid entityId)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, entityId);
        return await (await _dbSet.FindAsync(filter)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> ListAsync()
    {
        var filter = Builders<T>.Filter.Empty;
        return await (await _dbSet.FindAsync(filter)).ToListAsync();
    }

    public async Task<PaginableList<T>> PaginateAsync(int pageSize, int page)
    {
        return await PaginateAsync(pageSize, page, e => true);
    }

    public async Task<int> CountAsync()
    {
        var filter = Builders<T>.Filter.Empty;
        return (int)await _dbSet.CountDocumentsAsync(filter);
    }

    public async Task<T> CreateAsync(T newEntity)
    {
        _mongoDbContext.SetAuditDates(newEntity, true);

        await _dbSet.InsertOneAsync(newEntity);
        return newEntity;
    }

    public async Task<T> UpdateAsync(T updatedEntity)
    {
        _mongoDbContext.SetAuditDates(updatedEntity);

        var filter = Builders<T>.Filter.Eq(e => e.Id, updatedEntity.Id);
        await _dbSet.ReplaceOneAsync(filter, updatedEntity);

        return updatedEntity;
    }

    public async Task<T> DeleteAsync(T entityToDelete)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, entityToDelete.Id);
        await _dbSet.DeleteOneAsync(filter);

        return entityToDelete;
    }

    protected async Task<PaginableList<T>> PaginateAsync(int pageSize, int page, Func<T, bool> filter)
    {
        int countOfEntities = await CountAsync();
        int countOfPages = (int)Math.Ceiling((double)countOfEntities / pageSize);
        if (page > countOfPages) throw new PageOutOfBoundsException();

        var entities = (await ListAsync())
            .Where(e => filter(e))
            .Skip(pageSize * (page - 1)).Take(pageSize)
            .ToList();

        return new PaginableList<T>(entities, countOfEntities, pageSize, page, countOfPages);
    }
}
