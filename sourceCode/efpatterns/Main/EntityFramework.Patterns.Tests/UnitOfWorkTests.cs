using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class UnitOfWorkTests
    {

        private IRepository<ProductCategory> _catRepo;
        private IUnitOfWork _unitOfWork;
        private Context _ctx;
        private DbContextAdapter _adp;

        [TestFixtureSetUp]
        public void CreateContext()
        {
            _ctx = new Context();
            _adp = new DbContextAdapter(_ctx);

            _catRepo = new Repository<ProductCategory>(_adp);
            _unitOfWork = new UnitOfWork(_adp);
        }

        [Test]
        public void Commit_Store_Elements()
        {
            ProductCategory cat = new ProductCategory {Name = "Temporary cat"};
            _catRepo.Insert(cat);
            _unitOfWork.Commit();

            Assert.That(cat, Has.Property("Id").GreaterThan(0));

            using (var ctx = new Context())
            {
                var adp = new DbContextAdapter(ctx);
                var repo = new Repository<ProductCategory>(adp);
                
                Assert.That(repo.First(pc => pc.Name == cat.Name), Is.Not.Null);
            }
        }

    }
}