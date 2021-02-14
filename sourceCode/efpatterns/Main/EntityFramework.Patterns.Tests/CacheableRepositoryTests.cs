using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Security.Principal;
using EntityFramework.Patterns.Caching;
using EntityFramework.Patterns.Decorators;
using Moq;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class CacheableRepositoryTests
    {

        [TestFixtureSetUp]
        public void Init()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
        }

        private List<Product> GetProductList()
        {
            Product p1 = new Product
            {
                Id = 1,
                Name = "product 1",
                ProductCategory = new ProductCategory { Id = 1, Name = "cat 1" }
            };

            Product p2 = new Product
            {
                Id = 2,
                Name = "product 2",
                ProductCategory = new ProductCategory { Id = 1, Name = "cat 1" }
            };

            return new List<Product>(new[] { p1, p2 });
        }

        public CacheItemPolicy PolicyFactory()
        {
            return new CacheItemPolicy();
        }

        [Test]
        public void Storage_Is_Hit_Only_Once_Per_Query_When_Using_GetAll()
        {
            List<Product> lst = GetProductList();

            // Arrange
            Mock<IRepository<Product>> realRepo = new Mock<IRepository<Product>>();

            realRepo.Setup(rr => rr.GetAll())
                .Returns((Expression<Func<Product, object>>[] includeProperties) => lst);

            Mock<CacheableRepository<Product>> deco =
                new Mock<CacheableRepository<Product>>(realRepo.Object) { CallBase = true };

            // Act - call GetAll twice.
            IEnumerable<Product> ret = deco.Object.GetAll();

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(2));

            // Act - call GetAll a second time.
            lst.Clear(); // now data can come only from the cache.
            ret = deco.Object.GetAll();

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(2));
        }

        [Test]
        public void Storage_Is_Hit_Only_Once_Per_Query_When_Using_Find()
        {
            List<Product> lst = GetProductList();

            // Arrange
            Mock<IRepository<Product>> realRepo = new Mock<IRepository<Product>>();

            realRepo.Setup(rr => rr.Find(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(
                    (Expression<Func<Product, bool>> expr,
                     Expression<Func<Product, object>>[] includeProperties) =>
                    {
                        return lst.Where(expr.Compile());
                    });

            Mock<CacheableRepository<Product>> deco =
                new Mock<CacheableRepository<Product>>(realRepo.Object) { CallBase = true };

            // Act - call Find.
            IEnumerable<Product> ret = deco.Object.Find(p => p.Name == "product 2");

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(1));

            // Act - call Find a second time.
            lst.Clear(); // now data can come only from the cache.
            ret = deco.Object.Find(p => p.Name == "product 2");

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(1));
        }

        [Test]
        public void Storage_Is_Hit_Only_Once_Per_Query_When_Using_First()
        {
            List<Product> lst = GetProductList();

            // Arrange
            Mock<IRepository<Product>> realRepo = new Mock<IRepository<Product>>();

            realRepo.Setup(rr => rr.First(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(
                    (Expression<Func<Product, bool>> expr,
                     Expression<Func<Product, object>>[] includeProperties) =>
                    {
                        return lst.FirstOrDefault(expr.Compile());
                    });

            Mock<CacheableRepository<Product>> deco =
                new Mock<CacheableRepository<Product>>(realRepo.Object) { CallBase = true };

            // Act - call First.
            Product ret = deco.Object.First(p => p.Name == "product 2");

            // Verify
            Assert.That(ret, Is.Not.Null);

            // Act - call First a second time.
            lst.Clear(); // now data can come only from the cache.
            ret = deco.Object.First(p => p.Name == "product 2");

            // Verify
            Assert.That(ret, Is.Not.Null);
        }

        [Test]
        public void Storage_Is_Hit_Only_Once_Per_Query_When_Using_Single()
        {
            List<Product> lst = GetProductList();

            // Arrange
            Mock<IRepository<Product>> realRepo = new Mock<IRepository<Product>>();

            realRepo.Setup(rr => rr.Single(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns(
                    (Expression<Func<Product, bool>> expr,
                     Expression<Func<Product, object>>[] includeProperties) =>
                    {
                        return lst.SingleOrDefault(expr.Compile());
                    });

            Mock<CacheableRepository<Product>> deco =
                new Mock<CacheableRepository<Product>>(realRepo.Object) { CallBase = true };

            // Act - call First.
            Product ret = deco.Object.Single(p => p.Name == "product 2");

            // Verify
            Assert.That(ret, Is.Not.Null);

            // Act - call First a second time.
            lst.Clear(); // now data can come only from the cache.
            ret = deco.Object.Single(p => p.Name == "product 2");

            // Verify
            Assert.That(ret, Is.Not.Null);
        }

        [Test]
        public void Cache_Is_Invalidated_On_Deletion()
        {
            List<Product> lst = GetProductList();

            // Arrange
            Mock<IRepository<Product>> realRepo = new Mock<IRepository<Product>>();

            realRepo.Setup(rr => rr.GetAll())
                .Returns((Expression<Func<Product, object>>[] includeProperties) => lst);

            const string key = "{21DD98A6-65CF-4190-AF33-2FA945F86112}";
            
            Mock<CacheableRepository<Product>> deco =
                new Mock<CacheableRepository<Product>>(
                    realRepo.Object, new MemoryCacheProvider(query => key), 
                    new Func<CacheItemPolicy>(PolicyFactory), CacheableRepository<Product>.TypeInvalidation.OnDelete
                    ) { CallBase = true };

            // Act - call GetAll to cache elements.
            IEnumerable<Product> ret = deco.Object.GetAll();

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(2));

            // Act - Invalidate cache by deleting an item.
            deco.Object.Delete(ret.First());

            // Now there are 3 products.
            lst.Add(
                new Product
                {
                    Id = 999,
                    Name = "product 999",
                    ProductCategory = new ProductCategory { Id = 1, Name = "cat 1" }
                }
            );

            ret = deco.Object.GetAll();

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(3));
        }

        [Test]
        public void Cache_Is_Invalidated_On_Insert()
        {
            List<Product> lst = GetProductList();

            // Arrange
            Mock<IRepository<Product>> realRepo = new Mock<IRepository<Product>>();

            realRepo.Setup(rr => rr.GetAll())
                .Returns((Expression<Func<Product, object>>[] includeProperties) => lst);

            const string key = "{0914F8ED-1AD8-4768-B098-0F13E70E0B3A}";

            Mock<CacheableRepository<Product>> deco =
                new Mock<CacheableRepository<Product>>(
                    realRepo.Object, new MemoryCacheProvider(query => key),
                    new Func<CacheItemPolicy>(PolicyFactory), CacheableRepository<Product>.TypeInvalidation.OnInsert
                    ) { CallBase = true };

            // Act - call GetAll to cache elements.
            IEnumerable<Product> ret = deco.Object.GetAll();

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(2));

            // Act - Invalidate cache by inserting an item.
            deco.Object.Insert(new Product
                                   {
                                       Id = 888,
                                       Name = "product 888",
                                       ProductCategory = new ProductCategory {Id = 1, Name = "cat 1"}
                                   });

            // Now there is only 1 product in the backend.
            lst.Clear();
            lst.Add(
                new Product
                {
                    Id = 999,
                    Name = "product 999",
                    ProductCategory = new ProductCategory { Id = 1, Name = "cat 1" }
                }
            );

            ret = deco.Object.GetAll();

            // Verify 
            Assert.That(ret.ToList(), Has.Count.EqualTo(1));
            Assert.That(ret.First().Id, Is.EqualTo(999));
        }

        [Test]
        public void Cache_Is_Invalidated_On_Update()
        {
            List<Product> lst = GetProductList();

            // Arrange
            Mock<IRepository<Product>> realRepo = new Mock<IRepository<Product>>();

            realRepo.Setup(rr => rr.GetAll())
                .Returns((Expression<Func<Product, object>>[] includeProperties) => lst);

            const string key = "{A168E99A-F3F3-4D76-B5D6-D06886E85617}";

            Mock<CacheableRepository<Product>> deco =
                new Mock<CacheableRepository<Product>>(
                    realRepo.Object, new MemoryCacheProvider(query => key),
                    new Func<CacheItemPolicy>(PolicyFactory), CacheableRepository<Product>.TypeInvalidation.OnUpdate
                    ) { CallBase = true };

            // Act - call GetAll to cache elements.
            IEnumerable<Product> ret = deco.Object.GetAll();

            // Verify
            Assert.That(ret.ToList(), Has.Count.EqualTo(2));

            // Act - Invalidate cache by updating an item.
            Product u = ret.First();
            u.Name = "New Product 1";
            deco.Object.Update(u);

            // Now there is only 1 product in the backend.
            lst.Clear();
            lst.Add(
                new Product
                {
                    Id = 999,
                    Name = "product 999",
                    ProductCategory = new ProductCategory { Id = 1, Name = "cat 1" }
                }
            );

            ret = deco.Object.GetAll();

            // Verify 
            Assert.That(ret.ToList(), Has.Count.EqualTo(1));
        }
    }
}