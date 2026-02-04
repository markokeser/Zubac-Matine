namespace Zubac.Models
{
    public class DrinkArticle
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? Explanation { get; set; }
    }
}
