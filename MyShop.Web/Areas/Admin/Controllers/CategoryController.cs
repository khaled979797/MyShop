using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Utilities;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var categories = unitOfWork.Category.GetAll();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Add(category);
                unitOfWork.Complete();
                TempData["Create"] = "Item Has Been Created Successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Category category = unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Category newcategory)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Update(newcategory);
                unitOfWork.Complete();
                TempData["Update"] = "Item Has Been Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(newcategory);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            Category category = unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {
            Category category = unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            if (ModelState.IsValid)
            {
                unitOfWork.Category.Remove(category);
                unitOfWork.Complete();
                TempData["Delete"] = "Item Has Been Deleted Successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
