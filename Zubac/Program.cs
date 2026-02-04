using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zubac.Data;
using Zubac.Interfaces;
using Zubac.Models;
using Zubac.Services;

namespace Zubac
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IStatisticService, StatisticService>();
            builder.Services.AddScoped<ISettingsService, SettingsService>();
            builder.Services.AddScoped<IPasswordHasher<Users>, PasswordHasher<Users>>();
            builder.Services.AddScoped<IAiSommelierService, AiSommelierService>();
            builder.Services.AddHttpClient();

            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
        options.Cookie.Name = "Zubac.AuthCookie";
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);   // Cookie lasts 30 days
        options.SlidingExpiration = true;                 // Renew cookie on activity
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new MySqlServerVersion(new Version(8, 0, 36)) // change to your MySQL version
    )
);
            builder.Services.AddScoped<IAccountService, AccountService>();

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

            app.UseAuthorization();
            app.UseAuthentication();


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
