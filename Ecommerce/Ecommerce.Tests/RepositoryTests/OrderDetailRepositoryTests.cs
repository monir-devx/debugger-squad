using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Tests.RepositoryTests
{
    public class OrderDetailRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly OrderDetailRepository _orderDetailRepo;

        public OrderDetailRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _orderDetailRepo = new OrderDetailRepository(_db);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Seed necessary related data first
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

            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "test@example.com",
                Email = "test@example.com",
                Name = "Test User"
            };

            var orderHeader = new OrderHeader
            {
                Id = 1,
                ApplicationUserId = "user1",
                OrderDate = DateTime.Now,
                ShippingDate = DateTime.Now.AddDays(1),
                OrderTotal = 100,
                OrderStatus = "Pending",
                PaymentStatus = "Pending",
                PhoneNumber = "1234567890",
                StreetAddress = "123 Test St",
                City = "Testville",
                State = "TS",
                PostalCode = "12345",
                Name = "Test User"
            };

            _db.Products.Add(product);
            _db.ApplicationUsers.Add(user);
            _db.OrderHeaders.Add(orderHeader);

            // Seed order details
            _db.OrderDetails.AddRange(
                new OrderDetail
                {
                    Id = 1,
                    OrderHeaderId = 1,
                    ProductId = 1,
                    Count = 2,
                    Price = 90
                },
                new OrderDetail
                {
                    Id = 2,
                    OrderHeaderId = 1,
                    ProductId = 1,
                    Count = 3,
                    Price = 85
                }
            );

            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Get_ValidId_ReturnsOrderDetail()
        {
            // Act
            var orderDetail = _orderDetailRepo.Get(od => od.Id == 1, includeProperties: "OrderHeader,Product");

            // Assert
            Assert.NotNull(orderDetail);
            Assert.Equal(2, orderDetail.Count);
            Assert.Equal(90, orderDetail.Price);
            Assert.NotNull(orderDetail.OrderHeader);
            Assert.NotNull(orderDetail.Product);
        }

        [Fact]
        public void GetAll_ReturnsAllOrderDetails()
        {
            // Act
            var orderDetails = _orderDetailRepo.GetAll();

            // Assert
            Assert.Equal(2, orderDetails.Count());
        }

        [Fact]
        public void GetAll_WithIncludeProperties_ReturnsDetailsWithRelationships()
        {
            // Act
            var orderDetails = _orderDetailRepo.GetAll(includeProperties: "OrderHeader,Product");

            // Assert
            Assert.Equal(2, orderDetails.Count());
            Assert.All(orderDetails, od =>
            {
                Assert.NotNull(od.OrderHeader);
                Assert.NotNull(od.Product);
            });
        }

        [Fact]
        public void Add_ValidOrderDetail_InsertsIntoDatabase()
        {
            // Arrange
            var newOrderDetail = new OrderDetail
            {
                OrderHeaderId = 1,
                ProductId = 1,
                Count = 5,
                Price = 80
            };

            // Act
            _orderDetailRepo.Add(newOrderDetail);
            _db.SaveChanges();

            // Assert
            Assert.Equal(3, _db.OrderDetails.Count());
        }

        [Fact]
        public void Remove_ValidOrderDetail_DeletesFromDatabase()
        {
            // Arrange
            var orderDetail = _db.OrderDetails.Find(1);

            // Act
            _orderDetailRepo.Remove(orderDetail);
            _db.SaveChanges();

            // Assert
            Assert.Single(_db.OrderDetails);
        }

        
    }
}