using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zubac.Data;
using Zubac.Models;
using Zubac.Interfaces;

namespace Zubac.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAcccountService _service;

        public AccountController(IAcccountService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _service.Login(model);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            RestaurantData restaurantData = await _service.GetRestaurantData(user.RestaurantId);

           var claims = new List<Claim>
           {
     new Claim(ClaimTypes.Name, user.Username),
     new Claim("UserId", user.Id.ToString()),
     new Claim("UserRank", user.UserRank.ToString()),
     new Claim("RestaurantId", user.RestaurantId.ToString()),
     new Claim("FoodEnabled", restaurantData.FoodEnabled.ToString()),
     new Claim("FreeDrinksEnabled", restaurantData.FreeDrinksEnabled.ToString())
           };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
    .GetAwaiter().GetResult();   

            return RedirectToAction("Index", "Home");
        }
    }
}
