using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web.Mvc;
using Zubac.Data;
using Zubac.Interfaces;
using Zubac.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Zubac.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetOrders(int id, int restaurantId)
        {
            var orders = await _context.Orders
                .Where(o => o.Finished == false && o.CreatedBy == id && o.RestaurantId == restaurantId)
                .Include(o => o.OrderArticles)
                .ThenInclude(oa => oa.Article)
                .OrderByDescending(o => o.Created)
                .ToListAsync();

            return orders;
        }

        public async Task<ServiceResult> CreateOrder(MakeOrderViewModel model, int id, int restaurantId)
        {
            var orderTable = _context.Orders
                             .Where(o => o.TableNumber == model.TableNumber && o.Finished == false && o.CreatedBy == id && o.RestaurantId == restaurantId)
                             .FirstOrDefault();

            // Create new order
            if (orderTable == null)
            {
                var order = new Order
                {
                    TableNumber = model.TableNumber,
                    CreatedBy = id,
                    Created = DateTime.Now,
                    OrderArticles = new List<OrderArticle>(),
                    RestaurantId = restaurantId
                };

                foreach (var selected in model.SelectedArticles)
                {
                    if (selected.IsSelected && selected.Quantity > 0)
                    {
                        var articleExists = await _context.Articles.AnyAsync(a => a.Id == selected.ArticleId && a.RestaurantId == restaurantId);
                        if (!articleExists) continue;

                        order.OrderArticles.Add(new OrderArticle
                        {
                            ArticleId = selected.ArticleId,
                            Quantity = selected.Quantity,
                            RestaurantId = restaurantId
                        });
                    }
                }

                if (order.OrderArticles.Count == 0)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        ErrorMessage = "You must add at least one valid drink."
                    };
                }

                _context.Orders.Add(order);
            }
            else
            {
                orderTable.Created = DateTime.Now;
                var OrderArticles = new List<OrderArticle>();
                foreach (var selected in model.SelectedArticles)
                {
                    if (selected.IsSelected && selected.Quantity > 0)
                    {
                        var articleExists = await _context.Articles.AnyAsync(a => a.Id == selected.ArticleId && a.RestaurantId == restaurantId);
                        if (!articleExists) continue;

                        _context.OrderArticles.Add(new OrderArticle
                        {
                            OrderId = orderTable.Id,
                            ArticleId = selected.ArticleId,
                            Quantity = selected.Quantity,
                            RestaurantId = restaurantId
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true
            };
        }

        public async Task<ServiceResult> OrderOnBar(MakeOrderViewModel model, int id, int restaurantId)
        {
            var order = new Order
            {
                OnBar = true, // THIS marks the order as a bar order
                Finished = true,
                OrderArticles = new List<OrderArticle>(),
                Created = DateTime.Now,
                CreatedBy = id,
                RestaurantId = restaurantId
            };

            foreach (var selected in model.SelectedArticles)
            {
                if (selected.IsSelected && selected.Quantity > 0)
                {
                    var articleExists = await _context.Articles.AnyAsync(a => a.Id == selected.ArticleId);
                    if (!articleExists) continue;

                    order.OrderArticles.Add(new OrderArticle
                    {
                        ArticleId = selected.ArticleId,
                        Quantity = selected.Quantity,
                        RestaurantId = restaurantId
                    });
                }
            }

            if (order.OrderArticles.Count == 0)
            {
                model.Articles = await _context.Articles
                    .Where(x => x.RestaurantId == restaurantId)
                    .Select(a => new ArticleViewModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Price = a.Price,
                        IsFood = a.IsFood,
                        RestaurantId = restaurantId
                    }).ToListAsync();

                return new ServiceResult
                {
                    Success = false,
                    ErrorTitle = "OrderOnBar",
                    ErrorMessage = "You must add at least one valid drink."
                };
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true
            };
        }

        public async Task<ServiceResult> CreateFreeDrink(MakeOrderViewModel model, int id, int restaurantId)
        {
            var order = new Order
            {
                OnBar = true, // THIS marks the order as a bar order
                Finished = true,
                OrderArticles = new List<OrderArticle>(),
                Created = DateTime.Now,
                CreatedBy = id,
                IsFree = true,
                RestaurantId = restaurantId
            };

            foreach (var selected in model.SelectedArticles)
            {
                if (selected.IsSelected && selected.Quantity > 0)
                {
                    var articleExists = await _context.Articles.AnyAsync(a => a.Id == selected.ArticleId && a.RestaurantId == restaurantId);
                    if (!articleExists) continue;

                    order.OrderArticles.Add(new OrderArticle
                    {
                        ArticleId = selected.ArticleId,
                        Quantity = selected.Quantity,
                        RestaurantId = restaurantId
                    });
                }
            }

            if (order.OrderArticles.Count == 0)
            {
                model.Articles = await _context.Articles
                    .Where(x => x.RestaurantId == restaurantId)
                    .Select(a => new ArticleViewModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Price = a.Price,
                        IsFood = a.IsFood,
                        RestaurantId = restaurantId
                    }).ToListAsync();

                return new ServiceResult
                {
                    Success = false,
                    ErrorTitle = "FreeDrink",
                    ErrorMessage = "You must add at least one valid drink."
                };
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true
            };
        }

        public async Task<ServiceResult> FinishOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return new ServiceResult
            {
                Success = false
            };

            order.Finished = true;
            await _context.SaveChangesAsync();

            return new ServiceResult
            {
                Success = true
            };
        }

        public async Task<List<Article>> GetArticles(int restaurantId)
        {
            List<Article> articles = await _context.Articles.Where(x => x.RestaurantId == restaurantId).ToListAsync();

            return articles;
        }

        public async Task<List<ArticleViewModel>> GetModelArticles(int restaurantId)
        {
            List<ArticleViewModel> modelArticles = await _context.Articles.Where(x => x.RestaurantId == restaurantId).Select(a => new ArticleViewModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Price = a.Price,
                        IsFood = a.IsFood,
                        RestaurantId = restaurantId
                    }).ToListAsync();

            return modelArticles;
        }

        public async Task<List<Users>> GetWaiters(int restaurantId)
        {
            List<Users> waiters = await _context.Users.Where(x => x.RestaurantId == restaurantId).ToListAsync();

            return waiters;
        }

        public async Task<List<Order>> SearchOrders(int? tableNumber, int? createdBy, int restaurantId)
        {
            var query = _context.Orders
                .Where(x => x.RestaurantId == restaurantId)
                .Include(o => o.OrderArticles)
                .ThenInclude(oa => oa.Article)
                .AsQueryable();

            if (tableNumber.HasValue)
                query = query.Where(o => o.TableNumber == tableNumber.Value);

            if (createdBy.HasValue)
                query = query.Where(o => o.CreatedBy == createdBy.Value);

            return await query
                .OrderByDescending(o => o.Created)
                .ToListAsync();
        }
    }
}
