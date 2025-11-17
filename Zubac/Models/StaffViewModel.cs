namespace Zubac.Models
{
    public class StaffViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public int UserRank { get; set; }

        public string GetRankName()
        {
            return UserRank switch
            {
                0 => "Waiter",
                1 => "Bartender",
                2 => "Manager",
                3 => "Admin"
            };
        }
    }
}
