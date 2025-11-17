namespace Zubac.Models
{
    public class MakeOrderViewModel
    {
        public int TableNumber { get; set; }

        public List<ArticleViewModel> Articles { get; set; } = new();
        public List<SelectedArticleViewModel> SelectedArticles { get; set; } = new();
    }
}
