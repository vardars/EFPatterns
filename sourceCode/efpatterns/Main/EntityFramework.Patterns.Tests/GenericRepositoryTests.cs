using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class GenericRepositoryTests
    {

        private IRepository<Product> _productRepo;
        private IRepository<ProductCategory> _catRepo;
        private Context _ctx;
        private DbContextAdapter _adp;

        [TestFixtureSetUp]
        public void CreateContext()
        {
            _ctx = new Context();
            _adp = new DbContextAdapter(_ctx);

            _productRepo = new Repository<Product>(_adp);
            _catRepo = new Repository<ProductCategory>(_adp);
        }

        [Test]
        public void GetAll_Should_Returns_All_Elements()
        {
            IEnumerable<Product> lst = _productRepo.GetAll();
            Assert.That(lst.ToList(), Has.Count.EqualTo(3));
        }

        [Test]
        public void First_Return_First_Element_In_Sequence()
        {
            Product prod = _productRepo.First(p => p.Name.StartsWith("Roc"));
            Assert.That(prod, Has.Property("Id").EqualTo(1));
        }

        [Test]
        public void Single_Throws_If_More_Than_1_Element_Match()
        {
            Assert.Throws<InvalidOperationException>(() => _productRepo.Single(p => p.Name.StartsWith("Roc")));
        }

        [Test]
        public void Find_Returns_Several_Elements()
        {
            IEnumerable<Product> lst = _productRepo.Find(
                p => p.Id < 100 && p.Name.Contains("o") && p.Name.Length < 20);
            Assert.That(lst.ToList(), Has.Count.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Insert_Add_An_Element()
        {
            int count = _catRepo.GetAll().Count();

            ProductCategory pcat = new ProductCategory{Name = "New product category"};
            _catRepo.Insert(pcat);

            _ctx.SaveChanges();

            int count2 = _catRepo.GetAll().Count();

            Assert.That(pcat, Has.Property("Id").GreaterThan(0));
            Assert.That(count2, Is.GreaterThan(count));
        }

        [Test]
        public void Update_Modify_An_Element()
        {
            ProductCategory cat = _catRepo.First(c => c.Name == "Bike");
            Assert.That(cat, Is.Not.Null);

            cat.Name = "Bikes category";

            _catRepo.Update(cat);
            _ctx.SaveChanges();

            ProductCategory newCat = _catRepo.First(c => c.Name == cat.Name);
            Assert.That(newCat, Is.Not.Null);
        }

        [Test]
        public void Delete_Remove_An_Element()
        {
            int count = _catRepo.GetAll().Count();

            ProductCategory cat = _catRepo.First(p => p.Name.StartsWith("To be")); 
            Assert.That(cat, Is.Not.Null);

            _catRepo.Delete(cat);
            _ctx.SaveChanges();

            int count2 = _catRepo.GetAll().Count();

            Assert.That(count2, Is.LessThan(count));
        }

        [Test]
        public void Include_Prevent_LazyLoading()
        {
            _ctx.Configuration.LazyLoadingEnabled = false;

            Product prod = _productRepo.First(p => p.ProductCategoryId != null , p => p.ProductCategory);
            Assert.That(prod, Is.Not.Null);
            Assert.That(prod, Has.Property("ProductCategoryId").Not.Null);
            Assert.That(prod.ProductCategory.Name, Is.Not.Null);
        }
    }
}