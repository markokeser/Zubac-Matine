namespace Zubac.Models
{
    public class RestaurantSettings
    {
        public int Id { get; set; }

        public int AdminId { get; set; }
        public Users Admin { get; set; }

        public string Name { get; set; }

        public bool FoodEnabled { get; set; }
        public bool FreeDrinksEnabled { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public bool RealtimeCounting { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
