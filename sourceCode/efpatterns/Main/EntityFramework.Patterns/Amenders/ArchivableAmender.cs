using System;
using Afterthought;
using EntityFramework.Patterns.Extensions;

namespace EntityFramework.Patterns.Amenders
{
    public class ArchivableAmender<T> : Amendment<T, T>
    {

        public ArchivableAmender()
        {
            Implement<IArchivable>(explicitImplementation: false);
        }
    }
}