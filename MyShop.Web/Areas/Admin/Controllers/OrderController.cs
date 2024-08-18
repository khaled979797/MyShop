using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.DataAccess.Implementation;
using MyShop.Entities.Models;
using MyShop.Entities.Repositories;
using MyShop.Entities.ViewModels;
using MyShop.Utilities;
using Stripe;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        [BindProperty]
        public OrderVM orderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetData()
        {
            IEnumerable<OrderHeader> orderHeaders;
            orderHeaders = unitOfWork.OrderHeader.GetAll(IncludeWords: "ApplicationUser");
            return Json(new { data = orderHeaders });
        }

        [HttpGet]
        public IActionResult Details(int orderid)
        {
            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderid, IncludeWords: "ApplicationUser"),
                OrderDetails = unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderid, IncludeWords: "Product")
            };
            return View(orderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderFromDb = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderVM.OrderHeader.Id);

            orderFromDb.Name = orderVM.OrderHeader.Name;
            orderFromDb.Phone = orderVM.OrderHeader.Phone;
            orderFromDb.Address = orderVM.OrderHeader.Address;
            orderFromDb.City = orderVM.OrderHeader.City;

            if(orderVM.OrderHeader.Carrier != null)
            {
                orderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (orderVM.OrderHeader.TrackingNumber != null)
            {
                orderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }

            unitOfWork.OrderHeader.Update(orderFromDb);
            unitOfWork.Complete();

            TempData["Update"] = "Item Has Been Updated Successfully";
            return RedirectToAction("Details", "Order", new { orderid = orderFromDb.Id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProccess()
        {
            unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.Proccessing, null);
            unitOfWork.Complete();

            TempData["Update"] = "Order Status Has Been Updated Successfully";
            return RedirectToAction("Details", "Order", new { orderid = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartShipping()
        {
            var orderFromDb = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderVM.OrderHeader.Id);
            orderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            orderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            orderFromDb.OrderStatus = SD.Shipped;
            orderFromDb.ShippingDate = DateTime.Now;
            unitOfWork.OrderHeader.Update(orderFromDb);
            unitOfWork.Complete();

            TempData["Update"] = "Order Has Been Shipped Successfully";
            return RedirectToAction("Details", "Order", new { orderid = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderFromDb = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderVM.OrderHeader.Id);
            if(orderFromDb.PaymentStatus == SD.Approved)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderFromDb.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(option);

                unitOfWork.OrderHeader.UpdateStatus(orderFromDb.Id, SD.Cancelled, SD.Refund);
            }
            else
            {
                unitOfWork.OrderHeader.UpdateStatus(orderFromDb.Id, SD.Cancelled, SD.Cancelled);
            }

            unitOfWork.Complete();

            TempData["Update"] = "Order Has Been Canceled Successfully";
            return RedirectToAction("Details", "Order", new { orderid = orderVM.OrderHeader.Id });
        }
    }
}
