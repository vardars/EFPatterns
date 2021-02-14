using EntityFramework.Patterns.Caching;

namespace EntityFramework.Patterns
{
    public static class Config
    {
        public static class Cache
        {
            private static ICacheProvider _cacheProvider;

            public static ICacheProvider DefaultProvider
            {
                get { return _cacheProvider ?? (_cacheProvider = new MemoryCacheProvider()); }
                set { _cacheProvider = value; }
            }
        }
    }
}