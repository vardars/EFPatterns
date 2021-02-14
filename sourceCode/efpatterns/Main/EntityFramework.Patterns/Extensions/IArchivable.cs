using System;

namespace EntityFramework.Patterns.Extensions
{
    public interface IArchivable
    {
        string DeletedBy { get; set; }
        DateTime? Deleted { get; set; }
    }
}
