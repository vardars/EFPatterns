using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using EntityFramework.Patterns.Decorators;
using EntityFramework.Patterns.Extensions;
using Moq;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class ArchivableRepositoryTests
    {
        private List<ArchivableEntity> _list;
        private readonly Expression<Func<ArchivableEntity, bool>> _where = a => a.Value > 10;

        [TestFixtureSetUp]
        public void Init()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            ArchivableEntity ent1 = new ArchivableEntity {Value = 11};
            ArchivableEntity ent2 = new ArchivableEntity {Value = 13};
            ((IArchivable) ent1).Deleted = DateTime.Now;

            _list = new List<ArchivableEntity>(new[] { ent1, ent2 });
        }

        [Test]
        public void ArchivedEntities_Are_Automatically_Filtered_When_Using_GetAll()
        {
            // Arrange
            Mock<IRepository<ArchivableEntity>> realRepo = new Mock<IRepository<ArchivableEntity>>();

            realRepo.Setup(rr => rr.GetAll())
                .Returns((Expression<Func<ArchivableEntity, object>>[] includeProperties) =>
                             {
                                 return _list;
                             });

            Mock<ArchivableRepository<ArchivableEntity>> deco =
                new Mock<ArchivableRepository<ArchivableEntity>>(realRepo.Object) { CallBase = true };

            // Act
            IEnumerable<ArchivableEntity> ret = realRepo.Object.GetAll();
            IEnumerable<ArchivableEntity> ret2 = deco.Object.GetAll();

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(2));
            Assert.That(ret2.Cast<IArchivable>().Where(e => e.Deleted != null).ToList(), Has.Count.EqualTo(0));
        }

        [Test]
        public void ArchivedEntities_Are_Automatically_Filtered_When_Using_Find()
        {
            // Arrange
            Mock<IRepository<ArchivableEntity>> realRepo = new Mock<IRepository<ArchivableEntity>>();

            realRepo.Setup(rr => rr.Find(It.IsAny<Expression<Func<ArchivableEntity, bool>>>()))
                .Returns(
                    (Expression<Func<ArchivableEntity, bool>> expr,
                     Expression<Func<ArchivableEntity, object>>[] includeProperties) =>
                        {
                            return _list.Where(expr.Compile());
                        });

            Mock<ArchivableRepository<ArchivableEntity>> deco =
                new Mock<ArchivableRepository<ArchivableEntity>>(realRepo.Object) { CallBase = true };

            // Act
            IEnumerable<ArchivableEntity> ret = realRepo.Object.Find(_where);
            IEnumerable<ArchivableEntity> ret2 = deco.Object.Find(_where);

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(2));
            Assert.That(ret2.Cast<IArchivable>().Where(e => e.Deleted != null).ToList(), Has.Count.EqualTo(0));
        }

        [Test]
        public void ArchivedEntities_Are_Automatically_Filtered_When_Using_Single()
        {
            // Arrange
            Mock<IRepository<ArchivableEntity>> realRepo = new Mock<IRepository<ArchivableEntity>>();

            realRepo.Setup(rr => rr.Single(It.IsAny<Expression<Func<ArchivableEntity, bool>>>()))
                .Returns(
                    (Expression<Func<ArchivableEntity, bool>> expr,
                     Expression<Func<ArchivableEntity, object>>[] includeProperties) =>
                        {
                            return _list.Single(expr.Compile());
                        });

            Mock<ArchivableRepository<ArchivableEntity>> deco =
                new Mock<ArchivableRepository<ArchivableEntity>>(realRepo.Object) { CallBase = true };

            // Act (and verify)
            Assert.That(delegate { realRepo.Object.Single(_where); }, Throws.InstanceOf<InvalidOperationException>());

            // Verify
            Assert.That(deco.Object.Single(_where), Is.Not.Null);
        }

        [Test]
        public void ArchivedEntities_Are_Automatically_Filtered_When_Using_First()
        {
            // Arrange
            Mock<IRepository<ArchivableEntity>> realRepo = new Mock<IRepository<ArchivableEntity>>();

            realRepo.Setup(rr => rr.First(It.IsAny<Expression<Func<ArchivableEntity, bool>>>()))
                .Returns(
                    (Expression<Func<ArchivableEntity, bool>> expr,
                     Expression<Func<ArchivableEntity, object>>[] includeProperties) =>
                        {
                            return _list.First(expr.Compile());
                        });

            ArchivableEntity ent = realRepo.Object.First(_where);
            Assert.That(ent, Is.Not.Null);
            Assert.That(ent, Has.Property("Value").EqualTo(11));
            Assert.That(ent, Has.Property("Deleted").Not.EqualTo(null));

            Mock<ArchivableRepository<ArchivableEntity>> deco =
                new Mock<ArchivableRepository<ArchivableEntity>>(realRepo.Object) {CallBase = true};

            // Act
            ent = deco.Object.First(_where);

            // Verify
            Assert.That(ent, Is.Not.Null);
            Assert.That(ent, Has.Property("Value").EqualTo(13));
            Assert.That(ent, Has.Property("Deleted").EqualTo(null));
        }

        [Test]
        public void ArchivedEntities_Are_Automatically_Filtered_When_Using_AsQueryable()
        {
            // Arrange
            Mock<IRepository<ArchivableEntity>> realRepo = new Mock<IRepository<ArchivableEntity>>();

            realRepo.Setup(rr => rr.AsQueryable())
                .Returns(() => _list.AsQueryable());

            ArchivableEntity ent = realRepo.Object.AsQueryable().First();
            Assert.That(ent, Is.Not.Null);
            Assert.That(ent, Has.Property("Value").EqualTo(11));
            Assert.That(ent, Has.Property("Deleted").Not.EqualTo(null));

            Mock<ArchivableRepository<ArchivableEntity>> deco =
                new Mock<ArchivableRepository<ArchivableEntity>>(realRepo.Object) { CallBase = true };

            // Act
            ent = deco.Object.AsQueryable().First();

            // Verify
            Assert.That(ent, Is.Not.Null);
            Assert.That(ent, Has.Property("Value").EqualTo(13));
            Assert.That(ent, Has.Property("Deleted").EqualTo(null));
        }

        [Test]
        public void Deleted_Entities_Are_Just_Modified_Not_Deleted()
        {
            // Arrange
            ArchivableEntity ent1 = new ArchivableEntity { Value = 11 };
            ArchivableEntity ent2 = new ArchivableEntity { Value = 13 };
            ((IArchivable)ent1).Deleted = DateTime.Now;
            List<ArchivableEntity> l2 = new List<ArchivableEntity>(new[] { ent1, ent2 });

            Mock<IRepository<ArchivableEntity>> realRepo = new Mock<IRepository<ArchivableEntity>>();

            realRepo.Setup(rr => rr.Delete(It.IsAny<ArchivableEntity>()))
                .Callback((ArchivableEntity ent) => { l2.Remove(ent); });

            realRepo.Setup(rr => rr.Update(It.IsAny<ArchivableEntity>()))
                .Callback((ArchivableEntity ent) => { ((IArchivable) ent).Deleted = DateTime.Now; });

            Mock<ArchivableRepository<ArchivableEntity>> deco =
                new Mock<ArchivableRepository<ArchivableEntity>>(realRepo.Object) { CallBase = true };

            // Act
            deco.Object.Delete(ent2);
            
            // Verify
            realRepo.Verify(rr => rr.Delete(ent2), Times.Never());
            realRepo.Verify(rr => rr.Update(ent2), Times.Once());
            Assert.That(ent2, Has.Property("Deleted").Not.EqualTo(null));
            Assert.That(l2.Count, Is.EqualTo(2));
        }
    }
}