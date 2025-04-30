using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Tests.RepositoryTests
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ProductRepository _productRepo;

        public ProductRepositoryTests()
        {
            // Configure in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _productRepo = new ProductRepository(_db);

            // Seed initial data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add a category first
            _db.Categories.Add(new Category { Id = 1, Name = "Test Category" });

            // Add a product with images
            var product = new Product
            {
                Id = 1,
                Title = "Original Title",
                ISBN = "123-4567890",
                Price = 19.99,
                Description = "Original Description",
                CategoryId = 1,
                Author = "Original Author",
                ProductImages = new List<ProductImage>
                {
                    new ProductImage { Id = 1, ImageUrl = "image1.jpg" },
                    new ProductImage { Id = 2, ImageUrl = "image2.jpg" }
                }
            };
            _db.Products.Add(product);
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Get_ValidId_ReturnsProduct()
        {
            // Act
            var product = _productRepo.Get(p => p.Id == 1, includeProperties: "ProductImages");

            // Assert
            Assert.NotNull(product);
            Assert.Equal("Original Title", product.Title);
            Assert.Equal(2, product.ProductImages?.Count);
        }

        [Fact]
        public void GetAll_WithFilter_ReturnsFilteredProducts()
        {
            // Act
            var products = _productRepo.GetAll(p => p.Price > 10);

            // Assert
            Assert.Single(products);
        }

        [Fact]
        public void Add_ValidProduct_InsertsIntoDatabase()
        {
            // Arrange
            var newProduct = new Product
            {
                Id = 2,
                Title = "New Product",
                ISBN = "123-4567890",  // Required
                Author = "Test Author",  // Required
                Description = "Test Description",  // Required
                Price = 29.99,
                ListPrice = 39.99,  // Required
                Price50 = 34.99,    // Required
                Price100 = 29.99,   // Required
                CategoryId = 1      // Required (matches seeded category)
            };

            // Act
            _productRepo.Add(newProduct);
            _db.SaveChanges();

            // Assert
            Assert.Equal(2, _db.Products.Count());
        }

        [Fact]
        public void Add_ProductMissingRequiredFields_ThrowsException()
        {
            // Arrange
            var invalidProduct = new Product { Id = 3, Title = "Invalid" }; // Missing required fields

            // Act & Assert
            _productRepo.Add(invalidProduct);
            Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
        }

        [Fact]
        public void Remove_ValidProduct_DeletesFromDatabase()
        {
            // Arrange
            var product = _db.Products.First();

            // Act
            _productRepo.Remove(product);
            _db.SaveChanges();

            // Assert
            Assert.Empty(_db.Products);
        }

        [Fact]
        public void Update_ValidProduct_UpdatesAllProperties()
        {
            // Arrange
            var updatedProduct = new Product
            {
                Id = 1,
                Title = "Updated Title",
                ISBN = "987-6543210",
                Price = 29.99,
                Description = "Updated Description",
                CategoryId = 1,  // Keep same category to avoid FK issues
                Author = "Updated Author"
                // Note: Not including ProductImages as they might not be updated by default
            };

            // Act
            _productRepo.Update(updatedProduct);
            _db.SaveChanges();

            // Assert
            var productInDb = _db.Products.First();
            Assert.Equal("Updated Title", productInDb.Title);
            Assert.Equal("987-6543210", productInDb.ISBN);
            Assert.Equal(29.99, productInDb.Price);
            Assert.Equal("Updated Description", productInDb.Description);
            Assert.Equal("Updated Author", productInDb.Author);
        }

        [Fact]
        public void Update_InvalidId_NoChangesMade()
        {
            // Arrange
            var originalProduct = _db.Products.First();
            var invalidProduct = new Product { Id = 999, Title = "Fake" };

            // Act
            _productRepo.Update(invalidProduct);
            _db.SaveChanges();

            // Assert
            var currentProduct = _db.Products.First();
            Assert.Equal(originalProduct.Title, currentProduct.Title);
        }
    }
}