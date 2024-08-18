using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.DataAccess.Data;
using MyShop.Entities.Models;
using MyShop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.DbInitilizer
{
    public class DbInitilizer : IDbInitilizer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public DbInitilizer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public void Initilize()
        {
            //Migration
            try
            {
                if(_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch(Exception)
            {
                throw;
            }

            //Roles
            if (!_roleManager.RoleExistsAsync(SD.AdminRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.AdminRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.EditorRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole)).GetAwaiter().GetResult();


                //User
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "Admin@myshop.com",
                    Email = "Admin@myshop.com",
                    Name = "Administrator",
                    PhoneNumber = "1234567890",
                    Address = "Cairo",


                }, "Admin@1234").GetAwaiter().GetResult();

                ApplicationUser user = _context.ApplicationUsers.FirstOrDefault(x => x.Email == "Admin@myshop.com");

                _userManager.AddToRoleAsync(user, SD.AdminRole).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
