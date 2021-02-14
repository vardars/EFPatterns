using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace EntityFramework.Patterns.Caching
{
    public delegate void InvalidateCacheElement(object sender, EventArgs args);

    public interface ICacheProvider
    {
        event InvalidateCacheElement CacheElementInvalidated;
        
        bool Contains<T>(IEnumerable<T> query);
        IEnumerable<T> Get<T>(IEnumerable<T> query);
        void Add<T>(IEnumerable<T> query, CacheItemPolicy cachePolicy);
        void RemoveFromCache<T>(IEnumerable<T> query);
        void Invalidate<T>(T entity);
    }
}