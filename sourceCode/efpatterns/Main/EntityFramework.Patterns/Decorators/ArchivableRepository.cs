using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using EntityFramework.Patterns.Extensions;

namespace EntityFramework.Patterns.Decorators
{
    public class ArchivableRepository<T> : RepositoryDecoratorBase<T>
        where T : class
    {
        
        private readonly IRepository<T> _surrogate;
        private readonly Expression<Func<T, bool>> _notArchivedPredicate;

        public ArchivableRepository(IRepository<T> surrogate) : base(surrogate)
        {
            _surrogate = surrogate;
            if (typeof(IArchivable).IsAssignableFrom(typeof(T)))
                _notArchivedPredicate = BuildNotArchivedPredicate();
        }

        private Expression<Func<T, bool>> BuildNotArchivedPredicate()
        {
            ParameterExpression entityType = Expression.Parameter(typeof(T), "ent");
            Expression entIsNotDeleted = Expression.Equal(Expression.Property(entityType, "Deleted"), Expression.Constant(null));

            return Expression.Lambda<Func<T, bool>>(entIsNotDeleted, entityType);
        }

        private Expression<Func<T, bool>> ComposeWithNotArchivedPredicate(Expression<Func<T, bool>> where)
        {
            Expression<Func<T, bool>> composedExpression = where.And(_notArchivedPredicate);

            return composedExpression;
        }

        public override IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includeProperties)
        {
            if (typeof(IArchivable).IsAssignableFrom(typeof(T)))
                return _surrogate.Find(_notArchivedPredicate, includeProperties);
            
            return _surrogate.GetAll(includeProperties); 
        }

        public override IQueryable<T> AsQueryable()
        {
            return typeof(IArchivable).IsAssignableFrom(typeof(T)) ? base.AsQueryable().Where(_notArchivedPredicate) : base.AsQueryable();
        }

        public override IEnumerable<T> Find(Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] includeProperties)
        {
            if (typeof(IArchivable).IsAssignableFrom(typeof(T)))
                where = ComposeWithNotArchivedPredicate(where);

            return _surrogate.Find(where, includeProperties); 
        }

        public override T Single(Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] includeProperties)
        {
            if (typeof(IArchivable).IsAssignableFrom(typeof(T)))
                where = ComposeWithNotArchivedPredicate(where);

            return _surrogate.Single(where, includeProperties); 
        }

        public override T First(Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] includeProperties)
        {
            if (typeof(IArchivable).IsAssignableFrom(typeof(T)))
                where = ComposeWithNotArchivedPredicate(where);

            return _surrogate.First(where, includeProperties); 
        }

        public override void Delete(T entity)
        {
            IArchivable archivable = entity as IArchivable;
            if (archivable != null)
            {
                archivable.DeletedBy = Thread.CurrentPrincipal.Identity.Name;
                archivable.Deleted = DateTime.Now;
                _surrogate.Update(entity);
            }
            else
                _surrogate.Delete(entity);
        }
    }
}