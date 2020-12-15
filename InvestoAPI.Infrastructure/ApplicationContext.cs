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
            builder.Entity<WalletState>()
                .HasKey(t => new { t.WalletId, t.StockId });
            builder.Entity<Team>().HasOne(t => t.Owner).WithMany(u => u.OwningTeams);

            builder.Entity<TeamUser>()
                .HasKey(t => new { t.UserId, t.TeamId });

            builder.Entity<TeamUser>()
                .HasOne(tu => tu.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(tu => tu.TeamId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeamUser>()
                .HasOne(tu => tu.User)
                .WithMany(u => u.TeamUsers)
                .HasForeignKey(tu => tu.UserId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Team>()
                .HasIndex(t => t.Code)
                .IsUnique();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockDaily> StocksDaily { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletState> WalletsState { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Split> Split { get; set; }
        public DbSet<Team> Teams { get; set; }
    }
}
