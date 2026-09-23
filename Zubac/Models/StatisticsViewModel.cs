namespace Zubac.Models
{
    public class UserEarningsViewModel
    {
        public string Username { get; set; }
        public int UserRank { get; set; }
        public decimal Earnings { get; set; }

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
    }

    public class UserFreeOrdersViewModel
    {
        public string Username { get; set; }
        public int FreeOrdersCount { get; set; }
    }

    public class ArticleStatsViewModel
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Earnings { get; set; }
    }

    public class StatisticsViewModel
    {
        public List<UserEarningsViewModel> PaidUserStats { get; set; } = new();
        public List<UserFreeOrdersViewModel> FreeUserStats { get; set; } = new();
        public List<ArticleStatsViewModel> PaidArticles { get; set; } = new();
        public List<ArticleStatsViewModel> FreeArticles { get; set; } = new();
        public decimal TotalEarnings { get; set; }
        public int TotalFreeOrders { get; set; }
        public int TotalPaidDrinks { get; set; }
        public decimal TotalPaidEarnings { get; set; }

        // Tracking window the numbers were calculated for.
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
        public bool IsRealtime { get; set; }
    }
}
