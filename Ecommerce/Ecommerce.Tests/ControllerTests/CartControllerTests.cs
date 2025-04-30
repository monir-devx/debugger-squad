using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Ecommerce.Utility;
using EcommerceWeb.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using System.Linq.Expressions;
using System.Reflection;

namespace Ecommerce.Tests.ControllerTests
{
    public class CartControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CartController _controller;
        private readonly ClaimsPrincipal _user;
        private readonly string _userId = "test-user-id";

        public CartControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
                new Claim(ClaimTypes.Name, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            _user = new ClaimsPrincipal(identity);

            _controller = new CartController(_mockUnitOfWork.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _user }
                }
            };
        }


        [Fact]
        public void Summary_ReturnsViewResult_WithUserDetails()
        {
            // Arrange
            var shoppingCarts = new List<ShoppingCart>
            {
                new ShoppingCart {
                    Id = 1,
                    ProductId = 1,
                    Count = 3,
                    ApplicationUserId = _userId,
                    Product = new Ecommerce.Models.Product { Id = 1, Price = 10 }
                }
            };

            var user = new ApplicationUser
            {
                Id = _userId,
                Name = "Test User",
                StreetAddress = "123 St",
                City = "City",
                State = "State",
                PostalCode = "12345",
                PhoneNumber = "1234567890"
            };

            _mockUnitOfWork.Setup(u => u.ShoppingCart.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                "Product"))
                .Returns(shoppingCarts);

            _mockUnitOfWork.Setup(u => u.ApplicationUser.Get(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                null))
                .Returns(user);

            // Act
            var result = _controller.Summary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ShoppingCartVM>(viewResult.Model);
            Assert.Equal("Test User", model.OrderHeader.Name);
            Assert.Equal(30, model.OrderHeader.OrderTotal);
        }


        [Fact]
        public void Plus_IncreasesCount_AndRedirectsToIndex()
        {
            // Arrange
            var cart = new ShoppingCart { Id = 1, Count = 1, ApplicationUserId = _userId };
            _mockUnitOfWork.Setup(u => u.ShoppingCart.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                null))
                .Returns(cart);

            // Act
            var result = _controller.Plus(1);

            // Assert
            Assert.Equal(2, cart.Count);
            _mockUnitOfWork.Verify(u => u.ShoppingCart.Update(cart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public void Minus_DecreasesCount_WhenCountGreaterThan1()
        {
            // Arrange
            var cart = new ShoppingCart { Id = 1, Count = 2, ApplicationUserId = _userId };
            _mockUnitOfWork.Setup(u => u.ShoppingCart.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                null))
                .Returns(cart);

            // Act
            var result = _controller.Minus(1);

            // Assert
            Assert.Equal(1, cart.Count);
            _mockUnitOfWork.Verify(u => u.ShoppingCart.Update(cart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(50, 10)]
        [InlineData(51, 8)]
        [InlineData(100, 8)]
        [InlineData(101, 5)]
        public void GetPriceBasedOnQuantity_ReturnsCorrectPrice(int quantity, double expectedPrice)
        {
            // Arrange
            var shoppingCart = new ShoppingCart
            {
                Count = quantity,
                Product = new Ecommerce.Models.Product { Price = 10, Price50 = 8, Price100 = 5 }
            };

            // Act
            var method = typeof(CartController).GetMethod("GetPriceBasedOnQuantity",
                BindingFlags.NonPublic | BindingFlags.Static);
            var result = (double)method.Invoke(null, new object[] { shoppingCart });

            // Assert
            Assert.Equal(expectedPrice, result);
        }
    }
}