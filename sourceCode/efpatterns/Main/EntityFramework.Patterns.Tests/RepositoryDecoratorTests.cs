using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;

namespace EntityFramework.Patterns.Tests
{
    [TestFixture]
    public class RepositoryDecoratorTests
    {

        [Test]
        public void RepositoryDecoratorBase_Delegate_Calls_To_Surrogate()
        {
            Mock<IRepository<Product>> realRepo = new Mock<IRepository<Product>>();

            realRepo.Setup(rr => rr.AsQueryable()).Returns(new List<Product>{new Product()}.AsQueryable);
            realRepo.Setup(rr => rr.Delete(It.IsAny<Product>()));
            realRepo.Setup(rr => rr.Find(It.IsAny<Expression<Func<Product, bool>>>()));
            realRepo.Setup(rr => rr.GetAll());
            realRepo.Setup(rr => rr.Insert(It.IsAny<Product>()));
            realRepo.Setup(rr => rr.Single(It.IsAny<Expression<Func<Product, bool>>>()));
            realRepo.Setup(rr => rr.Update(It.IsAny<Product>()));


            Mock<RepositoryDecoratorBase<Product>> deco = new Mock<RepositoryDecoratorBase<Product>>(realRepo.Object)
                                                              {CallBase = true};

            deco.Object.AsQueryable();
            deco.Object.Delete(new Product());
            deco.Object.Find(p => p.Id > 0);
            deco.Object.GetAll();
            deco.Object.Insert(new Product());
            deco.Object.Single(p => p.Id > 0);
            deco.Object.Update(new Product());

            realRepo.VerifyAll();
        }
    }
}