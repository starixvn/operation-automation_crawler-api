using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Translator.Models;
using Translator.Models.LiveTennis;
using Tennis.Models;
using Tennis.Models.LiveTennis;

namespace Translator.Storage;

public class OperationCrawlerDbContext(DbContextOptions<OperationCrawlerDbContext> options) : DbContext(options)
{
    public DbSet<LiveTennisPlayer> LiveTennisPlayers { get; set; }

    public DbSet<Soccer7m> Soccer7m { get; set; }

    public DbSet<Basketball7m> Basketball7m { get; set; }

    public DbSet<Badminton> Badminton { get; set; }

    public DbSet<CountryCode> CountryCodes { get; set; }

    public DbSet<TennisLiveScore> TennisLiveScores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureSoccer7m(modelBuilder);
        ConfigureBasketball7m(modelBuilder);
        ConfigureBadminton(modelBuilder);
        ConfigureLiveTennis(modelBuilder);
        ConfigureCountryCode(modelBuilder);
        ConfigureTennisLiveScore(modelBuilder);
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

    private static void ConfigureCountryCode(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CountryCode>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<CountryCode>().ToTable("country_code");
    }

    private static void ConfigureTennisLiveScore(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TennisLiveScore>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<TennisLiveScore>().ToTable("tennis_live_scores");
        modelBuilder.Entity<TennisLiveScore>().Property(s => s.created_date).HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}