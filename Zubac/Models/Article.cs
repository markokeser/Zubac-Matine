using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zubac.Models
{
    [Table("Article")]
    public class Article
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public bool IsFood { get; set; }
        [Required]
        public int RestaurantId { get; set; }
        public ICollection<OrderArticle> OrderArticles { get; set; } = new List<OrderArticle>();
    }
}