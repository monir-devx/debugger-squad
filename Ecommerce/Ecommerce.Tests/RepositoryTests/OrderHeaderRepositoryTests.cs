using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository;
using Ecommerce.Models;
using Ecommerce.Utility;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Tests.RepositoryTests
{
    public class OrderHeaderRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly OrderHeaderRepository _orderHeaderRepo;

        public OrderHeaderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _orderHeaderRepo = new OrderHeaderRepository(_db);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add a user first
            _db.ApplicationUsers.Add(new ApplicationUser
            {
                Id = "user1",
                UserName = "test@example.com",
                Name = "Test User",
                Email = "test@example.com"
            });

            // Add order headers
            _db.OrderHeaders.AddRange(
                new OrderHeader
                {
                    Id = 1,
                    ApplicationUserId = "user1",
                    OrderDate = DateTime.Now,
                    ShippingDate = DateTime.Now.AddDays(1),
                    OrderTotal = 100.50,
                    OrderStatus = SD.StatusPending,
                    PaymentStatus = SD.PaymentStatusPending,
                    PhoneNumber = "1234567890",
                    StreetAddress = "123 Test St",
                    City = "Testville",
                    State = "TS",
                    PostalCode = "12345",
                    Name = "Test User"
                },
                new OrderHeader
                {
                    Id = 2,
                    ApplicationUserId = "user1",
                    OrderDate = DateTime.Now,
                    ShippingDate = DateTime.Now.AddDays(2),
                    OrderTotal = 200.75,
                    OrderStatus = SD.StatusApproved,
                    PaymentStatus = SD.PaymentStatusApproved,
                    PhoneNumber = "1234567890",
                    StreetAddress = "123 Test St",
                    City = "Testville",
                    State = "TS",
                    PostalCode = "12345",
                    Name = "Test User"
                }
            );
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        

        [Fact]
        public void UpdateStatus_ValidId_UpdatesStatus()
        {
            // Act
            _orderHeaderRepo.UpdateStatus(1, SD.StatusInProcess, SD.PaymentStatusApproved);
            _db.SaveChanges();

            // Assert
            var order = _db.OrderHeaders.Find(1);
            Assert.Equal(SD.StatusInProcess, order.OrderStatus);
            Assert.Equal(SD.PaymentStatusApproved, order.PaymentStatus);
        }

        [Fact]
        public void UpdateStatus_OnlyOrderStatus_UpdatesOnlyOrderStatus()
        {
            // Arrange
            var originalPaymentStatus = _db.OrderHeaders.Find(1).PaymentStatus;

            // Act
            _orderHeaderRepo.UpdateStatus(1, SD.StatusCancelled);
            _db.SaveChanges();

            // Assert
            var order = _db.OrderHeaders.Find(1);
            Assert.Equal(SD.StatusCancelled, order.OrderStatus);
            Assert.Equal(originalPaymentStatus, order.PaymentStatus);
        }

        [Fact]
        public void UpdateStatus_InvalidId_NoChangesMade()
        {
            // Arrange
            var originalOrder = _db.OrderHeaders.AsNoTracking().First(o => o.Id == 1);

            // Act
            _orderHeaderRepo.UpdateStatus(999, SD.StatusShipped);
            _db.SaveChanges();

            // Assert
            var order = _db.OrderHeaders.Find(1);
            Assert.Equal(originalOrder.OrderStatus, order.OrderStatus);
        }

        [Fact]
        public void UpdateStripePaymentID_ValidId_UpdatesPaymentInfo()
        {
            // Arrange
            var sessionId = "session_123";
            var paymentIntentId = "pi_123";
            var testDate = DateTime.Now;

            // Act
            _orderHeaderRepo.UpdateStripePaymentID(1, sessionId, paymentIntentId);
            _db.SaveChanges();

            // Assert
            var order = _db.OrderHeaders.Find(1);
            Assert.Equal(sessionId, order.SessionId);
            Assert.Equal(paymentIntentId, order.PaymentIntentId);
            Assert.True((order.PaymentDate - testDate).TotalSeconds < 5); // Within 5 seconds
        }

        [Fact]
        public void UpdateStripePaymentID_NullSessionId_OnlyUpdatesPaymentIntent()
        {
            // Arrange
            var originalSessionId = _db.OrderHeaders.Find(1).SessionId;
            var paymentIntentId = "pi_123";

            // Act
            _orderHeaderRepo.UpdateStripePaymentID(1, null, paymentIntentId);
            _db.SaveChanges();

            // Assert
            var order = _db.OrderHeaders.Find(1);
            Assert.Equal(originalSessionId, order.SessionId);
            Assert.Equal(paymentIntentId, order.PaymentIntentId);
        }

        [Fact]
        public void UpdateStripePaymentID_InvalidId_NoChangesMade()
        {
            // Arrange
            var originalOrder = _db.OrderHeaders.AsNoTracking().First(o => o.Id == 1);

            // Act
            _orderHeaderRepo.UpdateStripePaymentID(999, "session_123", "pi_123");
            _db.SaveChanges();

            // Assert
            var order = _db.OrderHeaders.Find(1);
            Assert.Equal(originalOrder.SessionId, order.SessionId);
            Assert.Equal(originalOrder.PaymentIntentId, order.PaymentIntentId);
        }

        [Fact]
        public void Add_OrderHeaderWithRequiredFields_SavesSuccessfully()
        {
            // Arrange
            var newOrder = new OrderHeader
            {
                ApplicationUserId = "user1",
                OrderDate = DateTime.Now,
                ShippingDate = DateTime.Now.AddDays(1),
                OrderTotal = 150.25,
                PhoneNumber = "1234567890",
                StreetAddress = "123 Test St",
                City = "Testville",
                State = "TS",
                PostalCode = "12345",
                Name = "Test User"
            };

            // Act
            _orderHeaderRepo.Add(newOrder);
            _db.SaveChanges();

            // Assert
            Assert.Equal(3, _db.OrderHeaders.Count());
        }

        [Fact]
        public void Get_WithIncludeProperties_ReturnsOrderWithRelationships()
        {
            // Act
            var order = _orderHeaderRepo.Get(o => o.Id == 1, includeProperties: "ApplicationUser");

            // Assert
            Assert.NotNull(order);
            Assert.NotNull(order.ApplicationUser);
            Assert.Equal("test@example.com", order.ApplicationUser.Email);
        }
    }
}