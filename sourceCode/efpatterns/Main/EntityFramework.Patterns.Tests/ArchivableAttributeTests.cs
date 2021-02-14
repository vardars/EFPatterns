using EntityFramework.Patterns.Extensions;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class ArchivableAttributeTests
    {

        [Test]
        public void ArchivableEntity_Should_Implements_IArchivable()
        {
           Assert.That(new ArchivableEntity(), Is.AssignableTo<IArchivable>());
        }

    }
}