using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using EntityFramework.Patterns.Caching;

namespace EntityFramework.Patterns.Decorators
{
    public class CacheableRepository<T> : RepositoryDecoratorBase<T>
         where T : class
    {

        [Flags]
        public enum TypeInvalidation
        {
            None = 0,
            OnInsert = 1,
            OnUpdate = 2,
            OnDelete = 4,
            All = 8
        }

        private readonly IRepository<T> _surrogate;
        private readonly ICacheProvider _cacheProvider;
        private readonly Func<CacheItemPolicy> _policyFactory;
        private readonly TypeInvalidation _typeInvalidation;

        public CacheableRepository(IRepository<T> surrogate) 
            : this(surrogate, new MemoryCacheProvider()) { }
        
        public CacheableRepository(IRepository<T> surrogate, ICacheProvider cacheProvider) 
            : this(surrogate, cacheProvider, () => new CacheItemPolicy()) { }

        public CacheableRepository(IRepository<T> surrogate, ICacheProvider cacheProvider, Func<CacheItemPolicy> policyFactory) 
            : this(surrogate, cacheProvider, policyFactory, TypeInvalidation.All) { }

        public CacheableRepository(IRepository<T> surrogate, ICacheProvider cacheProvider, Func<CacheItemPolicy> policyFactory, TypeInvalidation typeInvalidation)
            : base(surrogate)
        {
            _surrogate = surrogate;
            _cacheProvider = cacheProvider;
            _policyFactory = policyFactory;
            _typeInvalidation = typeInvalidation;
        }
        
        public override IEnumerable<T> Find(System.Linq.Expressions.Expression<Func<T, bool>> where, params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            IEnumerable<T> query = _surrogate.Find(where, includeProperties).AsQueryable();

            if (!_cacheProvider.Contains(query))
            {
                CacheItemPolicy policy = WrapPolicy();
                _cacheProvider.Add(query, policy);
            }
            else
                query = _cacheProvider.Get(query);

            return query;
        }

        public override T First(System.Linq.Expressions.Expression<Func<T, bool>> where, params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            IEnumerable<T> query = new []{_surrogate.First(where, includeProperties)}.AsQueryable();

            if (!_cacheProvider.Contains(query))
            {
                CacheItemPolicy policy = WrapPolicy();
                _cacheProvider.Add(query, policy);
            }
            else
                query = _cacheProvider.Get(query);

            return query.First();
        }

        public override T Single(System.Linq.Expressions.Expression<Func<T, bool>> where, params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            IEnumerable<T> query = new[] { _surrogate.Single(where, includeProperties) }.AsQueryable();

            if (!_cacheProvider.Contains(query))
            {
                CacheItemPolicy policy = WrapPolicy();
                _cacheProvider.Add(query, policy);
            }
            else
                query = _cacheProvider.Get(query);

            return query.Single();
        }

        public override IEnumerable<T> GetAll(params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            IEnumerable<T> query = _surrogate.GetAll(includeProperties).AsQueryable();

            if (!_cacheProvider.Contains(query))
            {
                CacheItemPolicy policy = WrapPolicy();
                _cacheProvider.Add(query, policy);
            }
            else
                query = _cacheProvider.Get(query);

            return query;
        }

        private CacheItemPolicy WrapPolicy()
        {
            CacheItemPolicy policy = _policyFactory();
            TypeChangeMonitor typeMonitor = new TypeChangeMonitor();
            policy.ChangeMonitors.Add(typeMonitor);
            _cacheProvider.CacheElementInvalidated += typeMonitor.OnInvalidateCacheElement;
            return policy;
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(T entity)
        {
            if ((_typeInvalidation & TypeInvalidation.OnInsert) == TypeInvalidation.OnInsert)
                _cacheProvider.Invalidate(entity);

            base.Insert(entity);
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public override void Update(T entity)
        {
            if ((_typeInvalidation & TypeInvalidation.OnUpdate) == TypeInvalidation.OnUpdate)
                _cacheProvider.Invalidate(entity);

            base.Update(entity);
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(T entity)
        {
            if ((_typeInvalidation & TypeInvalidation.OnDelete) == TypeInvalidation.OnDelete)
                _cacheProvider.Invalidate(entity);

            base.Delete(entity);
        }
    }
}