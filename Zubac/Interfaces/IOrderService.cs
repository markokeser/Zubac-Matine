using Zubac.Models;

namespace Zubac.Interfaces
{
    public interface IOrderService
    {
        public Task<List<Order>> GetOrders(int id, int adminId);

        public Task<ServiceResult> CreateOrder(MakeOrderViewModel model, int id, int adminId);

        public Task<ServiceResult> OrderOnBar(MakeOrderViewModel model, int id, int adminId);

        public Task<ServiceResult> CreateFreeDrink(MakeOrderViewModel model, int id, int adminId);

        public Task<ServiceResult> FinishOrder(int id);

        public Task<List<Article>> GetArticles(int adminId);

        public Task<List<ArticleViewModel>> GetModelArticles(int adminId);

        public Task<List<Users>> GetWaiters(int adminId);

        public Task<List<Order>> SearchOrders(int? tableNumber, int? createdBy, int adminId);
    }
}
