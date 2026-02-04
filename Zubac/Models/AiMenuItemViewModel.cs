namespace Zubac.Models
{
    public class AiMenuItemViewModel
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Type { get; set; } = "";
        public bool IsFood { get; set; }
    }

    public class AiMenuConfirmViewModel
    {
        public List<AiMenuItemViewModel> Items { get; set; } = new();
    }
}
