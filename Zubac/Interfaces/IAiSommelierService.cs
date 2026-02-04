using Zubac.Models;

namespace Zubac.Interfaces
{
    public interface IAiSommelierService
    {
        public Task<List<ArticleViewModel>> GetFoodArticlesAsync(int restaurantId);

        public Task<DrinkArticle?> GetDrinkRecommendationAsync(int restaurantId, int foodId);
    }
}
