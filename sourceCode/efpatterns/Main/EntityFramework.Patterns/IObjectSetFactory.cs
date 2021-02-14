using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace EntityFramework.Patterns
{
    public interface IObjectSetFactory : IDisposable
    {
        IObjectSet<T> CreateObjectSet<T>() where T : class;
        void ChangeObjectState(object entity, EntityState state);
    }
}