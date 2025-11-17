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
        // GET: OrderController
        public async Task<IActionResult> Index()
        {
            int id = int.Parse(User.FindFirst("UserId").Value);
            var orders = await _service.GetOrders(id);

            return View(orders);
        }

        // GET: CREATE VIEW PAGE
        public async Task<IActionResult> Create()
        {
            var articles = await _service.GetArticles();

            var model = new MakeOrderViewModel
            {
                Articles = articles.Select(a => new ArticleViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Price = a.Price
                }).ToList()
            };

            return View(model);
        }

        [HttpGet] // GET: FREE DRINK VIEW PAGE
        public async Task<IActionResult> FreeDrink()
        {
            var articles = await _service.GetModelArticles();
            var model = new MakeOrderViewModel
            {
                Articles = articles
            };

            return View(model);
        }

        [HttpGet] // GET: ORDER ON BAR VIEW PAGE
        public async Task<IActionResult> OrderOnBar()
        {
            var articles = await _service.GetModelArticles();
            var model = new MakeOrderViewModel
            {
                Articles = articles
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FindOrder()
        {
            var waiters = await _service.GetWaiters();

            var model = new FindOrderViewModel
            {
                Waiters = waiters
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FindOrder(FindOrderViewModel model)
        {
            model.Waiters = await _service.GetWaiters();

            model.Orders = await _service.SearchOrders(
                model.TableNumber,
                model.CreatedBy
            );

            return View(model);
        }

        // POST: CREATE ORDER
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(MakeOrderViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                model.Articles = await _service.GetModelArticles();

                ModelState.AddModelError("", "Please select at least one drink.");
                return View(model);
            }

            int id = int.Parse(User.FindFirst("UserId").Value);
            var response = await _service.CreateOrder(model, id);

            if(response.Success == false)
            {
                ModelState.AddModelError("", response.ErrorMessage);
                return View(model);
            }

            return RedirectToAction("Index", "Home"); // or wherever your order list is
        }

        [HttpPost] // POST: CREATE ORDER ON BAR
        public async Task<IActionResult> OrderOnBar(MakeOrderViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                model.Articles = await _service.GetModelArticles();

                ModelState.AddModelError("", "Please select at least one drink.");
                return View("OrderOnBar", model); // render the OrderOnBar.cshtml view
            }

            int id = int.Parse(User.FindFirst("UserId").Value);
            var response = await _service.OrderOnBar(model, id);

            if (response.Success == false)
            {
                ModelState.AddModelError(response.ErrorTitle, response.ErrorMessage);
                return View(model);
            }

            return RedirectToAction("OrderOnBar");
        }

        [HttpPost] // POST: CREATE FREE DRINK
        public async Task<IActionResult> FreeDrink(MakeOrderViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedArticles == null || model.SelectedArticles.Count == 0)
            {
                model.Articles = await _service.GetModelArticles();

                ModelState.AddModelError("", "Please select at least one drink.");
                return View("FreeDrink", model); // render the OrderOnBar.cshtml view
            }

            int id = int.Parse(User.FindFirst("UserId").Value);
            var response = await _service.CreateFreeDrink(model, id);

            if (response.Success == false)
            {
                ModelState.AddModelError(response.ErrorTitle, response.ErrorMessage);
                return View(model);
            }

            // POST-Redirect-GET: avoid resubmission on refresh
            return RedirectToAction("Index", "Home");
        }

        [HttpPost] // POST: FINISH ORDER
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
