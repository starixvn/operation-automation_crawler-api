namespace Tennis.Storage;
using Microsoft.EntityFrameworkCore;

public class TennisDbContext(DbContextOptions<TennisDbContext> options) : DbContext(options)
{
    public DbSet<TennisLeague> TennisLeagues { get; set; }
    public DbSet<TennisMatch> TennisMatches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TennisLeague>().ToTable("ATP_leagues");
        modelBuilder.Entity<TennisMatch>().ToTable("ATP_matches");
    }
}