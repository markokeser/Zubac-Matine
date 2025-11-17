using Zubac.Models;

namespace Zubac.Interfaces
{
    public interface IAcccountService
    {
        public Task<Users> Login(LoginViewModel model);
    }
}
