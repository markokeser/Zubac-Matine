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
                0 => "Bartender",
                1 => "Waiter",
                2 => "Manager",
                3 => "Admin"
            };
        }

        public int GetRankValue(string rankName)
        {
            return rankName switch
            {
                "Bartender" => 0,
                "Waiter" => 1,
                "Manager" => 2,
                "Admin" => 3,
                _ => -1
            };
        }
    }
}
