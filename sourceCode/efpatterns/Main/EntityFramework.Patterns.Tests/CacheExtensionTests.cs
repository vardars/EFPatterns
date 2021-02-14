using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using EntityFramework.Patterns.Caching;
using Moq;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class CacheExtensionTests
    {

        [Test]
        public void ToCacheStoresDataInCache()
        {
            // Arrange
            const string key = @"{CDC8B352-ACE1-41F9-BBC6-B958E714C211}";
            Config.Cache.DefaultProvider = new MemoryCacheProvider(keyResolver => key);
            IEnumerable<Product> lst = new[]
                                           {
                                               new Product{Id = 1, Name = "Product 1"},
                                               new Product{Id = 2, Name = "Product 2"},
                                               new Product{Id = 3, Name = "Product 3"}
                                           }; 
            // Act
            lst.ToCache();

            // Verify
            Assert.That(MemoryCache.Default.Contains(key));
            Assert.That(MemoryCache.Default.Get(key), Is.AssignableTo<IEnumerable<Product>>());
            Assert.That(MemoryCache.Default.Get(key) as IEnumerable<Product>, Has.Count.EqualTo(3));
        }

        [Test]
        public void FromCacheDoNotHitBackend()
        {
            // Arrange
            const string key = @"{9C318B2C-A13A-49B8-956D-2F9B7CA54ACB}";
            Config.Cache.DefaultProvider = new MemoryCacheProvider(keyResolver => key);
            IEnumerable<Product> lst = new[]
                                           {
                                               new Product{Id = 1, Name = "Product 1"},
                                               new Product{Id = 2, Name = "Product 2"},
                                               new Product{Id = 3, Name = "Product 3"}
                                           };
            lst.ToCache();

            // Create a mocked list that throws when iterated.
            Mock<IEnumerable<Product>> backend = new Mock<IEnumerable<Product>>();
            backend.Setup(bk => bk.GetEnumerator())
                .Throws<InvalidOperationException>();

            // Act
            IEnumerable<Product> ret = backend.Object.FromCache().ToList();

            // Verify
            Assert.That(ret, Has.Count.EqualTo(3));
            backend.Verify(bk => bk.GetEnumerator(), Times.Never());
        }
    }
}