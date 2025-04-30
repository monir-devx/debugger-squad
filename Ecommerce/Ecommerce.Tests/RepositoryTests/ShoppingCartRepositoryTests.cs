using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Tests.RepositoryTests
{
    public class ShoppingCartRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ShoppingCartRepository _shoppingCartRepo;

        public ShoppingCartRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _shoppingCartRepo = new ShoppingCartRepository(_db);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Seed required related data first
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "test@example.com",
                Email = "test@example.com",
                Name = "Test User"
            };

            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                ISBN = "TEST123",
                Author = "Test Author",
                Description = "Test Description",
                ListPrice = 100,
                Price = 90,
                Price50 = 85,
                Price100 = 80,
                CategoryId = 1
            };

            _db.ApplicationUsers.Add(user);
            _db.Products.Add(product);

            // Seed shopping carts
            _db.ShoppingCarts.AddRange(
                new ShoppingCart
                {
                    Id = 1,
                    ProductId = 1,
                    ApplicationUserId = "user1",
                    Count = 2
                },
                new ShoppingCart
                {
                    Id = 2,
                    ProductId = 1,
                    ApplicationUserId = "user1",
                    Count = 3
                }
            );

            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Get_ValidId_ReturnsShoppingCartWithRelationships()
        {
            // Act
            var cart = _shoppingCartRepo.Get(sc => sc.Id == 1, includeProperties: "Product,ApplicationUser");

            // Assert
            Assert.NotNull(cart);
            Assert.Equal(2, cart.Count);
            Assert.NotNull(cart.Product);
            Assert.NotNull(cart.ApplicationUser);
            Assert.Equal("Test Product", cart.Product.Title);
            Assert.Equal("test@example.com", cart.ApplicationUser.Email);
        }

        [Fact]
        public void GetAll_ReturnsAllShoppingCarts()
        {
            // Act
            var carts = _shoppingCartRepo.GetAll();

            // Assert
            Assert.Equal(2, carts.Count());
        }

        [Fact]
        public void GetAll_WithIncludeProperties_ReturnsCartsWithProductsAndUsers()
        {
            // Act
            var carts = _shoppingCartRepo.GetAll(includeProperties: "Product,ApplicationUser");

            // Assert
            Assert.Equal(2, carts.Count());
            Assert.All(carts, cart =>
            {
                Assert.NotNull(cart.Product);
                Assert.NotNull(cart.ApplicationUser);
            });
        }

        [Fact]
        public void Add_ValidShoppingCart_InsertsIntoDatabase()
        {
            // Arrange
            var newCart = new ShoppingCart
            {
                ProductId = 1,
                ApplicationUserId = "user1",
                Count = 5
            };

            // Act
            _shoppingCartRepo.Add(newCart);
            _db.SaveChanges();

            // Assert
            Assert.Equal(3, _db.ShoppingCarts.Count());
        }

        [Fact]
        public void Remove_ValidShoppingCart_DeletesFromDatabase()
        {
            // Arrange
            var cart = _db.ShoppingCarts.Find(1);

            // Act
            _shoppingCartRepo.Remove(cart);
            _db.SaveChanges();

            // Assert
            Assert.Single(_db.ShoppingCarts);
        }

        

        [Fact]
        public void Add_CartMissingRequiredFields_ThrowsException()
        {
            // Arrange
            var invalidCart = new ShoppingCart
            {
                Count = 1
                // Missing ProductId and ApplicationUserId
            };

            // Act & Assert
            _shoppingCartRepo.Add(invalidCart);
            Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
        }

        
    }
}