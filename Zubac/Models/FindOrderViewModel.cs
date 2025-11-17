using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Zubac.Models
{
    public class FindOrderViewModel
    {
        public int? TableNumber { get; set; }
        public int? CreatedBy { get; set; }

        public List<Users> Waiters { get; set; } = new List<Users>();
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
