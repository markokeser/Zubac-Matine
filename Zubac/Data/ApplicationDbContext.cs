using Microsoft.EntityFrameworkCore;
using Zubac.Models; // Make sure this matches your namespace

namespace Zubac.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderArticle> OrderArticles { get; set; }
        public DbSet<Users> Users { get; set; }

        public DbSet<StaffLink> StaffLinks { get; set; }

        public DbSet<RestaurantSettings> RestaurantSettings { get; set; }
    }
}
