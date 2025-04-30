using System.Linq.Expressions;
using System.Security.Claims;
using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Ecommerce.Utility;
using EcommerceWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Stripe;
using Stripe.Checkout;
using Xunit;

namespace Ecommerce.Tests.ControllerTests
{
    public class OrderControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly OrderController _controller;
        private readonly DefaultHttpContext _httpContext;
        private readonly ITempDataDictionary _tempData;

        public OrderControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _httpContext = new DefaultHttpContext();
            _tempData = new TempDataDictionary(_httpContext, Mock.Of<ITempDataProvider>());

            _controller = new OrderController(_mockUnitOfWork.Object)
            {
                TempData = _tempData,
                ControllerContext = new ControllerContext { HttpContext = _httpContext }
            };
        }

        // ====================== Details Action ======================
        [Fact]
        public void Details_ValidOrderId_ReturnsViewWithOrderVM()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1, ApplicationUserId = "user1" };
            var orderDetails = new List<OrderDetail>
    {
        new OrderDetail
        {
            OrderHeaderId = 1,
            Product = new Ecommerce.Models.Product()  // Explicit namespace
        }
    };

            _mockUnitOfWork.Setup(u => u.OrderHeader.Get(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>())).Returns(orderHeader);

            _mockUnitOfWork.Setup(u => u.OrderDetail.GetAll(
                It.IsAny<Expression<Func<OrderDetail, bool>>>(),
                It.IsAny<string>())).Returns(orderDetails);

            // Act
            var result = _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OrderVM>(viewResult.Model);
            Assert.Equal(1, model.OrderHeader.Id);
            Assert.Single(model.OrderDetail);
        }

        [Fact]
        public void Details_InvalidOrderId_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid order ID");

            // Act
            var result = _controller.Details(-1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ====================== UpdateOrderDetail Action ======================
        [Fact]
        public void UpdateOrderDetail_ValidModel_UpdatesOrderAndRedirects()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1 };
            _controller.OrderVM = new OrderVM { OrderHeader = orderHeader };

            _mockUnitOfWork.Setup(u => u.OrderHeader.Get(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                null)).Returns(orderHeader);

            // Simulate admin role
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, SD.Role_Admin) };
            var identity = new ClaimsIdentity(claims);
            _httpContext.User = new ClaimsPrincipal(identity);

            // Act
            var result = _controller.UpdateOrderDetail();

            // Assert
            _mockUnitOfWork.Verify(u => u.OrderHeader.Update(orderHeader), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            Assert.Equal("Order Details Updated Successfully.", _controller.TempData["Success"]);
            Assert.IsType<RedirectToActionResult>(result);
        }

        // ====================== StartProcessing Action ======================
        [Fact]
        public void StartProcessing_AdminRole_UpdatesStatusAndRedirects()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1 };
            _controller.OrderVM = new OrderVM { OrderHeader = orderHeader };

            var claims = new List<Claim> { new Claim(ClaimTypes.Role, SD.Role_Admin) };
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Mock the OrderHeader repository
            var mockOrderHeaderRepo = new Mock<IOrderHeaderRepository>();
            mockOrderHeaderRepo.Setup(repo => repo.UpdateStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            ));

            _mockUnitOfWork.SetupGet(u => u.OrderHeader).Returns(mockOrderHeaderRepo.Object);
            _mockUnitOfWork.Setup(u => u.Save()).Verifiable();

            // Act
            var result = _controller.StartProcessing();

            // Assert
            mockOrderHeaderRepo.Verify(repo => repo.UpdateStatus(
                1,
                SD.StatusInProcess,
                null
            ), Times.Once);

            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            Assert.Equal("Order Details Updated Successfully.", _controller.TempData["Success"]);
            Assert.IsType<RedirectToActionResult>(result);
        }

        // ====================== ShipOrder Action ======================
        [Fact]
        public void ShipOrder_ValidData_UpdatesShippingInfo()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1, PaymentStatus = SD.PaymentStatusDelayedPayment };
            _controller.OrderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 1, Carrier = "FedEx", TrackingNumber = "123" } };

            _mockUnitOfWork.Setup(u => u.OrderHeader.Get(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                null)).Returns(orderHeader);

            var claims = new List<Claim> { new Claim(ClaimTypes.Role, SD.Role_Admin) };
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            var result = _controller.ShipOrder();

            // Assert
            Assert.Equal(SD.StatusShipped, orderHeader.OrderStatus);
            Assert.Equal("FedEx", orderHeader.Carrier);
            Assert.Equal("123", orderHeader.TrackingNumber);
            Assert.IsType<RedirectToActionResult>(result);
        }

        // ====================== CancelOrder Action ======================
        [Fact]
        public void CancelOrder_RegularOrder_UpdatesStatusAndRedirects()
        {
            // Arrange
            var orderHeader = new OrderHeader
            {
                Id = 1,
                PaymentStatus = SD.PaymentStatusPending
            };

            _controller.OrderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 1 } };

            // Mock service layer
            _mockUnitOfWork.Setup(u => u.OrderHeader.Get(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                null))
                .Returns(orderHeader);

            // Act
            var result = _controller.CancelOrder();

            // Assert
            _mockUnitOfWork.Verify(u => u.OrderHeader.UpdateStatus(
                1,
                SD.StatusCancelled,
                SD.StatusCancelled
            ), Times.Once);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", ((RedirectToActionResult)result).ActionName);
        }

        [Fact]
        public void CancelOrder_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.OrderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 1 } };
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.CancelOrder();

            // Assert
            // Change expected type to match controller's actual return type
            Assert.IsType<BadRequestResult>(result);
            _mockUnitOfWork.Verify(u => u.OrderHeader.UpdateStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        [Fact]
        public void CancelOrder_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            _controller.OrderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 999 } };

            // Mock the OrderHeader repository
            var mockOrderHeaderRepo = new Mock<IOrderHeaderRepository>();
            mockOrderHeaderRepo.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                null))
                .Returns((OrderHeader)null);

            _mockUnitOfWork.SetupGet(u => u.OrderHeader).Returns(mockOrderHeaderRepo.Object);

            // Act
            var result = _controller.CancelOrder();

            // Assert
            Assert.IsType<NotFoundResult>(result);
            mockOrderHeaderRepo.Verify(repo => repo.UpdateStatus(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Never);

            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }

        // ====================== GetAll API ======================
        [Fact]
        public void GetAll_AdminRole_ReturnsAllOrders()
        {
            // Arrange
            var orders = new List<OrderHeader> { new OrderHeader { Id = 1 } };
            _mockUnitOfWork.Setup(u => u.OrderHeader.GetAll(null, "ApplicationUser")).Returns(orders);

            var claims = new List<Claim> { new Claim(ClaimTypes.Role, SD.Role_Admin) };
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            var result = _controller.GetAll(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal(orders, jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
        }
    }
}