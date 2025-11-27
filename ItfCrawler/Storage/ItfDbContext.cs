
using Microsoft.EntityFrameworkCore;

namespace ItfCrawler.Storage;

// ----------------------------------------------------------------------------------

// --- EF Core DbContext Class ---
public class ItfDbContext(DbContextOptions<ItfDbContext> options) : DbContext(options)
{
    // Constructor to receive DbContextOptions

    // DbSet properties for each table/entity
    public DbSet<ItfLeague> ItfLeagues { get; set; }
    public DbSet<ItfMatch> ItfMatches { get; set; }
    public DbSet<ItfPlayer> ItfPlayers { get; set; }
    public DbSet<TennisCountry> TennisCountries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Custom configurations (if needed).
        modelBuilder.Entity<ItfMatch>().Property(m => m.Id).ValueGeneratedNever();
        modelBuilder.Entity<ItfPlayer>().Property(p => p.Id).ValueGeneratedNever();

        base.OnModelCreating(modelBuilder);
    }
}