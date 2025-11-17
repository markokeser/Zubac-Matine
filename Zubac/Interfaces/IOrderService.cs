using Zubac.Models;

namespace Zubac.Interfaces
{
    public interface IOrderService
    {
        public Task<List<Order>> GetOrders(int id);

        public Task<ServiceResult> CreateOrder(MakeOrderViewModel model, int id);

        public Task<ServiceResult> OrderOnBar(MakeOrderViewModel model, int id);

        public Task<ServiceResult> CreateFreeDrink(MakeOrderViewModel model, int id);

        public Task<ServiceResult> FinishOrder(int id);

        public Task<List<Article>> GetArticles();

        public Task<List<ArticleViewModel>> GetModelArticles();

        public Task<List<Users>> GetWaiters();

        public Task<List<Order>> SearchOrders(int? tableNumber, int? createdBy);
    }
}
