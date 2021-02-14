using System.Collections.Generic;
using System.Data.Entity;
using EntityFramework.Patterns.Extensions;

namespace EntityFramework.Patterns.Tests
{
    
    public class Context : DbContext
    {
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductCategory> Categories { get; set; }
        public virtual DbSet<AuditableEntity> AuditableEntities { get; set; }
        public virtual DbSet<ArchivableEntity> ArchivableEntities { get; set; }

        public Context()
        {
            Database.SetInitializer(new TestDropStrategy());
        }
    }

    public class TestDropStrategy : DropCreateDatabaseAlways<Context> //DropCreateDatabaseIfModelChanges<Context>
    {
        protected override void Seed(Context context)
        {
            ProductCategory pcat = new ProductCategory {Name = "Bike"};
            new List<ProductCategory>
                {
                    pcat,
                    new ProductCategory{Name = "To be deleted"}
                }.ForEach(c => context.Categories.Add(c));
            
            context.SaveChanges();

            new List<Product>
                {
                    new Product {Id = 1, Name = "Rocky Mountain", ProductCategoryId = pcat.Id},
                    new Product {Id = 2, Name = "Skate Board"},
                    new Product {Id = 3, Name = "Rocky Mountain EVO", ProductCategoryId = pcat.Id},
                }.ForEach(p => context.Products.Add(p));

            context.SaveChanges();

            base.Seed(context);
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ProductCategoryId { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
    }

    public class ProductCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Auditable]
    public class AuditableEntity
    {
        public int Id { get; set; }
        public string Color { get; set; }
    }

    [Archivable]
    public class ArchivableEntity
    {
        public int Id { get; set; }
        public float Value { get; set; }
    }
}