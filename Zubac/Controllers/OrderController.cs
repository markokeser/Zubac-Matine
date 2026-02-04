using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Zubac.Data;
using Zubac.Interfaces;
using Zubac.Models;

namespace Zubac.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _service;
        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            int id = int.Parse(User.FindFirst("UserId").Value);
            var orders = await _service.GetOrders(id, restaurantId);

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            var articles = await _service.GetArticles(restaurantId);

            var model = new MakeOrderViewModel
            {
                Articles = articles.Where(x => x.RestaurantId == restaurantId && x.IsAvailable).Select(a => new ArticleViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Price = a.Price,
                    IsFood = a.IsFood,
                    RestaurantId = restaurantId
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FreeDrink()
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            var articles = await _service.GetModelArticles(restaurantId);
            var model = new MakeOrderViewModel
            {
                Articles = articles
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> OrderOnBar()
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            var articles = await _service.GetModelArticles(restaurantId);
            var model = new MakeOrderViewModel
            {
                Articles = articles
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FindOrder()
        {
            int RestaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            var waiters = await _service.GetWaiters(RestaurantId);

            var model = new FindOrderViewModel
            {
                Waiters = waiters
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FindOrder(FindOrderViewModel model)
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            model.Waiters = await _service.GetWaiters(restaurantId);

            model.Orders = await _service.SearchOrders(
                model.TableNumber,
                model.CreatedBy,
                restaurantId
            );

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(MakeOrderViewModel model)
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                model.Articles = await _service.GetModelArticles(restaurantId);

                ModelState.AddModelError("", "Please select at least one drink.");
                return View(model);
            }

            int id = int.Parse(User.FindFirst("UserId").Value);
            var response = await _service.CreateOrder(model, id, restaurantId);

            if(response.Success == false)
            {
                ModelState.AddModelError("", response.ErrorMessage);
                return View(model);
            }

            return RedirectToAction("Index", "Home"); // or wherever your order list is
        }

        [HttpPost]
        public async Task<IActionResult> OrderOnBar(MakeOrderViewModel model)
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                model.Articles = await _service.GetModelArticles(restaurantId);

                ModelState.AddModelError("", "Please select at least one drink.");
                return View("OrderOnBar", model); // render the OrderOnBar.cshtml view
            }

            int id = int.Parse(User.FindFirst("UserId").Value);
            var response = await _service.OrderOnBar(model, id, restaurantId);

            if (response.Success == false)
            {
                ModelState.AddModelError(response.ErrorTitle, response.ErrorMessage);
                return View(model);
            }

            return RedirectToAction("OrderOnBar");
        }

        [HttpPost]
        public async Task<IActionResult> FreeDrink(MakeOrderViewModel model)
        {
            int restaurantId = int.Parse(User.FindFirst("RestaurantId").Value);
            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                model.Articles = await _service.GetModelArticles(restaurantId);

                ModelState.AddModelError("", "Please select at least one drink.");
                return View("FreeDrink", model); // render the OrderOnBar.cshtml view
            }

            int id = int.Parse(User.FindFirst("UserId").Value);
            var response = await _service.CreateFreeDrink(model, id, restaurantId);

            if (response.Success == false)
            {
                ModelState.AddModelError(response.ErrorTitle, response.ErrorMessage);
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int id)
        {
            var response = await _service.FinishOrder(id);

            if (response.Success == false) return NotFound();

            return RedirectToAction(nameof(Index));
        }





        // GET: OrderController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: OrderController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: OrderController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
    }
}
