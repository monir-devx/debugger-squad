using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Utility;
using EcommerceWeb.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.Tests.ControllerTests
{
    public class HomeControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly HomeController _controller;
        private readonly DefaultHttpContext _httpContext;
        private readonly ITempDataDictionary _tempData;

        public HomeControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<HomeController>>();
            _httpContext = new DefaultHttpContext();
            _tempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());

            _controller = new HomeController(_mockLogger.Object, _mockUnitOfWork.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _httpContext },
                TempData = _tempData
            };
        }

        #region Index Tests
        [Fact]
        public void Index_ReturnsViewWithProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Title = "Test Product",
                    Category = new Category { Name = "Test Category" },
                    ProductImages = new List<ProductImage>()
                }
            };

            var mockProductRepo = new Mock<IProductRepository>();
            mockProductRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Product, bool>>>(),
                "Category,ProductImages"))
                .Returns(products);

            _mockUnitOfWork.SetupGet(u => u.Product).Returns(mockProductRepo.Object);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Test Product", model.First().Title);
        }
        #endregion

        #region Details GET Tests
        [Fact]
        public void Details_ReturnsViewWithShoppingCart()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                Category = new Category { Name = "Test Category" },
                ProductImages = new List<ProductImage>()
            };

            var mockProductRepo = new Mock<IProductRepository>();
            mockProductRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<Product, bool>>>(),
                "Category,ProductImages"))
                .Returns(product);

            _mockUnitOfWork.SetupGet(u => u.Product).Returns(mockProductRepo.Object);

            // Act
            var result = _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ShoppingCart>(viewResult.Model);
            Assert.Equal(1, model.Count);
            Assert.Equal("Test Product", model.Product.Title);
        }
        #endregion

        #region Details POST Tests
        [Fact]
        public void DetailsPOST_InvalidModel_ReturnsViewWithError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");
            var cart = new ShoppingCart { ProductId = 1, Count = 1 };

            // Act
            var result = _controller.Details(cart);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(cart, viewResult.Model);
            Assert.Equal("There was an error with your submission. Please check the form and try again.",
                _controller.TempData["error"]);
        }

        [Fact]
        public void DetailsPOST_NewItem_AddsToCartAndUpdatesSession()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "test-user") };
            var identity = new ClaimsIdentity(claims, "Test");
            _httpContext.User = new ClaimsPrincipal(identity);

            // Mock the session
            var mockSession = new Mock<ISession>();
            var sessionData = new Dictionary<string, byte[]>();
            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionData[key] = value);
            mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) => sessionData.TryGetValue(key, out value));
            _httpContext.Session = mockSession.Object;

            var cart = new ShoppingCart { ProductId = 1, Count = 1 };
            var mockShoppingCartRepo = new Mock<IShoppingCartRepository>();
            mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                null))
                .Returns((ShoppingCart)null);

            mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                null))
                .Returns(new List<ShoppingCart> { new ShoppingCart() }); // Return one item to test session count

            _mockUnitOfWork.SetupGet(u => u.ShoppingCart).Returns(mockShoppingCartRepo.Object);

            // Act
            var result = _controller.Details(cart);

            // Assert
            _mockUnitOfWork.Verify(u => u.ShoppingCart.Add(It.IsAny<ShoppingCart>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            Assert.Equal("Cart updated successfully", _controller.TempData["success"]);
            Assert.IsType<RedirectToActionResult>(result);

            // Verify session was set
            mockSession.Verify(s => s.Set(SD.SessionCart, It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public void DetailsPOST_ExistingItem_UpdatesQuantity()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "test-user") };
            var identity = new ClaimsIdentity(claims, "Test");
            _httpContext.User = new ClaimsPrincipal(identity);

            var existingCart = new ShoppingCart { Id = 1, ProductId = 1, Count = 1, ApplicationUserId = "test-user" };
            var newCart = new ShoppingCart { ProductId = 1, Count = 2 };

            var mockShoppingCartRepo = new Mock<IShoppingCartRepository>();
            mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                null))
                .Returns(existingCart);

            _mockUnitOfWork.SetupGet(u => u.ShoppingCart).Returns(mockShoppingCartRepo.Object);

            // Act
            var result = _controller.Details(newCart);

            // Assert
            Assert.Equal(3, existingCart.Count); // 1 existing + 2 new
            _mockUnitOfWork.Verify(u => u.ShoppingCart.Update(existingCart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            Assert.Equal("Cart updated successfully", _controller.TempData["success"]);
            Assert.IsType<RedirectToActionResult>(result);
        }
        #endregion

        #region Other Actions
        [Fact]
        public void Privacy_ReturnsView()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            // Arrange
            _httpContext.TraceIdentifier = "test-trace-id";

            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal("test-trace-id", model.RequestId);
        }
        #endregion
    }
}