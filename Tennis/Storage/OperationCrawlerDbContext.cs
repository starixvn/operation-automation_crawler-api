using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Tennis.Models.LiveTennis;

namespace Tennis.Storage;

public class OperationCrawlerDbContext(DbContextOptions<OperationCrawlerDbContext> options) : DbContext(options)
{
    public DbSet<LiveTennisPlayer> LiveTennisPlayers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LiveTennisPlayer>()
            .HasKey(p => p.id);

        modelBuilder.Entity<LiveTennisPlayer>()
            .Property(p => p.id)
            .HasColumnType("varchar(255)");

        modelBuilder.Entity<LiveTennisPlayer>().ToTable("live_tennis_players");
    }
}

public class OperationCrawlerDbContextFactory : IDesignTimeDbContextFactory<OperationCrawlerDbContext>
{
    public OperationCrawlerDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("OperationCrawlerDatabase");

        var optionsBuilder = new DbContextOptionsBuilder<OperationCrawlerDbContext>();

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            options => options.MigrationsAssembly(typeof(OperationCrawlerDbContext).Assembly.FullName)
        );

        return new OperationCrawlerDbContext(optionsBuilder.Options);
    }
}