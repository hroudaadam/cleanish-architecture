﻿using System;

namespace Onion.Core.Mapper
{
    public interface IMapper
    {
        TDest Map<TSource, TDest>(TSource source, Action<TDest> additionalProperties = null);
    }
}
