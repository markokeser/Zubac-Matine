namespace Zubac.Models
{
    public class UserEarningsViewModel
    {
        public string Username { get; set; }
        public decimal Earnings { get; set; }
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
        public List<UserEarningsViewModel> PaidUserStats { get; set; }
        public List<UserFreeOrdersViewModel> FreeUserStats { get; set; }
        public List<ArticleStatsViewModel> PaidArticles { get; set; }
        public List<ArticleStatsViewModel> FreeArticles { get; set; }
        public decimal TotalEarnings { get; set; }
        public int TotalFreeOrders { get; set; }
        public int TotalPaidDrinks { get; set; }
        public decimal TotalPaidEarnings { get; set; }
    }
}
