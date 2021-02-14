using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace EntityFramework.Patterns
{
    public class DbContextAdapter : IObjectSetFactory, IObjectContext
    {
        private readonly ObjectContext _context;

        public DbContextAdapter(DbContext context)
        {
            _context = context.GetObjectContext();
        }

        #region IObjectContext Members

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        #endregion

        #region IObjectSetFactory Members

        public void Dispose()
        {
            _context.Dispose();
        }

        public IObjectSet<T> CreateObjectSet<T>() where T : class
        {
            return _context.CreateObjectSet<T>();
        }

        public void ChangeObjectState(object entity, EntityState state)
        {
            _context.ObjectStateManager.ChangeObjectState(entity, state);
        }

        #endregion
    }
}