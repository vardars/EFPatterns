﻿using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace EntityFramework.Patterns
{
    public static class DbContextExtensions
    {
        public static ObjectContext GetObjectContext(this DbContext dbContext)
        {
            return ((IObjectContextAdapter) dbContext).ObjectContext;
        }
    }
}