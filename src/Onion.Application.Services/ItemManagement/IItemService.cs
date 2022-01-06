﻿using Onion.Application.Services.ItemManagement.Models;
using Onion.Core.Structures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onion.Application.Services.ItemManagement
{
    public interface IItemService
    {
        Task<IList<ItemRes>> ListAsync();
        Task<PaginableList<ItemRes>> PaginateAsync(int size, int page);
        Task<ItemRes> GetAsync(Guid itemId);
        Task<ItemRes> CreateAsync(ItemReq model);
        Task<ItemRes> DeleteAsync(Guid itemId);
        Task<ItemRes> UpdateAsync(Guid itemId, ItemReq model);

        Task<bool> FooAsync();
    }
}
