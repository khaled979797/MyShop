using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Repositories;
using MyShop.Utilities;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ViewBag.Orders = unitOfWork.OrderHeader.GetAll().Count();
            ViewBag.Categories = unitOfWork.Category.GetAll().Count();
            ViewBag.Users = unitOfWork.ApplicationUser.GetAll().Count();
            ViewBag.Products = unitOfWork.Product.GetAll().Count();
            return View();
        }
    }
}
