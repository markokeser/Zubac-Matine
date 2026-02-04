using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zubac.Data;
using Zubac.Models;
using Zubac.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Zubac.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Users?> Login(LoginViewModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
                return null;

            var hasher = new PasswordHasher<Users>();

            // Provera hash lozinke
            var result = hasher.VerifyHashedPassword(user, user.Password, model.Password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

        public async Task<RestaurantData> GetRestaurantData(int id)
        {
            RestaurantData response = await _context.RestaurantSettings
                .Where(x => x.Id == id)
                .Select(y => new RestaurantData
                {
                    Id = y.Id,
                    Name = y.Name,
                    FoodEnabled = y.FoodEnabled,
                    FreeDrinksEnabled = y.FreeDrinksEnabled
                }).FirstOrDefaultAsync();

            return response;
        }
    }
}
