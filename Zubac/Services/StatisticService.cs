using Microsoft.EntityFrameworkCore;
using Zubac.Data;
using Zubac.Interfaces;
using Zubac.Models;

namespace Zubac.Services
{

    public class StatisticService : IStatisticService
    {
    private readonly ApplicationDbContext _context;

        public StatisticService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StatisticsViewModel> GetStatistic(int restaurantId)
        {
            var paidStats = await _context.Users
            .Where(x => x.RestaurantId == restaurantId)
            .Select(u => new UserEarningsViewModel
            {
                Username = u.Username,
                UserRank = u.UserRank,
                Earnings = _context.Orders
                    .Where(o => o.CreatedBy == u.Id && !o.IsFree)
                    .SelectMany(o => o.OrderArticles)
                    .Sum(oa => (decimal?)oa.Quantity * oa.Article.Price) ?? 0m
            })
            .ToListAsync();

            var freeStats = await _context.Users
                .Where(x => x.RestaurantId == restaurantId)
                .Select(u => new UserFreeOrdersViewModel
                {
                    Username = u.Username,
                    FreeOrdersCount = _context.Orders
                        .Where(o => o.CreatedBy == u.Id && o.IsFree)
                        .Count()
                })
                .ToListAsync();

            var totalEarnings = paidStats.Sum(u => u.Earnings);
            var totalFreeOrders = freeStats.Sum(u => u.FreeOrdersCount);

            var model = new StatisticsViewModel
            {
                PaidUserStats = paidStats,
                FreeUserStats = freeStats,
                TotalEarnings = totalEarnings,
                TotalFreeOrders = totalFreeOrders
            };

            var paidArticles = await _context.Articles
                .Where(x => x.RestaurantId == restaurantId)
        .Select(a => new ArticleStatsViewModel
        {
            Name = a.Name,
            Quantity = _context.OrderArticles
                .Where(oa => oa.ArticleId == a.Id && !oa.Order.IsFree && oa.RestaurantId == restaurantId)
                .Sum(oa => (int?)oa.Quantity) ?? 0,
            Earnings = _context.OrderArticles
                .Where(oa => oa.ArticleId == a.Id && !oa.Order.IsFree && oa.RestaurantId == restaurantId)
                .Sum(oa => (decimal?)oa.Quantity * oa.Article.Price) ?? 0m
        })
        .Where(a => a.Quantity > 0)
        .OrderByDescending(a => a.Quantity)
        .ToListAsync();

            var totalPaidDrinks = paidArticles.Sum(a => a.Quantity);
            var totalPaidEarnings = paidArticles.Sum(a => a.Earnings);

            model.TotalPaidDrinks = totalPaidDrinks;
            model.TotalPaidEarnings = totalPaidEarnings;

            // Free articles stats
            var freeArticles = await _context.Articles
                .Where(x => x.RestaurantId == restaurantId)
                .Select(a => new ArticleStatsViewModel
                {
                    Name = a.Name,
                    Quantity = _context.OrderArticles
                        .Where(oa => oa.ArticleId == a.Id && oa.Order.IsFree)
                        .Sum(oa => (int?)oa.Quantity) ?? 0,
                    Earnings = 0 // not used for free drinks
                })
                .Where(a => a.Quantity > 0)
                .OrderByDescending(a => a.Quantity)
                .ToListAsync();

            model.PaidArticles = paidArticles;
            model.FreeArticles = freeArticles;

            return model;
        }
    }
}
