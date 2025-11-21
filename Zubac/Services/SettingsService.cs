using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web.Helpers;
using Zubac.Data;
using Zubac.Interfaces;
using Zubac.Models;

namespace Zubac.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Users> _passwordHasher;

        public SettingsService(ApplicationDbContext context, IPasswordHasher<Users> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<StaffViewModel>> GetStaff(int restaurantId)
        {
            List<StaffViewModel> response = await _context.Users.Where(x => x.RestaurantId == restaurantId).Select(a => new StaffViewModel
            {
                Id = a.Id,
                Username = a.Username,
                UserRank = a.UserRank
            }).ToListAsync();

            return response;
        }

        public async Task<string> CreateStaffLinkAsync(int staffId)
        {
            var token = Guid.NewGuid().ToString(); // generiše jedinstveni token

            var staffLink = new StaffLink
            {
                StaffId = staffId,
                Token = token,
                IsUsed = false,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(1) // link važi 1h
            };

            _context.StaffLinks.Add(staffLink);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<StaffLink> GetStaffLinkByTokenAsync(string token)
        {
            var response = await _context.StaffLinks
                .Include(sl => sl.Staff)
                .FirstOrDefaultAsync(sl => sl.Token == token && !sl.IsUsed);
            return response;
        }

        public async Task<bool> SetStaffPasswordAsync(int staffId, string newPassword, string token)
        {
            var staff = await _context.Users.FirstOrDefaultAsync(u => u.Id == staffId);
            if (staff == null) return false;

            var staffLink = await _context.StaffLinks
                .FirstOrDefaultAsync(sl => sl.Token == token && sl.StaffId == staffId && !sl.IsUsed);

            if (staffLink == null) return false;

            // Hashujemo novu lozinku
            staff.Password = _passwordHasher.HashPassword(staff, newPassword);

            // Obeležavamo link kao iskorišćen
            staffLink.IsUsed = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public bool VerifyPassword(Users user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<bool> AddStaffAsync(string username, int rank, int restaurantId)
        {
            // Da li već postoji username
            var exists = await _context.Users.AnyAsync(x => x.Username == username);
            if (exists)
                return false;

            // Kreiramo user-a bez passworda (password se kasnije setuje preko linka)
            var user = new Users
            {
                Username = username,
                UserRank = rank,
                Password = null,
                RestaurantId = restaurantId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteStaffAsync(int staffId)
        {
            var staff = await _context.Users.FirstOrDefaultAsync(u => u.Id == staffId);
            if (staff == null)
                return false;

            _context.Users.Remove(staff);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RestaurantSettingsViewModel?> GetRestaurantSettingsAsync(int restaurantId)
        {
            var settings = await _context.RestaurantSettings
                .FirstOrDefaultAsync(s => s.Id == restaurantId);

            if (settings == null) return null;

            return new RestaurantSettingsViewModel
            {
                Id = settings.Id,
                AdminId = settings.AdminId,
                FoodEnabled = settings.FoodEnabled,
                FreeDrinksEnabled = settings.FreeDrinksEnabled,
                StartTime = settings.StartTime
            };
        }

        public async Task<bool> SaveRestaurantSettingsAsync(RestaurantSettingsViewModel model)
        {
            var settings = await _context.RestaurantSettings
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (settings == null) return false;

            settings.FoodEnabled = model.FoodEnabled;
            settings.FreeDrinksEnabled = model.FreeDrinksEnabled;
            settings.StartTime = model.StartTime;
            settings.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RestaurantSettings> CreateDefaultSettingsAsync(int restaurantId)
        {
            var settings = await _context.RestaurantSettings
                .FirstOrDefaultAsync(s => s.Id == restaurantId);

            _context.RestaurantSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<List<ArticleViewModel>> GetArticlesAsync(int restaurantId, bool isFood)
        {
            return await _context.Articles
                .Where(a => a.RestaurantId == restaurantId && a.IsFood == isFood)
                .Select(a => new ArticleViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Price = a.Price,
                    IsFood = a.IsFood,
                    RestaurantId = a.RestaurantId
                }).ToListAsync();
        }

        public async Task AddArticleAsync(ArticleViewModel model)
        {
            var article = new Article
            {
                Name = model.Name,
                Price = model.Price,
                IsFood = model.IsFood,
                RestaurantId = model.RestaurantId
            };
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }
    }
}
