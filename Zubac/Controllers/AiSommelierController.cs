using Microsoft.AspNetCore.Mvc;
using Zubac.Interfaces;
using Zubac.Models;

namespace Zubac.Controllers
{
    public class AiSommelierController : Controller
    {
        private readonly IAiSommelierService _service;

        public AiSommelierController(IAiSommelierService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);

            var model = new AiSommelierViewModel
            {
                FoodArticles = await _service.GetFoodArticlesAsync(restaurantId)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetRecommendation(int foodId)
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);

            var drink = await _service.GetDrinkRecommendationAsync(restaurantId, foodId);

            if (drink == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No drink recommendation available.",
                    drinkId = 0
                });
            }

            return Json(new
            {
                success = true,
                message = drink.Explanation,
                drinkId = drink.Id
            });
        }
    }
}
