using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Ecommerce.Tests.RepositoryTests
{
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly CategoryRepository _categoryRepo;

        public CategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _categoryRepo = new CategoryRepository(_db);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _db.Categories.AddRange(
                new Category { Id = 1, Name = "Books", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Electronics", DisplayOrder = 2 }
            );
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Get_ValidId_ReturnsCategory()
        {
            // Act
            var category = _categoryRepo.Get(c => c.Id == 1);

            // Assert
            Assert.NotNull(category);
            Assert.Equal("Books", category.Name);
            Assert.Equal(1, category.DisplayOrder);
        }

        [Fact]
        public void Get_InvalidId_ReturnsNull()
        {
            // Act
            var category = _categoryRepo.Get(c => c.Id == 999);

            // Assert
            Assert.Null(category);
        }

        [Fact]
        public void GetAll_ReturnsAllCategories()
        {
            // Act
            var categories = _categoryRepo.GetAll();

            // Assert
            Assert.Equal(2, categories.Count());
        }

        [Fact]
        public void GetAll_WithFilter_ReturnsFilteredCategories()
        {
            // Act
            var categories = _categoryRepo.GetAll(c => c.DisplayOrder > 1);

            // Assert
            Assert.Single(categories);
            Assert.Equal("Electronics", categories.First().Name);
        }

        [Fact]
        public void Add_ValidCategory_InsertsIntoDatabase()
        {
            // Arrange
            var newCategory = new Category
            {
                Name = "Clothing",
                DisplayOrder = 3
            };

            // Act
            _categoryRepo.Add(newCategory);
            _db.SaveChanges();

            // Assert
            Assert.Equal(3, _db.Categories.Count());
            Assert.Contains(_db.Categories, c => c.Name == "Clothing");
        }

        [Fact]
        public void Remove_ValidCategory_DeletesFromDatabase()
        {
            // Arrange
            var category = _db.Categories.Find(1);

            // Act
            _categoryRepo.Remove(category);
            _db.SaveChanges();

            // Assert
            Assert.Single(_db.Categories);
            Assert.DoesNotContain(_db.Categories, c => c.Id == 1);
        }

        [Fact]
        public void RemoveRange_ValidCategories_DeletesMultiple()
        {
            // Arrange
            var categories = _db.Categories.ToList();

            // Act
            _categoryRepo.RemoveRange(categories);
            _db.SaveChanges();

            // Assert
            Assert.Empty(_db.Categories);
        }

        

        [Fact]
        public void Category_WithInvalidDisplayOrder_FailsValidation()
        {
            // Arrange
            var invalidCategory = new Category
            {
                Name = "Test",
                DisplayOrder = 101
            };

            // Act & Assert
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(invalidCategory,
                new ValidationContext(invalidCategory),
                validationResults,
                true);

            Assert.False(isValid);
            Assert.Contains(validationResults,
                v => v.ErrorMessage.Contains("Display Order must be between 1-100"));
        }

    }
}