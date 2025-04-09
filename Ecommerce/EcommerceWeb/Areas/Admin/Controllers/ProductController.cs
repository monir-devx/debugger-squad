using Ecommerce.DataAccess.Repository.IRepository;
using Ecommerce.DataAccess.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ecommerce.Models.ViewModels;

namespace EcommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u=>u.Id==id);
                return View(productVM);
            }
            
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string targetDirectory = Path.Combine(wwwRootPath, @"images\product");

                // Ensure target directory ends with a directory separator
                if (!targetDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    targetDirectory += Path.DirectorySeparatorChar;
                }

                if (file != null)
                {
                    // Generate a unique file name for the uploaded image
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    // Handle old image deletion securely
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        // Resolve full path and validate it
                        string canonicalOldPath = Path.GetFullPath(oldImagePath);

                        // Ensure the old image path is inside the target directory
                        if (canonicalOldPath.StartsWith(targetDirectory, StringComparison.Ordinal))
                        {
                            if (System.IO.File.Exists(canonicalOldPath))
                            {
                                System.IO.File.Delete(canonicalOldPath);
                            }
                        }
                        else
                        {
                            // Log or throw an exception if the old path is outside the target directory
                            TempData["error"] = "The image path is invalid.";
                            return View(productVM);
                        }
                    }

                    // Save the new image file securely
                    string newImagePath = Path.Combine(targetDirectory, fileName);
                    using (var fileStream = new FileStream(newImagePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    // Store the relative image URL
                    productVM.Product.ImageUrl = Path.Combine(@"\images\product", fileName);
                }

                // Add or update the product in the database
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                // Reload the category list if the model validation fails
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? productFromDb = _unitOfWork.Product.Get(u => u.Id == id);

            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _unitOfWork.Product.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
