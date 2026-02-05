using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
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
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public SettingsService(ApplicationDbContext context, IPasswordHasher<Users> passwordHasher, IHttpClientFactory factory, IConfiguration config)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _factory = factory;
            _config = config;
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
                StartTime = settings.StartTime,
                EndTime = settings.EndTime,
                RealtimeCounting = settings.RealtimeCounting
            };
        }

        public async Task<bool> SaveRestaurantSettingsAsync(RestaurantSettingsViewModel model)
        {
            var settings = await _context.RestaurantSettings
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if(settings.StartTime != model.StartTime)
            {
                List<Order> orders = await _context.Orders.Where(x => x.RestaurantId == model.Id && x.Finished == false).ToListAsync();
                if(orders.Count > 0)
                return false;
            }

            if (settings == null) return false;

            settings.FoodEnabled = model.FoodEnabled;
            settings.FreeDrinksEnabled = model.FreeDrinksEnabled;
            settings.StartTime = model.StartTime;
            settings.UpdatedAt = DateTime.Now;

            if(model.RealtimeCounting == true)
            {
                settings.RealtimeCounting = true;
                settings.EndTime = null;
            }
            else
            {
                settings.RealtimeCounting = false;
                settings.EndTime = model.EndTime;
            }


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
                .OrderBy(x => x.Type == "Cocktail" ? 1 :
                  x.Type == "Red Wine" ? 2 :
                  x.Type == "White Wine" ? 3 :
                  x.Type == "Rose Wine" ? 4 :
                  x.Type == "Beer" ? 5 :
                  x.Type == "Spirit" ? 6 :
                  x.Type == "NonAlcoholic" ? 7 :
                  999)
                .Select(a => new ArticleViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Price = a.Price,
                    IsFood = a.IsFood,
                    RestaurantId = a.RestaurantId,
                    Type = a.Type,
                    AiSommelierEnabled = a.AiSommelierEnabled,
                    IsAvailable = a.IsAvailable
                }).ToListAsync();
        }

        public async Task AddArticleAsync(ArticleViewModel model)
        {
            var article = new Article
            {
                Name = model.Name,
                Price = model.Price,
                IsFood = model.IsFood,
                RestaurantId = model.RestaurantId,
                Type = model.Type,
                AiSommelierEnabled = model.AiSommelierEnabled,
                IsAvailable = model.IsAvailable
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

        public bool ToggleSommelier(int articleId, bool enabled)
        {
            var article = _context.Articles
                .FirstOrDefault(a => a.Id == articleId);

            if (article == null)
                return false;

            article.AiSommelierEnabled = enabled;
            _context.SaveChanges();
            return true;
        }

        public bool ToggleAvailable(int articleId, bool enabled)
        {
            var article = _context.Articles
                .FirstOrDefault(a => a.Id == articleId);

            if (article == null)
                return false;

            article.IsAvailable = enabled;
            _context.SaveChanges();
            return true;
        }

        public async Task UpdateArticleAsync(Article updatedArticle)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == updatedArticle.Id);
            if (article == null)
                throw new Exception("Article not found");

            article.Name = updatedArticle.Name;
            article.Price = updatedArticle.Price;
            article.Type = updatedArticle.Type;
            article.IsAvailable = updatedArticle.IsAvailable;
            article.AiSommelierEnabled = updatedArticle.AiSommelierEnabled;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStaffAsync(StaffViewModel model)
        {
            var staff = await _context.Users.FindAsync(model.Id);
            if (staff == null) return false;

            staff.Username = model.Username;
            staff.UserRank = model.UserRank;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AiMenuItemViewModel>> ParseMenuFromImageAsync(IFormFile image)
        {
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("AI_PASS"));

            var requestBody = new
            {
                model = _config["OpenAI:VisionModel"], // npr "gpt-4.1" ili "gpt-4o"
                input = new object[]
    {
        new
        {
            role = "user",
            content = new object[]
            {
                new
                {
                    type = "input_text",
                    text =
@"This is a photo of a restaurant menu.

Rules:
- Extract ALL menu items.
- Detect if item is food or drink.
- Normalize prices to decimals.
- Guess item type.

Use ONLY the following types. If unsure, use ""Other"".

Food types: Soup, Salad, Meat, Fish, Dessert, Other
Drink types: Red Wine, White Wine, Rose Wine, Beer, Cocktail, Spirit, Non Alcoholic, Other

Important:
- Do NOT invent any other types.
- Return ONLY JSON, no extra text, no explanations.
- JSON format must be EXACTLY:

{
  ""items"": [
    { ""name"": """", ""price"": 0, ""type"": """", ""isFood"": true }
  ]
}"
                },
                new
                {
                    type = "input_image",
                    image_url = $"data:image/jpeg;base64,{base64Image}"
                }
            }
        }
    }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://api.openai.com/v1/responses",
                content
            );

            var responseJson = await response.Content.ReadAsStringAsync();

            // ✅ SIGURNO PARSIRANJE
            using var doc = JsonDocument.Parse(responseJson);

            if (!doc.RootElement.TryGetProperty("output", out var output))
                return new();
            var text = output[0]
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
                return new();

            text = text.Trim();

            // ✅ uklanjanje markdown code blocka
            if (text.StartsWith("```"))
            {
                text = text.Substring(text.IndexOf('{'));
                text = text.Substring(0, text.LastIndexOf('}') + 1);
            }

            try
            {
                var parsed = JsonDocument.Parse(text);
                var itemsJson = parsed.RootElement.GetProperty("items");

                var result = new List<AiMenuItemViewModel>();

                foreach (var item in itemsJson.EnumerateArray())
                {
                    result.Add(new AiMenuItemViewModel
                    {
                        Name = item.GetProperty("name").GetString() ?? "",
                        Price = item.GetProperty("price").GetDecimal(),
                        Type = item.GetProperty("type").GetString() ?? "Unknown",
                        IsFood = item.GetProperty("isFood").GetBoolean()
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                // ✅ privremeno za debug
                throw new Exception("AI JSON parse failed:\n" + text, ex);
            }
        }

        public async Task SaveAiMenuAsync(int restaurantId, List<AiMenuItemViewModel> items)
        {
            foreach (var item in items)
            {
                var article = new Article
                {
                    Name = item.Name,
                    Price = item.Price,
                    Type = item.Type,
                    IsFood = item.IsFood,
                    RestaurantId = restaurantId,

                    AiSommelierEnabled = false,
                    IsAvailable = true
                };

                _context.Articles.Add(article);
            }

            await _context.SaveChangesAsync();
        }
    }
}
