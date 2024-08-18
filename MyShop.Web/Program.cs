using Microsoft.EntityFrameworkCore;
using MyShop.DataAccess.Data;
using MyShop.DataAccess.Implementation;
using MyShop.Entities.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using MyShop.Utilities;
using Stripe;
using MyShop.Entities.Models;
using MyShop.DataAccess.DbInitilizer;

namespace MyShop.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection"));
            });

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(4))
                .AddDefaultTokenProviders().AddDefaultUI()
                .AddEntityFrameworkStores<AppDbContext>();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(4))
                .AddRoles<IdentityRole>()
                .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders().AddDefaultUI();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddSingleton<IEmailSender, EmailSender>();

            builder.Services.Configure<StripeData>(builder.Configuration.GetSection("stripe"));

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession();

            builder.Services.AddScoped<IDbInitilizer, DbInitilizer>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            StripeConfiguration.ApiKey = builder.Configuration.GetSection("stripe:Secretkey").Get<string>();
            
            SeedDb();

            app.UseAuthorization();

            app.UseSession();

            app.MapRazorPages();

            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "Admin",
                pattern: "{area=Admin}/{controller=Category}/{action=Index}/{id?}");

            //app.MapControllerRoute(
            //    name: "Customer",
            //    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.Run();

            //Email: Admin@myshop.com
            //Password: Admin@1234
            void SeedDb()
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbInitilizer = scope.ServiceProvider.GetRequiredService<IDbInitilizer>();
                    dbInitilizer.Initilize();
                }
            }
        }
    }
}
