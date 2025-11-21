namespace Zubac.Models
{
    public class ArticleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public bool IsFood { get; set; }
        public int RestaurantId { get; set; }
    }
}
