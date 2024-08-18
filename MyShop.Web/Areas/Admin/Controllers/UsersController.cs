using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.DataAccess.Data;
using MyShop.Utilities;
using System.Security.Claims;

namespace MyShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class UsersController : Controller
    {
        private readonly AppDbContext context;

        public UsersController(AppDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;
            return View(context.ApplicationUsers.Where(x => x.Id != userId).ToList());
        }

        public IActionResult LockUnlock(string? id)
        {
            var user = context.ApplicationUsers.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            if(user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now.AddHours(4);
            }
            else
            {
                user.LockoutEnd = DateTime.Now;

            }

            context.SaveChanges();
            return RedirectToAction("Index", "Users", new { area = "Admin"});
        }
    }
}
