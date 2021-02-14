using System;
using System.Threading;
using EntityFramework.Patterns.Extensions;

namespace EntityFramework.Patterns.Decorators
{
    public class AuditableRepository<T> : RepositoryDecoratorBase<T>
        where T : class
    {

        public AuditableRepository(IRepository<T> surrogate) : base(surrogate) { }

        public override void Insert(T entity)
        {
            IAuditable auditable = entity as IAuditable;
            if (auditable != null)
            {
                auditable.CreatedBy = Thread.CurrentPrincipal.Identity.Name;
                auditable.Created = DateTime.Now;
            }
            base.Insert(entity);
        }

        public override void Update(T entity)
        {
            IAuditable auditable = entity as IAuditable;
            if (auditable != null)
            {
                auditable.UpdatedBy = Thread.CurrentPrincipal.Identity.Name;
                auditable.Updated = DateTime.Now;
            }
            base.Update(entity);
        }
    }
}