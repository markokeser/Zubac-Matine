using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zubac.Interfaces;
using Zubac.Models;

namespace Zubac.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _service;
        public OrderController(IOrderService service)
        {
            _service = service;
        }

        private int RestaurantId => int.Parse(User.FindFirst("RestaurantId")!.Value);
        private int UserId => int.Parse(User.FindFirst("UserId")!.Value);

        private void Toast(string message, string type = "success")
        {
            TempData["Toast"] = message;
            TempData["ToastType"] = type;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _service.GetOrders(UserId, RestaurantId);

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new MakeOrderViewModel
            {
                Articles = await _service.GetModelArticles(RestaurantId)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FreeDrink()
        {
            var model = new MakeOrderViewModel
            {
                Articles = await _service.GetModelArticles(RestaurantId)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> OrderOnBar()
        {
            var model = new MakeOrderViewModel
            {
                Articles = await _service.GetModelArticles(RestaurantId)
            };

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> FindOrder()
        {
            var model = new FindOrderViewModel
            {
                Waiters = await _service.GetWaiters(RestaurantId)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> FindOrder(FindOrderViewModel model)
        {
            model.Waiters = await _service.GetWaiters(RestaurantId);

            model.Orders = await _service.SearchOrders(
                model.TableNumber,
                model.CreatedBy,
                RestaurantId
            );

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MakeOrderViewModel model)
        {
            if (model.TableNumber <= 0)
                ModelState.AddModelError("", "Please choose a table.");

            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                if (model.SelectedArticles == null || model.SelectedArticles.Count == 0)
                    ModelState.AddModelError("", "Please add at least one item.");

                model.Articles = await _service.GetModelArticles(RestaurantId);
                return View(model);
            }

            var response = await _service.CreateOrder(model, UserId, RestaurantId);

            if (response.Success == false)
            {
                ModelState.AddModelError("", response.ErrorMessage);
                model.Articles = await _service.GetModelArticles(RestaurantId);
                return View(model);
            }

            Toast($"Order for table {model.TableNumber} sent");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> OrderOnBar(MakeOrderViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                model.Articles = await _service.GetModelArticles(RestaurantId);

                ModelState.AddModelError("", "Please add at least one item.");
                return View("OrderOnBar", model);
            }

            var response = await _service.OrderOnBar(model, UserId, RestaurantId);

            if (response.Success == false)
            {
                ModelState.AddModelError("", response.ErrorMessage);
                model.Articles = await _service.GetModelArticles(RestaurantId);
                return View(model);
            }

            Toast("Bar order completed");
            return RedirectToAction("OrderOnBar");
        }

        [HttpPost]
        public async Task<IActionResult> FreeDrink(MakeOrderViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                model.Articles = await _service.GetModelArticles(RestaurantId);

                ModelState.AddModelError("", "Please add at least one item.");
                return View("FreeDrink", model);
            }

            var response = await _service.CreateFreeDrink(model, UserId, RestaurantId);

            if (response.Success == false)
            {
                ModelState.AddModelError("", response.ErrorMessage);
                model.Articles = await _service.GetModelArticles(RestaurantId);
                return View(model);
            }

            Toast("Free order logged");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int id)
        {
            var response = await _service.FinishOrder(id);

            if (response.Success == false) return NotFound();

            Toast("Order finished");
            return RedirectToAction(nameof(Index));
        }
    }
}
