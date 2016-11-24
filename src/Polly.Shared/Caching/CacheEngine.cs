﻿using System;
using System.Threading;

namespace Polly.Caching
{
    internal static partial class CacheEngine
    {
        internal static TResult Implementation<TResult>(
            ICacheProvider<TResult> cacheProvider,
            ITtlStrategy ttlStrategy,
            ICacheKeyStrategy cacheKeyStrategy,
            Func<CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string cacheKey = cacheKeyStrategy.GetCacheKey(context);
            if (cacheKey == null)
            {
                return action(cancellationToken);
            }

            object valueFromCache = cacheProvider.Get(cacheKey);
            if (valueFromCache != null)
            {
                return (TResult) valueFromCache;
            }

            TResult result = action(cancellationToken);

            TimeSpan ttl = ttlStrategy.GetTtl(context);
            if (ttl > TimeSpan.Zero)
            {
                cacheProvider.Put(cacheKey, result, ttl);
            }

            return result;
        }
    }
}