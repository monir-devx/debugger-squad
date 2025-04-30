using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.Models;
using EcommerceWeb.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Ecommerce.Tests.ControllerTests
{
    public class CategoryControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly CategoryController _controller;
        private readonly List<Category> _categories;

        public CategoryControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork.SetupGet(u => u.Category).Returns(_mockCategoryRepository.Object);
            _controller = new CategoryController(_mockUnitOfWork.Object);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            _categories = new List<Category>
            {
                new Category { Id = 1, Name = "Electronics", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Books", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Clothing", DisplayOrder = 3 }
            };
        }

        [Fact]
        public void Index_ReturnsViewWithAllCategories()
        {
            _mockCategoryRepository.Setup(r => r.GetAll(null, null)).Returns(_categories);

            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Category>>(viewResult.Model);
            Assert.Equal(3, ((List<Category>)model).Count);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Post_ValidModel_RedirectsToIndex()
        {
            var newCategory = new Category { Name = "Test Category", DisplayOrder = 4 };

            var result = _controller.Create(newCategory);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Category created successfully", _controller.TempData["success"]);
            _mockCategoryRepository.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void Create_Post_InvalidModel_ReturnsView()
        {
            var invalidCategory = new Category { Name = "Test", DisplayOrder = 0 };
            _controller.ModelState.AddModelError("DisplayOrder", "Required");

            var result = _controller.Create(invalidCategory);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            _mockCategoryRepository.Verify(r => r.Add(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void Edit_Get_ValidId_ReturnsViewWithCategory()
        {
            _mockCategoryRepository
    .Setup(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
    .Returns(new Category { Id = 1, Name = "Test Category", DisplayOrder = 1 });

            var result = _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Category>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public void Edit_Get_NullId_ReturnsNotFound()
        {
            var result = _controller.Edit((int?)null); // Explicitly cast to int?
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Get_NonExistentId_ReturnsNotFound()
        {
            // Mock the repository to return null when a non-existent category ID is requested
            _mockCategoryRepository
                .Setup(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
                .Returns((Category?)null); // Return null for a non-existent category

            var result = _controller.Edit(999); // 999 is a non-existent ID
            Assert.IsType<NotFoundResult>(result); // Assert that it returns NotFound
        }

        [Fact]
        public void Edit_Post_ValidModel_RedirectsToIndex()
        {
            var updatedCategory = new Category { Id = 1, Name = "Updated", DisplayOrder = 5 };

            var result = _controller.Edit(updatedCategory);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Category edited successfully", _controller.TempData["success"]);
            _mockCategoryRepository.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void Edit_Post_InvalidModel_ReturnsView()
        {
            var invalidCategory = new Category { Id = 1, Name = "Updated", DisplayOrder = 0 };
            _controller.ModelState.AddModelError("DisplayOrder", "Required");

            var result = _controller.Edit(invalidCategory);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
            _mockCategoryRepository.Verify(r => r.Update(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void Delete_Get_ValidId_ReturnsViewWithCategory()
        {
            _mockCategoryRepository
    .Setup(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
    .Returns(new Category { Id = 1, Name = "Test Category", DisplayOrder = 1 });

            var result = _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Category>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public void Delete_Get_NullId_ReturnsNotFound()
        {
            var result = _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_Post_ValidId_RedirectsToIndex()
        {
            var category = _categories.First();
            _mockCategoryRepository
    .Setup(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string>()))
    .Returns(new Category { Id = 1, Name = "Test Category", DisplayOrder = 1 });

            var result = _controller.DeletePOST(category.Id);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Category deleted successfully", _controller.TempData["success"]);
            _mockCategoryRepository.Verify(r => r.Remove(It.Is<Category>(c => c.Id == category.Id)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void Delete_Post_NullId_ReturnsNotFound()
        {
            var result = _controller.DeletePOST(null);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
