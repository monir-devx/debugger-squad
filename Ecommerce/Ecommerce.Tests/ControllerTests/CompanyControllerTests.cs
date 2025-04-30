using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Utility;
using EcommerceWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Ecommerce.Tests.ControllerTests
{
    public class CompanyControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CompanyController _controller;

        public CompanyControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _controller = new CompanyController(_mockUnitOfWork.Object);

            // Setup TempData
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());

            // Mock user with admin role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, SD.Role_Admin)
            }));
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public void Index_ReturnsViewWithCompanyList()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Id = 1, Name = "Company 1" },
                new Company { Id = 2, Name = "Company 2" }
            };

            var mockCompanyRepo = new Mock<ICompanyRepository>();
            mockCompanyRepo.Setup(repo => repo.GetAll(null, null))
                .Returns(companies.AsQueryable());

            _mockUnitOfWork.Setup(uow => uow.Company)
                .Returns(mockCompanyRepo.Object);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Company>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void Upsert_Get_NewCompany_ReturnsViewWithEmptyCompany()
        {
            // Act
            var result = _controller.Upsert((int?)null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Company>(viewResult.Model);
            Assert.Equal(0, model.Id);
        }

        [Fact]
        public void Upsert_Get_ExistingCompany_ReturnsViewWithCompany()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Existing Company" };

            var mockCompanyRepo = new Mock<ICompanyRepository>();
            mockCompanyRepo.Setup(repo => repo.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(),
                null))
                .Returns(company);

            _mockUnitOfWork.Setup(uow => uow.Company)
                .Returns(mockCompanyRepo.Object);

            // Act
            var result = _controller.Upsert(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Company>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public void Upsert_Post_ValidModel_CreatesNewCompany()
        {
            // Arrange
            var newCompany = new Company { Name = "New Company" };
            var mockCompanyRepo = new Mock<ICompanyRepository>();
            _mockUnitOfWork.Setup(uow => uow.Company).Returns(mockCompanyRepo.Object);

            // Act
            var result = _controller.Upsert(newCompany);

            // Assert
            mockCompanyRepo.Verify(repo => repo.Add(newCompany), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
            Assert.Equal("Company created successfully", _controller.TempData["success"]);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void Upsert_Post_ValidModel_UpdatesExistingCompany()
        {
            // Arrange
            var existingCompany = new Company { Id = 1, Name = "Existing Company" };
            var mockCompanyRepo = new Mock<ICompanyRepository>();

            // Setup the mock to return our repository
            _mockUnitOfWork.Setup(uow => uow.Company).Returns(mockCompanyRepo.Object);

            // Act
            var result = _controller.Upsert(existingCompany);

            // Assert
            mockCompanyRepo.Verify(repo => repo.Update(existingCompany), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
            Assert.Equal("Company created successfully", _controller.TempData["success"]);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void Upsert_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var invalidCompany = new Company();
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.Upsert(invalidCompany);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Company>(viewResult.Model);
            Assert.Equal(invalidCompany, model);
        }

        [Fact]
        public void GetAll_ReturnsJsonCompanyList()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Id = 1, Name = "Company 1" }
            };

            var mockCompanyRepo = new Mock<ICompanyRepository>();
            mockCompanyRepo.Setup(repo => repo.GetAll(null, null))
                .Returns(companies.AsQueryable());

            _mockUnitOfWork.Setup(uow => uow.Company)
                .Returns(mockCompanyRepo.Object);

            // Act
            var result = _controller.GetAll();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<Company>>>(
                Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult.Value));
            Assert.Single(response["data"]);
            Assert.Equal(1, response["data"].First().Id);
        }

        [Fact]
        public void Delete_ValidId_ReturnsSuccessJson()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Company 1" };

            var mockCompanyRepo = new Mock<ICompanyRepository>();
            mockCompanyRepo.Setup(repo => repo.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(),
                null))
                .Returns(company);

            _mockUnitOfWork.Setup(uow => uow.Company)
                .Returns(mockCompanyRepo.Object);

            // Act
            var result = _controller.Delete(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult.Value));
            Assert.True((bool)response["success"]);
            Assert.Equal("Company deleted successfully", response["message"].ToString());
            _mockUnitOfWork.Verify(uow => uow.Company.Remove(company), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void Delete_InvalidId_ReturnsFailureJson()
        {
            // Arrange
            Company nullCompany = null;

            var mockCompanyRepo = new Mock<ICompanyRepository>();
            mockCompanyRepo.Setup(repo => repo.Get(
                It.IsAny<System.Linq.Expressions.Expression<Func<Company, bool>>>(),
                null))
                .Returns(nullCompany);

            _mockUnitOfWork.Setup(uow => uow.Company)
                .Returns(mockCompanyRepo.Object);

            // Act
            var result = _controller.Delete(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult.Value));
            Assert.False((bool)response["success"]);
            Assert.Equal("Company not found. Deletion aborted.", response["message"].ToString());
        }
    }
}