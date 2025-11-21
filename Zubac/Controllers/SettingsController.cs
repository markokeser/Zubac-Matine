using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zubac.Interfaces;
using Zubac.Models;
using Zubac.Services;

namespace Zubac.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsService _service;

        public SettingsController(ISettingsService service)
        {
            _service = service;
        }

        // GET: SettingsController
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Staff()
        {
            int restaurantId = int.Parse(User.FindFirst("restaurantId").Value);
            var response = await _service.GetStaff(restaurantId);

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePasswordLink(int id)
        {
            var token = await _service.CreateStaffLinkAsync(id); // vraća token string ili pun link
            var link = Url.Action("SetPassword", "Settings", new { token = token }, Request.Scheme);

            return Json(new { success = true, link = link });
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword(string token)
        {
            var staffLink = await _service.GetStaffLinkByTokenAsync(token);
            if (staffLink == null)
                return NotFound("Invalid or expired link.");

            var staff = staffLink.Staff;

            var model = new SetPasswordViewModel
            {
                StaffId = staff.Id,
                Token = token,
                Username = staff.Username
            };

            return View(model);
        }

        // POST: /Settings/SetPassword
        [HttpPost]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _service.SetStaffPasswordAsync(model.StaffId, model.NewPassword, model.Token);
            if (success)
                return RedirectToAction("Login", "Account");

            ModelState.AddModelError("", "Unable to set password. Link might be invalid or expired.");
            return View(model);
        }

        [HttpGet]
        public IActionResult AddStaff()
        {
            return View(new AddStaffViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddStaff(AddStaffViewModel model)
        {
            int restaurantId = int.Parse(User.FindFirst("restaurantId").Value);
            if (!ModelState.IsValid)
                return View(model);

            var success = await _service.AddStaffAsync(model.Username, model.UserRank, restaurantId);

            if (!success)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            return RedirectToAction("Staff");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var success = await _service.DeleteStaffAsync(id);

            if (!success)
                return NotFound();

            return RedirectToAction("Staff"); // ili "Index" ako ti je view za Staff tamo
        }

        [HttpGet]
        public async Task<IActionResult> RestaurantSettings()
        {
            int restaurantId = int.Parse(User.FindFirst("restaurantId").Value);
            var model = await _service.GetRestaurantSettingsAsync(restaurantId);

            if (model == null)
            {
                var settings = await _service.CreateDefaultSettingsAsync(restaurantId);
                model = new RestaurantSettingsViewModel
                {
                    Id = settings.Id,
                    AdminId = settings.AdminId,
                    FoodEnabled = settings.FoodEnabled,
                    FreeDrinksEnabled = settings.FreeDrinksEnabled,
                    StartTime = settings.StartTime
                };
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSettings(RestaurantSettingsViewModel model)
        {
            if (!ModelState.IsValid) return View("RestaurantSettings", model);

            var success = await _service.SaveRestaurantSettingsAsync(model);

            if (success)
                return RedirectToAction("RestaurantSettings");

            ModelState.AddModelError("", "Unable to save settings.");
            return View("RestaurantSettings", model);
        }

        [HttpGet]
        public async Task<IActionResult> FoodSettings()
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            var foodList = await _service.GetArticlesAsync(restaurantId, true); // true = IsFood
            return View(foodList);
        }

        [HttpGet]
        public async Task<IActionResult> DrinksSettings()
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            var drinksList = await _service.GetArticlesAsync(restaurantId, false); // false = IsFood
            return View(drinksList);
        }

        [HttpPost]
        public async Task<IActionResult> AddArticle(ArticleViewModel model)
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            model.RestaurantId = restaurantId;

            if (!ModelState.IsValid)
                return RedirectToAction(model.IsFood ? "FoodSettings" : "DrinksSettings");

            await _service.AddArticleAsync(model);
            return RedirectToAction(model.IsFood ? "FoodSettings" : "DrinksSettings");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteArticle(int id, bool isFood)
        {
            await _service.DeleteArticleAsync(id);
            return RedirectToAction(isFood ? "FoodSettings" : "DrinksSettings");
        }
    }
}
