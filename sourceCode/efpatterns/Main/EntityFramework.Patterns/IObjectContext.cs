using System;

namespace EntityFramework.Patterns
{
    public interface IObjectContext : IDisposable
    {
        void SaveChanges();
    }
}