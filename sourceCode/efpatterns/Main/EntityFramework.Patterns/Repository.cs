using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFramework.Patterns
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IObjectSet<T> _objectSet;
        private readonly IObjectSetFactory _objectSetFactory;

        public Repository(IObjectSetFactory objectSetFactory)
        {
            _objectSet = objectSetFactory.CreateObjectSet<T>();
            _objectSetFactory = objectSetFactory;
        }

        #region IRepository<T> Members

        public IQueryable<T> AsQueryable()
        {
            return _objectSet;
        }

        public IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = AsQueryable();
            return PerformInclusions(includeProperties, query);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> where,
                                   params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = AsQueryable();
            query = PerformInclusions(includeProperties, query);
            return query.Where(where);
        }

        public T Single(Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = AsQueryable();
            query = PerformInclusions(includeProperties, query);
            return query.Single(where);
        }

        public T First(Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = AsQueryable();
            query = PerformInclusions(includeProperties, query);
            return query.First(where);
        }

        public void Delete(T entity)
        {
            _objectSet.DeleteObject(entity);
        }

        public void Insert(T entity)
        {
            _objectSet.AddObject(entity);
        }

        public void Update(T entity)
        {
            _objectSet.Attach(entity);
            _objectSetFactory.ChangeObjectState(entity, EntityState.Modified);
        }

        #endregion

        private static IQueryable<T> PerformInclusions(IEnumerable<Expression<Func<T, object>>> includeProperties,
                                                       IQueryable<T> query)
        {
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }
    }
}