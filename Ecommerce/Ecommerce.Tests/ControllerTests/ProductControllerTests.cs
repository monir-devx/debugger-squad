using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using Ecommerce.Models.ViewModels;
using Ecommerce.Utility;
using EcommerceWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Ecommerce.Tests.ControllerTests
{
    public class ProductControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _controller = new ProductController(_mockUnitOfWork.Object, _mockWebHostEnvironment.Object);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public void Index_ReturnsViewWithProductList()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Category = new Category { Name = "Category 1" } },
                new Product { Id = 2, Title = "Product 2", Category = new Category { Name = "Category 2" } }
            };

            var mockProductRepo = new Mock<IProductRepository>();
            mockProductRepo.Setup(repo => repo.GetAll(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<string>()))
                .Returns(products.AsQueryable());

            _mockUnitOfWork.Setup(uow => uow.Product)
                .Returns(mockProductRepo.Object);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Product>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void Upsert_Get_NewProduct_ReturnsViewWithViewModel()
        {
            // Arrange
            var categories = new List<Category>
            {
             new Category { Id = 1, Name = "Category 1" },
             new Category { Id = 2, Name = "Category 2" }
            };

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            mockCategoryRepo.Setup(repo => repo.GetAll(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>()))
                .Returns(categories.AsQueryable());

            _mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(mockCategoryRepo.Object);

            // Act
            var result = _controller.Upsert(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProductVM>(viewResult.Model);
            Assert.Equal(2, model.CategoryList.Count());

            // Changed from Assert.Null to check for default values
            Assert.NotNull(model.Product); // Product should be initialized
            Assert.Equal(0, model.Product.Id); // Id should be default (0)
            Assert.Null(model.Product.Title); // Other properties should be null/default
        }

        [Fact]
        public void Upsert_Get_ExistingProduct_ReturnsViewWithViewModel()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Existing Product" };
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" }
            };

            var mockProductRepo = new Mock<IProductRepository>();
            mockProductRepo.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<string>()))
                .Returns(product);

            var mockCategoryRepo = new Mock<ICategoryRepository>();
            mockCategoryRepo.Setup(repo => repo.GetAll(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>()))
                .Returns(categories.AsQueryable());

            _mockUnitOfWork.Setup(uow => uow.Product)
                .Returns(mockProductRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Category)
                .Returns(mockCategoryRepo.Object);

            // Act
            var result = _controller.Upsert(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProductVM>(viewResult.Model);
            Assert.Equal(1, model.Product.Id);
            Assert.Single(model.CategoryList);
        }

        [Fact]
        public void DeleteImage_ValidId_DeletesImageAndRedirects()
        {
            // Arrange
            var image = new ProductImage
            {
                Id = 1,
                ProductId = 1,
                ImageUrl = "\\test\\path.jpg"
            };

            var mockProductImageRepo = new Mock<IProductImageRepository>();
            mockProductImageRepo.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<ProductImage, bool>>>(),
                It.IsAny<string>()))
                .Returns(image);

            _mockUnitOfWork.Setup(uow => uow.ProductImage)
                .Returns(mockProductImageRepo.Object);

            _mockWebHostEnvironment.Setup(env => env.WebRootPath)
                .Returns("C:\\test");

            // Act
            var result = _controller.DeleteImage(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Upsert", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues["id"]);
            _mockUnitOfWork.Verify(uow => uow.ProductImage.Remove(It.IsAny<ProductImage>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
            Assert.Equal("Image Deleted Successfully", _controller.TempData["success"]);
        }

        [Fact]
        public void GetAll_ReturnsJsonProductList()
        {
            // Arrange
            var products = new List<Product>
            {
             new Product { Id = 1, Title = "Product 1" }
            };

            var mockProductRepo = new Mock<IProductRepository>();
            mockProductRepo.Setup(repo => repo.GetAll(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<string>()))
                .Returns(products.AsQueryable());

            _mockUnitOfWork.Setup(uow => uow.Product)
                .Returns(mockProductRepo.Object);

            // Act
            var result = _controller.GetAll();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<Product>>>(
                Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult.Value));

            Assert.NotNull(response);
            Assert.True(response.ContainsKey("data"));
            Assert.Single(response["data"]);
            Assert.Equal(1, response["data"].First().Id);
        }

        [Fact]
        public void Delete_ValidId_ReturnsSuccessJson()
        {
            // Arrange
            var product = new Product { Id = 1 };

            var mockProductRepo = new Mock<IProductRepository>();
            mockProductRepo.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<string>()))
                .Returns(product);

            _mockUnitOfWork.Setup(uow => uow.Product)
                .Returns(mockProductRepo.Object);

            _mockWebHostEnvironment.Setup(env => env.WebRootPath)
                .Returns("C:\\test");

            // Act
            var result = _controller.Delete(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);

            // Serialize and deserialize to properly access properties
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult.Value));

            Assert.NotNull(response);
            Assert.True(response.ContainsKey("success"));
            Assert.True((bool)response["success"]);
            Assert.True(response.ContainsKey("message"));
            Assert.Equal("Product deleted successfully", response["message"].ToString());

            _mockUnitOfWork.Verify(uow => uow.Product.Remove(It.IsAny<Product>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }
    }
}