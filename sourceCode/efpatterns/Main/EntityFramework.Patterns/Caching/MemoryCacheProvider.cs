using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Runtime.Caching;

namespace EntityFramework.Patterns.Caching
{

    public class MemoryCacheProvider : ICacheProvider
    {

        public event InvalidateCacheElement CacheElementInvalidated;

        private readonly Func<IEnumerable, string> _keyResolver;
        private readonly ObjectCache _cache = MemoryCache.Default;

        public MemoryCacheProvider()
        {
            _keyResolver = query => string.Format("{0} || {1}", query as ObjectQuery != null ? (query as ObjectQuery).ToTraceString() : query, query.GetType().AssemblyQualifiedName);
        }

        public MemoryCacheProvider(Func<IEnumerable, string> keyResolver)
        {
            if (keyResolver == null)
                throw new ArgumentNullException("keyResolver");
            _keyResolver = keyResolver;
        }

        public void RemoveFromCache<T>(IEnumerable<T> query)
        {
            _cache.Remove(GetCacheKey(query));
        }

        public void Invalidate<T>(T entity)
        {
            if (CacheElementInvalidated != null)
                CacheElementInvalidated(this, EventArgs.Empty);
        }

        public bool Contains<T>(IEnumerable<T> query)
        {
            return _cache.Contains(GetCacheKey(query));
        }

        public void Add<T>(IEnumerable<T> query, CacheItemPolicy cachePolicy)
        {
            _cache.Set(new CacheItem(GetCacheKey(query), query.ToList()), cachePolicy);
        }

        public IEnumerable<T> Get<T>(IEnumerable<T> query)
        {
            return _cache.Get(GetCacheKey(query)) as IEnumerable<T>;
        }

        private string GetCacheKey<T>(IEnumerable<T> query)
        {
            return _keyResolver(query.AsQueryable());
        }


    }
}