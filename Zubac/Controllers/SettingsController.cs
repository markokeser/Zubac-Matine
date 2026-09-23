using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Zubac.Interfaces;
using Zubac.Models;
using Zubac.Services;

namespace Zubac.Controllers
{
    [Authorize(Policy = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ISettingsService _service;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ISettingsService service, ILogger<SettingsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private void Toast(string message, string type = "success")
        {
            TempData["Toast"] = message;
            TempData["ToastType"] = type;
        }

        // GET: SettingsController
        public IActionResult Index()
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
            int userId = int.Parse(User.FindFirst("UserId").Value);
            if (userId == 1)
                return Json(new { success = false, message = "Demo user can't change passwords." });

            var token = await _service.CreateStaffLinkAsync(id); // vraća token string ili pun link
            var link = Url.Action("SetPassword", "Settings", new { token = token }, Request.Scheme);

            return Json(new { success = true, link = link });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SetPassword(string token)
        {
            var staffLink = await _service.GetStaffLinkByTokenAsync(token);
            if (staffLink == null)
            {
                Toast("This password link is invalid or has expired. Ask your manager for a new one.", "error");
                return RedirectToAction("Login", "Account");
            }

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
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _service.SetStaffPasswordAsync(model.StaffId, model.NewPassword, model.Token);
            if (success)
            {
                Toast("Password saved — you can sign in now.");
                return RedirectToAction("Login", "Account");
            }

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

            Toast($"{model.Username} added — send them a password link to finish setup.");
            return RedirectToAction("Staff");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            var success = await _service.DeleteStaffAsync(id);

            if (!success)
                return NotFound();

            Toast("Team member removed");
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
                    StartTime = settings.StartTime,
                    EndTime = settings.EndTime,
                    RealtimeCounting = settings.RealtimeCounting
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
            {
                var claims = User.Claims
        .Where(c => c.Type != "FoodEnabled" && c.Type != "FreeDrinksEnabled")
        .ToList();

                claims.Add(new Claim("FoodEnabled", model.FoodEnabled.ToString()));
                claims.Add(new Claim("FreeDrinksEnabled", model.FreeDrinksEnabled.ToString()));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                Toast("Settings saved");
                return RedirectToAction("RestaurantSettings");
            }

            ModelState.AddModelError("", "Unable to save settings. The start time can't be changed while there are open orders — finish them first.");
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
            Toast($"“{model.Name}” added to the menu");
            return RedirectToAction(model.IsFood ? "FoodSettings" : "DrinksSettings");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteArticle(int id, bool isFood)
        {
            await _service.DeleteArticleAsync(id);
            Toast("Removed from the menu");
            return RedirectToAction(isFood ? "FoodSettings" : "DrinksSettings");
        }

        [HttpPost]
        public IActionResult ToggleSommelier([FromBody] ToggleSommelierRequest req)
        {
            var success = _service.ToggleSommelier(req.Id, req.Enabled);

            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpPost]
        public IActionResult ToggleAvailable([FromBody] ToggleSommelierRequest req)
        {
            var success = _service.ToggleAvailable(req.Id, req.Enabled);

            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateArticle(ArticleViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            var article = new Article
            {
                Id = model.Id,
                Name = model.Name,
                Price = model.Price,
                Type = model.Type,
                IsAvailable = model.IsAvailable,
                AiSommelierEnabled = model.AiSommelierEnabled
            };

            await _service.UpdateArticleAsync(article);
            Toast($"“{model.Name}” updated");

            if (model.IsFood)
                return RedirectToAction("FoodSettings", "Settings");
            else
                return RedirectToAction("DrinksSettings", "Settings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStaff(StaffViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateStaffAsync(model);
            if (!result)
                return NotFound();

            Toast($"{model.Username} updated");
            // Možeš vratiti JSON da frontend zna da je OK
            return RedirectToAction("Staff", "Settings");
        }

        public IActionResult AiMenuBuilder()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadMenuImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return RedirectToAction(nameof(AiMenuBuilder));

            try
            {
                var items = await _service.ParseMenuFromImageAsync(image);
                return View("AiMenuBuilder", items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI menu parsing failed");
                ModelState.AddModelError("", "The AI couldn't read that photo. Try a sharper, well-lit image of the menu.");
                return View("AiMenuBuilder");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmAiMenu(AiMenuConfirmViewModel model)
        {
            if (model.Items == null || !model.Items.Any())
                return RedirectToAction(nameof(AiMenuBuilder));

            await _service.SaveAiMenuAsync(
                restaurantId: int.Parse(User.FindFirst("RestaurantId").Value), 
                model.Items
            );

            var foodCount = model.Items.Count(i => i.IsFood);
            Toast($"{model.Items.Count} items added to your menu");
            return RedirectToAction(foodCount > model.Items.Count / 2 ? "FoodSettings" : "DrinksSettings");
        }

    }
}
