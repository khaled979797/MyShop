using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;
using System.Security.Policy;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var products = unitOfWork.Product.GetAll(IncludeWords: "Category");
            return View(products);
        }
        public IActionResult GetData()
        {
            var products = unitOfWork.Product.GetAll(IncludeWords:"Category");
            return Json(new {data = products });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductVM productVM, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    using (var filesStream = new FileStream(Path.Combine(Upload, filename + ext), FileMode.Create))
                    {
                        file.CopyTo(filesStream);
                    }
                    productVM.Product.Img = @"Images\Products\" + filename + ext;
                }
                unitOfWork.Product.Add(productVM.Product);
                unitOfWork.Complete();
                TempData["Create"] = "Item Has Been Created Successfully";
                return RedirectToAction("Index");
            }
            return View(productVM);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = unitOfWork.Product.GetFirstOrDefault(x => x.Id == id),
                CategoryList = unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);
                    if(productVM.Product.Img != null)
                    {
                        var oldImg = Path.Combine(RootPath, productVM.Product.Img.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImg))
                        {
                            System.IO.File.Delete(oldImg);
                        }
                    }

                    using (var filesStream = new FileStream(Path.Combine(Upload, filename + ext), FileMode.Create))
                    {
                        file.CopyTo(filesStream);
                    }
                    productVM.Product.Img = @"Images\Products\" + filename + ext;
                }

                unitOfWork.Product.Update(productVM.Product);
                unitOfWork.Complete();
                TempData["Update"] = "Item Has Been Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(productVM);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Product product = unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            
            if (product == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }

            unitOfWork.Product.Remove(product);
            var oldImg = Path.Combine(webHostEnvironment.WebRootPath, product.Img.TrimStart('\\'));
            if (System.IO.File.Exists(oldImg))
            {
                System.IO.File.Delete(oldImg);
            }
            unitOfWork.Complete();
            return Json(new { success = true, message = "Item Has Been Deleted Successfully" });
        }
    }
}
