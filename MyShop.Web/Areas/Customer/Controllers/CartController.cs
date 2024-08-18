using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;
using Stripe.Checkout;
using System.Security.Claims;

namespace MyShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartList = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value, IncludeWords: "Product"),
            };
            foreach(var item in ShoppingCartVM.CartList)
            {
                ShoppingCartVM.TotalCarts += (item.Product.Price * item.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartid)
		{
            var shoppingCart = unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartid);
            unitOfWork.ShoppingCart.IncreaseCount(shoppingCart, 1);
            unitOfWork.Complete();
            return RedirectToAction("Index");
		}

		public IActionResult Minus(int cartid)
		{
			var shoppingCart = unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartid);

            if(shoppingCart.Count <= 1)
            {
                unitOfWork.ShoppingCart.Remove(shoppingCart);
                var count = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);

    //            unitOfWork.Complete();
				//return RedirectToAction("Index", "Home");
			}
			else
            {
				unitOfWork.ShoppingCart.DecreaseCount(shoppingCart, 1);
			}
			unitOfWork.Complete();
			return RedirectToAction("Index");
		}

		public IActionResult Remove(int cartid)
		{
			var shoppingCart = unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartid);
			unitOfWork.ShoppingCart.Remove(shoppingCart);
			unitOfWork.Complete();
            var count = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);
            return RedirectToAction("Index");
		}

        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartList = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value, IncludeWords: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.Phone = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

            foreach (var item in ShoppingCartVM.CartList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Product.Price * item.Count);
            }
            return View(ShoppingCartVM);
        }


        [HttpPost]
        public IActionResult Summary(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM.CartList = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value, IncludeWords: "Product");

            
            shoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            shoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;


            foreach (var item in shoppingCartVM.CartList)
            {
                shoppingCartVM.OrderHeader.TotalPrice += (item.Product.Price * item.Count);
            }

            unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            unitOfWork.Complete();

            foreach (var item in shoppingCartVM.CartList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };
                unitOfWork.OrderDetail.Add(orderDetail);
                unitOfWork.Complete();
            }

            string domain = "https://localhost:44350/";

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"Customer/Cart/Index",
            };

            foreach (var item in shoppingCartVM.CartList)
            {
                var sessionLineOption = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = Convert.ToInt64(item.Product.Price*100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineOption);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            shoppingCartVM.OrderHeader.SessionId = session.Id;
            unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

            //unitOfWork.ShoppingCart.RemoveRange(shoppingCartVM.CartList);
            //unitOfWork.Complete();
            //return RedirectToAction("Index", "Home");
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id);

            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            orderHeader.PaymentIntentId = session.PaymentIntentId;
            if(session.PaymentStatus.ToLower() == "paid")
            {
                unitOfWork.OrderHeader.UpdateStatus(id, SD.Approved, SD.Approved);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
                unitOfWork.Complete();
            }

            List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            unitOfWork.Complete();
            return View(id);
        }
    }
}
