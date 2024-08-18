using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Utilities;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index(int? page)
        {
            var pageNumber = page ?? 1;
            int pageSize = 8;
            var products = unitOfWork.Product.GetAll().ToPagedList(pageNumber, pageSize);
            return View(products);
        }


        public IActionResult Details(int ProductId)
        {
            ShoppingCart cart = new()
            {
                ProductId = ProductId,
                Product = unitOfWork.Product.GetFirstOrDefault(x => x.Id == ProductId, IncludeWords: "Category"),
                Count = 1
            };

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartObj = unitOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);

            if (cartObj == null)
            {
                unitOfWork.ShoppingCart.Add(shoppingCart);
                unitOfWork.Complete();
                HttpContext.Session.SetInt32(SD.SessionKey,
                    unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count());
            }
            else
            {
                unitOfWork.ShoppingCart.IncreaseCount(cartObj, shoppingCart.Count);
                unitOfWork.Complete();
            }

            return RedirectToAction("Index");
        }
    }
}
