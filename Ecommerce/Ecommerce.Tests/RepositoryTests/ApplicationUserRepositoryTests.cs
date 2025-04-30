using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Tests.RepositoryTests
{
    public class ApplicationUserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ApplicationUserRepository _userRepo;

        public ApplicationUserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _userRepo = new ApplicationUserRepository(_db);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _db.ApplicationUsers.AddRange(
                new ApplicationUser
                {
                    Id = "user1",
                    UserName = "user1@test.com",
                    Email = "user1@test.com",
                    Name = "John Doe",
                    StreetAddress = "123 Main St",
                    City = "New York",
                    State = "NY",
                    PostalCode = "10001",
                    CompanyId = 1
                },
                new ApplicationUser
                {
                    Id = "user2",
                    UserName = "user2@test.com",
                    Email = "user2@test.com",
                    Name = "Jane Smith",
                    StreetAddress = "456 Oak Ave",
                    City = "Los Angeles",
                    State = "CA",
                    PostalCode = "90001"
                }
            );
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Get_ValidId_ReturnsUserWithProperties()
        {
            // Act
            var user = _userRepo.Get(u => u.Id == "user1", includeProperties: "Company");

            // Assert
            Assert.NotNull(user);
            Assert.Equal("John Doe", user.Name);
            Assert.Equal("123 Main St", user.StreetAddress);
            Assert.Equal(1, user.CompanyId);
        }

        [Fact]
        public void GetAll_WithIncludeProperties_ReturnsUsersWithRelationships()
        {
            // Act
            var users = _userRepo.GetAll(includeProperties: "Company");

            // Assert
            Assert.Equal(2, users.Count());
            Assert.Contains(users, u => u.CompanyId == 1);
        }

        [Fact]
        public void Add_ValidUser_InsertsIntoDatabase()
        {
            // Arrange
            var newUser = new ApplicationUser
            {
                Id = "user3",
                UserName = "user3@test.com",
                Email = "user3@test.com",
                Name = "New User"
            };

            // Act
            _userRepo.Add(newUser);
            _db.SaveChanges();

            // Assert
            Assert.Equal(3, _db.ApplicationUsers.Count());
        }

        [Fact]
        public void Remove_ValidUser_DeletesFromDatabase()
        {
            // Arrange
            var user = _db.ApplicationUsers.Find("user1");

            // Act
            _userRepo.Remove(user);
            _db.SaveChanges();

            // Assert
            Assert.Single(_db.ApplicationUsers);
        }

        

        [Fact]
        public void Add_UserMissingRequiredFields_ThrowsException()
        {
            // Arrange
            var invalidUser = new ApplicationUser
            {
                Id = "invalid",
                // Missing required Name and Email
            };

            // Act & Assert
            _userRepo.Add(invalidUser);
            Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
        }
    }
}