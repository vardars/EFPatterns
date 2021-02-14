using System;
using System.Collections.Generic;
using System.Reflection;
using Afterthought;
using EntityFramework.Patterns.Amenders;
using EntityFramework.Patterns.Extensions;

namespace EntityFramework.Patterns
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AmendAttribute : Attribute, IAmendmentAttribute
    {
        IEnumerable<ITypeAmendment> IAmendmentAttribute.GetAmendments(Type target)
        {
            if (target.GetCustomAttributes(typeof(AuditableAttribute), true).Length > 0)
            {
                ConstructorInfo constructorInfo = typeof (AuditableAmender<>).MakeGenericType(target).GetConstructor(Type.EmptyTypes);
                if (constructorInfo != null)
                    yield return (ITypeAmendment) constructorInfo.Invoke(new object[0]);
            }
            if (target.GetCustomAttributes(typeof(ArchivableAttribute), true).Length > 0)
            {
                ConstructorInfo constructorInfo = typeof(ArchivableAmender<>).MakeGenericType(target).GetConstructor(Type.EmptyTypes);
                if (constructorInfo != null)
                    yield return (ITypeAmendment)constructorInfo.Invoke(new object[0]);
            }
        }
    }
}