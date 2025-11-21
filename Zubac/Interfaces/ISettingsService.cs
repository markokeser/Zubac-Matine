using Zubac.Models;

namespace Zubac.Interfaces
{
    public interface ISettingsService
    {
        public Task<List<StaffViewModel>> GetStaff(int adminId);

        public Task<string> CreateStaffLinkAsync(int staffId);

        public Task<bool> SetStaffPasswordAsync(int staffId, string newPassword, string token);

        public bool VerifyPassword(Users user, string password);

        public Task<StaffLink> GetStaffLinkByTokenAsync(string token);

        public Task<bool> AddStaffAsync(string username, int rank, int adminId);

        public Task<bool> DeleteStaffAsync(int staffId);

        public Task<RestaurantSettingsViewModel?> GetRestaurantSettingsAsync(int adminId);

        public Task<bool> SaveRestaurantSettingsAsync(RestaurantSettingsViewModel model);

        public Task<RestaurantSettings> CreateDefaultSettingsAsync(int adminId);

        public Task<List<ArticleViewModel>> GetArticlesAsync(int adminId, bool isFood);

        public Task AddArticleAsync(ArticleViewModel model);

        public Task DeleteArticleAsync(int id);
    }
}
