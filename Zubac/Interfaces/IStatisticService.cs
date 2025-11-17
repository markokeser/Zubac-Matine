using Zubac.Models;

namespace Zubac.Interfaces
{
    public interface IStatisticService
    {
        public Task<StatisticsViewModel> GetStatistic();
    }
}
