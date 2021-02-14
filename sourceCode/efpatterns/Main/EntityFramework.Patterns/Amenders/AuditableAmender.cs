using System;
using Afterthought;
using EntityFramework.Patterns.Extensions;

namespace EntityFramework.Patterns.Amenders
{
    public class AuditableAmender<T> : Amendment<T, T>
    {

        public AuditableAmender()
        {
            Implement<IAuditable>(explicitImplementation: false);
        }
    }
}