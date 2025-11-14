using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Tennis.Models;
using Tennis.Models.LiveTennis;

namespace Tennis.Storage;

public class OperationCrawlerDbContext(DbContextOptions<OperationCrawlerDbContext> options) : DbContext(options)
{
    public DbSet<LiveTennisPlayer> LiveTennisPlayers { get; set; }

    public DbSet<Soccer7m> Soccer7m { get; set; }

    public DbSet<Basketball7m> Basketball7m { get; set; }

    public DbSet<Badminton> Badminton { get; set; }

    public DbSet<AdminTeam> AdminTeam { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureSoccer7m(modelBuilder);
        ConfigureBasketball7m(modelBuilder);
        ConfigureBadminton(modelBuilder);
        ConfigureLiveTennis(modelBuilder);
        ConfigureAdminTeam(modelBuilder);
    }

    private static void ConfigureSoccer7m(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Soccer7m>()
            .HasKey(s => s.id);

        modelBuilder.Entity<Soccer7m>().ToTable("soccer_7m");
    }

    private static void ConfigureBasketball7m(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Basketball7m>()
            .HasKey(s => s.id);

        modelBuilder.Entity<Basketball7m>().ToTable("basketball_7m");
    }

    private static void ConfigureBadminton(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Badminton>()
            .HasKey(s => s.id);

        modelBuilder.Entity<Badminton>().ToTable("badminton");
    }

    private static void ConfigureLiveTennis(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LiveTennisPlayer>()
            .HasKey(p => p.id);

        modelBuilder.Entity<LiveTennisPlayer>()
            .Property(p => p.id)
            .HasColumnType("varchar(255)");

        modelBuilder.Entity<LiveTennisPlayer>().ToTable("live_tennis");
    }

    private static void ConfigureAdminTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminTeam>()
            .HasKey(p => p.id);

        modelBuilder.Entity<AdminTeam>()
            .Property(p => p.id)
            .HasColumnType("int")
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<AdminTeam>().ToTable("admin_teams");
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