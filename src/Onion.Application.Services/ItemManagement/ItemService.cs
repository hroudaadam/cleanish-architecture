﻿using Onion.Application.DataAccess.Entities;
using Onion.Application.DataAccess.Exceptions.Item;
using Onion.Application.DataAccess.Repositories;
using Onion.Application.Services.ItemManagement.Models;
using Onion.Application.Services.Security;
using Onion.Application.Services.Security.Models;
using Onion.Core.Mapper;
using Onion.Core.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Onion.Application.Services.ItemManagement
{
    public class ItemService : IItemService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ISecurityContextProvider _securityContextProvider;

        public ItemService(
            IRepositoryManager repositoryManager,
            IMapper mapper,
            ISecurityContextProvider securityContextProvider)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _securityContextProvider = securityContextProvider;
        }

        public async Task<ItemRes> GetAsync(Guid itemId)
        {
            var item = await _repositoryManager.ItemRepository.GetByIdAsync(itemId);
            if (item == null) throw new ItemNotFoundException(itemId);
            return _mapper.Map<Item, ItemRes>(item);
        }

        public async Task<IList<ItemRes>> ListAsync()
        {
            var items = await _repositoryManager.ItemRepository.ListAsync();
            return items.Select(i => _mapper.Map<Item, ItemRes>(i)).ToList();
        }

        public async Task<PaginableList<ItemRes>> PaginateAsync(int size, int page)
        {
            var paginable = await _repositoryManager.ItemRepository.PaginateAsync(size, page);
            return paginable.Transform(paginable.Data.Select(i => _mapper.Map<Item, ItemRes>(i)).ToList());
        }

        public async Task<ItemRes> CreateAsync(ItemReq model)
        {
            Item newItem = _mapper.Map<ItemReq, Item>(model);
            newItem = await _repositoryManager.ItemRepository.CreateAsync(newItem);

            return _mapper.Map<Item, ItemRes>(newItem);
        }

        public async Task<ItemRes> DeleteAsync(Guid itemId)
        {
            Item itemToDelete = await _repositoryManager.ItemRepository.GetByIdAsync(itemId);
            if (itemToDelete == null) throw new ItemNotFoundException(itemId);

            itemToDelete = await _repositoryManager.ItemRepository.DeleteAsync(itemToDelete);

            return _mapper.Map<Item, ItemRes>(itemToDelete);
        }

        public async Task<ItemRes> UpdateAsync(Guid itemId, ItemReq model)
        {
            var itemToUpdate = await _repositoryManager.ItemRepository.GetByIdAsync(itemId);
            if (itemToUpdate == null) throw new ItemNotFoundException(itemId);

            itemToUpdate.Title = model.Title;
            itemToUpdate.Description = model.Description;

            await _repositoryManager.ItemRepository.UpdateAsync(itemToUpdate);
            return _mapper.Map<Item, ItemRes>(itemToUpdate);
        }

        public Task<bool> FooAsync()
        {
            if (_securityContextProvider.SecurityContext.IsUser)
            {
                var currentUser = (UserSecurityContext)_securityContextProvider.SecurityContext;
            }
            return Task.FromResult(true);
        }


    }
}
