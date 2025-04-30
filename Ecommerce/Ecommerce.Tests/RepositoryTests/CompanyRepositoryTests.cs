using Ecommerce.DataAccess.Data;
using Ecommerce.DataAccess.Repository;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Tests.RepositoryTests
{
    public class CompanyRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly CompanyRepository _companyRepo;

        public CompanyRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _companyRepo = new CompanyRepository(_db);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _db.Companies.AddRange(
                new Company
                {
                    Id = 1,
                    Name = "Tech Solutions",
                    StreetAddress = "123 Tech St",
                    City = "Tech City",
                    State = "CA",
                    PostalCode = "12345",
                    PhoneNumber = "555-1234"
                },
                new Company
                {
                    Id = 2,
                    Name = "Book World",
                    StreetAddress = "456 Book Ave",
                    City = "Library Town",
                    State = "NY",
                    PostalCode = "67890",
                    PhoneNumber = "555-5678"
                }
            );
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public void Get_ValidId_ReturnsCompany()
        {
            // Act
            var company = _companyRepo.Get(c => c.Id == 1);

            // Assert
            Assert.NotNull(company);
            Assert.Equal("Tech Solutions", company.Name);
        }

        [Fact]
        public void GetAll_ReturnsAllCompanies()
        {
            // Act
            var companies = _companyRepo.GetAll();

            // Assert
            Assert.Equal(2, companies.Count());
        }

        [Fact]
        public void Add_ValidCompany_InsertsIntoDatabase()
        {
            // Arrange
            var newCompany = new Company
            {
                Name = "New Company",
                StreetAddress = "789 New St",
                City = "New City",
                State = "TX",
                PostalCode = "54321",
                PhoneNumber = "555-9999"
            };

            // Act
            _companyRepo.Add(newCompany);
            _db.SaveChanges();

            // Assert
            Assert.Equal(3, _db.Companies.Count());
        }

        [Fact]
        public void Remove_ValidCompany_DeletesFromDatabase()
        {
            // Arrange
            var company = _db.Companies.Find(1);

            // Act
            _companyRepo.Remove(company);
            _db.SaveChanges();

            // Assert
            Assert.Single(_db.Companies);
        }

        

        [Fact]
        public void Add_CompanyMissingRequiredName_ThrowsException()
        {
            // Arrange
            var invalidCompany = new Company
            {
                StreetAddress = "123 Test St",
                City = "Testville",
                State = "TS",
                PostalCode = "12345",
                PhoneNumber = "1234567890"
            };

            // Act & Assert
            _companyRepo.Add(invalidCompany);
            Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
        }
    }
}