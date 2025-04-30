using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Ecommerce.Utility;
using EcommerceWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace Ecommerce.Tests.ControllerTests
{
    public class UserControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UserController _controller;
        private readonly DefaultHttpContext _httpContext;
        private readonly ITempDataDictionary _tempData;

        public UserControllerTests()
        {
            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Mock RoleManager
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _httpContext = new DefaultHttpContext();
            _tempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());

            _controller = new UserController(
                _mockUserManager.Object,
                _mockUnitOfWork.Object,
                _mockRoleManager.Object)
            {
                TempData = _tempData,
                ControllerContext = new ControllerContext { HttpContext = _httpContext }
            };
        }

        #region Index Tests
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
        #endregion

        #region RoleManagement GET Tests
        [Fact]
        public void RoleManagement_InvalidUserId_ReturnsNotFound()
        {
            // Act - Explicitly testing the GET version (string parameter)
            var result = _controller.RoleManagment((string)null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void RoleManagement_ValidUserId_ReturnsViewWithViewModel()
        {
            // Arrange
            var userId = "user1";
            var user = new ApplicationUser { Id = userId, CompanyId = 1 };
            var company = new Company { Id = 1, Name = "Test Company" };
            var roles = new List<IdentityRole>
    {
        new IdentityRole(SD.Role_Admin),
        new IdentityRole(SD.Role_Company)
    };

            // Mock ApplicationUser repository
            var mockAppUserRepo = new Mock<IApplicationUserRepository>();
            mockAppUserRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                "Company")).Returns(user);

            // Mock Company repository - using ICompanyRepository instead of IRepository<Company>
            var mockCompanyRepo = new Mock<ICompanyRepository>();
            mockCompanyRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Company, bool>>>(),
                null))  // Explicitly pass null for includeProperties
                .Returns(new List<Company> { company });

            // Setup UnitOfWork to return the mocked repositories
            _mockUnitOfWork.SetupGet(u => u.ApplicationUser).Returns(mockAppUserRepo.Object);
            _mockUnitOfWork.SetupGet(u => u.Company).Returns(mockCompanyRepo.Object);

            _mockRoleManager.Setup(r => r.Roles).Returns(roles.AsQueryable());
            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(new List<string> { SD.Role_Admin });

            // Act
            var result = _controller.RoleManagment(userId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RoleManagmentVM>(viewResult.Model);
            Assert.Equal(userId, model.ApplicationUser.Id);
            Assert.Equal(2, model.RoleList.Count());
            Assert.Single(model.CompanyList);
        }
        #endregion

        #region RoleManagement POST Tests
        [Fact]
        public void RoleManagementPost_InvalidModel_ReturnsSameView()
        {
            // Arrange
            var vm = new RoleManagmentVM();
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.RoleManagment(vm);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(vm, viewResult.Model);
        }

        [Fact]
        public void RoleManagementPost_RoleChanged_UpdatesUserRolesAndCompany()
        {
            // Arrange
            var vm = new RoleManagmentVM
            {
                ApplicationUser = new ApplicationUser { Id = "user1", Role = SD.Role_Company, CompanyId = 1 }
            };

            var user = new ApplicationUser { Id = "user1" };
            var oldRole = SD.Role_Admin;

            _mockUnitOfWork.Setup(u => u.ApplicationUser.Get(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                null)).Returns(user);

            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(new List<string> { oldRole });

            // Act
            var result = _controller.RoleManagment(vm);

            // Assert
            _mockUserManager.Verify(u => u.RemoveFromRoleAsync(user, oldRole), Times.Once);
            _mockUserManager.Verify(u => u.AddToRoleAsync(user, SD.Role_Company), Times.Once);
            _mockUnitOfWork.Verify(u => u.ApplicationUser.Update(user), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }
        #endregion

        #region API CALLS Tests
        [Fact]
        public void GetAll_ReturnsJsonWithUserData()
        {
            // Arrange
            var users = new List<ApplicationUser>
    {
        new ApplicationUser { Id = "user1", Company = new Company { Name = "Test" } }
    };

            // Create a mock for the ApplicationUser repository
            var mockAppUserRepo = new Mock<IApplicationUserRepository>();
            mockAppUserRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(), // filter
                "Company" // includeProperties
            )).Returns(users);

            // Setup UnitOfWork to return the mocked repository
            _mockUnitOfWork.SetupGet(u => u.ApplicationUser).Returns(mockAppUserRepo.Object);

            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(new List<string> { SD.Role_Admin });

            // Act
            var result = _controller.GetAll();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            // Use reflection to access the anonymous type properties
            var valueType = jsonResult.Value.GetType();
            var dataProperty = valueType.GetProperty("data");
            Assert.NotNull(dataProperty);

            var dataValue = dataProperty.GetValue(jsonResult.Value) as IEnumerable<ApplicationUser>;
            Assert.NotNull(dataValue);
            Assert.Single(dataValue);
            Assert.Equal("user1", dataValue.First().Id);
        }

        [Fact]
        public void LockUnlock_ValidUser_LocksUser()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", LockoutEnd = null };
            _mockUnitOfWork.Setup(u => u.ApplicationUser.Get(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                null)).Returns(user);

            // Act
            var result = _controller.LockUnlock("user1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value.GetType().GetProperty("success").GetValue(jsonResult.Value));
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void LockUnlock_InvalidUser_ReturnsError()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.ApplicationUser.Get(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                null)).Returns((ApplicationUser)null);

            // Act
            var result = _controller.LockUnlock("invalid");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False((bool)jsonResult.Value.GetType().GetProperty("success").GetValue(jsonResult.Value));
        }
        #endregion
    }
}