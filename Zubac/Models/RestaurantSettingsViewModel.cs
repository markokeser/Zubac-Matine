namespace Zubac.Models
{
    public class RestaurantSettingsViewModel
    {
        public int Id { get; set; }

        public int AdminId { get; set; }

        public bool FoodEnabled { get; set; }

        public bool FreeDrinksEnabled { get; set; }

        public DateTime StartTime { get; set; }
    }
}
