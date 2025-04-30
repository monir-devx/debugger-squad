using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Tests.RepositoryTests
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_db);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Seed minimal data for cross-repository tests
            _db.Categories.Add(new Category { Id = 1, Name = "Test Category", DisplayOrder = 1 });
            _db.Products.Add(new Product
            {
                Id = 1,
                Title = "Test Product",
                ISBN = "TEST123",
                Author = "Author",
                Description = "Desc",
                ListPrice = 100,
                Price = 90,
                Price50 = 85,
                Price100 = 80,
                CategoryId = 1
            });
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
            // Removed _unitOfWork.Dispose() since UnitOfWork doesn't implement IDisposable
        }

        [Fact]
        public void Constructor_InitializesAllRepositories()
        {
            // Assert
            Assert.NotNull(_unitOfWork.Category);
            Assert.NotNull(_unitOfWork.Product);
            Assert.NotNull(_unitOfWork.Company);
            Assert.NotNull(_unitOfWork.ShoppingCart);
            Assert.NotNull(_unitOfWork.ApplicationUser);
            Assert.NotNull(_unitOfWork.OrderHeader);
            Assert.NotNull(_unitOfWork.OrderDetail);
            Assert.NotNull(_unitOfWork.ProductImage);
        }

        [Fact]
        public void Save_WithChanges_PersistsAllChanges()
        {
            // Arrange
            var newCategory = new Category { Name = "New Category", DisplayOrder = 2 };
            _unitOfWork.Category.Add(newCategory);

            var newProduct = new Product
            {
                Title = "New Product",
                ISBN = "NEW123",
                Author = "Author",
                Description = "Desc",
                ListPrice = 50,
                Price = 45,
                Price50 = 40,
                Price100 = 35,
                CategoryId = 1
            };
            _unitOfWork.Product.Add(newProduct);

            // Act
            _unitOfWork.Save();

            // Assert
            Assert.Equal(2, _db.Categories.Count());
            Assert.Equal(2, _db.Products.Count());
        }

        [Fact]
        public void Save_WithoutChanges_DoesNotThrow()
        {
            // Act & Assert (should not throw)
            _unitOfWork.Save();
        }

        

        [Fact]
        public void Repositories_CanPerformCRUDOperations()
        {
            // Test one repository as representative sample
            // Arrange
            var category = new Category { Name = "CRUD Test", DisplayOrder = 5 };

            // Act - Create
            _unitOfWork.Category.Add(category);
            _unitOfWork.Save();

            // Assert - Read
            var retrieved = _unitOfWork.Category.Get(c => c.Id == category.Id);
            Assert.Equal("CRUD Test", retrieved.Name);

            // Act - Update
            retrieved.Name = "Updated";
            _unitOfWork.Category.Update(retrieved);
            _unitOfWork.Save();

            // Assert
            var updated = _unitOfWork.Category.Get(c => c.Id == category.Id);
            Assert.Equal("Updated", updated.Name);

            // Act - Delete
            _unitOfWork.Category.Remove(updated);
            _unitOfWork.Save();

            // Assert
            Assert.Null(_unitOfWork.Category.Get(c => c.Id == category.Id));
        }

        
    }
}