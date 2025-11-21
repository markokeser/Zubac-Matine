using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zubac.Models
{
    [Table("Orders")]
    public class Order
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public bool Finished { get; set; } = false;

        public bool OnBar { get; set; } = false;
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public bool IsFree { get; set; } = false;
        [Required]
        public int RestaurantId { get; set; }

        public ICollection<OrderArticle> OrderArticles { get; set; } = new List<OrderArticle>();
    }
}
