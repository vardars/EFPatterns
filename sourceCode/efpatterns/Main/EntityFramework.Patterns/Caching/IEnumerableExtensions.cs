using System.Collections.Generic;
using System.Runtime.Caching;

namespace EntityFramework.Patterns.Caching
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> ToCache<T>(this IEnumerable<T> query)
        {
            ICacheProvider provider = Config.Cache.DefaultProvider;
            provider.Add(query, new CacheItemPolicy());
            return query;
        }

        public static IEnumerable<T> FromCache<T>(this IEnumerable<T> query)
        {
            ICacheProvider provider = Config.Cache.DefaultProvider;
            return provider.Contains(query) ? provider.Get(query) : query;
        }
    }
}   