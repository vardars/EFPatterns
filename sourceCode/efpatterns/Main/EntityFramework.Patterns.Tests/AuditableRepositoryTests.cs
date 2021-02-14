using System;
using System.Security.Principal;
using System.Threading;
using EntityFramework.Patterns.Decorators;
using EntityFramework.Patterns.Extensions;
using Moq;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class AuditableRepositoryTests
    {
        [TestFixtureSetUp]
        public void Init()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
        }

        [Test]
        public void Audit_Properties_Are_Modified()
        {
            Mock<IRepository<AuditableEntity>> realRepo = new Mock<IRepository<AuditableEntity>>();
            realRepo.Setup(rr => rr.Insert(It.IsAny<AuditableEntity>()));

            Mock<AuditableRepository<AuditableEntity>> deco = new Mock<AuditableRepository<AuditableEntity>>(realRepo.Object)
                                                              {CallBase = true};

            AuditableEntity ent = new AuditableEntity();
            deco.Object.Insert(ent);
            Assert.That(ent, Is.AssignableTo<IAuditable>());
            Assert.That(ent, Has.Property("Created").Not.Null);
            Assert.That(ent, Has.Property("CreatedBy").Not.Null);
            Assert.That(ent, Has.Property("CreatedBy").EqualTo(Thread.CurrentPrincipal.Identity.Name));
            
            realRepo.VerifyAll();
        }
    }
}