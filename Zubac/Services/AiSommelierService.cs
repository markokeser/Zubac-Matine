using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Zubac.Data;
using Zubac.Interfaces;
using Zubac.Models;

namespace Zubac.Services
{
    public class AiSommelierService : IAiSommelierService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public AiSommelierService(ApplicationDbContext context, IHttpClientFactory factory, IConfiguration config)
        {
            _context = context;
            _factory = factory;
            _config = config;
        }

        public async Task<List<ArticleViewModel>> GetFoodArticlesAsync(int restaurantId)
        {
            return await _context.Articles
                .Where(a => a.RestaurantId == restaurantId && a.IsFood && a.AiSommelierEnabled == true)
                .Select(a => new ArticleViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    Price = a.Price,
                    IsFood = a.IsFood,
                    RestaurantId = restaurantId
                })
                .ToListAsync();
        }

        public async Task<DrinkArticle?> GetDrinkRecommendationAsync(int restaurantId, int foodId)
        {
            var food = await _context.Articles.FirstOrDefaultAsync(x => x.Id == foodId);
            var drinks = await _context.Articles
                .Where(x => x.RestaurantId == restaurantId && !x.IsFood && x.AiSommelierEnabled == true)
                .ToListAsync();

            if (food == null || !drinks.Any())
                return null;

            string drinkListText = string.Join("\n",
                drinks.Select(d =>
                    $"- {d.Name} (Type: {(string.IsNullOrEmpty(d.Type) ? "Unknown" : d.Type)})"
                )
            );

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config["OpenAI:ApiKey"]);

            var requestBody = new
            {
                model = _config["OpenAI:Model"],
                messages = new[]
                {
        new {
            role = "system",
            content = "You are an expert AI sommelier. Choose ONLY ONE drink that pairs best with the food. Then explain the pairing in 2–3 sentences."
        },
        new {
            role = "user",
            content =
                $"Food: {food.Name}\n" +
                $"Available drinks (with type):\n{drinkListText}\n\n" +
                $"Respond ONLY in this JSON format:\n" +
                "{\"drink\": \"Drink Name From List\", \"reason\": \"Why this drink pairs well (2-3 sentences).\"}"
        }
    }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var aiText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(aiText))
                return null;

            try
            {
                var parsed = JsonDocument.Parse(aiText);
                string drinkName = parsed.RootElement.GetProperty("drink").GetString();
                string explanation = parsed.RootElement.GetProperty("reason").GetString();

                var selected = drinks.FirstOrDefault(d =>
                    d.Name.Equals(drinkName, StringComparison.OrdinalIgnoreCase));

                if (selected != null)
                {
                    return new DrinkArticle
                    {
                        Id = selected.Id,
                        Name = selected.Name,
                        Explanation = explanation
                    };
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
