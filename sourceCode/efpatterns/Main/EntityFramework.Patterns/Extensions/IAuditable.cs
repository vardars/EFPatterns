using System;

namespace EntityFramework.Patterns.Extensions
{
    public interface IAuditable
    {
        string CreatedBy { get; set; }
        DateTime Created { get; set; }
        string UpdatedBy { get; set; }
        DateTime? Updated { get; set; }
    }
}
