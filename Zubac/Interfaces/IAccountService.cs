using Zubac.Models;

namespace Zubac.Interfaces
{
    public interface IAccountService
    {
        public Task<Users> Login(LoginViewModel model);

        public Task<RestaurantData> GetRestaurantData(int id);
    }
}
