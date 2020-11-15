using InvestoAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestoAPI.Infrastructure
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Stock>()
                .HasKey(t => new { t.StockId, t.Date });
            builder.Entity<StockDaily>()
                .HasKey(t => new { t.StockId, t.Date });
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockDaily> StocksDaily { get; set; }
    }
}
