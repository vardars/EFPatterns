using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.Patterns
{
    public abstract class RepositoryDecoratorBase<T> : IRepository<T>
        where T : class
    {
        
        private readonly IRepository<T> _surrogate;

        public RepositoryDecoratorBase(IRepository<T> surrogate)
        {
            _surrogate = surrogate;
        }

        public virtual IQueryable<T> AsQueryable()
        {
            return _surrogate.AsQueryable();
        }

        public virtual IEnumerable<T> GetAll(params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            return _surrogate.GetAll(includeProperties);
        }

        public virtual IEnumerable<T> Find(System.Linq.Expressions.Expression<Func<T, bool>> where, params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            return _surrogate.Find(where, includeProperties);
        }

        public virtual T Single(System.Linq.Expressions.Expression<Func<T, bool>> where, params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            return _surrogate.Single(where, includeProperties);
        }

        public virtual T First(System.Linq.Expressions.Expression<Func<T, bool>> where, params System.Linq.Expressions.Expression<Func<T, object>>[] includeProperties)
        {
            return _surrogate.First(where, includeProperties);
        }

        public virtual void Delete(T entity)
        {
            _surrogate.Delete(entity);
        }

        public virtual void Insert(T entity)
        {
            _surrogate.Insert(entity);
        }

        public virtual void Update(T entity)
        {
            _surrogate.Update(entity);
        }
    }
}
