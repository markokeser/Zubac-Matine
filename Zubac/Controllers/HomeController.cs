using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Zubac.Interfaces;
using Zubac.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Zubac.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountService _accountService;

        public HomeController(ILogger<HomeController> logger, IAccountService accountService)
        {
            _logger = logger;
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            RestaurantData restaurantData = await _accountService.GetRestaurantData(restaurantId);
            var claims = User.Claims
        .Where(c => c.Type != "FoodEnabled" && c.Type != "FreeDrinksEnabled")
        .ToList();

            claims.Add(new Claim("FoodEnabled", restaurantData.FoodEnabled.ToString()));
            claims.Add(new Claim("FreeDrinksEnabled", restaurantData.FreeDrinksEnabled.ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
