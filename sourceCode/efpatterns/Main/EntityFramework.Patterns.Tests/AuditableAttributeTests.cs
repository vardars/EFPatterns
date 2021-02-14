using EntityFramework.Patterns.Extensions;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class AuditableAttributeTests
    {

        [Test]
        public void AuditableEntity_Should_Implements_IAuditable()
        {
           Assert.That(new AuditableEntity(), Is.AssignableTo<IAuditable>());
        }

    }
}